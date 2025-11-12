using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using cellPhoneS_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Models;

public class Image
{
    public long Id { get; set; }
    [Required]
    public string? OriginUrl { get; set; }
    [Required]
    public string? BlobUrl { get; set; }
    [Required]
    public string? MimeType { get; set; }
    [Required]
    public string? Name { get; set; }
    [Required]
    public string? Alt { get; set; }
    [Required]
    public string Status { get; set; } = "in-use";
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
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
