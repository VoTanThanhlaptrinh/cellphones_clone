using cellphones_backend.Models;
using cellPhoneS_backend.Models;

namespace cellphones_backend.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order> GetOrderByUserId(string userId);
    Task<List<Order>> GetOrders(int page, int pageSize);
}