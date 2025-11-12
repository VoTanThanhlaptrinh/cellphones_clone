using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using cellPhoneS_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Models;

public class Product
{
    public long Id { get; set; }
    [Required]
    public string? Name { get; set; }
    [Range(0, Double.MaxValue)]
    public double BasePrice { get; set; }

    [Range(0, Double.MaxValue)]
    public double SalePrice { get; set; }
    public string? Version { get; set; }
    public string? Status { get; set; } = "active";
    public string? ImageUrl { get; set; }
    public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    public ICollection<ProductSpecification> ProductSpecification { get; set; } = new List<ProductSpecification>();
    public ICollection<CategoryProduct> CategoryProducts { get; set; } = new List<CategoryProduct>();
    public ICollection<Commitment> commitments { get; set; } = new List<Commitment>();
    public ICollection<Store> stores { get;  set; } = new List<Store>();
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
