using cellphones_backend.Data;
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

    public async Task DeleteUser(string id)
    {
        var res = await _userManager.FindByIdAsync(id);
        if (res == null)
            throw new NotFoundException("khong tim thay user");
        res.Status = "deleted";
        await _userManager.UpdateAsync(res);
    }

    public async Task<User> GetDetails(string id)
    {
        var res = await _userManager.FindByIdAsync(id);
        if (res == null)
            throw new NotFoundException("khong tim thay user");
        return res;
    }

    public async Task<IList<string>> GetRole(User user)
    {
        return await _userManager.GetRolesAsync(user);
    }

    public async Task<List<UserResponse>> GetUsers(int page, int pageSize)
    {
        var res = await _userManager.Users.Skip(page * pageSize).Take(pageSize)
                    .Select(x => new UserResponse(x.Fullname!, x.PhoneNumber!)).ToListAsync();
        return res;
    }

    public Task<User> UpdateUser(string id, User updatedUser)
    {
        throw new NotImplementedException();
    }
}
