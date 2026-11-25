using cellphones_backend.Data;
using cellPhoneS_backend.Constants;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace cellPhoneS_backend.Services.Implement;

public class AdminOrderServiceImpl : IAdminOrderService
{
    private static readonly string[] AllStatuses =
        { OrderStatuses.Pending, OrderStatuses.Paid, OrderStatuses.Shipping, OrderStatuses.Completed, OrderStatuses.Cancelled };

    private readonly ApplicationDbContext _context;
    private readonly OrderService _orderService;

    public AdminOrderServiceImpl(ApplicationDbContext context, OrderService orderService)
    {
        _context = context;
        _orderService = orderService;
    }

    public async Task<PagedResult<AdminOrderView>> GetOrders(AdminOrderFilter filter)
    {
        var query = _context.Orders.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var status = AllStatuses.FirstOrDefault(s => string.Equals(s, filter.Status.Trim(), StringComparison.OrdinalIgnoreCase))
                ?? throw new BadRequestException("status phải là Pending, Paid, Shipping, Completed hoặc Cancelled.");
            query = query.Where(o => o.Status == status);
        }
        if (!string.IsNullOrWhiteSpace(filter.Type))
        {
            var type = filter.Type.Trim().ToLowerInvariant();
            if (type != OrderTypes.Pickup && type != OrderTypes.Delivery)
                throw new BadRequestException("type phải là pickup hoặc delivery.");
            query = query.Where(o => o.Type == type);
        }
        var (start, end) = AdminDateRange.ToUtcRange(filter.From, filter.To);
        if (start.HasValue) query = query.Where(o => o.CreateDate >= start.Value);
        if (end.HasValue) query = query.Where(o => o.CreateDate < end.Value);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            var pattern = $"%{search}%";
            if (long.TryParse(search, out var id))
                query = query.Where(o => o.Id == id
                    || EF.Functions.ILike(o.CreateUser!.Email!, pattern) || EF.Functions.ILike(o.CreateUser.Fullname!, pattern));
            else
                query = query.Where(o => EF.Functions.ILike(o.CreateUser!.Email!, pattern) || EF.Functions.ILike(o.CreateUser.Fullname!, pattern));
        }

        var ascending = string.Equals(filter.SortDir, "asc", StringComparison.OrdinalIgnoreCase);
        // sortBy chỉ nhận whitelist; Id là khoá phụ để phân trang ổn định.
        var sorted = (filter.SortBy?.ToLowerInvariant(), ascending) switch
        {
            ("id", true) => query.OrderBy(o => o.Id),
            ("id", false) => query.OrderByDescending(o => o.Id),
            ("status", true) => query.OrderBy(o => o.Status).ThenBy(o => o.Id),
            ("status", false) => query.OrderByDescending(o => o.Status).ThenByDescending(o => o.Id),
            ("totalamount", true) => query.OrderBy(o => o.OrderDetails.Sum(d => d.Price * d.Quantity) + o.Fee.FeeDetails.Sum(f => f.Value)).ThenBy(o => o.Id),
            ("totalamount", false) => query.OrderByDescending(o => o.OrderDetails.Sum(d => d.Price * d.Quantity) + o.Fee.FeeDetails.Sum(f => f.Value)).ThenByDescending(o => o.Id),
            (_, true) => query.OrderBy(o => o.CreateDate).ThenBy(o => o.Id),
            (_, false) => query.OrderByDescending(o => o.CreateDate).ThenByDescending(o => o.Id)
        };

        var (pageIndex, pageSize) = PagingExtensions.NormalizePaging(filter.PageIndex, filter.PageSize);
        var total = await sorted.CountAsync();
        var rows = await sorted
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

    public async Task<AdminOrderDetail> GetOrder(long orderId)
    {
        var row = await _context.Orders.AsNoTracking()
            .Where(o => o.Id == orderId)
            .Select(o => new
            {
                o.Id,
                o.CreateDate,
                o.UpdateDate,
                o.Status,
                o.Type,
                o.PaymentAttempt,
                CustomerId = o.CreateBy,
                CustomerName = o.CreateUser!.Fullname,
                CustomerEmail = o.CreateUser.Email,
                CustomerPhone = o.CreateUser.PhoneNumber,
                UpdatedByEmail = o.UpdateUser!.Email,
                Fees = o.Fee.FeeDetails.Select(f => new FeeView { Name = f.Name, Value = f.Value }).ToList(),
                Lines = o.OrderDetails.Select(d => new
                {
                    d.ProductOrderDetailId,
                    ProductName = d.Product!.Name,
                    d.Product.ImageUrl,
                    d.ColorId,
                    ColorName = d.Color!.Name,
                    d.Quantity,
                    d.Price,
                    d.StoreHouseId,
                    d.StoreHouse!.Street,
                    d.StoreHouse.District,
                    d.StoreHouse.City
                }).ToList()
            })
            .FirstOrDefaultAsync();
        if (row == null) throw new NotFoundException("Không tìm thấy đơn hàng.");

        var transactions = await _context.PaymentTransactions.AsNoTracking()
            .Where(t => t.OrderId == orderId)
            .OrderByDescending(t => t.CreateDate)
            .Select(t => new AdminTransactionView(t.Id, t.OrderId, t.Amount, t.Provider, t.Reference, t.Status,
                t.TransactionDate, t.CreateDate, t.Order!.CreateUser!.Email))
            .ToListAsync();

        var shippingFee = row.Fees.Sum(f => f.Value);
        // Một sản phẩm có thể xuất từ nhiều kho nên gộp theo (sản phẩm, màu), giống lịch sử mua hàng của khách.
        var items = row.Lines
            .GroupBy(l => (l.ProductOrderDetailId, l.ColorId))
            .Select(g => new OrderHistoryItemView(
                g.Key.ProductOrderDetailId, g.First().ProductName, g.First().ImageUrl,
                g.Key.ColorId, g.First().ColorName, g.Sum(l => l.Quantity), g.First().Price))
            .ToList();
        var storeHouses = row.Lines
            .GroupBy(l => l.StoreHouseId)
            .Select(g => new AdminOrderStoreHouseView
            {
                Id = g.Key,
                Street = g.First().Street,
                District = g.First().District,
                City = g.First().City,
                Items = g.GroupBy(l => (l.ProductOrderDetailId, l.ColorId))
                    .Select(p => new AdminOrderAllocationView(p.Key.ProductOrderDetailId, p.First().ProductName,
                        p.Key.ColorId, p.First().ColorName, p.Sum(l => l.Quantity)))
                    .ToList()
            })
            .ToList();

        return new AdminOrderDetail
        {
            Id = row.Id,
            CreateDate = row.CreateDate,
            UpdateDate = row.UpdateDate,
            Status = row.Status,
            Type = row.Type,
            CustomerId = row.CustomerId,
            CustomerName = row.CustomerName,
            CustomerEmail = row.CustomerEmail,
            CustomerPhone = row.CustomerPhone,
            UpdatedByEmail = row.UpdatedByEmail,
            PaymentAttempt = row.PaymentAttempt,
            ItemCount = items.Sum(i => i.Quantity),
            ShippingFee = shippingFee,
            TotalAmount = items.Sum(i => i.Price * i.Quantity) + shippingFee,
            Items = items,
            Fees = row.Fees,
            StoreHouses = storeHouses,
            Transactions = transactions
        };
    }

    public async Task<AdminOrderDetail> UpdateStatus(long orderId, UpdateOrderStatusRequest request, string adminId)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new NotFoundException("Không tìm thấy đơn hàng.");
        try
        {
            if (request.Status == OrderStatuses.Shipping) order.Ship(adminId);
            else order.Complete(adminId);
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }
        await _context.SaveChangesAsync();
        return await GetOrder(orderId);
    }

    public async Task<CancelOrderResult> Cancel(long orderId, string adminId)
    {
        var previousStatus = await _orderService.AdminCancelOrder(orderId, adminId);
        // Gateway chưa có refund: đơn đã thu tiền thì báo để admin hoàn tiền thủ công.
        var refundRequired = await _context.PaymentTransactions.AsNoTracking()
            .AnyAsync(t => t.OrderId == orderId && t.Status == PaymentTransactionStatuses.Success);
        return new CancelOrderResult(previousStatus, refundRequired);
    }

    public async Task<string> SyncPayment(long orderId)
    {
        if (!await _context.Orders.AnyAsync(o => o.Id == orderId))
            throw new NotFoundException("Không tìm thấy đơn hàng.");

        await _orderService.HandlePaymentCallback(orderId);
        return await _context.Orders.AsNoTracking().Where(o => o.Id == orderId).Select(o => o.Status).FirstAsync();
    }
}
