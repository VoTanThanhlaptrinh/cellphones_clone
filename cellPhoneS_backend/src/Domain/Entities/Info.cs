using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Models;

public class Info
{
    public long Id { get; set; }       // Khoá chính
    [Required]
    public string? Type { get; set; }
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
