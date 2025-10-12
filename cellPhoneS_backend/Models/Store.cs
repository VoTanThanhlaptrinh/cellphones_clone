using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Models;

public class Store
{
    public long Id { get; set; }
    [ForeignKey(nameof(StoreHouse))]
    public long StoreHouseId { get; set; }
    [Required]
    public StoreHouse? StoreHouse { get; set; }
    public long ProductId { get; set; }
    [Required]
    public Product? Product { get; set; }
    [Required][Range(0,int.MaxValue)]
    public int Quantity { get; set; }
    public string? Status { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }

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
