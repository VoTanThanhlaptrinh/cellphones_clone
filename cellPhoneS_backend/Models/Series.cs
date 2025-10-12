using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Models;

public class Series
{
    public long Id { get; set; }
    [ForeignKey(nameof(Brand))]
    public long BrandId { get; set; }
    [Required]
    public Brand? Brand { get; set; }
    [Required]
    public string? Name { get; set; }
    public string? Status { get; set; }         // trạng thái (active/inactive/deleted...)
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
