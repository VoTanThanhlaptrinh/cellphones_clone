using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Models;

public class Student
{
    public long Id { get; set; }
    public long InfoId { get; set; }
    [Required]
    [ForeignKey(nameof(InfoId))]
    public Info? Info { get; set; }
    // Theo validator
    [Required]
    public string? TypeSchool { get; set; }
    [Required]
    public string? NameSchool { get; set; }
    [Required]
    public string? IdStudent { get; set; }
    [Required]
    public string? NameInCard { get; set; }
    public DateOnly ExpiredDateCard { get; set; }
    [Required]
    public string? EmailSchool { get; set; }

    // Lưu file thẻ (varbinary). DTO có thể là IFormFile, entity lưu byte[]
    [Required]
    public byte[]? FrontFaceCard { get; set; }
    public byte[]? BehindFaceCard { get; set; }
    [Required]
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
