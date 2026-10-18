using cellphones_backend.Models;
using cellPhoneS_backend.Constants;
using cellPhoneS_backend.DTOs.Account;
using cellPhoneS_backend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Services.Implement;

public class UserServiceImpl : UserService
{
    private readonly UserManager<User> _userManager;

    public UserServiceImpl(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserResponse> GetMe(string userId)
    {
        var user = await FindUser(userId);
        return ToResponse(user);
    }

    public async Task<UserResponse> UpdateMe(string userId, UpdateMeRequest request)
    {
        var user = await FindUser(userId);

        var phone = request.Phone!.Trim();
        if (phone != user.PhoneNumber
            && await _userManager.Users.AnyAsync(x => x.PhoneNumber == phone && x.Id != user.Id))
        {
            throw new ConflictException("Số điện thoại đã được sử dụng");
        }

        user.Fullname = request.Fullname!.Trim();
        user.Birth = request.Birth!.Value;
        user.PhoneNumber = phone;
        user.UpdateDate = DateTime.UtcNow;
        user.UpdateBy = user.Id;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new BadRequestException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }
        return ToResponse(user);
    }

    public async Task<IList<string>> GetRole(User user)
    {
        return await _userManager.GetRolesAsync(user);
    }

    private async Task<User> FindUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.Status == UserStatuses.Deleted)
            throw new NotFoundException("Không tìm thấy người dùng");
        return user;
    }

    private static UserResponse ToResponse(User user) =>
        new(user.Id, user.Email, user.Fullname, user.PhoneNumber, user.Birth);
}
