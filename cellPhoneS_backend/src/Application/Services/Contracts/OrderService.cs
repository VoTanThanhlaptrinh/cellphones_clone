using System;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs.Requests;
using cellPhoneS_backend.DTOs.Responses;

namespace cellPhoneS_backend.Services.Interface;

public interface OrderService
{
    public Task<ServiceResult<string>> GetOrderByUserId(string userId);
    public Task<ServiceResult<string>> GetOrderById(string orderId);
    public Task<ServiceResult<string>> GetOrders(int page, int pageSize);
    public Task<ServiceResult<string>> DeleteOrder(string userId, long orderId);
    public Task<ServiceResult<string>> Checkout(string userId, long orderId);
    Task<ServiceResult<OrderView>> CreatePickupOrder(string userId, PickupOrderRequest payload);
    Task<ServiceResult<OrderView>> CreateDeliveryOrder(string userId, DeliveryOrderRequest payload);
}
