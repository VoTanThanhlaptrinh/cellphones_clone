using cellphones_backend.Data;
using cellphones_backend.DTOs.Responses;
using cellphones_backend.Models;
using cellPhoneS_backend.DTOs.Account;
using cellPhoneS_backend.Services;
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

    public async Task<ServiceResult<string>> DeleteUser(string id)
    {
        var res = await _userManager.FindByIdAsync(id);
        if (res == null)
            return ServiceResult<string>.Fail("khong tim thay user", ServiceErrorType.NotFound);
        res.Status = "deleted";
        var result = await _userManager.UpdateAsync(res);
        return ServiceResult<string>.Success("User deleted successfully", "");
    }

    public async Task<ServiceResult<User>> GetDetails(string id)
    {
        var res = await _userManager.FindByIdAsync(id);
        if (res == null)
            return ServiceResult<User>.Fail("khong tim thay user", ServiceErrorType.NotFound);
        return ServiceResult<User>.Success(res, "Thanh cong");
    }

    public async Task<ServiceResult<IList<string>>> GetRole(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return ServiceResult<IList<string>>.Success(roles, "success");
    }

    public async Task<ServiceResult<List<UserResponse>>> GetUsers(int page, int pageSize)
    {
        var res = await _userManager.Users.Skip(page * pageSize).Take(pageSize)
                    .Select(x => new UserResponse(x.Fullname!, x.PhoneNumber!)).ToListAsync();
        if (res.Count == 0)
            return ServiceResult<List<UserResponse>>.Fail("Index out of range", ServiceErrorType.BadRequest);
        return ServiceResult<List<UserResponse>>.Success(res, "success");
    }

    public Task<ServiceResult<User>> UpdateUser(string id, User updatedUser)
    {
        throw new NotImplementedException();
    }
}
