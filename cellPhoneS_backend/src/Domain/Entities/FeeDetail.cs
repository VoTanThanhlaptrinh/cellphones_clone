using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cellPhoneS_backend.Models;

public class FeeDetail
{
    [Required]
    [Key]
    public long Id { get; set; }
    [Required]
    public string Name { get; set; } = default!;
    [Required]
    public long Value { get; set; }
    public long FeeId { get; set; }
    [ForeignKey(nameof(FeeId))]
    public Fee Fee { get; set; } = default!;
    public string Status { get; set; } = "active";        // trạng thái (active/inactive/deleted...)
    [Required]
    public DateTime CreateDate { get; set; }  // ngày tạo
    [Required]
    public DateTime UpdateDate { get; set; }  // ngày cập nhật
}
