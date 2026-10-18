using cellPhoneS_backend.Services;

namespace cellPhoneS_backend.DTOs.Responses;

/// <summary>Đổi bộ lọc <c>from/to</c> (ngày theo giờ Việt Nam, gồm cả ngày <c>to</c>) sang khoảng UTC <c>[start, end)</c>.</summary>
public static class AdminDateRange
{
    private static readonly TimeZoneInfo VnTz = FindVnTimeZone();

    private static TimeZoneInfo FindVnTimeZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); }
    }

    /// <summary>Thiếu <paramref name="from"/> hoặc <paramref name="to"/> thì đầu tương ứng là null (không giới hạn). <c>to &lt; from</c> → <see cref="BadRequestException"/>.</summary>
    public static (DateTime? Start, DateTime? End) ToUtcRange(DateOnly? from, DateOnly? to)
    {
        if (from.HasValue && to.HasValue && to.Value < from.Value)
            throw new BadRequestException("to phải lớn hơn hoặc bằng from.");
        return (from.HasValue ? ToUtc(from.Value) : null, to.HasValue ? ToUtc(to.Value.AddDays(1)) : null);
    }

    private static DateTime ToUtc(DateOnly day)
        => DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeToUtc(day.ToDateTime(TimeOnly.MinValue), VnTz), DateTimeKind.Utc);
}
