using System.Collections.Generic;

namespace cellPhoneS_backend.DTOs.Responses;

public record CategoryResponse(
    long Id, 
    string? Name, 
    string? SlugName, 
    ICollection<DemandResponse>? Demands, 
    ICollection<BrandResponse>? Brands, 
    IEnumerable<ProductView>? Products // Leaving ProductView intact for now if not refactored yet
);

public record CategoryDetailResponse(
    long Id, 
    string? Name, 
    string? SlugName, 
    ICollection<DemandResponse>? Demands, 
    ICollection<BrandResponse>? Brands, 
    IEnumerable<ProductView>? Products, 
    long Total
);
