using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Models;

public class Criterion
{
    public long Id { get; set; }
    [ForeignKey(nameof(Category))]
    public long CategoryId { get; set; }
    [Required]
    public Category? Category { get; set; }
    [Required]
    public string? Name { get; set; }

    public ICollection<CriterionDetail> CriterionDetail { get; set; } = new List<CriterionDetail>();

    public string Status { get; set; } = "active";         // trạng thái (active/inactive/deleted...)
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
