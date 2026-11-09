using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LabControlApi.Data;
using LabControlApi.DTOs.Push;
using LabControlApi.Models;
using LabControlApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;

namespace LabControlApi.Services
{
    public class MqttIntegrationService : BackgroundService
    {
        private readonly ILogger<MqttIntegrationService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _config;
        private IMqttClient? _client;
        private readonly string _host;
        private readonly int _port;
        private readonly string _topic;

        // Limiares de Alertas configuráveis
        private readonly double _maxTemperature;
        private readonly double _maxVibration;
        private readonly double _maxRpm;
        private readonly double _maxEnergyUsage;

        // Cache em memória para deduplicação (Chave de mensagem -> Data de recepção)
        private readonly ConcurrentDictionary<string, DateTime> _deduplicationCache = new();

        // Controle para evitar rajadas de alertas repetidos no push da mesma máquina (MachineId+Tipo -> Data último alerta)
        private readonly ConcurrentDictionary<string, DateTime> _lastAlertSent = new();

        // Propriedades reservadas de metadados que NUNCA devem virar métricas
        private static readonly HashSet<string> ReservedMetadataKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "machineid", "deviceid", "messageid", "id", "timestamp", "collectedat",
            "time", "datetime", "date", "unit", "units", "type", "metadata"
        };

        public MqttIntegrationService(
            ILogger<MqttIntegrationService> logger,
            IServiceProvider serviceProvider,
            IConfiguration config)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _config = config;

            _host = config["Mqtt:Host"] ?? "localhost";
            _port = int.TryParse(config["Mqtt:Port"], out var p) ? p : 1883;
            _topic = config["Mqtt:Topic"] ?? "lab/machine/metrics";

            _maxTemperature = double.TryParse(config["AlertThresholds:MaxTemperature"], out var t) ? t : 80.0;
            _maxVibration = double.TryParse(config["AlertThresholds:MaxVibration"], out var v) ? v : 5.0;
            _maxRpm = double.TryParse(config["AlertThresholds:MaxRpm"], out var r) ? r : 3500.0;
            _maxEnergyUsage = double.TryParse(config["AlertThresholds:MaxEnergyUsage"], out var e) ? e : 100.0;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(_host, _port)
                .WithClientId("labcontrol-backend-" + Guid.NewGuid().ToString("N")[..6])
                .WithCleanSession()
                .WithTimeout(TimeSpan.FromSeconds(5))
                .Build();

            _client.ApplicationMessageReceivedAsync += async e =>
            {
                await HandleIncomingMessageAsync(e);
            };

            _client.DisconnectedAsync += async e =>
            {
                _logger.LogWarning("Conexão com broker MQTT perdida ({Reason}). Tentando reconectar...", e.Reason);
                await Task.CompletedTask;
            };

