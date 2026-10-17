using System.Text.Json;
using cellphones_backend.Repositories;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services.Interface;
namespace cellPhoneS_backend.Services.Implement;

public class ShippingFeeServiceImpl : ShippingFeeService
{
    private string GHTK_API_URL;
    private string GHTK_TOKEN;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IStoreRepository _storeRepository;
    public ShippingFeeServiceImpl(HttpClient httpClient, IConfiguration configuration,
        IStoreRepository storeRepository)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _storeRepository = storeRepository;

        GHTK_API_URL = _configuration["Ghtk:BaseUrl"]!;
        GHTK_TOKEN = _configuration["Ghtk:TokenShipFee"]!;
    }

    public async Task<long> CalculateAccurateShippingFee(List<PackageToShip> packages, string destProvince, string destDistrict)
    {
        long totalFee = 0;

        foreach (var package in packages)
        {
            var ghtkPayload = new GhtkFeeRequest
            {
                PickProvince = package.PickProvince,
                PickDistrict = package.PickDistrict,
                Province = destProvince,
                District = destDistrict,
                Weight = package.TotalQuantity * 500
            };

            var response = await CallGHTKApi(ghtkPayload); // Hàm CallGHTKApi giữ nguyên như cũ của bạn
            totalFee += response?.Fee?.ShipFee ?? 0;
        }

        return totalFee;
    }
    private async Task<GhtkFeeResponse> CallGHTKApi(GhtkFeeRequest payload)
    {
        try
        {
            // API Tính phí của GHTK sử dụng method GET, nên ta phải build query parameters
            var queryParams = new List<string>
            {
                $"pick_province={Uri.EscapeDataString(payload.PickProvince)}",
                $"pick_district={Uri.EscapeDataString(payload.PickDistrict)}",
                $"province={Uri.EscapeDataString(payload.Province)}",
                $"district={Uri.EscapeDataString(payload.District)}",
                $"weight={payload.Weight}"
            };

            var requestUrl = $"{GHTK_API_URL}?{string.Join("&", queryParams)}";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            // Đính kèm Token bảo mật vào Header
            request.Headers.Add("Token", GHTK_TOKEN);

            // Bắn request
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<GhtkFeeResponse>(jsonString);
            }

            // Nếu GHTK trả về lỗi (400, 500...), bạn có thể quăng Exception hoặc return null
            return null;
        }
        catch (Exception ex)
        {
            // Log lỗi ra console/file để sau này debug
            Console.WriteLine($"Lỗi khi gọi API GHTK: {ex.Message}");
            return null;
        }
    }
}