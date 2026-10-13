using System;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;

namespace cellPhoneS_backend.Services.Interface;

public interface OrderService
{
    public Task<string> GetOrderByUserId(string userId);
    public Task<string> GetOrderById(string orderId);
    public Task<string> GetOrders(int page, int pageSize);
    public Task DeleteOrder(string userId, long orderId);
    public Task Checkout(string userId, long orderId);
    Task<OrderView> CreatePickupOrder(string userId, PickupOrderRequest payload);
    Task<OrderView> CreateDeliveryOrder(string userId, DeliveryOrderRequest payload);
}
