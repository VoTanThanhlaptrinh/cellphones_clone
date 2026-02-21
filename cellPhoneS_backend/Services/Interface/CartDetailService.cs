using System;
using cellphones_backend.Models;

namespace cellPhoneS_backend.Services.Interface;

public interface CartDetailService
{
    public Task<List<CartDetail>> GetCartDetails(string userId, List<long> cartItemIds);
}
