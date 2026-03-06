using cellphones_backend.Data;
using cellphones_backend.Models;
using cellPhoneS_backend.Models;

namespace cellphones_backend.Repositories;

public class OrderDetailRepository : BaseRepository<OrderDetail>, IOrderDetailRepository
{
    public OrderDetailRepository(ApplicationDbContext context) : base(context) {}

    public Task<List<OrderDetail>> CreateOrderDetails(string orderId, long cartItemId)
    {
        throw new NotImplementedException();
    }
}
