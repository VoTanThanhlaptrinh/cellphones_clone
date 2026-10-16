namespace cellPhoneS_backend.DTOs;

/// <summary>
/// Thông tin hiển thị trên thẻ sản phẩm. Stock/Rating/ReviewCount/Highlights/DefaultColorId được
/// bổ sung bởi <c>ProductCardEnricher</c>; Stock = null nghĩa là chưa biết tồn kho.
/// </summary>
public record ProductView(
    long? Id,
    string? ImgUrl,
    string? ProductName,
    double BasePrice,
    double SalePrice,
    int? Stock,
    double? Rating,
    int ReviewCount,
    IReadOnlyList<string>? Highlights,
    long? DefaultColorId)
{
    // Constructor rút gọn cho các truy vấn chỉ cần thông tin cơ bản (expression tree không dùng được tham số mặc định)
    public ProductView(long? id, string? imgUrl, string? productName, double basePrice, double salePrice)
        : this(id, imgUrl, productName, basePrice, salePrice, null, null, 0, null, null) { }
}
