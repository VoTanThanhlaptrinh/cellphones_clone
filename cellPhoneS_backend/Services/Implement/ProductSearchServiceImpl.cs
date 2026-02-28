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
    public async Task<List<ProductSearchResult>> SearchAsync(string keyword)
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

    public async Task<string> updateAllSlugNamesAsync()
    {
        var results = new List<string>();

        // 1. Update Categories
        var categoryResult = await updateSlugNamesAsync();
        results.Add($"Categories: {categoryResult}");

        // 2. Update Brands
        var brandResult = await updateBrandSlugNamesAsync();
        results.Add($"Brands: {brandResult}");

        // 3. Update Demands
        var demandResult = await updateDemandSlugNamesAsync();
        results.Add($"Demands: {demandResult}");

        // 4. Update Series
        var seriesResult = await updateSeriesSlugNamesAsync();
        results.Add($"Series: {seriesResult}");

        return string.Join(" | ", results);
    }

    public async Task<string> updateSlugNamesAsync()
    {
        int batchSize = 1000; // Xử lý 1000 dòng mỗi lần
        long lastProcessedId = 0; // Đánh dấu ID cuối cùng của batch
        bool hasMoreData = true;

        // Vẫn cần HashSet để theo dõi lỗi trùng Slug xuyên suốt toàn bộ quá trình chạy
        // Một HashSet chứa 100.000 chuỗi ngắn chỉ tốn vài MB RAM, rất an toàn.
        var globalExistingSlugs = new HashSet<string>();

        while (hasMoreData)
        {
            // 1. Lấy dữ liệu theo Batch (Keyset Pagination)
            var categoriesBatch = await _context.Categories
                .Where(c => c.Id > lastProcessedId)
                .OrderBy(c => c.Id)
                .Take(batchSize)
                .ToListAsync();

            if (!categoriesBatch.Any())
            {
                hasMoreData = false;
                break; // Hết dữ liệu
            }

            // 2. Xử lý logic tạo Slug
            foreach (var category in categoriesBatch)
            {
                string baseSlug = SlugHelper.GenerateSlug(category.Name); // Giả sử cột tên là Name
                string finalSlug = baseSlug;
                int counter = 1;

                // Chống trùng lặp
                while (globalExistingSlugs.Contains(finalSlug))
                {
                    finalSlug = $"{baseSlug}-{counter}";
                    counter++;
                }

                globalExistingSlugs.Add(finalSlug);
                category.SlugName = finalSlug;

                // Cập nhật mốc ID cho batch tiếp theo
                lastProcessedId = category.Id;
            }

            // 3. Lưu Batch xuống Database
            await _context.SaveChangesAsync();

            // 4. BẮT BUỘC: Giải phóng bộ nhớ của EF Core Tracking
            _context.ChangeTracker.Clear();
        }
        return "Slug names updated successfully!";
    }

    public async Task<string> updateBrandSlugNamesAsync()
    {
        int batchSize = 1000; // Xử lý 1000 dòng mỗi lần
        long lastProcessedId = 0; // Đánh dấu ID cuối cùng của batch
        bool hasMoreData = true;

        // Vẫn cần HashSet để theo dõi lỗi trùng Slug xuyên suốt toàn bộ quá trình chạy
        // Một HashSet chứa 100.000 chuỗi ngắn chỉ tốn vài MB RAM, rất an toàn.
        var globalExistingSlugs = new HashSet<string>();

        while (hasMoreData)
        {
            // 1. Lấy dữ liệu theo Batch (Keyset Pagination)
            var brandsBatch = await _context.Brands
                .Where(c => c.Id > lastProcessedId)
                .OrderBy(c => c.Id)
                .Take(batchSize)
                .ToListAsync();

            if (!brandsBatch.Any())
            {
                hasMoreData = false;
                break; // Hết dữ liệu
            }

            // 2. Xử lý logic tạo Slug
            foreach (var brand in brandsBatch)
            {
                string baseSlug = SlugHelper.GenerateSlug(brand.Name); // Giả sử cột tên là Name
                string finalSlug = baseSlug;
                int counter = 1;

                // Chống trùng lặp
                while (globalExistingSlugs.Contains(finalSlug))
                {
                    finalSlug = $"{baseSlug}-{counter}";
                    counter++;
                }

                globalExistingSlugs.Add(finalSlug);
                brand.SlugName = finalSlug;

                // Cập nhật mốc ID cho batch tiếp theo
                lastProcessedId = brand.Id;
            }

            // 3. Lưu Batch xuống Database
            await _context.SaveChangesAsync();

            // 4. BẮT BUỘC: Giải phóng bộ nhớ của EF Core Tracking
            _context.ChangeTracker.Clear();
        }
        return "Slug names updated successfully!";
    }

    public async Task<string> updateDemandSlugNamesAsync()
    {
        int batchSize = 1000; // Xử lý 1000 dòng mỗi lần
        long lastProcessedId = 0; // Đánh dấu ID cuối cùng của batch
        bool hasMoreData = true;

        // Vẫn cần HashSet để theo dõi lỗi trùng Slug xuyên suốt toàn bộ quá trình chạy
        // Một HashSet chứa 100.000 chuỗi ngắn chỉ tốn vài MB RAM, rất an toàn.
        var globalExistingSlugs = new HashSet<string>();

        while (hasMoreData)
        {
            // 1. Lấy dữ liệu theo Batch (Keyset Pagination)
            var demandsBatch = await _context.Demands
                .Where(c => c.Id > lastProcessedId)
                .OrderBy(c => c.Id)
                .Take(batchSize)
                .ToListAsync();

            if (!demandsBatch.Any())
            {
                hasMoreData = false;
                break; // Hết dữ liệu
            }

            // 2. Xử lý logic tạo Slug
            foreach (var demand in demandsBatch)
            {
                string baseSlug = SlugHelper.GenerateSlug(demand.Name); // Giả sử cột tên là Name
                string finalSlug = baseSlug;
                int counter = 1;

                // Chống trùng lặp
                while (globalExistingSlugs.Contains(finalSlug))
                {
                    finalSlug = $"{baseSlug}-{counter}";
                    counter++;
                }

                globalExistingSlugs.Add(finalSlug);
                demand.SlugName = finalSlug;

                // Cập nhật mốc ID cho batch tiếp theo
                lastProcessedId = demand.Id;
            }

            // 3. Lưu Batch xuống Database
            await _context.SaveChangesAsync();

            // 4. BẮT BUỘC: Giải phóng bộ nhớ của EF Core Tracking
            _context.ChangeTracker.Clear();
        }
        return "Slug names updated successfully!";
    }

    public async Task<string> updateSeriesSlugNamesAsync()
    {
        int batchSize = 1000; // Xử lý 1000 dòng mỗi lần
        long lastProcessedId = 0; // Đánh dấu ID cuối cùng của batch
        bool hasMoreData = true;

        // Vẫn cần HashSet để theo dõi lỗi trùng Slug xuyên suốt toàn bộ quá trình chạy
        // Một HashSet chứa 100.000 chuỗi ngắn chỉ tốn vài MB RAM, rất an toàn.
        var globalExistingSlugs = new HashSet<string>();

        while (hasMoreData)
        {
            // 1. Lấy dữ liệu theo Batch (Keyset Pagination)
            var seriesBatch = await _context.Series
                .Where(c => c.Id > lastProcessedId)
                .OrderBy(c => c.Id)
                .Take(batchSize)
                .ToListAsync();

            if (!seriesBatch.Any())
            {
                hasMoreData = false;
                break; // Hết dữ liệu
            }

            // 2. Xử lý logic tạo Slug
            foreach (var series in seriesBatch)
            {
                string baseSlug = SlugHelper.GenerateSlug(series.Name); // Giả sử cột tên là Name
                string finalSlug = baseSlug;
                int counter = 1;

                // Chống trùng lặp
                while (globalExistingSlugs.Contains(finalSlug))
                {
                    finalSlug = $"{baseSlug}-{counter}";
                    counter++;
                }

                globalExistingSlugs.Add(finalSlug);
                series.SlugName = finalSlug;

                // Cập nhật mốc ID cho batch tiếp theo
                lastProcessedId = series.Id;
            }

            // 3. Lưu Batch xuống Database
            await _context.SaveChangesAsync();

            // 4. BẮT BUỘC: Giải phóng bộ nhớ của EF Core Tracking
            _context.ChangeTracker.Clear();
        }
        return "Slug names updated successfully!";
    }
}
