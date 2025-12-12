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
    string Versions,
    string? ImageUrl,
    ICollection<ImageDTO> ProductImages,
    ICollection<SpecificationDTO> ProductSpecification,
    ICollection<string> Commitments,
    int Quantity,
    string? Street,
    string? District,
    string? City
);
