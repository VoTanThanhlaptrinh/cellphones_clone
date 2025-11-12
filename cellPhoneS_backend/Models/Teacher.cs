using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Models;

public class Teacher
{
    public long Id { get; set; }
    // FK tới Info
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
    public string? IdTeacher { get; set; }
    [Required]
    public string? NameInCard { get; set; }
    [Required]
    public string? EmailSchool { get; set; }

    // Lưu file thẻ (varbinary)
    [Required]
    public byte[]? FrontFaceCard { get; set; }
    [Required]
    public byte[]? BehindFaceCard { get; set; }

    public string Status { get; set; } = "active";         // trạng thái (active/inactive/deleted...)
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
