using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Models;

public class Commitment
{
    public long Id { get; set; }

    // FK tới bảng Product

    public long ProductCommitmentId { get; set; }
    [Required]
    [ForeignKey(nameof(ProductCommitmentId))]
    public Product? Product { get; set; }
    [Required]
    // Nội dung cam kết (mô tả chi tiết)
    public string? Context { get; set; }
    public string Status { get; set; } = "active";        // trạng thái (active/inactive/deleted...)
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
