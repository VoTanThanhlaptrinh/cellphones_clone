using System;
using cellphones_backend.DTOs.Responses;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs.Account;
using Microsoft.AspNetCore.Identity;
using cellPhoneS_backend.Services;

namespace cellphones_backend.Services;

public interface UserService
{
    Task<ServiceResult<List<UserResponse>>> GetUsers(int page,int pageSize);
    Task<ServiceResult<User>> GetDetails(string id);
    Task<ServiceResult<User>> UpdateUser(string id, User updatedUser);
    Task<ServiceResult<string>> DeleteUser(string id);
    Task<ServiceResult<IList<string>>> GetRole(User user);
}
