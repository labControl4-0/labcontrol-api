using System.Security.Claims;
using System.Text.RegularExpressions;
using LabControlApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabControlApi.Controllers;

[ApiController]
[Authorize]
[Route("api/devices")]
public class DevicesController(AppDbContext db) : ControllerBase
{
    private static readonly Regex ExpoToken = new(
        @"\A(?:Expo|Exponent)PushToken\[[A-Za-z0-9_-]{1,200}\]\z",
        RegexOptions.CultureInvariant);

    public sealed record Registration(string PushToken, string Platform);

    [HttpPost]
    public async Task<IActionResult> Register(Registration input, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        if (!ExpoToken.IsMatch(input.PushToken) || input.Platform is not ("android" or "ios"))
            return BadRequest(new { message = "Token Expo ou plataforma inválida." });

        // Um token identifica uma instalação. Ao trocar de conta, invalida os envios da associação anterior.
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "PushDevices" ("Token", "UserId", "RegistrationId", "Platform")
            VALUES ({input.PushToken}, {userId}, {Guid.NewGuid()}, {input.Platform})
            ON CONFLICT ("Token") DO UPDATE SET
                "RegistrationId" = CASE
                    WHEN "PushDevices"."UserId" = EXCLUDED."UserId" THEN "PushDevices"."RegistrationId"
                    ELSE EXCLUDED."RegistrationId"
                END,
                "UserId" = EXCLUDED."UserId",
                "Platform" = EXCLUDED."Platform"
            """, cancellationToken);
        return NoContent();
    }

    public sealed record Removal(string PushToken);

    [HttpDelete]
    public async Task<IActionResult> Unregister(Removal input, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        if (!ExpoToken.IsMatch(input.PushToken))
            return BadRequest(new { message = "Token Expo inválido." });

        await db.PushDevices.Where(device => device.Token == input.PushToken && device.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
        return NoContent();
    }
}
