using System;
using System.Collections.ObjectModel;

namespace cellPhoneS_backend.DTOs.Responses;

public record StoreView(string? City, List<DistrictView>? Districts);

public record DistrictView(string? District, List<StreetView>? Streets);

public record StreetView(long Id, string? Street);

public record StoreHouseView(long Id, string? City, string? District, string? Street);
