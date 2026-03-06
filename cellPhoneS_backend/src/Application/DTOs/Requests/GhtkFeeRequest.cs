using System.Text.Json.Serialization;
namespace cellPhoneS_backend.DTOs.Requests;

public class GhtkFeeRequest
{
    // --- Thông tin kho gửi (Cửa hàng của bạn) ---
    [JsonPropertyName("pick_province")]
    public string PickProvince { get; set; } = null!;

    [JsonPropertyName("pick_district")]
    public string PickDistrict { get; set; } = null!;

    [JsonPropertyName("pick_ward")]
    public string? PickWard { get; set; }

    // --- Thông tin người nhận (Khách hàng) ---
    [JsonPropertyName("province")]
    public string Province { get; set; } = null!;

    [JsonPropertyName("district")]
    public string District { get; set; } = null!;

    [JsonPropertyName("ward")]
    public string? Ward { get; set; }

    // --- Thông tin gói hàng ---
    [JsonPropertyName("weight")]
    public int Weight { get; set; } // Bắt buộc: Cân nặng (Gram)

    [JsonPropertyName("value")]
    public int? Value { get; set; } // Giá trị đơn hàng (VNĐ) - GHTK dùng cái này để tính phí bảo hiểm

    [JsonPropertyName("transport")]
    public string? Transport { get; set; } // "road" (đường bộ) hoặc "fly" (đường bay)
}

// Record phụ để truyền data vào hàm tính phí
public record PackageToShip(string PickProvince, string PickDistrict, int TotalQuantity);

