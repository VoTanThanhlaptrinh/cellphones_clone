using cellphones_backend.Data;
using cellphones_backend.DTOs.Responses;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Services.Implement;

public class UserServiceImpl : UserService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly ApplicationDbContext _applicationDbContext;

    public UserServiceImpl(UserManager<User> userManager, RoleManager<Role> roleManager, ApplicationDbContext applicationDbContext)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _applicationDbContext = applicationDbContext;
    }

    public ApiResponse<string> DeleteUser(string id)
    {
        throw new NotImplementedException();
    }

    public ApiResponse<User> GetDetails(string id)
    {
        var res = _userManager.FindByIdAsync(id).Result;
        if (res == null)
            return new ApiResponse<User>("khong tim thay user", null!, 404);
        return new ApiResponse<User>("Thanh cong", res, 200);
    }

    public IList<string> GetRole(User user)
    {
        return _userManager.GetRolesAsync(user).Result;
    }

    public async Task<ApiResponse<List<UserResponse>>> GetUsers(int page, int pageSize)
    {
        var res = await _userManager.Users.Skip(page * pageSize).Take(pageSize)
                    .Select(x => new UserResponse(x.Fullname!, x.PhoneNumber!)).ToListAsync();
        if (res.Count == 0) // Check for list not null
            return new ApiResponse<List<UserResponse>>("Index out of range", new List<UserResponse>(), 400);
        return new ApiResponse<List<UserResponse>>("thanh cong", res, 200); // return success list
    }

    public ApiResponse<User> UpdateUser(string id, User updatedUser)
    {
        throw new NotImplementedException();
    }
}
