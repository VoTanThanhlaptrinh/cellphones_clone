using System.Data.Common;
using cellphones_backend.Data;
using cellPhoneS_backend.Constants;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace cellPhoneS_backend.Services.Implement;

public class AdminInventoryServiceImpl : IAdminInventoryService
{
    private static readonly string[] StoreHouseSortKeys = { "createDate", "updateDate", "city", "totalStock", "id" };
    private static readonly string[] InventorySortKeys = { "updateDate", "createDate", "quantity", "productName" };

    // Cộng dồn nguyên tử: đặt hàng (OrderServiceImpl) cũng trừ/cộng kho trong transaction của nó nên không đọc-sửa-ghi qua EF.
    private const string UpsertSetSql = """
        INSERT INTO "Stores" ("StoreHouseId", "ProductId", "ColorId", "Quantity", "Status", "CreateDate", "UpdateDate", "CreateBy", "UpdateBy")
        VALUES (@storeHouseId, @productId, @colorId, @quantity, 'active', @now, @now, @userId, @userId)
        ON CONFLICT ("StoreHouseId", "ProductId", "ColorId")
        DO UPDATE SET "Quantity" = EXCLUDED."Quantity", "UpdateDate" = EXCLUDED."UpdateDate", "UpdateBy" = EXCLUDED."UpdateBy"
        RETURNING "Quantity"
        """;

    private const string UpsertAddSql = """
        INSERT INTO "Stores" ("StoreHouseId", "ProductId", "ColorId", "Quantity", "Status", "CreateDate", "UpdateDate", "CreateBy", "UpdateBy")
        VALUES (@storeHouseId, @productId, @colorId, @quantity, 'active', @now, @now, @userId, @userId)
        ON CONFLICT ("StoreHouseId", "ProductId", "ColorId")
        DO UPDATE SET "Quantity" = "Stores"."Quantity" + EXCLUDED."Quantity", "UpdateDate" = EXCLUDED."UpdateDate", "UpdateBy" = EXCLUDED."UpdateBy"
        RETURNING "Quantity"
        """;

    private const string SubtractSql = """
        UPDATE "Stores"
        SET "Quantity" = "Quantity" + @delta, "UpdateDate" = @now, "UpdateBy" = @userId
        WHERE "StoreHouseId" = @storeHouseId AND "ProductId" = @productId AND "ColorId" = @colorId
          AND "Quantity" + @delta >= 0
        RETURNING "Quantity"
        """;

    private readonly ApplicationDbContext _context;

    public AdminInventoryServiceImpl(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AdminStoreHouseView>> GetStoreHouses(AdminStoreHouseFilter f)
    {
        var status = RecordStatuses.ParseFilter(f.Status);
        var desc = QueryOrderingExtensions.IsDescending(f.SortDir);
        var query = _context.StoreHouses.AsNoTracking().AsQueryable();
        query = status == null ? query.Where(h => h.Status != RecordStatuses.Deleted) : query.Where(h => h.Status == status);
        if (!string.IsNullOrWhiteSpace(f.Province))
        {
            var province = $"%{f.Province.Trim()}%";
            query = query.Where(h => EF.Functions.ILike(h.City!, province));
        }
        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            var pattern = $"%{f.Search.Trim()}%";
            query = query.Where(h => EF.Functions.ILike(h.Street!, pattern) || EF.Functions.ILike(h.District!, pattern)
                                     || EF.Functions.ILike(h.City!, pattern));
        }

        var ordered = f.SortBy?.Trim().ToLowerInvariant() switch
        {
            null or "" or "createdate" => query.OrderByDirection(h => h.CreateDate, desc),
            "updatedate" => query.OrderByDirection(h => h.UpdateDate, desc),
            "city" => query.OrderByDirection(h => h.City, desc),
            "totalstock" => query.OrderByDirection(h => _context.Stores.Where(s => s.StoreHouseId == h.Id).Sum(s => (int?)s.Quantity) ?? 0, desc),
            "id" => query.OrderByDirection(h => h.Id, desc),
            _ => throw QueryOrderingExtensions.InvalidSort(f.SortBy, StoreHouseSortKeys)
        };

        return await ordered.ThenBy(h => h.Id)
            .Select(h => new AdminStoreHouseView(
                h.Id, h.Street, h.District, h.City, h.Status,
                _context.Stores.Where(s => s.StoreHouseId == h.Id).Sum(s => (int?)s.Quantity) ?? 0,
                _context.Stores.Where(s => s.StoreHouseId == h.Id).Select(s => s.ProductId).Distinct().Count(),
                h.CreateDate, h.UpdateDate))
            .ToPagedResultAsync(f.PageIndex, f.PageSize);
    }

    public async Task<PagedResult<InventoryRowView>> GetInventory(InventoryFilter f)
    {
        var desc = QueryOrderingExtensions.IsDescending(f.SortDir);
        var query = _context.Stores.AsNoTracking()
            .Where(s => s.Product!.Status != RecordStatuses.Deleted && s.StoreHouse!.Status != RecordStatuses.Deleted);
        if (f.ProductId.HasValue) query = query.Where(s => s.ProductId == f.ProductId);
        if (f.StoreHouseId.HasValue) query = query.Where(s => s.StoreHouseId == f.StoreHouseId);
        if (f.ColorId.HasValue) query = query.Where(s => s.ColorId == f.ColorId);
        if (f.LowStock.HasValue) query = query.Where(s => s.Quantity <= f.LowStock);
        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            var pattern = $"%{f.Search.Trim()}%";
            query = query.Where(s => EF.Functions.ILike(s.Product!.Name!, pattern));
        }

