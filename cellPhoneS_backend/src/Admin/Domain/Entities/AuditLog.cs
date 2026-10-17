using System.ComponentModel.DataAnnotations;

namespace cellphones_backend.Models;

/// <summary>Một thao tác ghi (POST/PUT/PATCH/DELETE) dưới /api/admin/*, được <c>AdminAuditFilter</c> ghi tự động.</summary>
public class AuditLog
{
    public long Id { get; set; }
    [Required, MaxLength(450)] public string UserId { get; set; } = default!;
    [MaxLength(256)] public string? UserEmail { get; set; }
    [Required, MaxLength(10)] public string Method { get; set; } = default!;      // POST/PUT/PATCH/DELETE
    [Required, MaxLength(256)] public string Path { get; set; } = default!;       // /api/admin/orders/15/status
    [MaxLength(64)] public string? Resource { get; set; }                         // orders
    [MaxLength(64)] public string? ResourceId { get; set; }                       // 15
    public int StatusCode { get; set; }
    [MaxLength(4000)] public string? Payload { get; set; }                        // body đã cắt, bỏ trường nhạy cảm
    [MaxLength(64)] public string? Ip { get; set; }
    public DateTime CreateDate { get; set; }
}
