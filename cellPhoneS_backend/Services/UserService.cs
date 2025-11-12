using System;
using cellphones_backend.DTOs.Responses;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs.Account;
using Microsoft.AspNetCore.Identity;

namespace cellphones_backend.Services;

public interface UserService
{
    public Task<ApiResponse<List<UserResponse>>> GetUsers(int page,int pageSize);
    public ApiResponse<User> GetDetails(string id);
    public ApiResponse<User> UpdateUser(string id, User updatedUser);
    public ApiResponse<string> DeleteUser(string id);
    public IList<string> GetRole(User user);
}
