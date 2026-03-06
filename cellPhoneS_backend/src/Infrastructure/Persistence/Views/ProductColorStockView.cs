using System;
using Microsoft.EntityFrameworkCore;

namespace cellPhoneS_backend.Data.View;

[Keyless]
public class ProductColorStockView
{
    public long ProductId { get; set; }
    public long ColorId { get; set; }
    public int TotalQuantity { get; set; }
}
