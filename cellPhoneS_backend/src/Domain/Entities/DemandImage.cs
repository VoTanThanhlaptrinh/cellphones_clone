using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using cellphones_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace cellPhoneS_backend.Models;

public class DemandImage
{
    public long DemandId { get; set; }
    [Required]
    [ForeignKey(nameof(DemandId))]
    public Demand? Demand { get; set; }   // FK tới Demand
    public long ImageId { get; set; }
    [Required]
    [ForeignKey(nameof(ImageId))]
    public Image? Image { get; set; }   // FK tới Image
    public string Status { get; set; } = "active";        // trạng thái (active/inactive/deleted...)
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
