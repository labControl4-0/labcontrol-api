using System.Text;
using System.Text.Json;
using LabControlApi.Data;
using LabControlApi.DTOs.Chat;
using LabControlApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LabControlApi.Services
{
    public class ChatService : IChatService
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ChatService(AppDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient("gemini");
            _configuration = configuration;
        }

        public async Task<ChatResponseDto> Chat(string message, Guid userId)
        {
            var contextText = await BuildContextAsync(userId);
            var reply = await CallGeminiAsync(message, contextText);
            return new ChatResponseDto { Reply = reply };
        }

        private async Task<string> BuildContextAsync(Guid userId)
        {
            var plants = await _context.Plants
                .Where(p => p.UserId == userId)
                .Include(p => p.Machines)
                    .ThenInclude(m => m.MachineEvents.Where(e => e.ResolvedAt == null))
                .ToListAsync();

            if (!plants.Any())
                return "O usuário não possui plantas cadastradas.";

            var sb = new StringBuilder();
            sb.AppendLine("Dados atuais do sistema LabControl:");
            sb.AppendLine();

            foreach (var plant in plants)
            {
                sb.AppendLine($"Planta: {plant.Name} (ID: {plant.Id})");
                if (!string.IsNullOrEmpty(plant.Description))
                    sb.AppendLine($"  Descrição: {plant.Description}");

                if (!plant.Machines.Any())
                {
                    sb.AppendLine("  Nenhuma máquina cadastrada.");
                }
                else
                {
                    foreach (var machine in plant.Machines)
                    {
                        sb.AppendLine($"  Máquina: {machine.Name} | Modelo: {machine.Model} | Status: {machine.Status}");

                        var openEvents = machine.MachineEvents.ToList();
                        if (openEvents.Any())
                        {
                            foreach (var ev in openEvents)
                                sb.AppendLine($"    [EVENTO ABERTO] Tipo: {ev.EventType} | {ev.Message} (desde {ev.CreatedAt:dd/MM/yyyy HH:mm})");
                        }
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        private async Task<string> CallGeminiAsync(string userMessage, string contextText)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("Gemini API key not configured.");

            var systemPrompt =
                "Você é o assistente do sistema LabControl.\n\n" +
                "REGRAS OBRIGATÓRIAS — siga-as sem exceção:\n" +
                "1. Responda APENAS com base nos dados abaixo. NUNCA invente, suponha ou adicione informações que não estejam nos dados.\n" +
                "2. Se o usuário perguntar sobre algo que não está nos dados, responda exatamente: 'Não encontrei essa informação nos dados do sistema.'\n" +
                "3. Não mencione nenhum lab, máquina ou sensor que não apareça explicitamente nos dados abaixo.\n" +
                "4. Responda sempre em português, de forma clara e direta.\n\n" +
                "=== DADOS REAIS DO SISTEMA ===\n" +
                contextText +
                "=== FIM DOS DADOS ===";

            var requestBody = new
            {
                system_instruction = new
                {
                    parts = new[] { new { text = systemPrompt } }
                },
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = userMessage } }
                    }
                },
                generationConfig = new
                {
                    maxOutputTokens = 1024,
                    temperature = 0.7
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Gemini API error: {response.StatusCode} - {responseBody}");

            using var doc = JsonDocument.Parse(responseBody);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text ?? "Não foi possível obter uma resposta.";
        }
    }
}
