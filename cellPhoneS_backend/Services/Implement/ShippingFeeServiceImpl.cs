using System;
using System.Text;
using System.Text.Json;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services.Interface;

namespace cellPhoneS_backend.Services.Implement;

public class ShippingFeeServiceImpl : ShippingFeeService
{
    private readonly IConfiguration _configuration;
    public ShippingFeeServiceImpl(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public async Task<ShippingFeeResponse> CalculateShippingFee(ShippingRequest payload)
    {
        using (var client = new HttpClient())
        {
            // 1. Chuẩn bị dữ liệu JSON
            string jsonContent = JsonSerializer.Serialize(payload);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            // 2. Thêm Token vào Header để xác thực
            client.DefaultRequestHeaders.Add("Token", _configuration["Ghtk:Token"]);
            client.DefaultRequestHeaders.Add("X-Client-Source", _configuration["Ghtk:PartnerCode"]);
            // 3. Gửi POST request
            var url = GenerateURL(payload);
            var response = await client.GetAsync(url);
            var resJson = await response.Content.ReadAsStringAsync();
            // 4. Đọc kết quả trả về
            return JsonSerializer.Deserialize<ShippingFeeResponse>(resJson)!;
        }
    }
    private string GenerateURL(ShippingRequest payload)
    {
        if (payload != null && payload.AllFieldsNotNull())
        {
            var baseUrl = _configuration["Ghtk:BaseUrl"];
            var api = _configuration["Ghtk:ShipFeeAPI"];

            var sb = new StringBuilder();
            sb.Append(baseUrl);
            sb.Append(api);
            sb.Append("?");

            sb.Append("pick_province=").Append(Uri.EscapeDataString(payload.pick_province!));
            sb.Append("&pick_district=").Append(Uri.EscapeDataString(payload.pick_district!));
            sb.Append("&pick_ward=").Append(Uri.EscapeDataString(payload.pick_ward!));
            sb.Append("&pick_street=").Append(Uri.EscapeDataString(payload.pick_street!));
            sb.Append("&address=").Append(Uri.EscapeDataString(payload.address!));
            sb.Append("&province=").Append(Uri.EscapeDataString(payload.province!));
            sb.Append("&district=").Append(Uri.EscapeDataString(payload.district!));
            sb.Append("&street=").Append(Uri.EscapeDataString(payload.street!));
            sb.Append("&weight=").Append(payload.weight);
            sb.Append("&value=").Append(payload.value);
            sb.Append("&transport=").Append(Uri.EscapeDataString(payload.transport!));
            return sb.ToString();
        }
        return string.Empty;
    }
}
