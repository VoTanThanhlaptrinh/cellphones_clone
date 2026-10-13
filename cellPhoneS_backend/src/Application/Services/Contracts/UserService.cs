using System;
using cellphones_backend.DTOs.Responses;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs.Account;
using Microsoft.AspNetCore.Identity;
using cellPhoneS_backend.Services;

namespace cellphones_backend.Services;

public interface UserService
{
    Task<List<UserResponse>> GetUsers(int page,int pageSize);
    Task<User> GetDetails(string id);
    Task<User> UpdateUser(string id, User updatedUser);
    Task DeleteUser(string id);
    Task<IList<string>> GetRole(User user);
}
