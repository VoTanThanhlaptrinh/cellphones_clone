using System.Text.Json.Serialization;
namespace cellPhoneS_backend.DTOs.Responses;

public class GhtkFeeResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("fee")]
        public GhtkFeeDetail? Fee { get; set; }
    }

    public class GhtkFeeDetail
    {
        [JsonPropertyName("fee")]
        public int ShipFee { get; set; } // Đây chính là số tiền ship hiển thị cho user!

        [JsonPropertyName("insurance_fee")]
        public int InsuranceFee { get; set; }

        [JsonPropertyName("delivery")]
        public bool Delivery { get; set; } // Check xem GHTK có hỗ trợ giao tuyến này không
    }
