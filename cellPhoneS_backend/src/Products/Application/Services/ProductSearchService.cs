using cellPhoneS_backend.Models;

namespace cellPhoneS_backend.Services.Interface;

public interface ProductSearchService
{
    public  Task<List<ProductSearchResult>> SearchAsync(string keyword);
    public  Task<List<ProductSearchResult>> InitializeAsync();
    // Xoá cache để lần tìm kiếm sau nạp lại (gọi sau khi admin thêm/sửa/xoá sản phẩm)
    public void RefreshCache();
    // Làm mới materialized view mv_ProductSearch rồi xoá cache (gọi sau khi admin ghi sản phẩm)
    public Task RefreshAsync();
}
