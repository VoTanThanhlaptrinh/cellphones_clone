using System;

namespace cellPhoneS_backend.DTOs.Responses;

public class OrderDetailView
{
    public int Id { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductImage { get; set; } = string.Empty;
    public long ColorId { get; set; }
    public string ColorName { get; set; } = string.Empty;
    
    public double Price { get; set; }
    public int Quantity { get; set; }
    public double SubTotal => Price * Quantity; 
}
