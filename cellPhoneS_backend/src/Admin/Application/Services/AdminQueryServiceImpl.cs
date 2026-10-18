using cellphones_backend.Data;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace cellPhoneS_backend.Services.Implement;

public class AdminQueryServiceImpl : IAdminQueryService
{
    private readonly ApplicationDbContext _context;

    public AdminQueryServiceImpl(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AdminUserView>> GetUsers(int pageIndex, int pageSize, string? search)
    {
        (pageIndex, pageSize) = PagingExtensions.NormalizePaging(pageIndex, pageSize);
        var query = _context.Users.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(u => EF.Functions.ILike(u.Email!, pattern) || EF.Functions.ILike(u.Fullname!, pattern));
        }

        var total = await query.CountAsync();
        var users = await query
            .OrderByDescending(u => u.CreateDate)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new { u.Id, u.Email, u.Fullname, u.PhoneNumber, u.Status, u.CreateDate })
            .ToListAsync();

        // Role nằm ở bảng nối của Identity; lấy một lần cho cả trang thay vì truy vấn từng user.
        var ids = users.Select(u => u.Id).ToList();
        var roles = await _context.UserRoles
            .Where(ur => ids.Contains(ur.UserId))
            .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
            .ToListAsync();
        var rolesByUser = roles.ToLookup(r => r.UserId, r => r.Name!);

        var items = users.Select(u => new AdminUserView(
            u.Id, u.Email, u.Fullname, u.PhoneNumber, u.Status,
            rolesByUser[u.Id].OrderBy(n => n).ToList(), u.CreateDate)).ToList();
        return new PagedResult<AdminUserView>(items, total, pageIndex, pageSize);
    }

    public async Task<PagedResult<AdminOrderView>> GetOrders(int pageIndex, int pageSize, string? status)
    {
        (pageIndex, pageSize) = PagingExtensions.NormalizePaging(pageIndex, pageSize);
        var query = _context.Orders.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(o => o.Status == status);
        }

        var total = await query.CountAsync();
        var rows = await query
            .OrderByDescending(o => o.CreateDate)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new
            {
                o.Id,
                o.CreateDate,
                o.Status,
                o.Type,
                CustomerName = o.CreateUser!.Fullname,
                CustomerEmail = o.CreateUser.Email,
                ItemCount = o.OrderDetails.Sum(d => d.Quantity),
                ShippingFee = o.Fee.FeeDetails.Sum(f => f.Value),
                GoodsTotal = o.OrderDetails.Sum(d => d.Price * d.Quantity)
            })
            .ToListAsync();

        var items = rows.Select(r => new AdminOrderView
        {
            Id = r.Id,
            CreateDate = r.CreateDate,
            Status = r.Status,
            Type = r.Type,
            CustomerName = r.CustomerName,
            CustomerEmail = r.CustomerEmail,
            ItemCount = r.ItemCount,
            ShippingFee = r.ShippingFee,
            TotalAmount = r.GoodsTotal + r.ShippingFee
        }).ToList();
        return new PagedResult<AdminOrderView>(items, total, pageIndex, pageSize);
    }

    public async Task<AdminOrderView> GetOrder(long orderId)
    {
        var row = await _context.Orders.AsNoTracking()
            .Where(o => o.Id == orderId)
            .Select(o => new
            {
                o.Id,
                o.CreateDate,
                o.Status,
                o.Type,
                CustomerName = o.CreateUser!.Fullname,
                CustomerEmail = o.CreateUser.Email,
                ShippingFee = o.Fee.FeeDetails.Sum(f => f.Value),
                Lines = o.OrderDetails.Select(d => new
                {
                    d.ProductOrderDetailId,
                    ProductName = d.Product!.Name,
                    d.Product.ImageUrl,
                    d.ColorId,
                    ColorName = d.Color!.Name,
                    d.Quantity,
                    d.Price
                }).ToList()
            })
            .FirstOrDefaultAsync();
        if (row == null) throw new NotFoundException("Không tìm thấy đơn hàng.");

        // Một sản phẩm có thể xuất từ nhiều kho nên gộp theo (sản phẩm, màu), giống lịch sử mua hàng của khách.
        var items = row.Lines
            .GroupBy(l => (l.ProductOrderDetailId, l.ColorId))
            .Select(g => new OrderHistoryItemView(
                g.Key.ProductOrderDetailId, g.First().ProductName, g.First().ImageUrl,
                g.Key.ColorId, g.First().ColorName, g.Sum(l => l.Quantity), g.First().Price))
            .ToList();

        return new AdminOrderView
        {
            Id = row.Id,
            CreateDate = row.CreateDate,
            Status = row.Status,
            Type = row.Type,
            CustomerName = row.CustomerName,
            CustomerEmail = row.CustomerEmail,
            ItemCount = items.Sum(i => i.Quantity),
            ShippingFee = row.ShippingFee,
            TotalAmount = items.Sum(i => i.Price * i.Quantity) + row.ShippingFee,
            Items = items
        };
    }

    public async Task<PagedResult<AdminTransactionView>> GetTransactions(int pageIndex, int pageSize)
    {
        return await _context.PaymentTransactions.AsNoTracking()
            .OrderByDescending(t => t.CreateDate)
            .Select(t => new AdminTransactionView(
                t.Id, t.OrderId, t.Amount, t.Provider, t.Reference, t.Status,
                t.TransactionDate, t.CreateDate, t.Order!.CreateUser!.Email))
            .ToPagedResultAsync(pageIndex, pageSize);
    }
}