        var ordered = f.SortBy?.Trim().ToLowerInvariant() switch
        {
            null or "" or "updatedate" => query.OrderByDirection(s => s.UpdateDate, desc),
            "createdate" => query.OrderByDirection(s => s.CreateDate, desc),
            "quantity" => query.OrderByDirection(s => s.Quantity, desc),
            "productname" => query.OrderByDirection(s => s.Product!.Name, desc),
            _ => throw QueryOrderingExtensions.InvalidSort(f.SortBy, InventorySortKeys)
        };

        return await ordered.ThenBy(s => s.StoreHouseId).ThenBy(s => s.ProductId).ThenBy(s => s.ColorId)
            .Select(s => new InventoryRowView(
                new InventoryRefView(s.StoreHouseId, s.StoreHouse!.Street + ", " + s.StoreHouse.District + ", " + s.StoreHouse.City),
                new InventoryProductRefView(s.ProductId, s.Product!.Name, s.Product.ImageUrl),
                new InventoryRefView(s.ColorId, s.Color!.Name),
                s.Quantity, s.Status, s.UpdateDate))
            .ToPagedResultAsync(f.PageIndex, f.PageSize);
    }

    public async Task SetQuantity(SetInventoryRequest request, string userId)
    {
        await EnsureReferencesExist(request.StoreHouseId, request.ProductId, request.ColorId);
        await ExecuteQuantity(UpsertSetSql, request.StoreHouseId, request.ProductId, request.ColorId, request.Quantity, userId);
    }

    public async Task<int> Adjust(AdjustInventoryRequest request, string userId)
    {
        if (request.Delta == 0) throw new BadRequestException("Delta phải khác 0.");

        if (request.Delta > 0)
        {
            await EnsureReferencesExist(request.StoreHouseId, request.ProductId, request.ColorId);
            return (await ExecuteQuantity(UpsertAddSql, request.StoreHouseId, request.ProductId, request.ColorId, request.Delta, userId))!.Value;
        }

        // Trừ: điều kiện "Quantity + delta >= 0" nằm trong cùng câu UPDATE nên không thể trừ lố khi đang có đơn đặt hàng.
        var result = await ExecuteQuantity(SubtractSql, request.StoreHouseId, request.ProductId, request.ColorId, request.Delta, userId);
        if (result == null)
        {
            var exists = await _context.Stores.AnyAsync(s => s.StoreHouseId == request.StoreHouseId
                && s.ProductId == request.ProductId && s.ColorId == request.ColorId);
            throw new BadRequestException(exists
                ? "Không đủ tồn kho: số lượng sau điều chỉnh sẽ âm."
                : "Chưa có tồn kho cho tổ hợp kho/sản phẩm/màu này nên không thể trừ.");
        }
        return result.Value;
    }

    private async Task EnsureReferencesExist(long storeHouseId, long productId, long colorId)
    {
        if (!await _context.StoreHouses.AnyAsync(h => h.Id == storeHouseId && h.Status != RecordStatuses.Deleted))
            throw new NotFoundException("Không tìm thấy kho.");
        if (!await _context.Products.AnyAsync(p => p.Id == productId && p.Status != RecordStatuses.Deleted))
            throw new NotFoundException("Không tìm thấy sản phẩm.");
        if (!await _context.Colors.AnyAsync(c => c.Id == colorId && c.Status != RecordStatuses.Deleted))
            throw new NotFoundException("Không tìm thấy màu.");
    }

    /// <summary>Chạy một câu INSERT/UPDATE ... RETURNING "Quantity"; trả null nếu không dòng nào khớp (điều kiện WHERE không thoả).</summary>
    private async Task<int?> ExecuteQuantity(string sql, long storeHouseId, long productId, long colorId, int quantityOrDelta, string userId)
    {
        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State != System.Data.ConnectionState.Open;
        if (wasClosed) await _context.Database.OpenConnectionAsync();
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Transaction = _context.Database.CurrentTransaction?.GetDbTransaction();
            AddParameter(command, "storeHouseId", storeHouseId);
            AddParameter(command, "productId", productId);
            AddParameter(command, "colorId", colorId);
            AddParameter(command, "quantity", quantityOrDelta);
            AddParameter(command, "delta", quantityOrDelta);
            AddParameter(command, "now", DateTime.UtcNow);
            AddParameter(command, "userId", userId);
            var scalar = await command.ExecuteScalarAsync();
            return scalar == null || scalar is DBNull ? null : Convert.ToInt32(scalar);
        }
        finally
        {
            if (wasClosed) await _context.Database.CloseConnectionAsync();
        }
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        // Mỗi câu SQL chỉ dùng một phần các tham số; Npgsql bỏ qua tham số không xuất hiện trong câu lệnh.
        command.Parameters.Add(new NpgsqlParameter(name, value));
    }
}
