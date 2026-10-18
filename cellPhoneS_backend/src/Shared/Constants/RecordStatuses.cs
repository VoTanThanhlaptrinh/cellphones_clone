namespace cellPhoneS_backend.Constants;

/// <summary>Trạng thái dùng chung của sản phẩm, danh mục, thương hiệu, dòng, nhu cầu, kho (xoá = xoá mềm sang <see cref="Deleted"/>).</summary>
public static class RecordStatuses
{
    public const string Active = "active";
    public const string Inactive = "inactive";
    public const string Deleted = "deleted";

    public static readonly IReadOnlyList<string> All = new[] { Active, Inactive, Deleted };

    /// <summary>Chuẩn hoá tham số lọc <c>status</c> của list admin: null/trống = null (mặc định ẩn deleted), sai giá trị -> 400.</summary>
    public static string? ParseFilter(string? status)
    {
        if (string.IsNullOrWhiteSpace(status)) return null;
        var value = status.Trim().ToLowerInvariant();
        if (!All.Contains(value))
            throw new cellPhoneS_backend.Services.BadRequestException($"status không hợp lệ. Giá trị cho phép: {string.Join(", ", All)}.");
        return value;
    }
}
