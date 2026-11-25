using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LabControlApi.Data;
using LabControlApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LabControlApi.Services;

public class ExpoPushWorker(
    IServiceScopeFactory scopes,
    IHttpClientFactory clients,
    ILogger<ExpoPushWorker> logger) : BackgroundService
{
    private const int MaxAttempts = 5;
    private static readonly TimeSpan ReceiptDelay = TimeSpan.FromMinutes(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        do
        {
            try
            {
                using var scope = scopes.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await ProcessBatch(db, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception)
            {
                // Não registrar a exceção: o provider ou o banco pode incluir dados da requisição.
                logger.LogWarning("Falha ao processar a fila de push. A fila permanece no banco.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ProcessBatch(AppDbContext db, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var due = await db.PushDeliveries.AsNoTracking()
            .Where(item => item.Outcome == null && item.NextAttemptAt <= now)
            .OrderBy(item => item.NextAttemptAt).Take(20).ToListAsync(ct);

        if (due.Count == 0) return;
        using var client = clients.CreateClient("expo-push");

        foreach (var item in due)
        {
            // Reserva cada item antes de enviar, inclusive quando há mais de uma instância da API.
            var lease = DateTime.UtcNow.AddMinutes(2);
            var claimed = await db.PushDeliveries
                .Where(row => row.Id == item.Id && row.Outcome == null && row.NextAttemptAt == item.NextAttemptAt)
                .ExecuteUpdateAsync(update => update.SetProperty(row => row.NextAttemptAt, lease), ct);
            if (claimed != 1) continue;

            item.NextAttemptAt = lease;
            db.Attach(item);
            var unavailable = await ProcessDelivery(db, client, item, ct);
            await db.SaveChangesAsync(ct);
            db.Entry(item).State = EntityState.Detached;
            if (item.Outcome != null)
                logger.LogInformation("Envio push {DeliveryId} para evento {EventId}: {Outcome}",
                    item.Id, item.EventId, item.Outcome);
            if (unavailable) break;
        }
    }

    private static async Task<bool> ProcessDelivery(
        AppDbContext db, HttpClient client, PushDelivery item, CancellationToken ct)
    {
        var token = await db.PushDevices.Where(device =>
                device.RegistrationId == item.RegistrationId && device.UserId == item.UserId)
            .Select(device => device.Token).SingleOrDefaultAsync(ct);
        var evt = await db.MachineEvents.AsNoTracking()
            .Where(e => e.Id == item.EventId && e.Machine.Plant.UserId == item.UserId)
            .SingleOrDefaultAsync(ct);
        var category = evt == null ? null : PushCategories.FromEvent(evt.EventType);
        if (token == null || evt == null || category == null)
        {
            item.Outcome = "cancelled";
            return false;
        }

        item.Attempts++;
        try
        {
            var data = new
            {
                categoria = category,
                avisoId = evt.Id,
                machineId = evt.MachineId,
                origem = "LabControl"
            };
            using var response = item.TicketId == null
                ? await client.PostAsJsonAsync("send", new
                {
                    to = token,
                    title = "Novo aviso no LabControl",
                    body = "Abra o app para consultar o evento.",
                    channelId = "machine-alerts",
                    sound = "default",
                    ttl = 3600,
                    data
                }, ct)
                : await client.PostAsJsonAsync("getReceipts", new { ids = new[] { item.TicketId } }, ct);

            if (response.StatusCode == HttpStatusCode.TooManyRequests || (int)response.StatusCode >= 500)
            {
                Retry(item);
                return true;
            }

            if (!response.IsSuccessStatusCode)
            {
                item.Outcome = "provider_rejected";
                return true;
            }

            using var json = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(ct));
            if (!json.RootElement.TryGetProperty("data", out var result))
            {
                item.Outcome = "provider_rejected";
                return true;
            }

            if (item.TicketId is { } receiptId)
            {
                if (!result.TryGetProperty(receiptId, out var receipt))
                {
                    Retry(item, ReceiptDelay);
                    return false;
                }
                result = receipt;
            }

            if (result.GetProperty("status").GetString() == "ok")
            {
                if (item.TicketId != null)
                {
                    item.Outcome = "accepted_by_provider";
                    return false;
                }

                var ticketId = result.GetProperty("id").GetString();
                if (string.IsNullOrWhiteSpace(ticketId))
                {
                    item.Outcome = "invalid_provider_response";
                    return false;
                }

                item.TicketId = ticketId;
                item.Attempts = 0;
                item.NextAttemptAt = DateTime.UtcNow.Add(ReceiptDelay);
                return false;
            }

            var error = result.TryGetProperty("details", out var details)
                && details.TryGetProperty("error", out var code) ? code.GetString() : null;
            if (error == "DeviceNotRegistered")
            {
                await db.PushDevices.Where(device => device.RegistrationId == item.RegistrationId)
                    .ExecuteDeleteAsync(ct);
                item.Outcome = "device_not_registered";
            }
            else if (error == "MessageRateExceeded" && item.TicketId == null)
            {
                Retry(item);
            }
            else
            {
                item.Outcome = "provider_rejected";
            }
            return false;
        }
        catch (Exception ex) when (!ct.IsCancellationRequested &&
            ex is HttpRequestException or OperationCanceledException or IOException)
        {
            Retry(item);
            return true;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or KeyNotFoundException)
        {
            item.Outcome = "invalid_provider_response";
            return false;
        }
    }

    private static void Retry(PushDelivery item, TimeSpan? delay = null)
    {
        if (item.Attempts >= MaxAttempts)
        {
            item.Outcome = item.TicketId == null ? "send_attempts_exhausted" : "receipt_unavailable";
            return;
        }
        item.NextAttemptAt = DateTime.UtcNow.Add(delay ?? TimeSpan.FromSeconds(15 * Math.Pow(2, item.Attempts)));
    }
}
