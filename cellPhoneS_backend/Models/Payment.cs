using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Models;

public class Payment
{
    public long Id { get; set; }
    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = default!; // tên phương thức thanh toán
    [MaxLength(512)]
    public string? ImageURL { get; set; } // ảnh đại diện (logo)
    [Required]
    [MaxLength(64)]
    public string PaymentType { get; set; } = default!; // ví dụ: "cash", "credit_card", "e_wallet"
    public string Status { get; set; } = "active";
    [Required]
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }
    [Required]
    public string CreateBy { get; set; } = default!;
    [ForeignKey(nameof(CreateBy))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public User? CreateUser { get; set; }
    public string UpdateBy { get; set; } = default!;
    [ForeignKey(nameof(UpdateBy))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public User? UpdateUser { get; set; }
}
