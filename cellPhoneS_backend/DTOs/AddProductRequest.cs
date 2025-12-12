using System;
using System.ComponentModel.DataAnnotations;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs;

namespace cellPhoneS_backend.J2O;

public record AddProductRequest(string? Name, double BasePrice
                                    , double SalePrice, string? Version, ImageDTO? ThumbnailImage
                                    , List<ImageDTO>? ProductImages, List<SpecificationDTO>? Specifications
                                    , long? CategoryId, long? BrandId, List<string>? Commitments)
{
    public string? Status { get; set; } = "active";
}