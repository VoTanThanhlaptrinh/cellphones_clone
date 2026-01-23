using System;

namespace cellPhoneS_backend.DTOs;

public class ProductIndexModel
{
    public long Id { get; set; }          // Để khi click vào thì biết link tới đâu
    public string? Name { get; set; }     // Để Search và Hiển thị
    public string? ImageUrl { get; set; } // Để Hiển thị ảnh nhỏ (Thumbnail)
    public double BasePrice { get; set; }
    public double SalePrice { get; set; }
}
