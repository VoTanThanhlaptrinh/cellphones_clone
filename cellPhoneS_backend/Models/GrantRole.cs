using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Models;

public class GrantRole
{
    public int Id { get; set; }
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = null!;
    [Required]
    public User User { get; set; } = null!;
    [ForeignKey(nameof(Role))]
    public int RoleId { get; set; }
    [Required]
    public Role? Role { get; set; }
    [Required]
    public string Status { get; set; } = "active";        // trạng thái (active/inactive/deleted...)
    [Required]
    public DateTime CreateDate { get; set; }  // ngày tạo
    public DateTime UpdateDate { get; set; }  // ngày cập nhật
    public string CreateBy { get; set; } = default!;
    [ForeignKey(nameof(CreateBy))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public User CreateUser { get; set; } = default!;

    public string UpdateBy { get; set; } = default!;
    [ForeignKey(nameof(UpdateBy))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public User UpdateUser { get; set; } = default!;
}
