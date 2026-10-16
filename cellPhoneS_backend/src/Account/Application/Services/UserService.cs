using cellphones_backend.Models;
using cellPhoneS_backend.DTOs.Account;

namespace cellphones_backend.Services;

public interface UserService
{
    Task<List<UserResponse>> GetUsers(int page,int pageSize);
    Task<User> GetDetails(string id);
    Task<User> UpdateUser(string id, User updatedUser);
    Task DeleteUser(string id);
    Task<IList<string>> GetRole(User user);
}
