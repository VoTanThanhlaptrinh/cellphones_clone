using System;
using cellphones_backend.Data;
using cellPhoneS_backend.Models;
using cellPhoneS_backend.Services.Interface;
using cellPhoneS_backend.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace cellPhoneS_backend.Services.Implement;

public class ProductSearchServiceImpl : ProductSearchService
{
private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private const string CACHE_KEY = "PRODUCTS_SEARCH_CACHE";
    private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
    public ProductSearchServiceImpl(ApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }
    public async Task<List<ProductSearchResult>> InitializeAsync()
    {
        // Query DB (Nặng)
        var data = await _context.ProductSearchResults
            .Select(p => new ProductSearchResult
            {
                Id = p.Id,
                Name = p.Name,
                ImageUrl = p.ImageUrl,
                SalePrice = p.SalePrice,
                SearchVector = p.SearchVector
            })
            .AsNoTracking()
            .ToListAsync();

        // Cấu hình Cache
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromHours(1)) // 1 tiếng hết hạn
            .SetPriority(CacheItemPriority.High);

        // Lưu vào RAM
        _cache.Set(CACHE_KEY, data, cacheOptions);

        return data;
    }
    // 2. HÀM GET DATA (Logic bạn muốn)
    // Nhiệm vụ: Kiểm tra Cache -> Có thì trả về -> Không có thì gọi Init
    // ============================================================
    private async Task<List<ProductSearchResult>> GetCachedDataAsync()
    {
        // BƯỚC 1: Kiểm tra nhẹ nhàng xem có Cache chưa
        if (_cache.TryGetValue(CACHE_KEY, out List<ProductSearchResult> cachedList))
        {
            // Có rồi -> Trả về ngay lập tức (Nhanh nhất)
            return cachedList;
        }

        // BƯỚC 2: Nếu chưa có (Cache Miss), phải gọi Init.
        // NHƯNG phải dùng khóa (WaitAsync) để tránh 100 người cùng gọi Init
        await _lock.WaitAsync();
        try
        {
            // Kiểm tra lại lần nữa (Double-check locking)
            // Vì có thể trong lúc chờ khóa, người trước đã nạp xong rồi
            if (_cache.TryGetValue(CACHE_KEY, out cachedList))
            {
                return cachedList;
            }

            // Nếu vẫn chưa có -> Gọi hàm Init của bạn
            return await InitializeAsync();
        }
        finally
        {
            // Mở khóa cho người sau
            _lock.Release();
        }
    }

    // 2. SEARCH LOGIC: Thuật toán C# thay thế cho SQL
    public  async Task<List<ProductSearchResult>> SearchAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return new List<ProductSearchResult>();
        // Lấy data từ RAM (2500 items)
        var allProducts = await GetCachedDataAsync();

        // Xử lý keyword: Bỏ dấu + Lower + Trim
        string cleanKeyword = StringHelper.RemoveSign(keyword).ToLower().Trim();

        // Ngưỡng chấp nhận sai số (cho Levenshtein)
        // Ví dụ: từ khóa dài 5 ký tự, cho phép sai 2 ký tự
        int maxDistance = Math.Max(2, cleanKeyword.Length / 3); 

        var results = allProducts
            .Select(p => new 
            { 
                Product = p, 
                Score = CalculateScore(p.SearchVector, cleanKeyword, maxDistance) 
            })
            .Where(x => x.Score > 0) // Chỉ lấy kết quả có liên quan
            .OrderByDescending(x => x.Score) // Điểm cao xếp trước
            .Take(5) // Top 5
            .Select(x => x.Product)
            .ToList();

        return results;
    }

    // Hàm chấm điểm (Core Algorithm)
    private int CalculateScore(string vector, string keyword, int maxDistance)
    {
        // Ưu tiên 1 (100 điểm): Contains - Chứa chính xác cụm từ
        // Vì vector và keyword đều đã sạch, dùng Contains cực nhanh
        if (vector.Contains(keyword)) 
        {
            return 100;
        }

        // Ưu tiên 2 (50 - Dist điểm): Fuzzy - Gần đúng
        // Tính khoảng cách Levenshtein
        int distance = StringHelper.ComputeLevenshteinDistance(vector, keyword);
        
        if (distance <= maxDistance)
        {
            // Sai càng ít điểm càng cao
            return 50 - distance;
        }

        // Không liên quan
        return 0;
    }
    
    // Hàm Refresh dùng cho Admin khi thêm/sửa sản phẩm
    public void RefreshCache()
    {
        _cache.Remove(CACHE_KEY);
    }
}
