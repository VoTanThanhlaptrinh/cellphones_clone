namespace cellPhoneS_backend.DTOs.Responses;

/// <summary>Kho trong trang quản trị, kèm tổng tồn (mọi sản phẩm và màu) và số sản phẩm khác nhau đang có trong kho.</summary>
public record AdminStoreHouseView(
    long Id,
    string? Street,
    string? District,
    string? City,
    string Status,
    int TotalStock,
    int ProductCount,
    DateTime CreateDate,
    DateTime UpdateDate);

/// <summary>Tham chiếu gọn tới kho / sản phẩm / màu trong một dòng tồn (<c>Name</c> của kho là địa chỉ).</summary>
public record InventoryRefView(long Id, string? Name);

public record InventoryProductRefView(long Id, string? Name, string? ImageUrl);

public record InventoryRowView(
    InventoryRefView StoreHouse,
    InventoryProductRefView Product,
    InventoryRefView Color,
    int Quantity,
    string? Status,
    DateTime UpdateDate);
