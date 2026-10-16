using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using cellPhoneS_backend.Constants;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Models;

/// <summary>
/// Một lần thử thanh toán online của đơn hàng qua payment gateway. Dòng được lưu TRƯỚC khi gọi gateway
/// để mọi lần thử lại sau lỗi mạng dùng đúng <see cref="IdempotencyKey"/> và không sinh payment thứ hai.
/// </summary>
[Index(nameof(IdempotencyKey), IsUnique = true)]
[Index(nameof(OrderId))]
public class OnlinePayment
{
    public long Id { get; set; }

    public long OrderId { get; set; }
    [ForeignKey(nameof(OrderId))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Order? Order { get; set; }

    /// <summary>PAYOS hoặc VNPAY.</summary>
    [Required]
    [MaxLength(16)]
    public string Provider { get; set; } = default!;

    [Required]
    [MaxLength(128)]
    public string IdempotencyKey { get; set; } = default!;

    /// <summary>Lần thử thứ mấy của đơn (bắt đầu từ 1).</summary>
    public int Attempt { get; set; }

    /// <summary>Số tiền (VND) của lần thanh toán này.</summary>
    public long Amount { get; set; }

    /// <summary>Id payment ở gateway; null cho tới khi gateway trả lời.</summary>
    public long? GatewayPaymentId { get; set; }

    /// <summary>Creating, rồi trạng thái của gateway: PENDING, PAID, FAILED, EXPIRED, CANCELLED, REVIEW_REQUIRED.</summary>
    [Required]
    [MaxLength(32)]
    public string Status { get; set; } = OnlinePaymentStatuses.Creating;

    [MaxLength(1024)]
    public string? CheckoutUrl { get; set; }

    public DateTime? ExpiresAt { get; set; }

    [Required]
    public DateTime CreateDate { get; set; }

    public DateTime UpdateDate { get; set; }

    #region Trạng thái và quy tắc chuyển trạng thái

    /// <summary>Chưa kết thúc: còn phải hỏi gateway.</summary>
    [NotMapped] public bool IsOpen => OnlinePaymentStatuses.Open.Contains(Status);

    /// <summary>Khách còn thanh toán được: gateway báo PENDING và đã có link.</summary>
    [NotMapped] public bool CanPay => Status == OnlinePaymentStatuses.Pending && CheckoutUrl != null;

    /// <summary>
    /// Lần thử mới ở trạng thái "Creating". Key phải được lưu vào DB trước khi gọi gateway
    /// để mọi lần thử lại dùng đúng key này.
    /// </summary>
    public static OnlinePayment Start(long orderId, string provider, int attempt, long amount)
    {
        var now = DateTime.UtcNow;
        return new OnlinePayment
        {
            OrderId = orderId,
            Provider = provider,
            Attempt = attempt,
            Amount = amount,
            Status = OnlinePaymentStatuses.Creating,
            IdempotencyKey = $"cellphones-order-{orderId}-attempt-{attempt}",
            CreateDate = now,
            UpdateDate = now
        };
    }

    /// <summary>Ghi kết quả gateway trả về.</summary>
    public void Apply(long gatewayPaymentId, string provider, string status, string? checkoutUrl, DateTime? expiresAtUtc)
    {
        GatewayPaymentId = gatewayPaymentId;
        Provider = provider;
        Status = status;
        CheckoutUrl = checkoutUrl;
        ExpiresAt = expiresAtUtc;
        UpdateDate = DateTime.UtcNow;
    }

    /// <summary>Gateway từ chối dứt khoát (4xx): lần thử sau phải dùng key mới.</summary>
    public void Reject() => SetStatus(OnlinePaymentStatuses.Rejected);

    public void Cancel() => SetStatus(OnlinePaymentStatuses.Cancelled);

    private void SetStatus(string status)
    {
        Status = status;
        UpdateDate = DateTime.UtcNow;
    }

    #endregion
}
