using cellphones_backend.Models;
using cellPhoneS_backend.DTOs.Account;

namespace cellphones_backend.Services;

public interface UserService
{
    Task<UserResponse> GetMe(string userId);
    Task<UserResponse> UpdateMe(string userId, UpdateMeRequest request);
    Task<IList<string>> GetRole(User user);
}
