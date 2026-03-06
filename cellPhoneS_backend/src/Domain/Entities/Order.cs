using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using cellPhoneS_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Models;

public class Order
{
    [Required]
    public long Id { get; set; }
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    public string Status { get; set; } = "Pending";      // trạng thái (active/inactive/deleted...)
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
}
