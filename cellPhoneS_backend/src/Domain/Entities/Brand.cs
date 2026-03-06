using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Models;

public class Brand
{
    public long Id { get; set; }
    public long CategoryId { get; set; }
    [Required]
    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; set; }
    [Required]
    public string? Name { get; set; }
    [Required]
    [MaxLength(255)]
    public string SlugName { get; set; }
    public long? ImageId { get; set; }
    [ForeignKey(nameof(ImageId))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public Image? Image { get; set; }
    public ICollection<Series> Series { get; set; } = new List<Series>();
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
