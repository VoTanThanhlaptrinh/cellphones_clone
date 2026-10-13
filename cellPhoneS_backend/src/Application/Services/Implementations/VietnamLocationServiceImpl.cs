using System.Text.Json;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services.Interface;
using Microsoft.Extensions.Caching.Memory;

namespace cellPhoneS_backend.Services.Implement;

public class VietnamLocationServiceImpl : IVietnamLocationService
{
    private const string CacheKeyPrefix = "VN_PROVINCES_DEPTH_";
    private readonly IMemoryCache _memoryCache;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<VietnamLocationServiceImpl> _logger;
    private readonly int _defaultDepth;
    private readonly int _minDepth;
    private readonly int _maxDepth;
    private readonly TimeSpan _cacheDuration;
    private readonly TimeSpan _staleRetentionDuration;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private sealed record VietnamLocationCacheEntry(List<VietnamProvinceDto> Data, DateTimeOffset CachedAt);

    public VietnamLocationServiceImpl(
        IMemoryCache memoryCache,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<VietnamLocationServiceImpl> logger)
    {
        _memoryCache = memoryCache;
        _httpClientFactory = httpClientFactory;
        _logger = logger;

        _defaultDepth = GetConfigInt(configuration, "VietnamAdministrativeApi:DefaultDepth", 3);
        _minDepth = GetConfigInt(configuration, "VietnamAdministrativeApi:MinDepth", 1);
        _maxDepth = GetConfigInt(configuration, "VietnamAdministrativeApi:MaxDepth", 3);

        var cacheHours = GetConfigInt(configuration, "VietnamAdministrativeApi:CacheHours", 24);
        if (cacheHours < 1)
        {
            cacheHours = 24;
        }

        _cacheDuration = TimeSpan.FromHours(cacheHours);
        _staleRetentionDuration = TimeSpan.FromHours(cacheHours * 2);
    }

    public async Task<List<VietnamProvinceDto>> GetLocationsAsync(int depth, CancellationToken ct = default)
    {
        if (depth < _minDepth || depth > _maxDepth)
        {
            throw new BadRequestException($"Invalid depth. Supported range is {_minDepth} to {_maxDepth}.");
        }

        var cacheKey = BuildCacheKey(depth);
        if (_memoryCache.TryGetValue(cacheKey, out VietnamLocationCacheEntry? cached) && cached is not null)
        {
            var age = DateTimeOffset.UtcNow - cached.CachedAt;
            if (age <= _cacheDuration)
            {
                return cached.Data;
            }

            var refreshedData = await TryFetchAndCacheAsync(depth, cacheKey, ct);
            if (refreshedData is not null)
            {
                return refreshedData;
            }

            _logger.LogWarning("Using stale Vietnam location cache for depth {Depth}.", depth);
            return cached.Data;
        }

        var data = await TryFetchAndCacheAsync(depth, cacheKey, ct);
        if (data is null)
        {
            throw new InternalServerException("Failed to fetch Vietnam locations from upstream service.");
        }

        return data;
    }

    public async Task WarmUpAsync(CancellationToken ct = default)
    {
        try
        {
            await GetLocationsAsync(_defaultDepth, ct);
        }
        catch (AppException ex)
        {
            _logger.LogWarning("Vietnam location warm-up failed for depth {Depth}: {Message}", _defaultDepth, ex.Message);
        }
    }

    private async Task<List<VietnamProvinceDto>?> TryFetchAndCacheAsync(int depth, string cacheKey, CancellationToken ct)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("VietnamAdministrativeApi");
            using var response = await client.GetAsync($"?depth={depth}", ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Vietnam administrative API returned non-success status code {StatusCode} for depth {Depth}.",
                    (int)response.StatusCode,
                    depth);
                return null;
            }

            await using var responseStream = await response.Content.ReadAsStreamAsync(ct);
            var data = await JsonSerializer.DeserializeAsync<List<VietnamProvinceDto>>(responseStream, _jsonOptions, ct);
            if (data is null || data.Count == 0)
            {
                _logger.LogWarning("Vietnam administrative API returned empty data for depth {Depth}.", depth);
                return null;
            }

            var cacheEntry = new VietnamLocationCacheEntry(data, DateTimeOffset.UtcNow);
            _memoryCache.Set(
                cacheKey,
                cacheEntry,
                new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(_staleRetentionDuration)
                    .SetPriority(CacheItemPriority.High));

            return data;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Vietnam administrative data for depth {Depth}.", depth);
            return null;
        }
    }

    private string BuildCacheKey(int depth) => $"{CacheKeyPrefix}{depth}";

    private static int GetConfigInt(IConfiguration configuration, string key, int fallbackValue)
    {
        return int.TryParse(configuration[key], out var value) ? value : fallbackValue;
    }
}
