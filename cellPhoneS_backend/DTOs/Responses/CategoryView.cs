using System;
using cellphones_backend.Models;

namespace cellPhoneS_backend.DTOs.Responses;

public record CategoryView(long Id, string? Name, ICollection<DemandView>? Demands, ICollection<BrandView>? Brands, IEnumerable<ProductView>? Products);
public record CategoryDetailView(long Id, string? Name, ICollection<DemandView>? Demands, ICollection<BrandView>? Brands, IEnumerable<ProductView>? Products,long total);

