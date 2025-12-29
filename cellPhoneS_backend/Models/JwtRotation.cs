using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using cellphones_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace cellPhoneS_backend.Models;

public class JwtRotation
{
    [Key]
    public long Id { get; set; }
    public string? UserId { get; set; }
    [Required]
    [ForeignKey(nameof(UserId))]
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public User? UserRef { get; set; }
    public string? SessionId { get; set; }
    public string? TokenHash { get; set; }
    public DateTime? ExprireAt { get; set; }
    public DateTime? RevokeAt { get; set; } = null;
    public string? ReplaceByTokenHash { get; set; } = null;

}
