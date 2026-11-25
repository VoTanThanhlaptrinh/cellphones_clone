namespace cellPhoneS_backend.DTOs.Responses;

/// <summary>Dòng sản phẩm trong danh sách admin (kèm tổng tồn kho, danh mục và trạng thái).</summary>
public record AdminProductView(
    long Id,
    string? Name,
    string? ImageUrl,
    double BasePrice,
    double SalePrice,
    string? Status,
    int Stock,
    List<string> Categories,
    DateTime CreateDate,
    DateTime UpdateDate);

/// <summary>Đủ dữ liệu cho form sửa sản phẩm (ảnh, màu, thông số, cam kết, danh mục, tồn kho theo kho).</summary>
public record AdminProductDetail(
    long Id,
    string? Name,
    string? Version,
    double BasePrice,
    double SalePrice,
    string? Status,
    string? ImageUrl,
    int Stock,
    DateTime CreateDate,
    DateTime UpdateDate,
    List<AdminProductImageView> Images,
    List<AdminProductColorView> Colors,
    List<AdminProductSpecificationView> Specifications,
    List<AdminProductCommitmentView> Commitments,
    List<AdminProductCategoryView> Categories,
    List<AdminProductStockView> Stocks);

public record AdminProductImageView(long ImageId, string? Url, string? Name, string? Alt, string? MimeType, string? Status);

public record AdminProductColorView(long Id, string? Name, string? ImageUrl, string Status);

public record AdminProductSpecificationView(long Id, string? Name, List<AdminSpecificationDetailView> Details);

public record AdminSpecificationDetailView(long Id, string? Name, string? Value);

public record AdminProductCommitmentView(long Id, string? Context, string Status);

public record AdminProductCategoryView(long Id, string? Name);

/// <summary>Tồn của sản phẩm tại một kho theo từng màu.</summary>
public record AdminProductStockView(
    long StoreHouseId,
    string StoreHouse,
    long ColorId,
    string? ColorName,
    int Quantity,
    string? Status);
