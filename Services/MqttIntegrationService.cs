using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LabControlApi.DTOs.MachineMetric;
using LabControlApi.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
// note: MQTTnet v4 has types under MQTTnet.Client; avoid referencing MQTTnet.Client.Options namespace directly

namespace LabControlApi.Services
{
    public class MqttIntegrationService : BackgroundService
    {
        private readonly ILogger<MqttIntegrationService> _logger;
    private readonly IServiceProvider _serviceProvider;
        private IMqttClient? _client;
        private readonly string _host;
        private readonly int _port;
        private readonly string _topic;

        public MqttIntegrationService(ILogger<MqttIntegrationService> logger, IServiceProvider serviceProvider, Microsoft.Extensions.Configuration.IConfiguration config)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _host = config["Mqtt:Host"] ?? "host.docker.internal";
            _port = int.TryParse(config["Mqtt:Port"], out var p) ? p : 1883;
            _topic = config["Mqtt:Topic"] ?? "lab/machine/metrics";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();

            var options = new MQTTnet.Client.MqttClientOptionsBuilder()
                .WithTcpServer(_host, _port)
                .WithClientId("labcontrol-backend")
                .WithCleanSession()
                .Build();

            _client.ApplicationMessageReceivedAsync += async e =>
            {
                try
                {
                    var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload ?? Array.Empty<byte>());
                    _logger.LogInformation("MQTT message received on {Topic}: {Payload}", e.ApplicationMessage.Topic, payload);

                    // Expect payload like: { "machineId": "...", "temperature": 25 }
                    using var doc = JsonDocument.Parse(payload);
                    var root = doc.RootElement;
                    if (!root.TryGetProperty("machineId", out var mid)) return;

                    var machineIdStr = mid.GetString();
                    if (!Guid.TryParse(machineIdStr, out var machineId))
                    {
                        _logger.LogWarning("Invalid machineId in MQTT payload: {MachineId}", machineIdStr);
                        return;
                    }

                    // For each metric field, call AddMetric
                    var timestamp = DateTime.UtcNow;
                    foreach (var prop in root.EnumerateObject())
                    {
                        if (prop.NameEquals("machineId")) continue;
                        var name = prop.Name;
                        if (!prop.Value.TryGetDouble(out var value)) continue;

                        var dto = new CreateMachineMetricDto
                        {
                            MachineId = machineId,
                            Name = name,
                            Value = value,
                            Timestamp = timestamp
                        };

                        // Resolve scoped IMachineMetricService per message
                        try
                        {
                            using var scope = _serviceProvider.CreateScope();
                            var metricService = scope.ServiceProvider.GetRequiredService<LabControlApi.Services.Interfaces.IMachineMetricService>();
                            await metricService.AddMetric(dto, Guid.Empty);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to persist metric from MQTT");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error parsing MQTT message");
                }
            };

            _client.DisconnectedAsync += async e =>
            {
                _logger.LogWarning("Disconnected from MQTT broker");
                await Task.CompletedTask;
            };

            try
            {
                await _client.ConnectAsync(options, stoppingToken);
                // Subscribe after successful connect
                if (_client.IsConnected)
                {
                    await _client.SubscribeAsync(_topic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce, stoppingToken);
                    _logger.LogInformation("Subscribed to topic {Topic}", _topic);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to MQTT broker");
            }

            // Keep running until cancelled
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
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
