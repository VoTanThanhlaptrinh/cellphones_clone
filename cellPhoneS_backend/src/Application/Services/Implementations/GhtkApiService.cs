using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

/// <summary>
/// Cung cấp các phương thức tích hợp với API Giao Hàng Tiết Kiệm (GHTK).
/// Xử lý các nghiệp vụ liên quan đến vận chuyển như tính phí, tạo đơn hàng.
/// </summary>
public class GhtkApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiToken;
    private readonly string _baseUrl = "https://services.giaohangtietkiem.vn";
    private readonly IConfiguration _configuration;
    public GhtkApiService(IConfiguration configuration)
    {
        _configuration = configuration;
        _apiToken = _configuration["Ghtk:TokenShipFee"]!;
        if (string.IsNullOrEmpty(_apiToken))
        {
            throw new ArgumentException("GHTK API token is not configured.");
        }

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Token", _apiToken);
    }
    public async Task<string> CalculateShippingFeeAsync()
    {
        // 1. Thông tin địa chỉ lấy hàng (Pickup Location)
        string pickProvince = "Khánh Hòa";
        string pickDistrict = "Thành phố Nha Trang";
        string pickWard = "Phường Lộc Thọ";
        string pickAddress = "Số 1 Trần Phú";

        // 2. Thông tin địa chỉ giao hàng (Delivery Location)
        string province = "Hà Nội";
        string district = "Quận Cầu Giấy";
        string ward = "Phường Dịch Vọng Hậu";
        string address = "Số 123 Đường Xuân Thủy";

        // 3. Thông tin kiện hàng (Package Details)
        int weight = 500;
        int value = 1000000;
        string deliverOption = "none";

        // 4. Xây dựng chuỗi tham số truy vấn (URL Encoded Query String)
        var queryParams = new List<string>
        {
            $"pick_province={Uri.EscapeDataString(pickProvince)}",
            $"pick_district={Uri.EscapeDataString(pickDistrict)}",
            $"pick_ward={Uri.EscapeDataString(pickWard)}",
            $"pick_address={Uri.EscapeDataString(pickAddress)}",
            $"province={Uri.EscapeDataString(province)}",
            $"district={Uri.EscapeDataString(district)}",
            $"ward={Uri.EscapeDataString(ward)}",
            $"address={Uri.EscapeDataString(address)}",
            $"weight={weight}",
            $"value={value}",
            $"deliver_option={deliverOption}"
        };

        string queryString = string.Join("&", queryParams);
        string endpoint = $"{_baseUrl}/services/shipment/fee?{queryString}";

        // 5. Thực thi HTTP GET Request
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("Failed to calculate shipping fee from GHTK API.", ex);
        }
    }
}