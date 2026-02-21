using cellphones_backend.Data;
using cellphones_backend.Models;
using cellPhoneS_backend.Models;

namespace cellphones_backend.Repositories;

public class OrderRepository : BaseRepository<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context) {}
    public Task<bool> DeleteOrder(string userId, string orderId)
    {
        throw new NotImplementedException();
    }

    public Task<Order> GetOrderById(string orderId)
    {
        throw new NotImplementedException();
    }

    public Task<Order> GetOrderByUserId(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Order>> GetOrders(int page, int pageSize)
    {
        throw new NotImplementedException();
    }
}
