namespace cellPhoneS_backend.Utils;

/// <summary>Đổi ngày theo giờ Việt Nam (Asia/Ho_Chi_Minh, UTC+7, không có DST) sang UTC để so với cột <c>CreateDate</c> (UTC).</summary>
public static class VietnamTime
{
    public const string PostgresZone = "Asia/Ho_Chi_Minh";
    public static readonly TimeZoneInfo Zone = FindZone();

    private static TimeZoneInfo FindZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById(PostgresZone); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); }
    }

    /// <summary>00:00 giờ Việt Nam của <paramref name="day"/> dưới dạng UTC.</summary>
    public static DateTime ToUtc(DateOnly day)
        => DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeToUtc(day.ToDateTime(TimeOnly.MinValue), Zone), DateTimeKind.Utc);

    /// <summary>Ngày hôm nay theo giờ Việt Nam.</summary>
    public static DateOnly Today() => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Zone));
}
