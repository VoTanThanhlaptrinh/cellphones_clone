using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using cellPhoneS_backend.Constants;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Models;

/// <summary>
/// Một giao dịch thanh toán thành công của đơn hàng, ghi lại khi webhook của cổng thanh toán xác nhận.
/// (Bảng `Payment` là danh mục phương thức thanh toán, không phải giao dịch.)
/// </summary>
public class PaymentTransaction
{
    public long Id { get; set; }

    public long OrderId { get; set; }
    [ForeignKey(nameof(OrderId))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Order? Order { get; set; }

    /// <summary>Số tiền đã thanh toán (VND).</summary>
    public long Amount { get; set; }

    [Required]
    [MaxLength(32)]
    public string Provider { get; set; } = "PayOS";

    /// <summary>Mã tham chiếu giao dịch phía cổng thanh toán.</summary>
    [MaxLength(128)]
    public string? Reference { get; set; }

    [MaxLength(128)]
    public string? PaymentLinkId { get; set; }

    [MaxLength(512)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(32)]
    public string Status { get; set; } = PaymentTransactionStatuses.Success;

    /// <summary>Thời điểm giao dịch theo cổng thanh toán (UTC); null nếu không đọc được.</summary>
    public DateTime? TransactionDate { get; set; }

    [Required]
    public DateTime CreateDate { get; set; }
}
