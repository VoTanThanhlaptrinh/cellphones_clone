using System;
using cellPhoneS_backend.Models;

namespace cellPhoneS_backend.Services.Interface;

public interface FeeService
{
    Task<Fee> CreatePickupFee(long totalFee);
}
