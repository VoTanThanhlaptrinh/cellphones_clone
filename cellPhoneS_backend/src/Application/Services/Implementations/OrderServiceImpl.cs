using System;
using cellphones_backend.Data;
using cellphones_backend.Models;
using cellphones_backend.Repositories;
using cellPhoneS_backend.DTOs;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;
using cellPhoneS_backend.Models;
using cellPhoneS_backend.Services.Interface;

namespace cellPhoneS_backend.Services.Implement;

public class OrderServiceImpl : OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly CartDetailService _cartDetailService;
    private readonly IOrderDetailRepository _orderDetailRepository;
    private readonly ShippingFeeService _shippingFeeService;
    private readonly IStoreService _storeService;
    private readonly ApplicationDbContext _context;
    public OrderServiceImpl(IOrderRepository orderRepository, CartDetailService cartDetailService, IOrderDetailRepository orderDetailRepository, ShippingFeeService shippingFeeService, IStoreService storeService, ApplicationDbContext context)
    {
        _orderRepository = orderRepository;
        _cartDetailService = cartDetailService;
        _orderDetailRepository = orderDetailRepository;
        _shippingFeeService = shippingFeeService;
        _storeService = storeService;
        _context = context;
    }

    public async Task<ServiceResult<string>> Checkout(string userId, long orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            return ServiceResult<string>.Fail("Order not found");
        }
        if (order.CreateBy != userId)
        {
            return ServiceResult<string>.Fail("User is not authorized to checkout this order");
        }
        order.Status = "Completed";
        await _orderRepository.UpdateAsync(order);
        await _orderRepository.SaveChangesAsync();
        return ServiceResult<string>.Success("", "Order checked out successfully");
    }

    public async Task<ServiceResult<OrderView>> CreateDeliveryOrder(string userId, DeliveryOrderRequest payload)
    {
        // 1. Validate payload
        if (payload.CartDetailIds == null || !payload.CartDetailIds.Any())
            return ServiceResult<OrderView>.Fail("Cart item IDs cannot be null or empty");

        var cartDetails = await _cartDetailService.GetCartDetails(userId, payload.CartDetailIds);
        if (cartDetails == null || !cartDetails.Any())
            return ServiceResult<OrderView>.Fail("No cart items found for the user");

        // 2. Fetch inventory data
        var allPotentialStores = await _storeService.AllocateAllStockAsync(cartDetails, payload.ProvinceName);
        if (allPotentialStores == null || !allPotentialStores.Any())
            throw new ArgumentException("No store found containing these products.");

        // 3. Allocate inventory to optimal stores
        var finalOrderDetails = AllocateDeliveryInventory(cartDetails, allPotentialStores, payload.ProvinceName, userId);

        // 4. Calculate shipping fee
        long totalShippingFee = await CalculateDeliveryShippingFeeAsync(finalOrderDetails, allPotentialStores, payload.ProvinceName, payload.DistrictName);

        // 5. Begin DB Transaction
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 5.1. Save Order and Details
            var order = await SaveOrderAsync(userId, "delivery", finalOrderDetails, totalShippingFee);

            // 5.2. Clear purchased items from cart
            await _cartDetailService.DeleteRangeAsync(cartDetails);
            await _context.SaveChangesAsync();

            // 5.3. Commit transaction
            await transaction.CommitAsync();

            // 6. Map to View
            var orderView = MapToOrderView(order);

            return ServiceResult<OrderView>.Success(orderView, "Delivery order created successfully");
        }
        catch (Exception ex)
        {
            // Rollback on failure
            await transaction.RollbackAsync();

            // TODO: Add _logger.LogError here
            return ServiceResult<OrderView>.Fail($"Failed to create delivery order: {ex.Message}");
        }
    }

    public async Task<ServiceResult<OrderView>> CreatePickupOrder(string userId, PickupOrderRequest payload)
    {
        // 1. Validate payload
        if (payload.CartDetailIds == null || !payload.CartDetailIds.Any())
            return ServiceResult<OrderView>.Fail("Cart item IDs cannot be null or empty");

        // 2. Fetch cart details
        var cartDetails = await _cartDetailService.GetCartDetails(userId, payload.CartDetailIds);
        if (cartDetails == null || !cartDetails.Any())
            return ServiceResult<OrderView>.Fail("No cart items found for the user");

        // 3. Gọi StoreService để lấy Tồn kho của cửa hàng này (Giao tiếp giữa các Service)
        // Cần tạo hàm này bên StoreService để query trực tiếp theo danh sách ProductId và StoreHouseId
        var storeInventory = await _storeService.GetInventoryForStoreAsync(cartDetails, payload.StoreHouseId);

        if (storeInventory == null || !storeInventory.Any())
            throw new ArgumentException("Cửa hàng này hiện không có sẵn các sản phẩm trong giỏ.");

        // 4. Begin DB Transaction
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 5. Allocate inventory (Xử lý hoàn toàn trong RAM)
            var finalOrderDetails = AllocatePickupInventory(cartDetails, storeInventory, payload.StoreHouseId, userId);

            // 6. Save Order and Details (Fee = 0)
            var order = await SaveOrderAsync(userId, "pickup", finalOrderDetails, 0);

            // 7. Clear purchased items from cart
            await _cartDetailService.DeleteRangeAsync(cartDetails);

            // 8. Lưu tất cả thay đổi (Lúc này EF Core sẽ tracking list storeInventory và update CSDL)
            await _context.SaveChangesAsync();

            // 9. Commit transaction
            await transaction.CommitAsync();

            // 10. Map to View
            var orderView = MapToOrderView(order);

            return ServiceResult<OrderView>.Success(orderView, "Pickup order created successfully");
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
        if (order == null)
        {
            return ServiceResult<string>.Fail("Order not found");
        }
        if (order.CreateBy != userId)
        {
            return ServiceResult<string>.Fail("User is not authorized to delete this order");
        }
        await _orderRepository.RemoveAsync(order);
        await _orderRepository.SaveChangesAsync();
        return ServiceResult<string>.Success("", "Order deleted successfully");
    }

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
    // 1. Hàm lưu Database dùng chung
    private async Task<Order> SaveOrderAsync(string userId, string orderType, List<OrderDetail> orderDetails, long? shippingFee)
    {
        Fee fee = null;
        if (shippingFee.HasValue && shippingFee.Value > 0)
        {
            fee = new Fee
            {
                FeeDetails = new List<FeeDetail>
            {
                new FeeDetail { Value = shippingFee.Value, Name = "Shipping fee calculated by GHTK" }
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
            Status = "Pending",
            UpdateBy = userId,
            UpdateDate = DateTime.UtcNow
        };

        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();

        return order;
    }

    // 2. Hàm Map ra View dùng chung
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
            Fee = order.Fee?.FeeDetails?.Select(fd => new FeeView { Name = fd.Name, Value = fd.Value }).ToList()
                  ?? new List<FeeView>()
        };
    }
    // 3. Logic chia kho gom hàng (Chỉ dành cho Delivery)
    private List<OrderDetail> AllocateDeliveryInventory(List<CartDetail> cartDetails, List<Store> allPotentialStores, string provinceName, string userId)
    {
        var finalOrderDetails = new List<OrderDetail>();

        foreach (var cartItem in cartDetails)
        {
            var candidateStores = allPotentialStores
                .Where(s => s.ProductId == cartItem.ProductCartDetailId && s.ColorId == cartItem.ColorId)
                .OrderByDescending(s => s.StoreHouse?.City == provinceName)
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
                    Price = cartItem.Product.SalePrice,
                    CreateBy = userId,
                    CreateDate = DateTime.UtcNow,
                    // QUAN TRỌNG: Phải gán 2 Object này để lát nữa hàm MapToOrderView không bị lỗi NullReference
                    Product = cartItem.Product,
                    Color = cartItem.Color
                });

                remainingQty -= takeQty;
                store.Quantity -= takeQty;
            }

            if (remainingQty > 0)
            {
                throw new Exception($"Sản phẩm {cartItem.Product.Name} màu {cartItem.Color?.Name} không đủ hàng!");
            }
        }

        return finalOrderDetails;
    }

    // 4. Logic gọi API tính phí ship (Chỉ dành cho Delivery)
    private async Task<long> CalculateDeliveryShippingFeeAsync(List<OrderDetail> orderDetails, List<Store> allPotentialStores, string destProvince, string destDistrict)
    {
        // Lưu ý: Code cũ của bạn bị mất cái * 500 gram, mình đã thêm lại để GHTK không báo lỗi cân nặng
        int defaultWeight = 500;

        var packagesToShip = orderDetails
            .GroupBy(od => od.StoreHouseId)
            .Select(group =>
            {
                var storeInfo = allPotentialStores.First(s => s.StoreHouseId == group.Key).StoreHouse;
                int totalWeight = group.Sum(od => od.Quantity) * defaultWeight;
                return new PackageToShip(storeInfo.City, storeInfo.District, totalWeight);
            })
            .ToList();

        return await _shippingFeeService.CalculateAccurateShippingFee(packagesToShip, destProvince, destDistrict);
    }
    private List<OrderDetail> AllocatePickupInventory(List<CartDetail> cartDetails, List<Store> storeInventory, long storeHouseId, string userId)
    {
        var finalOrderDetails = new List<OrderDetail>();

        foreach (var cartItem in cartDetails)
        {
            // TÌM VÀ SẮP XẾP KHO:
            var candidateStores = storeInventory
                .Where(s => s.ProductId == cartItem.ProductCartDetailId && s.ColorId == cartItem.ColorId)
                // ƯU TIÊN 1: Kho khách hàng chọn đến lấy sẽ được đẩy lên đầu tiên
                .OrderByDescending(s => s.StoreHouseId == storeHouseId)
                // ƯU TIÊN 2: Các kho khác, kho nào nhiều hàng hơn thì rút trước
                .ThenByDescending(s => s.Quantity)
                .ToList();

            int remainingQty = cartItem.Quantity;

            foreach (var store in candidateStores)
            {
                if (remainingQty <= 0) break;

                int takeQty = Math.Min(store.Quantity, remainingQty);

                // Tạo OrderDetail
                finalOrderDetails.Add(new OrderDetail
                {
                    ProductOrderDetailId = cartItem.ProductCartDetailId,
                    ColorId = cartItem.ColorId,
                    Quantity = takeQty,
                    StoreHouseId = store.StoreHouseId, // QUAN TRỌNG: Lưu ID kho THỰC TẾ bị trừ hàng để sau này shop biết đường luân chuyển
                    Price = cartItem.Product!.SalePrice,
                    CreateBy = userId,
                    CreateDate = DateTime.UtcNow,
                    Product = cartItem.Product,
                    Color = cartItem.Color
                });

                remainingQty -= takeQty;

                // Trừ số lượng ảo trong RAM
                store.Quantity -= takeQty;
            }

            // Nếu đã vét sạch tất cả các kho trên hệ thống mà vẫn không đủ số lượng
            if (remainingQty > 0)
            {
                throw new Exception($"Sản phẩm {cartItem.Product?.Name} màu {cartItem.Color?.Name} hiện không đủ số lượng trên toàn hệ thống!");
            }
        }

        return finalOrderDetails;
    }
}
