using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace cellphones_backend.Models;

public class CartDetail
{
    public long Id { get; set; }

    // Liên kết giỏ hàng
    [Required]
    public long CartId { get; set; }
    [ForeignKey(nameof(CartId))]
    public Cart? Cart { get; set; }

    // Liên kết sản phẩm
    public long ProductCartDetailId { get; set; }
    [Required]
    [ForeignKey(nameof(ProductCartDetailId))]
    public Product? Product { get; set; }
    public long ColorId {get; set; }
    [Required]
    [ForeignKey(nameof(ColorId))]
    public Color? Color {get; set; }
    [Required]
    [Range(0,int.MaxValue)]
    public int Quantity { get; set; }
    // Audit fields
    public string? Status { get; set; } = "active";
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
