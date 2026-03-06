using System;

namespace cellPhoneS_backend.DTOs.Requests;

public record DeliveryOrderRequest(
    List<long> CartDetailIds, 
    string ProvinceName, 
    string DistrictName,  
    string Street
);
