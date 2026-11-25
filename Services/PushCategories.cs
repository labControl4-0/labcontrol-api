namespace LabControlApi.Services;

internal static class PushCategories
{
    public static string? FromEvent(string eventType)
    {
        var category = eventType.Trim().ToLowerInvariant();
        return category is "failure" or "warning" or "maintenance" or "note" ? category : null;
    }
}
