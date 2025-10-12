using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Models;

public class User : IdentityUser
{

    [Required]
    public string? Fullname { get; set; } // NOT NULL
    public DateOnly Birth { get; set; }  // NOT NULL
    public string Status { get; set; } = "active";      // trạng thái (active/inactive/deleted...)
    public Cart Cart { get; set; } = default!; // Navigation property for Cart
    public Info Info { get; set; } = default!; // Navigation property for Info
    [Required]
    public DateTime CreateDate { get; set; }  // ngày tạo
    public DateTime UpdateDate { get; set; }  // ngày cập nhật
    [ForeignKey(nameof(User))]
    public string UpdateBy { get; set; } = default!;
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public User UpdateUser { get; set; } = default!;
}