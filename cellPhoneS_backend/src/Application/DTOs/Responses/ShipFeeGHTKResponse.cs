using System;
using cellphones_backend.Models;

namespace cellPhoneS_backend.DTOs.Responses;

public class ShipFeeGHTKResponse
{
    public long ShippingFee { get; set; }
    public List<Store>? Stores { get; set; }
}
