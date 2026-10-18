namespace cellPhoneS_backend.Constants;

/// <summary>Trạng thái hiển thị của đánh giá (<c>Review.Status</c>). Chỉ <see cref="Visible"/> hiện ở storefront và được tính điểm trung bình.</summary>
public static class ReviewStatuses
{
    public const string Visible = "visible";
    public const string Hidden = "hidden";

    public static bool IsValid(string? status) => status is Visible or Hidden;
}
