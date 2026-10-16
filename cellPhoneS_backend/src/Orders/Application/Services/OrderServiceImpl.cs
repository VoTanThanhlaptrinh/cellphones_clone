using System.Text;
using cellphones_backend.Data;
using cellphones_backend.Models;
using cellphones_backend.Repositories;
using cellPhoneS_backend.Constants;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Models;
using cellPhoneS_backend.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace cellPhoneS_backend.Services.Implement;

public class OrderServiceImpl : OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly CartDetailService _cartDetailService;
    private readonly IOrderDetailRepository _orderDetailRepository;
    private readonly ShippingFeeService _shippingFeeService;
    private readonly IStoreService _storeService;
    private readonly ApplicationDbContext _context;
    private readonly IPaymentGatewayClient _gateway;
    private readonly ILogger<OrderServiceImpl> _logger;
    private readonly IConfiguration _configuration;

    public OrderServiceImpl(
        IOrderRepository orderRepository,
        CartDetailService cartDetailService,
        IOrderDetailRepository orderDetailRepository,
        ShippingFeeService shippingFeeService,
        IStoreService storeService,
        ApplicationDbContext context,
        IPaymentGatewayClient gateway,
        ILogger<OrderServiceImpl> logger,
        IConfiguration configuration)
    {
        _orderRepository = orderRepository;
        _cartDetailService = cartDetailService;
        _orderDetailRepository = orderDetailRepository;
        _shippingFeeService = shippingFeeService;
        _storeService = storeService;
        _context = context;
        _gateway = gateway;
        _logger = logger;
        _configuration = configuration;
    }

    #region PUBLIC APIs (Nghiệp vụ chính)

    public async Task Checkout(string userId, long orderId)
    {
        var order = await GetOrderWithDetailsAsync(orderId);
        if (order == null) throw new NotFoundException("Order not found");
        if (!order.IsOwnedBy(userId)) throw new ForbiddenException("User is not authorized to checkout this order");
        if (order.IsCancelled) throw new BadRequestException("Đơn hàng đã bị hủy.");

        await RemoveOrderedCartItemsAsync(order);
        await _context.SaveChangesAsync();
    }

    public async Task MarkOrderPaid(long orderId, PaymentConfirmation confirmation)
    {
        var order = await GetOrderWithDetailsAsync(orderId);
        if (order == null || order.IsCancelled) return;

        order.MarkPaid();

        // Có thể được gọi lại nhiều lần: mỗi đơn chỉ ghi một giao dịch thành công.
        if (!await _context.PaymentTransactions.AnyAsync(t => t.OrderId == order.Id && t.Status == PaymentTransactionStatuses.Success))
        {
            _context.PaymentTransactions.Add(new PaymentTransaction
            {
                OrderId = order.Id,
                Amount = confirmation.Amount,
                Provider = confirmation.Provider,
                Reference = confirmation.Reference,
                PaymentLinkId = confirmation.PaymentLinkId,
                Description = confirmation.Description,
                Status = PaymentTransactionStatuses.Success,
                TransactionDate = confirmation.TransactionDate,
                CreateDate = DateTime.UtcNow
            });
        }
        await RemoveOrderedCartItemsAsync(order);
        await _context.SaveChangesAsync();
    }

    /// <summary>Lịch sử đơn hàng của chính người dùng, mới nhất trước; gồm cả đơn đã huỷ.</summary>
    public async Task<PagedResult<OrderHistoryView>> GetMyOrders(string userId, int pageIndex, int pageSize)
    {
        pageIndex = Math.Max(pageIndex, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = _context.Orders.AsNoTracking().Where(o => o.CreateBy == userId);
        var totalCount = await query.CountAsync();

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
            .ToListAsync();

        var items = rows.Select(r =>
        {
            // Một sản phẩm có thể được xuất từ nhiều kho nên gộp theo (sản phẩm, màu) cho dễ đọc.
            var lines = r.Lines
                .GroupBy(l => (l.ProductOrderDetailId, l.ColorId))
                .Select(g => new OrderHistoryItemView(
                    g.Key.ProductOrderDetailId,
                    g.First().ProductName,
                    g.First().ImageUrl,
                    g.Key.ColorId,
                    g.First().ColorName,
                    g.Sum(l => l.Quantity),
                    g.First().Price))
                .ToList();
            return new OrderHistoryView
            {
                Id = r.Id,
                CreateDate = r.CreateDate,
                Status = r.Status,
                Type = r.Type,
                ShippingFee = r.ShippingFee,
                TotalAmount = lines.Sum(l => l.Price * l.Quantity) + r.ShippingFee,
                Items = lines
            };
        }).ToList();

        return new PagedResult<OrderHistoryView>(items, totalCount, pageIndex, pageSize);
    }

    public async Task<string> GetOrderStatus(string userId, long orderId)
    {
        var order = await GetOrderWithDetailsAsync(orderId);
        if (order == null) throw new NotFoundException("Không tìm thấy đơn hàng.");
        if (!order.IsOwnedBy(userId)) throw new ForbiddenException("Bạn không có quyền xem đơn hàng này.");

        // Kết quả thanh toán online chỉ tin từ gateway, nên đơn còn Pending thì hỏi lại trước khi trả lời.
        if (order.IsPending)
        {
            await SyncLatestOnlinePaymentAsync(order);
        }
        return order.Status;
    }

    public async Task<OnlinePaymentView> StartOnlinePayment(string userId, long orderId, string provider)
    {
        provider = (provider ?? "").Trim().ToUpperInvariant();
        if (!PaymentProviders.Online.Contains(provider)) throw new BadRequestException("Cổng thanh toán không hợp lệ.");

        var order = await _context.Orders
            .Include(o => o.OrderDetails)
            .Include(o => o.Fee).ThenInclude(f => f.FeeDetails)
            .FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) throw new NotFoundException("Không tìm thấy đơn hàng.");
        if (!order.IsOwnedBy(userId)) throw new ForbiddenException("Bạn không có quyền thanh toán đơn hàng này.");
        if (!order.IsPending) throw new BadRequestException("Đơn hàng không còn chờ thanh toán.");

        var latest = await GetLatestOnlinePaymentAsync(orderId);
        if (latest != null)
        {
            // Còn link đang chờ thì dùng lại, kể cả khi khách đổi cổng: gateway chỉ cho một payment hoạt động mỗi đơn.
            GatewayPayment? live;
            try
            {
                live = await SyncOnlinePaymentAsync(order, latest, throwOnError: true);
                if (latest.Status == OnlinePaymentStatuses.Pending && latest.ExpiresAt <= DateTime.UtcNow)
                {
                    // Link đã hết hạn nhưng gateway chưa đối soát: huỷ để tạo được link mới (VNPay không huỷ được thì giữ nguyên).
                    live = await TryCancelExpiredAsync(order, latest) ?? live;
                }
            }
            catch (PaymentGatewayException ex)
            {
                throw ToAppException(ex);
            }
            if (latest.Status == OnlinePaymentStatuses.Paid || latest.Status == OnlinePaymentStatuses.ReviewRequired || latest.CanPay)
            {
                return ToView(latest, live);
            }
        }

        // Chỉ để thử thanh toán thật với số tiền nhỏ: đặt PaymentGateway:TestAmountOverride (user-secrets) thì mọi đơn bị tính số tiền này.
        // Bỏ cấu hình đó là về lại tiền thật của đơn. Số tiền cố định theo từng lần thử nên link đã có thì giữ nguyên số tiền cũ.
        var total = order.GetTotalAmount();
        var amount = _configuration.GetValue<long?>(PaymentConfigKeys.TestAmountOverride) ?? total;
        if (amount != total)
        {
            _logger.LogWarning("TestAmountOverride đang bật: đơn {OrderId} thanh toán {Amount} thay vì {Real}", orderId, amount, total);
        }

        // Lần thử trước chưa kịp nhận trả lời từ gateway: gửi lại đúng Idempotency-Key cũ để không sinh payment thứ hai.
        var row = latest is { Status: OnlinePaymentStatuses.Creating } && latest.Provider == provider ? latest : null;
        if (row == null)
        {
            row = OnlinePayment.Start(orderId, provider, (latest?.Attempt ?? 0) + 1, amount);
            // Key phải nằm trong DB trước khi gọi gateway để mọi lần thử lại dùng đúng key này.
            _context.OnlinePayments.Add(row);
            await _context.SaveChangesAsync();
        }

        GatewayPayment gatewayPayment;
        try
        {
            gatewayPayment = await _gateway.Checkout(orderId, row.Amount, provider, row.IdempotencyKey);
            await ApplyGatewayPaymentAsync(order, row, gatewayPayment);
        }
        catch (PaymentGatewayException ex) when (ex.Code == GatewayErrorCodes.PaymentAlreadyPending)
        {
            try
            {
                gatewayPayment = await _gateway.GetStatus(orderId) ?? throw ToAppException(ex);
                await ApplyGatewayPaymentAsync(order, row, gatewayPayment);
            }
            catch (PaymentGatewayException inner)
            {
                throw ToAppException(inner);
            }
        }
        catch (PaymentGatewayException ex)
        {
            // Lỗi 4xx là dứt khoát: lần thử sau phải dùng key mới. Lỗi mạng/5xx giữ "Creating" để thử lại cùng key.
            if (ex.Status is >= 400 and < 500)
            {
                row.Reject();
                await _context.SaveChangesAsync();
            }
            throw ToAppException(ex);
        }
        return ToView(row, gatewayPayment);
    }

    public async Task<OnlinePaymentView> GetOnlinePayment(string userId, long paymentId)
    {
        var payment = await _context.OnlinePayments.FirstOrDefaultAsync(p => p.Id == paymentId);
        if (payment == null) throw new NotFoundException("Không tìm thấy giao dịch thanh toán.");
        var order = await GetOrderWithDetailsAsync(payment.OrderId);
        if (order == null) throw new NotFoundException("Không tìm thấy đơn hàng.");
        if (!order.IsOwnedBy(userId)) throw new ForbiddenException("Bạn không có quyền xem giao dịch này.");

        // Chỉ lần thanh toán mới nhất của đơn mới còn hiệu lực; các lần cũ đã bị thay thế nên không hỏi gateway nữa.
        GatewayPayment? live = null;
        var latest = await GetLatestOnlinePaymentAsync(payment.OrderId);
        if (latest?.Id == payment.Id && payment.IsOpen && order.IsPending)
        {
            live = await SyncOnlinePaymentAsync(order, payment, throwOnError: false);
        }
        return ToView(payment, live);
    }

    public async Task SyncPendingOnlinePayments()
    {
        var since = DateTime.UtcNow.AddHours(-24);
        var orderIds = await _context.OnlinePayments
            .Where(p => OnlinePaymentStatuses.Open.Contains(p.Status) && p.CreateDate > since)
            .Select(p => p.OrderId)
            .Distinct()
            .ToListAsync();

        foreach (var orderId in orderIds)
        {
            try
            {
                var order = await GetOrderWithDetailsAsync(orderId);
                if (order != null && order.IsPending)
                {
                    await SyncLatestOnlinePaymentAsync(order);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đồng bộ thanh toán online của đơn {OrderId} lỗi", orderId);
            }
        }
    }

    public async Task<OrderView> CreateDeliveryOrder(string userId, DeliveryOrderRequest payload)
    {
        if (payload.CartDetailIds == null || !payload.CartDetailIds.Any())
            throw new BadRequestException("Cart item IDs cannot be null or empty");

        var cartDetails = await _cartDetailService.GetCartDetails(userId, payload.CartDetailIds);
        if (cartDetails == null || !cartDetails.Any())
            throw new NotFoundException("No cart items found for the user");

        var allPotentialStores = await _storeService.AllocateAllStockAsync(cartDetails);
        if (allPotentialStores == null || !allPotentialStores.Any())
            throw new BadRequestException("No store found containing these products.");

        // Sử dụng hàm Core đã gộp: Ưu tiên kho nằm trong cùng Province
        var finalOrderDetails = await AllocateInventoryCoreAsync(
            cartDetails,
            allPotentialStores,
            s => s.City == payload.ProvinceName,
            userId);

        long totalShippingFee = await CalculateDeliveryShippingFeeAsync(finalOrderDetails, allPotentialStores, payload.ProvinceName, payload.DistrictName);

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var order = await SaveOrderAsync(userId, OrderTypes.Delivery, finalOrderDetails, totalShippingFee);

            // Giỏ hàng chỉ bị xoá khi thanh toán xong (xem Checkout / MarkOrderPaid) để khách còn sửa được thông tin nhận hàng.
            await transaction.CommitAsync();

            return MapToOrderView(order);
        }
        catch (AppException)
        {
            await transaction.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new InternalServerException($"Failed to create delivery order: {ex.Message}");
        }
    }

    public async Task<OrderView> CreatePickupOrder(string userId, PickupOrderRequest payload)
    {
        if (payload.CartDetailIds == null || !payload.CartDetailIds.Any())
            throw new BadRequestException("Cart item IDs cannot be null or empty");

        var cartDetails = await _cartDetailService.GetCartDetails(userId, payload.CartDetailIds);
        if (cartDetails == null || !cartDetails.Any())
            throw new NotFoundException("No cart items found for the user");

        var storeInventory = await _storeService.AllocateAllStockAsync(cartDetails);
        if (storeInventory == null || !storeInventory.Any())
            throw new BadRequestException("Cửa hàng này hiện không có sẵn các sản phẩm trong giỏ.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Sử dụng hàm Core đã gộp: Ưu tiên kho có StoreHouseId trùng với khách chọn
            var finalOrderDetails = await AllocateInventoryCoreAsync(
                cartDetails,
                storeInventory,
                s => s.StoreHouseId == payload.StoreHouseId,
                userId);

            var order = await SaveOrderAsync(userId, OrderTypes.Pickup, finalOrderDetails, 0);

            // Giỏ hàng chỉ bị xoá khi thanh toán xong (xem Checkout / MarkOrderPaid) để khách còn sửa được thông tin nhận hàng.
            await transaction.CommitAsync();

            return MapToOrderView(order);
        }
        catch (AppException)
        {
            await transaction.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new InternalServerException($"Failed to create pickup order: {ex.Message}");
        }
    }

    public async Task DeleteOrder(string userId, long orderId)
    {
        var order = await GetOrderWithDetailsAsync(orderId);
        if (order == null) throw new NotFoundException("Không tìm thấy đơn hàng.");
        if (!order.IsOwnedBy(userId)) throw new ForbiddenException("Bạn không có quyền hủy đơn hàng này.");

        if (!order.CanBeCancelled)
            throw new BadRequestException($"Không thể hủy đơn hàng đang ở trạng thái: {order.Status}");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            if (order.OrderDetails != null && order.OrderDetails.Any())
            {
                await RestoreInventoryAsync(order.OrderDetails.ToList());
            }

            bool isRefundSuccessful = await ProcessRefundIfNecessaryAsync(order);
            if (!isRefundSuccessful)
            {
                throw new Exception("Quá trình hoàn tiền thất bại. Hệ thống tự động hủy thao tác.");
            }

            order.Cancel(userId);

            await _orderRepository.UpdateAsync(order);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (AppException)
        {
            await transaction.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new InternalServerException($"Lỗi khi hủy đơn hàng: {ex.Message}");
        }

        // Sau khi đơn đã huỷ thì link thanh toán cũ không được phép nhận tiền nữa.
        await CancelOnlinePaymentQuietlyAsync(orderId);
    }

    #endregion

    #region PRIVATE HELPERS (Logic nền tảng)

    #region Online payment (payment gateway)

    private Task<OnlinePayment?> GetLatestOnlinePaymentAsync(long orderId)
    {
        return _context.OnlinePayments
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.Attempt)
            .FirstOrDefaultAsync();
    }

    /// <summary>Hỏi gateway về lần thanh toán online mới nhất của đơn nếu nó chưa kết thúc. Gateway lỗi thì bỏ qua, lần hỏi sau sẽ thử lại.</summary>
    private async Task SyncLatestOnlinePaymentAsync(Order order)
    {
        var latest = await GetLatestOnlinePaymentAsync(order.Id);
        if (latest != null && latest.IsOpen)
        {
            await SyncOnlinePaymentAsync(order, latest, throwOnError: false);
        }
    }

    /// <summary>Trả payment gateway vừa hỏi được (kèm thông tin hiển thị QR); null khi gateway chưa có payment hoặc lỗi tạm thời.</summary>
    private async Task<GatewayPayment?> SyncOnlinePaymentAsync(Order order, OnlinePayment payment, bool throwOnError)
    {
        try
        {
            var gatewayPayment = await _gateway.GetStatus(order.Id);
            if (gatewayPayment != null)
            {
                await ApplyGatewayPaymentAsync(order, payment, gatewayPayment);
            }
            return gatewayPayment;
        }
        catch (PaymentGatewayException ex) when (!throwOnError)
        {
            _logger.LogWarning("Không hỏi được gateway cho đơn {OrderId}: {Code} traceId={TraceId}", order.Id, ex.Code, ex.TraceId);
            return null;
        }
    }

    /// <summary>Link PayOS đã quá hạn mà gateway còn ghi PENDING: huỷ ở gateway để khách tạo được link mới. Không huỷ được thì trả null.</summary>
    private async Task<GatewayPayment?> TryCancelExpiredAsync(Order order, OnlinePayment payment)
    {
        try
        {
            var cancelled = await _gateway.Cancel(order.Id, "Link thanh toán đã hết hạn");
            await ApplyGatewayPaymentAsync(order, payment, cancelled);
            return cancelled;
        }
        catch (PaymentGatewayException ex)
        {
            _logger.LogInformation("Không huỷ được link hết hạn của đơn {OrderId}: {Code}", order.Id, ex.Code);
            return null;
        }
    }

    /// <summary>Ghi kết quả gateway vào lần thanh toán; thấy PAID thì đánh dấu đơn đã trả (idempotent).</summary>
    private async Task ApplyGatewayPaymentAsync(Order order, OnlinePayment payment, GatewayPayment gatewayPayment)
    {
        payment.Apply(gatewayPayment.PaymentId, gatewayPayment.Provider, gatewayPayment.Status, gatewayPayment.CheckoutUrl, gatewayPayment.ExpiresAt?.UtcDateTime);

        if (gatewayPayment.Status == OnlinePaymentStatuses.Paid)
        {
            if (order.IsCancelled)
            {
                _logger.LogError("Đơn {OrderId} đã huỷ nhưng gateway báo đã nhận tiền (payment {PaymentId}), cần hoàn tiền thủ công", order.Id, gatewayPayment.PaymentId);
            }
            await MarkOrderPaid(order.Id, new PaymentConfirmation(
                (long)gatewayPayment.TotalAmount,
                PaymentProviders.DisplayName(gatewayPayment.Provider),
                gatewayPayment.TransactionId,
                gatewayPayment.PaymentId.ToString(),
                $"Thanh toán online qua gateway (payment {gatewayPayment.PaymentId})",
                gatewayPayment.PaidAt?.UtcDateTime));
        }
        else
        {
            if (gatewayPayment.Status == OnlinePaymentStatuses.ReviewRequired)
            {
                _logger.LogError("Thanh toán của đơn {OrderId} cần xử lý thủ công (REVIEW_REQUIRED, payment {PaymentId})", order.Id, gatewayPayment.PaymentId);
            }
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>Huỷ link thanh toán còn mở của đơn đã huỷ. Gateway không huỷ được (VNPay, khách vừa trả tiền, lỗi mạng…) thì chỉ ghi log.</summary>
    private async Task CancelOnlinePaymentQuietlyAsync(long orderId)
    {
        var open = await _context.OnlinePayments
            .Where(p => p.OrderId == orderId && OnlinePaymentStatuses.Open.Contains(p.Status))
            .ToListAsync();
        if (open.Count == 0) return;

        try
        {
            await _gateway.Cancel(orderId, "Đơn hàng đã bị hủy");
            foreach (var payment in open)
            {
                payment.Cancel();
            }
            await _context.SaveChangesAsync();
        }
        catch (PaymentGatewayException ex)
        {
            _logger.LogWarning("Không huỷ được link thanh toán của đơn {OrderId}: {Code} traceId={TraceId}", orderId, ex.Code, ex.TraceId);
        }
    }

    /// <summary>Thông tin QR/tài khoản chỉ lấy từ gateway ở lần hỏi này (không lưu DB) và chỉ khi khách còn có thể thanh toán.</summary>
    private static OnlinePaymentView ToView(OnlinePayment payment, GatewayPayment? live)
    {
        var canPay = payment.CanPay;
        var info = canPay && live?.PaymentInfo is { } i
            ? new OnlinePaymentInfo(i.QrCode, i.BankBin, i.AccountNumber, i.AccountName, i.TransferContent)
            : null;
        return new OnlinePaymentView(
            payment.Id,
            payment.OrderId,
            payment.Amount,
            payment.Status,
            payment.Provider,
            canPay ? payment.CheckoutUrl : null,
            canPay ? payment.ExpiresAt : null,
            info);
    }

    private static AppException ToAppException(PaymentGatewayException ex)
    {
        return ex.Code switch
        {
            GatewayErrorCodes.PaymentProviderNotEnabled or GatewayErrorCodes.PaymentProviderNotAllowed
                => new BadRequestException("Cổng thanh toán này hiện chưa khả dụng, vui lòng chọn cổng khác."),
            GatewayErrorCodes.InvalidAmount or GatewayErrorCodes.InvalidPaymentRequest
                => new BadRequestException("Số tiền đơn hàng không phù hợp với cổng thanh toán này."),
            GatewayErrorCodes.PaymentAlreadyPending
                => new ConflictException("Đơn hàng đang có một giao dịch chờ thanh toán."),
            _ when ex.Status == 0 || ex.Status >= 500
                => new InternalServerException("Cổng thanh toán đang bận, vui lòng thử lại sau ít phút."),
            _ => new InternalServerException("Không tạo được giao dịch thanh toán, vui lòng thử lại hoặc chọn phương thức khác.")
        };
    }

    #endregion

    /// <summary>
    /// Hàm lõi dùng chung để chia kho cho cả Pickup và Delivery, loại bỏ lặp code.
    /// </summary>
    private async Task<List<OrderDetail>> AllocateInventoryCoreAsync(
        List<CartDetail> cartDetails,
        List<StoreInventoryDTO> storeInventory,
        Func<StoreInventoryDTO, bool> primarySortCondition,
        string userId)
    {
        var finalOrderDetails = new List<OrderDetail>();
        var modifiedStores = new HashSet<StoreInventoryDTO>();

        foreach (var cartItem in cartDetails)
        {
            var candidateStores = storeInventory
                .Where(s => s.ProductId == cartItem.ProductCartDetailId && s.ColorId == cartItem.ColorId)
                .OrderByDescending(primarySortCondition) // Động: Tỉnh thành (Delivery) hoặc ID Kho (Pickup)
                .ThenByDescending(s => s.Quantity)
                .ToList();

            int remainingQty = cartItem.Quantity;

            foreach (var store in candidateStores)
            {
                if (remainingQty <= 0) break;

                int takeQty = Math.Min(store.Quantity, remainingQty);

                finalOrderDetails.Add(new OrderDetail
                {
                    ProductOrderDetailId = cartItem.ProductCartDetailId,
                    ColorId = cartItem.ColorId,
                    Quantity = takeQty,
                    StoreHouseId = store.StoreHouseId,
                    Price = cartItem.Product!.SalePrice,
                    CreateBy = userId,
                    CreateDate = DateTime.UtcNow,
                    Product = cartItem.Product,
                    Color = cartItem.Color,
                    UpdateBy = userId,
                    UpdateDate = DateTime.UtcNow
                });

                remainingQty -= takeQty;
                store.Quantity -= takeQty;
                modifiedStores.Add(store);
            }

            if (remainingQty > 0)
                throw new BadRequestException($"Sản phẩm {cartItem.Product?.Name} màu {cartItem.Color?.Name} hiện không đủ số lượng!");
        }

        if (modifiedStores.Any())
        {
            await _storeService.UpdateStores(modifiedStores.ToList());
        }

        return finalOrderDetails;
    }

    /// <summary>Tải đơn kèm chi tiết (FindAsync không nạp OrderDetails nên không dùng được cho hoàn kho / xoá giỏ).</summary>
    private Task<Order?> GetOrderWithDetailsAsync(long orderId)
    {
        return _context.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == orderId);
    }

    private async Task RemoveOrderedCartItemsAsync(Order order)
    {
        var ordered = order.OrderDetails
            .Select(od => (ProductId: od.ProductOrderDetailId, od.ColorId))
            .ToHashSet();
        if (ordered.Count == 0) return;

        var productIds = ordered.Select(o => o.ProductId).Distinct().ToList();
        var candidates = await _context.CartDetails
            .Where(c => c.CreateBy == order.CreateBy && productIds.Contains(c.ProductCartDetailId))
            .ToListAsync();
        var toRemove = candidates.Where(c => ordered.Contains((c.ProductCartDetailId, c.ColorId))).ToList();
        if (toRemove.Count > 0)
        {
            await _cartDetailService.DeleteRangeAsync(toRemove);
        }
    }

    private async Task<Order> SaveOrderAsync(string userId, string orderType, List<OrderDetail> orderDetails, long? shippingFee)
    {
        var order = Order.Create(userId, orderType, orderDetails, Fee.ForShipping(shippingFee));

        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();

        return order;
    }

    private OrderView MapToOrderView(Order order)
    {
        return new OrderView
        {
            Id = order.Id,
            CreateDate = order.CreateDate,
            OrderDetails = order.OrderDetails.Select(od => new OrderDetailView(
                new ProductView(
                    od.Product.Id,
                    od.Product.ImageUrl,
                    od.Product.Name,
                    od.Product.BasePrice,
                    od.Product.SalePrice
                ),
                od.ColorId,
                od.Color?.Name,
                od.Color?.Image?.BlobUrl,
                od.Quantity
            )).ToList(),
            Fee = order.Fee?.FeeDetails?.Select(fd => new FeeView { Name = fd.Name, Value = fd.Value }).ToList() ?? new List<FeeView>()
        };
    }

    private async Task<long> CalculateDeliveryShippingFeeAsync(List<OrderDetail> orderDetails, List<StoreInventoryDTO> allPotentialStores, string destProvince, string destDistrict)
    {
        int defaultWeight = 500;

        var packagesToShip = orderDetails
            .GroupBy(od => od.StoreHouseId)
            .Select(group =>
            {
                var storeInfo = allPotentialStores.First(s => s.StoreHouseId == group.Key);
                int totalWeight = group.Sum(od => od.Quantity) * defaultWeight;
                return new PackageToShip(storeInfo.City, storeInfo.District, totalWeight);
            })
            .ToList();

        return await _shippingFeeService.CalculateAccurateShippingFee(packagesToShip, destProvince, destDistrict);
    }

    private async Task RestoreInventoryAsync(List<OrderDetail> orderDetails)
    {
        if (orderDetails == null || !orderDetails.Any()) return;

        const int chunkSize = 50; // Mỗi đợt ta chỉ gửi 50 câu lệnh UPDATE

        for (int i = 0; i < orderDetails.Count; i += chunkSize)
        {
            // Cắt ra một "lát" (chunk) gồm tối đa 50 phần tử
            var currentChunk = orderDetails.Skip(i).Take(chunkSize).ToList();

            var sqlBatch = new StringBuilder();
            var currentTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            foreach (var od in currentChunk)
            {
                sqlBatch.Append($@"
                    UPDATE ""Stores"" 
                    SET ""Quantity"" = ""Quantity"" + {od.Quantity}, 
                        ""UpdateDate"" = '{currentTime}'
                    WHERE ""StoreHouseId"" = {od.StoreHouseId} 
                      AND ""ProductId"" = {od.ProductOrderDetailId} 
                      AND ""ColorId"" = {od.ColorId};
                ");
            }

            // Gửi từng đợt xuống Database
            await _context.Database.ExecuteSqlRawAsync(sqlBatch.ToString());
        }
    }

    private async Task<bool> ProcessRefundIfNecessaryAsync(Order order)
    {
        return true;
    }
    #endregion

    #region NOT IMPLEMENTED YET
    public Task<string> GetOrderById(string orderId)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetOrderByUserId(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetOrders(int page, int pageSize)
    {
        throw new NotImplementedException();
    }
    #endregion
}