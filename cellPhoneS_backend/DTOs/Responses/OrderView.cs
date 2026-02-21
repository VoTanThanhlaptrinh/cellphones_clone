using System;

namespace cellPhoneS_backend.DTOs.Responses;

public class OrderView
{
    public long Id { get; set; }
    public string? Status { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime? UpdateDate { get; set; } 

    public string? CreateBy { get; set; } 
    public string? CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderDetailView> OrderDetails { get; set; } = new List<OrderDetailView>();
}
