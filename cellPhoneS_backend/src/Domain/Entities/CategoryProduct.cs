using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using cellphones_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace cellPhoneS_backend.Models;

public class CategoryProduct
{
    public long CategoryId { get; set; }
    [Required]
    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; set; } // FK tới Category
    public long ProductId { get; set; }
    [Required]
    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; } // FK tới Product
    public string Status { get; set; } = "active";      // trạng thái (active/inactive/deleted...)
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
