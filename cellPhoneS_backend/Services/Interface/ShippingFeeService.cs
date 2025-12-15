using System;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;

namespace cellPhoneS_backend.Services.Interface;

public interface ShippingFeeService
{
    public Task<ShippingFeeResponse> CalculateShippingFee(ShippingRequest payload);
}
