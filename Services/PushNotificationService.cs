using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LabControlApi.DTOs.Push;
using LabControlApi.Repositories.Interfaces;
using LabControlApi.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace LabControlApi.Services
{
    public class PushNotificationService : IPushNotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly IDeviceRepository _deviceRepository;
        private readonly ILogger<PushNotificationService> _logger;
        private const string ExpoPushEndpoint = "https://exp.host/--/api/v2/push/send";

        public PushNotificationService(
            IHttpClientFactory httpClientFactory,
            IDeviceRepository deviceRepository,
            ILogger<PushNotificationService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("expo");
            _deviceRepository = deviceRepository;
            _logger = logger;
        }

        public async Task<bool> SendPushToUserAsync(Guid userId, SendPushNotificationDto notification)
        {
            var devices = await _deviceRepository.GetDevicesByUserIdAsync(userId);
            var deviceTokens = devices.Select(d => d.DeviceToken).Distinct().ToList();

            if (!deviceTokens.Any())
            {
                _logger.LogInformation("Nenhum dispositivo ativo encontrado para o usuário {UserId}.", userId);
                return false;
            }

            _logger.LogInformation("Disparando push para usuário {UserId} em {Count} dispositivo(s). Categoria: {Category}, Origem: {Origin}",
                userId, deviceTokens.Count, notification.Category, notification.Origin);

            return await SendBatchAsync(deviceTokens, notification);
        }

        public async Task<int> SendPushToAllUsersAsync(SendPushNotificationDto notification)
        {
            var devices = await _deviceRepository.GetAllActiveDevicesAsync();
            var deviceTokens = devices.Select(d => d.DeviceToken).Distinct().ToList();

            if (!deviceTokens.Any())
            {
                _logger.LogInformation("Nenhum dispositivo registrado no sistema para envio de broadcast.");
                return 0;
            }

            _logger.LogInformation("Disparando broadcast push para {Count} dispositivos. Categoria: {Category}",
                deviceTokens.Count, notification.Category);

            var success = await SendBatchAsync(deviceTokens, notification);
            return success ? deviceTokens.Count : 0;
        }

        private async Task<bool> SendBatchAsync(List<string> tokens, SendPushNotificationDto notification)
        {
            var payloads = tokens.Select(token => new
            {
                to = token,
                sound = "default",
                title = notification.Title,
                body = notification.Message,
                data = new
                {
                    categoria = notification.Category,
                    avisoId = notification.AvisoId,
                    origem = notification.Origin,
                    extra = notification.AdditionalData
                }
            }).ToList();

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(payloads),
                Encoding.UTF8,
                "application/json"
            );

            const int maxRetries = 2;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var response = await _httpClient.PostAsync(ExpoPushEndpoint, jsonContent);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Expo Push API retornou status HTTP {StatusCode} (Tentativa {Attempt}/{MaxRetries})",
                            response.StatusCode, attempt, maxRetries);

                        if (attempt == maxRetries) return false;
                        await Task.Delay(500 * attempt);
                        continue;
                    }

                    // Processar tickets e verificar se há DeviceNotRegistered para desativar tokens inválidos
                    await ProcessExpoTicketsResponseAsync(tokens, responseBody);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro de rede ao enviar notificação push via Expo (Tentativa {Attempt}/{MaxRetries})",
                        attempt, maxRetries);

                    if (attempt == maxRetries) return false;
                    await Task.Delay(500 * attempt);
                }
            }

            return false;
        }

        private async Task ProcessExpoTicketsResponseAsync(List<string> tokens, string responseBody)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseBody);
                if (!doc.RootElement.TryGetProperty("data", out var dataElement) ||
                    dataElement.ValueKind != JsonValueKind.Array)
                {
                    return;
                }

                var invalidTokens = new List<string>();
                int index = 0;

                foreach (var ticket in dataElement.EnumerateArray())
                {
                    if (index >= tokens.Count) break;
                    var token = tokens[index];

                    if (ticket.TryGetProperty("status", out var status) && status.GetString() == "error")
                    {
                        var error = string.Empty;
                        if (ticket.TryGetProperty("details", out var details) &&
                            details.TryGetProperty("error", out var errCode))
                        {
                            error = errCode.GetString() ?? string.Empty;
                        }

                        _logger.LogWarning("Falha no ticket Expo para token {MaskedToken}: {Error}",
                            MaskToken(token), error);

                        if (error == "DeviceNotRegistered")
                        {
                            invalidTokens.Add(token);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Push entregue ao Expo com sucesso para {MaskedToken}",
                            MaskToken(token));
                    }

                    index++;
                }

                if (invalidTokens.Any())
                {
                    _logger.LogInformation("Desativando {Count} token(s) inválido(s)/desregistrados.", invalidTokens.Count);
                    await _deviceRepository.DeactivateTokensAsync(invalidTokens);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao analisar tickets de resposta da API do Expo.");
            }
        }

        /// <summary>
        /// Mascara o token para garantir que nenhum token apareça em logs públicos (requisito de segurança).
        /// Exemplo: ExponentPushToken[abcdef123456] -> Exponent...3456]
        /// </summary>
        public static string MaskToken(string? token)
        {
            if (string.IsNullOrEmpty(token)) return "[null]";
            if (token.Length <= 10) return "***";
            return string.Concat(token.AsSpan(0, 8), "...", token.AsSpan(token.Length - 5));
        }
    }
}

