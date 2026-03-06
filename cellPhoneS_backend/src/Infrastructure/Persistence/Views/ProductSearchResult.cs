using System;

namespace cellPhoneS_backend.Models;

public class ProductSearchResult
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? ImageUrl { get; set; }
    public double SalePrice { get; set; }
    public double BasePrice { get; set; }
    public string? SearchVector { get; set; }
}
