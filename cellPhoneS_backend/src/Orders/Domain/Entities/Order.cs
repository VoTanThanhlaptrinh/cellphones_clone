using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using cellPhoneS_backend.Constants;
using cellPhoneS_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Models;

public class Order
{
    [Required]
    public long Id { get; set; }
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    public string Status { get; set; } = OrderStatuses.Pending;      // xem OrderStatuses
    [Required]
    public string Type { get; set; } = default!; // loại đơn hàng (pickup/delivery)
    public long FeeId { get; set; }  // tổng giá trị đơn hàng
    [ForeignKey(nameof(FeeId))]
    public Fee Fee { get; set; } = default!; // phí vận chuyển (nếu có)
    [Required]
    public DateTime CreateDate { get; set; }  // ngày tạo
    public DateTime UpdateDate { get; set; }  // ngày cập nhật
    public string CreateBy { get; set; } = default!;
    [Required]
    [ForeignKey(nameof(CreateBy))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public User? CreateUser { get; set; } = default!;
    public string UpdateBy { get; set; } = default!;
    [DeleteBehavior(DeleteBehavior.Restrict)]
    [ForeignKey(nameof(UpdateBy))]
    public User? UpdateUser { get; set; } = default!;

    #region Trạng thái và quy tắc chuyển trạng thái

    [NotMapped] public bool IsPending => Status == OrderStatuses.Pending;
    [NotMapped] public bool IsCancelled => Status == OrderStatuses.Cancelled;

    /// <summary>Đơn đã hoàn tất hoặc đã huỷ thì không huỷ lại được.</summary>
    [NotMapped] public bool CanBeCancelled => Status != OrderStatuses.Completed && Status != OrderStatuses.Cancelled;

    public bool IsOwnedBy(string userId) => CreateBy == userId;

    /// <summary>Tạo đơn mới ở trạng thái chờ thanh toán.</summary>
    public static Order Create(string userId, string type, List<OrderDetail> orderDetails, Fee fee)
    {
        var now = DateTime.UtcNow;
        return new Order
        {
            CreateBy = userId,
            OrderDetails = orderDetails,
            CreateDate = now,
            Type = type,
            Fee = fee,
            Status = OrderStatuses.Pending,
            UpdateBy = userId,
            UpdateDate = now
        };
    }

    /// <summary>Pending → Paid. Idempotent: trạng thái khác thì giữ nguyên. Trả true khi có chuyển.</summary>
    public bool MarkPaid()
    {
        if (!IsPending) return false;
        Status = OrderStatuses.Paid;
        UpdateDate = DateTime.UtcNow;
        return true;
    }

    /// <summary>Chuyển sang Cancelled; gọi sai trạng thái (xem <see cref="CanBeCancelled"/>) sẽ ném <see cref="InvalidOperationException"/>.</summary>
    public void Cancel(string userId)
    {
        if (!CanBeCancelled) throw new InvalidOperationException($"Không thể hủy đơn hàng đang ở trạng thái: {Status}");
        Status = OrderStatuses.Cancelled;
        UpdateDate = DateTime.UtcNow;
        UpdateBy = userId;
    }

    /// <summary>Tiền hàng cộng phí vận chuyển (VND). Cần nạp sẵn <see cref="OrderDetails"/> và <see cref="Fee"/>.</summary>
    public long GetTotalAmount()
    {
        var goods = OrderDetails.Sum(d => d.Price * d.Quantity);
        var shipping = Fee?.FeeDetails.Sum(f => f.Value) ?? 0;
        return (long)Math.Round(goods) + shipping;
    }

    #endregion
}
