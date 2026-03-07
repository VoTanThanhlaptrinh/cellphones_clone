using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cellphones_backend.Data;
using cellphones_backend.Models;
using cellphones_backend.Repositories;
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

    #region Constants
    private static class OrderStatuses
    {
        public const string Pending = "Pending";
        public const string Paid = "Paid";
        public const string Completed = "Completed";
        public const string Shipping = "Shipping";
        public const string Cancelled = "Cancelled";
    }

    private static class OrderTypes
    {
        public const string Delivery = "delivery";
        public const string Pickup = "pickup";
    }
    #endregion

    public OrderServiceImpl(
        IOrderRepository orderRepository,
        CartDetailService cartDetailService,
        IOrderDetailRepository orderDetailRepository,
        ShippingFeeService shippingFeeService,
        IStoreService storeService,
        ApplicationDbContext context)
    {
        _orderRepository = orderRepository;
        _cartDetailService = cartDetailService;
        _orderDetailRepository = orderDetailRepository;
        _shippingFeeService = shippingFeeService;
        _storeService = storeService;
        _context = context;
    }

    #region PUBLIC APIs (Nghiệp vụ chính)

    public async Task<ServiceResult<string>> Checkout(string userId, long orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return ServiceResult<string>.Fail("Order not found");
        if (order.CreateBy != userId) return ServiceResult<string>.Fail("User is not authorized to checkout this order");

        order.Status = OrderStatuses.Paid;
        await _orderRepository.UpdateAsync(order);
        await _orderRepository.SaveChangesAsync();

        return ServiceResult<string>.Success("", "Order checked out successfully");
    }

    public async Task<ServiceResult<OrderView>> CreateDeliveryOrder(string userId, DeliveryOrderRequest payload)
    {
        if (payload.CartDetailIds == null || !payload.CartDetailIds.Any())
            return ServiceResult<OrderView>.Fail("Cart item IDs cannot be null or empty");

        var cartDetails = await _cartDetailService.GetCartDetails(userId, payload.CartDetailIds);
        if (cartDetails == null || !cartDetails.Any())
            return ServiceResult<OrderView>.Fail("No cart items found for the user");

        var allPotentialStores = await _storeService.AllocateAllStockAsync(cartDetails);
        if (allPotentialStores == null || !allPotentialStores.Any())
            throw new ArgumentException("No store found containing these products.");

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

            await _cartDetailService.DeleteRangeAsync(cartDetails); // IsHardDelete = true bên trong
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return ServiceResult<OrderView>.Success(MapToOrderView(order), "Delivery order created successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return ServiceResult<OrderView>.Fail($"Failed to create delivery order: {ex.Message}");
        }
    }

    public async Task<ServiceResult<OrderView>> CreatePickupOrder(string userId, PickupOrderRequest payload)
    {
        if (payload.CartDetailIds == null || !payload.CartDetailIds.Any())
            return ServiceResult<OrderView>.Fail("Cart item IDs cannot be null or empty");

        var cartDetails = await _cartDetailService.GetCartDetails(userId, payload.CartDetailIds);
        if (cartDetails == null || !cartDetails.Any())
            return ServiceResult<OrderView>.Fail("No cart items found for the user");

        var storeInventory = await _storeService.AllocateAllStockAsync(cartDetails);
        if (storeInventory == null || !storeInventory.Any())
            throw new ArgumentException("Cửa hàng này hiện không có sẵn các sản phẩm trong giỏ.");

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

            await _cartDetailService.DeleteRangeAsync(cartDetails);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return ServiceResult<OrderView>.Success(MapToOrderView(order), "Pickup order created successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return ServiceResult<OrderView>.Fail($"Failed to create pickup order: {ex.Message}");
        }
    }

    public async Task<ServiceResult<string>> DeleteOrder(string userId, long orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return ServiceResult<string>.Fail("Không tìm thấy đơn hàng.");
        if (order.CreateBy != userId) return ServiceResult<string>.Fail("Bạn không có quyền hủy đơn hàng này.");

        if (order.Status == OrderStatuses.Completed || order.Status == OrderStatuses.Cancelled)
            return ServiceResult<string>.Fail($"Không thể hủy đơn hàng đang ở trạng thái: {order.Status}");

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

            order.Status = OrderStatuses.Cancelled;
            order.UpdateDate = DateTime.UtcNow;
            order.UpdateBy = userId;

            await _orderRepository.UpdateAsync(order);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return ServiceResult<string>.Success("", "Hủy đơn hàng thành công.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return ServiceResult<string>.Fail($"Lỗi khi hủy đơn hàng: {ex.Message}");
        }
    }

    #endregion

    #region PRIVATE HELPERS (Logic nền tảng)

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
                throw new Exception($"Sản phẩm {cartItem.Product?.Name} màu {cartItem.Color?.Name} hiện không đủ số lượng!");
        }

        if (modifiedStores.Any())
        {
            await _storeService.UpdateStores(modifiedStores.ToList());
        }

        return finalOrderDetails;
    }

    private async Task<Order> SaveOrderAsync(string userId, string orderType, List<OrderDetail> orderDetails, long? shippingFee)
    {
        Fee fee = null;
        if (shippingFee.HasValue && shippingFee.Value > 0)
        {
            fee = new Fee
            {
                FeeDetails = new List<FeeDetail>
                {
                    new FeeDetail { Value = shippingFee.Value, Name = "Shipping fee calculated by GHTK", Status = "active", CreateDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow }
                }
            };
        }

        var order = new Order
        {
            CreateBy = userId,
            OrderDetails = orderDetails,
            CreateDate = DateTime.UtcNow,
            Type = orderType,
            Fee = fee,
            Status = OrderStatuses.Pending,
            UpdateBy = userId,
            UpdateDate = DateTime.UtcNow
        };

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
                    od.Product.Name,
                    od.Product.ImageUrl,
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
    public Task<ServiceResult<string>> GetOrderById(string orderId)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<string>> GetOrderByUserId(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<string>> GetOrders(int page, int pageSize)
    {
        throw new NotImplementedException();
    }
    #endregion
}