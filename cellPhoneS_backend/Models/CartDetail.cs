using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Models;

public class CartDetail
{
    public long Id { get; set; }

    // Liên kết giỏ hàng
    [Required]
    public long CartId { get; set; }
    public Cart? Cart { get; set; }

    // Liên kết sản phẩm
    [ForeignKey(nameof(Product))]
    public long ProductCartDetailId { get; set; }
    [Required]
    public Product? Product { get; set; }

    // Số lượng
    [Required][Range(0,int.MaxValue)]
    public int Quantity { get; set; }

    // Audit fields
    public string? Status { get; set; } = "pending";
    [Required]
    public DateTime CreateDate { get; set; }  // ngày tạo
    public DateTime UpdateDate { get; set; }  // ngày cập nhật
    [ForeignKey(nameof(User))]
    public string CreateBy { get; set; } = default!;
    [Required]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public User CreateUser { get; set; } = default!;
    [ForeignKey(nameof(User))]
    public string UpdateBy { get; set; } = default!;
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public User UpdateUser { get; set; } = default!;
}
