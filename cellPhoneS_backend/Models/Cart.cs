using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Models;

public class Cart
{
    public long Id { get; set; }
    // Audit fields
    public string Status { get; set; } = "active";
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
    // Navigation
    public ICollection<CartDetail> CartDetails { get; set; } = new List<CartDetail>();
}
