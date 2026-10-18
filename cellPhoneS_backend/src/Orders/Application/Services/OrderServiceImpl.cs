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
            await SyncOrderPaymentAsync(order);
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

        // Gateway là nguồn sự thật: còn link đang chờ thì dùng lại, kể cả khi khách đổi cổng (gateway chỉ cho một payment hoạt động mỗi đơn).
        GatewayPayment? live;
        try
        {
            live = await SyncOrderPaymentAsync(order, throwOnError: true);
            if (live is { Status: GatewayPaymentStatuses.Pending, ExpiresAt: { } expiresAt } && expiresAt <= DateTimeOffset.UtcNow)
            {
                // Link đã hết hạn nhưng gateway chưa đối soát: huỷ để tạo được link mới (VNPay không huỷ được thì giữ nguyên).
                live = await TryCancelExpiredAsync(order) ?? live;
            }
        }
        catch (PaymentGatewayException ex)
        {
            throw ToAppException(ex);
        }
        if (live != null)
        {
            if (live.Status is GatewayPaymentStatuses.Paid or GatewayPaymentStatuses.ReviewRequired || CanPay(live))
            {
                return ToView(live);
            }
            // Lần trước đã kết thúc không thành công: lần này phải dùng Idempotency-Key mới.
            if (GatewayPaymentStatuses.IsFailedTerminal(live.Status))
            {
                order.NextPaymentAttempt();
            }
        }

        // Chỉ để thử thanh toán thật với số tiền nhỏ: đặt PaymentGateway:TestAmountOverride (user-secrets) thì mọi đơn bị tính số tiền này.
        // Bỏ cấu hình đó là về lại tiền thật của đơn.
        var total = order.GetTotalAmount();
        var amount = _configuration.GetValue<long?>(PaymentConfigKeys.TestAmountOverride) ?? total;
        if (amount != total)
        {
            _logger.LogWarning("TestAmountOverride đang bật: đơn {OrderId} thanh toán {Amount} thay vì {Real}", orderId, amount, total);
        }

        // Số lần thử phải nằm trong DB trước khi gọi gateway để mọi lần thử lại sau lỗi mạng dùng đúng Idempotency-Key này.
        order.StartPaymentAttempt();
        await _context.SaveChangesAsync();

        GatewayPayment gatewayPayment;
        try
        {
            gatewayPayment = await _gateway.Checkout(orderId, amount, provider, order.PaymentIdempotencyKey);
            await ApplyGatewayStatusAsync(order, gatewayPayment);
        }
        catch (PaymentGatewayException ex) when (ex.Code == GatewayErrorCodes.PaymentAlreadyPending)
        {
            try
            {
                gatewayPayment = await _gateway.GetStatus(orderId) ?? throw ToAppException(ex);
                await ApplyGatewayStatusAsync(order, gatewayPayment);
            }
            catch (PaymentGatewayException inner)
            {
                throw ToAppException(inner);
            }
        }
        catch (PaymentGatewayException ex)
        {
            // Lỗi 4xx là dứt khoát: lần thử sau phải dùng key mới. Lỗi mạng/5xx (và "đang xử lý") giữ nguyên số lần để thử lại cùng key.
            if (ex.Status is >= 400 and < 500 && ex.Code != GatewayErrorCodes.IdempotencyInProgress)
            {
                order.NextPaymentAttempt();
                await _context.SaveChangesAsync();
            }
            throw ToAppException(ex);
        }
        return ToView(gatewayPayment);
    }

    public async Task<OnlinePaymentView> GetOnlinePayment(string userId, long orderId)
    {
        var order = await GetOrderWithDetailsAsync(orderId);
        if (order == null) throw new NotFoundException("Không tìm thấy đơn hàng.");
        if (!order.IsOwnedBy(userId)) throw new ForbiddenException("Bạn không có quyền xem giao dịch này.");

        GatewayPayment? payment;
        try
        {
            payment = await SyncOrderPaymentAsync(order, throwOnError: true);
        }
        catch (PaymentGatewayException ex)
        {
            throw ToAppException(ex);
        }
        if (payment == null) throw new NotFoundException("Không tìm thấy giao dịch thanh toán.");
        return ToView(payment);
    }

    public async Task SyncPendingOnlinePayments()
    {
        var since = DateTime.UtcNow.AddHours(-24);
        var orderIds = await _context.Orders
            .Where(o => o.Status == OrderStatuses.Pending && o.PaymentAttempt > 0 && o.UpdateDate > since)
            .Select(o => o.Id)
            .ToListAsync();

        foreach (var orderId in orderIds)
        {
            try
            {
                var order = await GetOrderWithDetailsAsync(orderId);
                if (order != null && order.IsPending)
                {
                    await SyncOrderPaymentAsync(order);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đồng bộ thanh toán online của đơn {OrderId} lỗi", orderId);
            }
        }
    }

    public async Task HandlePaymentCallback(long orderId)
    {
        var order = await GetOrderWithDetailsAsync(orderId);
        if (order == null)
        {
            // Gateway sẽ thử lại tới 10 lần, nhưng đơn không tồn tại thì thử lại cũng vô ích: ghi log và coi như đã nhận.
            _logger.LogWarning("Callback thanh toán cho đơn {OrderId} không tồn tại", orderId);
            return;
        }

        try
        {
            // Lỗi gọi gateway phải trả 5xx để gateway gửi lại callback.
            await SyncOrderPaymentAsync(order, throwOnError: true);
        }
        catch (PaymentGatewayException ex)
        {
            throw ToAppException(ex);
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

        await CancelWithRestockAsync(order, userId);
    }

    public async Task<string> AdminCancelOrder(long orderId, string adminId)
    {
        var order = await GetOrderWithDetailsAsync(orderId);
        if (order == null) throw new NotFoundException("Không tìm thấy đơn hàng.");

        if (!order.CanBeCancelled)
            throw new ConflictException($"Không thể hủy đơn hàng đang ở trạng thái: {order.Status}");

        var previousStatus = order.Status;
        await CancelWithRestockAsync(order, adminId);
        return previousStatus;
    }

    #endregion

    #region PRIVATE HELPERS (Logic nền tảng)

    #region Online payment (payment gateway)

    /// <summary>
    /// Hỏi gateway (nguồn sự thật) về payment của đơn rồi áp dụng kết quả. Trả payment vừa hỏi được; null khi đơn chưa từng thử thanh toán online,
    /// gateway chưa có payment, hoặc (khi <paramref name="throwOnError"/> = false) gateway lỗi tạm thời, lần hỏi sau sẽ thử lại.
    /// </summary>
    private async Task<GatewayPayment?> SyncOrderPaymentAsync(Order order, bool throwOnError = false)
    {
        if (order.PaymentAttempt == 0) return null;
        try
        {
            var gatewayPayment = await _gateway.GetStatus(order.Id);
            if (gatewayPayment != null)
            {
                await ApplyGatewayStatusAsync(order, gatewayPayment);
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
    private async Task<GatewayPayment?> TryCancelExpiredAsync(Order order)
    {
        try
        {
            var cancelled = await _gateway.Cancel(order.Id, "Link thanh toán đã hết hạn");
            await ApplyGatewayStatusAsync(order, cancelled);
            return cancelled;
        }
        catch (PaymentGatewayException ex)
        {
            _logger.LogInformation("Không huỷ được link hết hạn của đơn {OrderId}: {Code}", order.Id, ex.Code);
            return null;
        }
    }

    /// <summary>Áp dụng trạng thái gateway báo: PAID thì đánh dấu đơn đã trả (idempotent), REVIEW_REQUIRED thì báo cần xử lý thủ công. Không lưu trạng thái payment ở CellphoneS.</summary>
    private async Task ApplyGatewayStatusAsync(Order order, GatewayPayment gatewayPayment)
    {
        if (gatewayPayment.Status == GatewayPaymentStatuses.Paid)
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
        else if (gatewayPayment.Status == GatewayPaymentStatuses.ReviewRequired)
        {
            _logger.LogError("Thanh toán của đơn {OrderId} cần xử lý thủ công (REVIEW_REQUIRED, payment {PaymentId})", order.Id, gatewayPayment.PaymentId);
        }
    }

    /// <summary>Huỷ link thanh toán còn mở của đơn đã huỷ. Gateway không huỷ được (VNPay, khách vừa trả tiền, chưa có payment, lỗi mạng…) thì chỉ ghi log.</summary>
    private async Task CancelOnlinePaymentQuietlyAsync(Order order)
    {
        if (order.PaymentAttempt == 0) return;

        try
        {
            await _gateway.Cancel(order.Id, "Đơn hàng đã bị hủy");
        }
        catch (PaymentGatewayException ex) when (ex.Code is GatewayErrorCodes.PaymentNotFound or GatewayErrorCodes.PaymentNotCancellable)
        {
            _logger.LogInformation("Không có link thanh toán nào cần huỷ cho đơn {OrderId}: {Code}", order.Id, ex.Code);
        }
        catch (PaymentGatewayException ex)
        {
            _logger.LogWarning("Không huỷ được link thanh toán của đơn {OrderId}: {Code} traceId={TraceId}", order.Id, ex.Code, ex.TraceId);
        }
    }

    /// <summary>Khách còn thanh toán được: gateway báo PENDING và đã có link.</summary>
    private static bool CanPay(GatewayPayment payment) =>
        payment.Status == GatewayPaymentStatuses.Pending && payment.CheckoutUrl != null;

    /// <summary>Thông tin QR/tài khoản chỉ lấy từ gateway ở lần hỏi này (không lưu DB) và chỉ khi khách còn có thể thanh toán.</summary>
    private static OnlinePaymentView ToView(GatewayPayment payment)
    {
        var canPay = CanPay(payment);
        var info = canPay && payment.PaymentInfo is { } i
            ? new OnlinePaymentInfo(i.QrCode, i.BankBin, i.AccountNumber, i.AccountName, i.TransferContent)
            : null;
        return new OnlinePaymentView(
            payment.OrderId,
            (long)payment.TotalAmount,
            payment.Status,
            payment.Provider,
            canPay ? payment.CheckoutUrl : null,
            canPay ? payment.ExpiresAt?.UtcDateTime : null,
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

    /// <summary>
    /// Huỷ đơn và hoàn tồn kho trong cùng một transaction; dùng chung cho khách huỷ (<see cref="DeleteOrder"/>) và admin huỷ.
    /// Trạng thái đơn được "chiếm" bằng một UPDATE có điều kiện (Status còn như lúc đọc), nên hai yêu cầu huỷ đồng thời
    /// chỉ có một yêu cầu thắng và kho chỉ được hoàn một lần; yêu cầu thua nhận 409.
    /// </summary>
    private async Task CancelWithRestockAsync(Order order, string actorId)
    {
        var previousStatus = order.Status;
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            if (!await ProcessRefundIfNecessaryAsync(order))
            {
                throw new Exception("Quá trình hoàn tiền thất bại. Hệ thống tự động hủy thao tác.");
            }

            order.Cancel(actorId); // luật trạng thái nằm trên entity (ném InvalidOperationException nếu sai)

            var claimed = await _context.Orders
                .Where(o => o.Id == order.Id && o.Status == previousStatus)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(o => o.Status, OrderStatuses.Cancelled)
                    .SetProperty(o => o.UpdateDate, order.UpdateDate)
                    .SetProperty(o => o.UpdateBy, actorId));
            // Entity trong bộ nhớ đã mang đúng giá trị vừa ghi nên không cần SaveChanges lần nữa.
            _context.Entry(order).State = EntityState.Unchanged;
            if (claimed == 0)
                throw new ConflictException("Trạng thái đơn hàng vừa thay đổi, vui lòng tải lại và thử lại.");

            await RestoreInventoryAsync(order.OrderDetails, actorId);
            await transaction.CommitAsync();
        }
        catch (AppException)
        {
            await transaction.RollbackAsync();
            throw;
        }
        catch (InvalidOperationException ex)
        {
            await transaction.RollbackAsync();
            throw new ConflictException(ex.Message);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new InternalServerException($"Lỗi khi hủy đơn hàng: {ex.Message}");
        }

        // Sau khi đơn đã huỷ thì link thanh toán cũ không được phép nhận tiền nữa.
        await CancelOnlinePaymentQuietlyAsync(order);
    }

    /// <summary>
    /// Cộng lại <c>Store.Quantity</c> theo từng dòng đơn (kho × sản phẩm × màu), bằng UPDATE cộng dồn nguyên tử để không
    /// đè lên đặt hàng / điều chỉnh tồn diễn ra cùng lúc. Phải chạy trong transaction của lần huỷ.
    /// </summary>
    private async Task RestoreInventoryAsync(IEnumerable<OrderDetail> orderDetails, string actorId)
    {
        var now = DateTime.UtcNow;
        var groups = orderDetails.GroupBy(d => new { d.StoreHouseId, ProductId = d.ProductOrderDetailId, d.ColorId });
        foreach (var group in groups)
        {
            var key = group.Key;
            var quantity = group.Sum(d => d.Quantity);
            var updated = await _context.Stores
                .Where(s => s.StoreHouseId == key.StoreHouseId && s.ProductId == key.ProductId && s.ColorId == key.ColorId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Quantity, x => x.Quantity + quantity)
                    .SetProperty(x => x.UpdateDate, now)
                    .SetProperty(x => x.UpdateBy, actorId));
            if (updated == 0)
            {
                _logger.LogWarning("Không hoàn được tồn kho {Quantity} cho kho {StoreHouseId}, sản phẩm {ProductId}, màu {ColorId}: không còn dòng tồn kho",
                    quantity, key.StoreHouseId, key.ProductId, key.ColorId);
            }
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