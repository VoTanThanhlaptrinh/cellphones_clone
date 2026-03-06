using System;

namespace cellPhoneS_backend.DTOs.Requests;

public record PickupOrderRequest(List<long> CartDetailIds, long StoreHouseId);