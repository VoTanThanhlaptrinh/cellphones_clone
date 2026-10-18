namespace cellPhoneS_backend.DTOs.Responses;

/// <summary>
/// Danh mục trong trang quản trị. Số con chỉ đếm bản ghi chưa xoá; <c>ProductCount</c> đếm sản phẩm <c>active</c> gắn trực tiếp.
/// </summary>
public record AdminCategoryView(
    long Id,
    string? Name,
    string SlugName,
    long? ParentCategoryId,
    string? ParentCategoryName,
    string? Status,
    int ChildCount,
    int BrandCount,
    int DemandCount,
    int ProductCount,
    DateTime CreateDate,
    DateTime UpdateDate);

/// <summary>
/// Thương hiệu trong trang quản trị. Sản phẩm không gắn trực tiếp với thương hiệu nên <c>ProductCount</c> là số sản phẩm
/// <c>active</c> của danh mục chứa thương hiệu (cách storefront lọc theo thương hiệu).
/// </summary>
public record AdminBrandView(
    long Id,
    string? Name,
    string SlugName,
    long CategoryId,
    string? CategoryName,
    long? ImageId,
    string? ImageUrl,
    string Status,
    int SeriesCount,
    int ProductCount,
    DateTime CreateDate,
    DateTime UpdateDate);

/// <summary><c>ProductCount</c> như <see cref="AdminBrandView"/>: sản phẩm active của danh mục chứa thương hiệu.</summary>
public record AdminSeriesView(
    long Id,
    string? Name,
    string SlugName,
    long BrandId,
    string? BrandName,
    long CategoryId,
    string? CategoryName,
    string? Status,
    int ProductCount,
    DateTime CreateDate,
    DateTime UpdateDate);

public record AdminDemandView(
    long Id,
    string? Name,
    string SlugName,
    long CategoryId,
    string? CategoryName,
    string Status,
    DateTime CreateDate,
    DateTime UpdateDate);
