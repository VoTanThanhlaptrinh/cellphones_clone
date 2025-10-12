using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Models;

public class Product
{
    public long Id { get; set; }
    [Required]
    public string? Name { get; set; }
    [Required]
    [Range(0, Double.MaxValue)]
    public double Price { get; set; }
    public ICollection<Color> Colors { get; set; } = new List<Color>();
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
