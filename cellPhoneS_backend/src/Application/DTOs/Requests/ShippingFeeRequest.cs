using System;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs.Responses;

namespace cellPhoneS_backend.DTOs.Requests;

public record ShippingFeeRequest(List<CartDetail> CartDetails, string ProvinceName, string DistrictName, string StreetName);
