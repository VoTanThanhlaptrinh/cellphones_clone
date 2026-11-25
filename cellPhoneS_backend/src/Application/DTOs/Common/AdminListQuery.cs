namespace cellPhoneS_backend.DTOs.Responses;

/// <summary>
/// Tham số chung của danh sách admin (nhận bằng <c>[FromQuery]</c>). Filter riêng của từng resource kế thừa class này,
/// vd. <c>AdminOrderFilter : AdminListQuery { string? Type }</c>. Xem docs/ADMIN_API_GUIDE.md mục 3.2.
/// </summary>
public class AdminListQuery
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? SortBy { get; set; }
    public string SortDir { get; set; } = "desc";
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
}
