using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Models;

public class ProductSpecification
{
    public long ProductId { get; set; }
    [Required]
    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; } // FK tới Product
    public long SpecificationId { get; set; }
    [Required]
    [ForeignKey(nameof(SpecificationId))]
    public Specification? Specification { get; set; } // FK tới SpecificationDetail
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
