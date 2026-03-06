using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using cellphones_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace cellPhoneS_backend.Models;

public class ProductImage
{
    public Guid Id { get; set; }
    public long ProductId { get; set; }
    [Required]
    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }

    public long ImageId { get; set; }
    [Required]
    [ForeignKey(nameof(ImageId))]
    public Image? Image { get; set; }
    public string? Status { get; set; } = "active";
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
