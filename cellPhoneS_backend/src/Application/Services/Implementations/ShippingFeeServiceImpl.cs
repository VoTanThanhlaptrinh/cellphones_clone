using System;
using System.Text.Json;
using System.Net.Http;
using System.Linq;
using cellphones_backend.Models;
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
    private readonly ICartDetailRepository _cartDetailRepository;

    public ShippingFeeServiceImpl(HttpClient httpClient, IConfiguration configuration,
        IStoreRepository storeRepository, ICartDetailRepository cartDetailRepository)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _storeRepository = storeRepository;
        _cartDetailRepository = cartDetailRepository;

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
    private async Task<long> CalculateTotalFee(ShippingFeeRequest payload)
    {
        // 1. Gán cân nặng mặc định (Ví dụ 500 gram/sản phẩm) tránh lỗi 0 gram
        int defaultWeight = 500;

        // 2. Tính TỔNG cân nặng của toàn bộ giỏ hàng
        int totalWeight = payload.CartDetails.Sum(item => item.Quantity * defaultWeight);

        // 3. Tìm "Kho Vàng": Tạm thời lấy kho chứa sản phẩm ĐẦU TIÊN làm điểm gửi cho cả đơn hàng
        var firstItem = payload.CartDetails.First();
        var store = await _storeRepository.FindByProductAndColorAsync(firstItem.ProductCartDetailId, firstItem.ColorId);

        if (store == null)
        {
            throw new ArgumentException("Không tìm thấy cửa hàng nào chứa sản phẩm này.");
        }

        // 4. Build Payload dạng phẳng (khớp với DTO chúng ta đã tạo)
        var ghtkPayload = new GhtkFeeRequest
        {
            PickProvince = store.StoreHouse!.City!,
            PickDistrict = store.StoreHouse!.District!,
            Province = payload.ProvinceName,
            District = payload.DistrictName,
            Weight = totalWeight
        };

        // 5. Gọi API ĐÚNG 1 LẦN duy nhất
        var response = await CallGHTKApi(ghtkPayload);

        // 6. Trả về tiền Ship, nếu lỗi trả về 0 hoặc có thể set 1 mức giá fix (vd: 30000)
        return response?.Fee?.ShipFee ?? 0;
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