            // Loop de conexão e monitoramento resiliente com reconexão automática
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_client == null || !_client.IsConnected)
                    {
                        _logger.LogInformation("Tentando conectar ao broker MQTT ({Host}:{Port})...", _host, _port);
                        await _client!.ConnectAsync(options, stoppingToken);

                        if (_client.IsConnected)
                        {
                            await _client.SubscribeAsync(_topic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, stoppingToken);
                            _logger.LogInformation("Conectado ao broker MQTT e inscrito no tópico '{Topic}'.", _topic);
                        }
                    }
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogWarning("MQTT: Broker indisponível em {Host}:{Port} ({Message}). Tentando novamente em 30 segundos...", _host, _port, ex.Message);
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }

                // Limpeza periódica do cache de deduplicação (itens mais velhos que 10 minutos)
                CleanExpiredDeduplicationKeys();

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        private async Task HandleIncomingMessageAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload ?? Array.Empty<byte>());
                if (string.IsNullOrWhiteSpace(payload)) return;

                using var doc = JsonDocument.Parse(payload);
                var root = doc.RootElement;

                // 1. Validação de MachineId
                if (!root.TryGetProperty("machineId", out var midProp))
                {
                    _logger.LogWarning("MQTT: Mensagem rejeitada. Campo 'machineId' ausente.");
                    return;
                }

                var machineIdStr = midProp.GetString();
                if (!Guid.TryParse(machineIdStr, out var machineId))
                {
                    _logger.LogWarning("MQTT: Mensagem rejeitada. 'machineId' '{InvalidId}' não é um GUID válido.", machineIdStr);
                    return;
                }

                // 2. Extração do Timestamp (garantindo que timestamp NÃO vire métrica)
                DateTime collectedAt = DateTime.UtcNow;
                if (root.TryGetProperty("timestamp", out var tsProp) || root.TryGetProperty("collectedAt", out tsProp))
                {
                    if (tsProp.ValueKind == JsonValueKind.String && DateTime.TryParse(tsProp.GetString(), out var parsedDate))
                    {
                        collectedAt = parsedDate.ToUniversalTime();
                    }
                    else if (tsProp.ValueKind == JsonValueKind.Number && tsProp.TryGetInt64(out var unixEpoch))
                    {
                        // Suporte a timestamp unix em segundos ou milissegundos
                        collectedAt = unixEpoch > 10_000_000_000
                            ? DateTimeOffset.FromUnixTimeMilliseconds(unixEpoch).UtcDateTime
                            : DateTimeOffset.FromUnixTimeSeconds(unixEpoch).UtcDateTime;
                    }
                }

                // 3. Deduplicação de Mensagem
                string deduplicationKey;
                if (root.TryGetProperty("messageId", out var msgIdProp) && !string.IsNullOrWhiteSpace(msgIdProp.GetString()))
                {
                    deduplicationKey = msgIdProp.GetString()!;
                }
                else
                {
                    deduplicationKey = $"{machineId}:{collectedAt.Ticks}:{payload.GetHashCode()}";
                }

                if (!_deduplicationCache.TryAdd(deduplicationKey, DateTime.UtcNow))
                {
                    _logger.LogInformation("MQTT: Mensagem duplicada ignorada (Chave: {Key}).", deduplicationKey);
                    return;
                }

                // 4. Processamento no Banco de Dados
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var pushService = scope.ServiceProvider.GetRequiredService<IPushNotificationService>();

                // Validar se a máquina existe e obter o UserId do proprietário da planta
                var machine = await dbContext.Machines
                    .Include(m => m.Plant)
                    .FirstOrDefaultAsync(m => m.Id == machineId);

                if (machine == null)
                {
                    _logger.LogWarning("MQTT: Mensagem rejeitada. Máquina {MachineId} não encontrada no banco de dados.", machineId);
                    return;
                }

                // 5. Mapear e Salvar Métrica
                var metric = new MachineMetric
                {
                    MachineId = machineId,
                    CollectedAt = collectedAt
                };

                bool hasAnyMetric = false;

                // Suporte a payload estruturado: { "temperature": 85, "rpm": 1500, ... }
                foreach (var prop in root.EnumerateObject())
                {
                    if (ReservedMetadataKeys.Contains(prop.Name)) continue;

                    if (!prop.Value.TryGetDouble(out var numValue)) continue;

                    hasAnyMetric = true;
                    var propName = prop.Name.ToLowerInvariant();

                    switch (propName)
                    {
                        case "temperature":
                        case "temp":
                            metric.Temperature = numValue;
                            break;
                        case "rpm":
                        case "speed":
                            metric.Rpm = numValue;
                            break;
                        case "vibration":
                        case "vib":
                            metric.Vibration = numValue;
                            break;
                        case "energyusage":
                        case "energy":
                        case "power":
                            metric.EnergyUsage = numValue;
                            break;
                    }
                }

                // Suporte a formato unitário: { "metric": "temperature", "value": 85 }
                if (!hasAnyMetric && root.TryGetProperty("metric", out var metricNameProp) && root.TryGetProperty("value", out var valProp))
                {
                    var mName = metricNameProp.GetString()?.ToLowerInvariant();
                    if (valProp.TryGetDouble(out var numVal))
                    {
                        hasAnyMetric = true;
                        if (mName == "temperature" || mName == "temp") metric.Temperature = numVal;
                        else if (mName == "rpm" || mName == "speed") metric.Rpm = numVal;
                        else if (mName == "vibration" || mName == "vib") metric.Vibration = numVal;
                        else if (mName == "energyusage" || mName == "energy" || mName == "power") metric.EnergyUsage = numVal;
                    }
                }

                if (!hasAnyMetric)
                {
                    _logger.LogWarning("MQTT: Mensagem recebida sem valores numéricos válidos para métricas.");
                    return;
                }

                dbContext.MachineMetrics.Add(metric);
                await dbContext.SaveChangesAsync();
                _logger.LogInformation("MQTT: Telemetria da máquina {MachineName} persistida com sucesso.", machine.Name);

                // 6. Avaliação de Limiares de Alertas e Disparo de Push
                await CheckAlertsAndTriggerPushAsync(machine, metric, dbContext, pushService);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MQTT: Erro inesperado ao processar mensagem recebida.");
            }
        }

        private async Task CheckAlertsAndTriggerPushAsync(
            Machine machine,
            MachineMetric metric,
            AppDbContext dbContext,
            IPushNotificationService pushService)
        {
            var triggeredAlerts = new List<(string EventType, string Message)>();

            if (metric.Temperature > _maxTemperature)
            {
                triggeredAlerts.Add((
                    "ALERTA_TEMPERATURA",
                    $"Temperatura crítica na máquina '{machine.Name}': {metric.Temperature:F1}°C (Limite: {_maxTemperature}°C)"
                ));
            }

            if (metric.Vibration > _maxVibration)
            {
                triggeredAlerts.Add((
                    "ALERTA_VIBRACAO",
                    $"Vibração excessiva na máquina '{machine.Name}': {metric.Vibration:F2} mm/s (Limite: {_maxVibration} mm/s)"
                ));
            }

            if (metric.Rpm > _maxRpm)
            {
                triggeredAlerts.Add((
                    "ALERTA_RPM",
                    $"Sobrerrotação na máquina '{machine.Name}': {metric.Rpm:F0} RPM (Limite: {_maxRpm} RPM)"
                ));
            }

            if (metric.EnergyUsage > _maxEnergyUsage)
            {
                triggeredAlerts.Add((
                    "ALERTA_CONSUMO",
                    $"Consumo elétrico crítico na máquina '{machine.Name}': {metric.EnergyUsage:F1} kWh (Limite: {_maxEnergyUsage} kWh)"
                ));
            }

            foreach (var alert in triggeredAlerts)
            {
                var throttleKey = $"{machine.Id}:{alert.EventType}";
                var now = DateTime.UtcNow;

                // Não reenvia o mesmo tipo de alerta dentro de 2 minutos para evitar inundar o operador de notificações
                if (_lastAlertSent.TryGetValue(throttleKey, out var lastSent) && (now - lastSent).TotalMinutes < 2)
                {
                    continue;
                }

                _lastAlertSent[throttleKey] = now;

                // Gravar evento no banco de dados
                var machineEvent = new MachineEvent
                {
                    Id = Guid.NewGuid(),
                    MachineId = machine.Id,
                    EventType = alert.EventType,
                    Message = alert.Message,
                    CreatedAt = now
                };

                dbContext.MachineEvents.Add(machineEvent);
                await dbContext.SaveChangesAsync();

                _logger.LogWarning("ALERTA DETECTADO: {EventType} na máquina {MachineName}. Mensagem: {Message}",
                    alert.EventType, machine.Name, alert.Message);

                // Disparar Push Notification para o usuário dono da planta
                if (machine.Plant != null)
                {
                    var pushDto = new SendPushNotificationDto
                    {
                        Title = $"⚠️ Alerta: {machine.Name}",
                        Message = alert.Message,
                        Category = "ALERTA",
                        AvisoId = machineEvent.Id,
                        Origin = "MQTT-Telemetry-Monitor",
                        AdditionalData = new
                        {
                            machineId = machine.Id,
                            machineName = machine.Name,
                            eventType = alert.EventType,
                            plantId = machine.PlantId
                        }
                    };

                    await pushService.SendPushToUserAsync(machine.Plant.UserId, pushDto);
                }
            }
        }

        private void CleanExpiredDeduplicationKeys()
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-10);
            foreach (var kvp in _deduplicationCache)
            {
                if (kvp.Value < cutoff)
                {
                    _deduplicationCache.TryRemove(kvp.Key, out _);
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_client != null && _client.IsConnected)
            {
                await _client.DisconnectAsync();
            }
            await base.StopAsync(cancellationToken);
        }
    }
}
