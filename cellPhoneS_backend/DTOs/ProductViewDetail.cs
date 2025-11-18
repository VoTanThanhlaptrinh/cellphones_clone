using System;
using System.Drawing;
using cellphones_backend.Models;
using cellPhoneS_backend.Models;

namespace cellPhoneS_backend.DTOs;

public record ProductViewDetail(
    long Id,
    string? Name,
    double BasePrice,
    double SalePrice,
    string? Version,
    string? ImageUrl,
    ICollection<ProductImage> ProductImages,
    ICollection<ProductSpecification> ProductSpecification,
    ICollection<CategoryProduct> categoryProducts,
    ICollection<Commitment> Commitments,
    int Quantity,
    string? Street,
    string? District,
    string? City
);
