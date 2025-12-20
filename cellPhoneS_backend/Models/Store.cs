using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Models;

public class Store
{
    public long StoreHouseId { get; set; }
    [Required]
    [ForeignKey(nameof(StoreHouseId))]
    public StoreHouse? StoreHouse { get; set; }
    public long ProductId { get; set; }
    [Required]
    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; } // FK tới Product
    public long ColorId { get; set; }
    [Required]
    [ForeignKey(nameof(ColorId))]
    public Color? Color { get; set; }     // FK tới Image
    [Required][Range(0,int.MaxValue)]
    public int Quantity { get; set; }
    public string? Status { get; set; } = "active";
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }
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
