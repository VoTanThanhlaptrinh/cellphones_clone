using System;
using cellphones_backend.Models;
using cellphones_backend.Repositories;
using cellPhoneS_backend.Services.Interface;

namespace cellPhoneS_backend.Services.Implement;

public class OrderServiceImpl : OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly CartDetailService _cartDetailService;
    private readonly IOrderDetailRepository _orderDetailRepository;
    public OrderServiceImpl(IOrderRepository orderRepository, CartDetailService cartDetailService, IOrderDetailRepository orderDetailRepository)
    {
        _orderRepository = orderRepository;
        _cartDetailService = cartDetailService;
        _orderDetailRepository = orderDetailRepository;
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

    public async Task<ServiceResult<Order>> CreateOrder(string userId, List<long> cartItemIds)
    {
        if (cartItemIds == null || cartItemIds.Count == 0)
        {
            return ServiceResult<Order>.Fail("Cart item IDs cannot be null or empty");
        }
        var cartDetails = _cartDetailService.GetCartDetails(userId, cartItemIds).Result;
        Console.WriteLine($"Cart details count: {cartDetails.ToString()}");
        if (cartDetails == null || cartDetails.Count == 0)
        {
            return ServiceResult<Order>.Fail("No cart items found for the user");
        }
        var orderDetails = cartDetails.Select(cartDetail => new OrderDetail
        {
            ProductOrderDetailId = cartDetail.ProductCartDetailId,
            Quantity = cartDetail.Quantity,
            Price = cartDetail.Product!.SalePrice,
            ColorId = cartDetail.ColorId,
            CreateBy = userId,
            CreateDate = DateTime.UtcNow,
            UpdateBy = userId,
            UpdateDate = DateTime.UtcNow
        }).ToList();

        var order = new Order
        {
            CreateBy = userId,
            OrderDetails = orderDetails,
            CreateDate = DateTime.UtcNow,
            Status = "Pending",
            UpdateBy = userId,
            UpdateDate = DateTime.UtcNow
        };
        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();
        return ServiceResult<Order>.Success(order, "Order created successfully");
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

}
