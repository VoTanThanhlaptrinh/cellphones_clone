using cellphones_backend.Models;

namespace cellPhoneS_backend.DTOs.Requests;

public record ShippingFeeRequest(List<CartDetail> CartDetails, string ProvinceName, string DistrictName, string StreetName);
