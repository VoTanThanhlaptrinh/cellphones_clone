using System;

namespace cellPhoneS_backend.DTOs.Responses;

public class ShippingFeeResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public Fee Fee { get; set; }
}

public class Fee
{
    public string Name { get; set; }
    public int FeeValue { get; set; } // vì "fee" trùng tên class, nên đổi tên field
    public int Insurance_Fee { get; set; }
    public string Delivery_Type { get; set; }
    public int A { get; set; }
    public string Dt { get; set; }
    public List<ExtFee> ExtFees { get; set; }
    public bool Delivery { get; set; }
}

public class ExtFee
{
    public string Display { get; set; }
    public string Title { get; set; }
    public int Amount { get; set; }
    public string Type { get; set; }
}

