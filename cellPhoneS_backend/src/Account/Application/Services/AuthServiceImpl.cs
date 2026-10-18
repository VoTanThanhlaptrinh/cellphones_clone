using System.Security.Claims;
using cellphones_backend.DTOs.Account;
using cellphones_backend.Models;
using cellPhoneS_backend.Constants;
using cellPhoneS_backend.Models;
using cellPhoneS_backend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Services.Implement;

public class AuthServiceImpl : AuthService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly JwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;

    public AuthServiceImpl(UserManager<User> userManager, RoleManager<Role> roleManager, JwtTokenService jwtTokenService, IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
    }

    private async Task<User> ReturnUserIfExistEmail(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return null!;
        return user;
    }

    private async Task<User> ReturnUserIfExistPhone(string phone)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == phone);
        if (user == null) return null!;
        return user;
    }

    // Tài khoản bị khoá/xoá không được cấp token (cả đăng nhập mật khẩu lẫn Google)
    private static void EnsureActive(User user)
    {
        if (user.Status != UserStatuses.Active)
        {
            throw new UnauthorizedException("Tài khoản đã bị khoá");
        }
    }

    public async Task<string> GenerateJwtToken(User user)
    {
        // JwtTokenService reverted to string return
        return await _jwtTokenService.GenerateJwtToken(user);
    }

    public async Task<string> Login(LoginDTO loginDTO, HttpContext context)
    {
        var user = await ReturnUserIfExistEmail(loginDTO.Email);
        if (user == null)
        {
            throw new UnauthorizedException("Email not found");
        }

        if (!await _userManager.CheckPasswordAsync(user, loginDTO.Password))
        {
            throw new UnauthorizedException("Incorrect password");
        }

        EnsureActive(user);

        var userAgent = context.Request.Headers["User-Agent"].ToString();
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        var refreshToken = Guid.NewGuid().ToString();
        var sessionId = Guid.NewGuid().ToString();

        var tokenModel = new JwtRefreshes
        {
            UserId = user.Id,
            SessionId = sessionId,
            UserAgent = userAgent,
            IpAddress = ipAddress,
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _jwtTokenService.SaveRefreshTokenToRedisAsync(refreshToken, tokenModel, TimeSpan.FromDays(7));

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        context.Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

        var jwtToken = await GenerateJwtToken(user);

        return jwtToken;
    }
    public async Task Register(RegisterDTO register, HttpContext context)
    {
        var user = await ReturnUserIfExistPhone(register.Phone);
        if (user != null)
        {
            throw new BadRequestException("Phone already exists");
        }
        user = await ReturnUserIfExistEmail(register.Email);
        if (user != null)
        {
            throw new BadRequestException("Email already exists");
        }
        var newUser = new User
        {
            UserName = register.Email,
            Email = register.Email,
            PhoneNumber = register.Phone,
            Fullname = register.FullName,
            Birth = register.BirthDay,
            CreateDate = DateTime.UtcNow,
            UpdateDate = DateTime.UtcNow,
            Status = "active",

        };
        await this._userManager.CreateAsync(newUser, register.Password);
        if (!await _roleManager.RoleExistsAsync("USER"))
        {
            var role = new Role()
            {
                Name = "USER",
                Status = "active",
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            await _roleManager.CreateAsync(role);
        }
        await this._userManager.AddToRoleAsync(newUser, "USER");

    }

    public Task<string> StudentRegister(StudentRegisterDTO register)
    {
        throw new NotImplementedException();
    }

    public Task<string> TeacherRegister(TeacherRegisterDTO register)
    {
        throw new NotImplementedException();
    }

    public async Task<string> GetInfoAfterLoginByGoogle(HttpContext context)
    {
        var user = context.User;
        var email = user.FindFirst(ClaimTypes.Email)?.Value;
        var fullName = user.FindFirst(ClaimTypes.Name)?.Value;
        var userLogin = await ReturnUserIfExistEmail(email!);
        if(userLogin == null)
        {
            var newUser = new User
            {
                UserName = email,
                Email = email,
                Fullname = fullName,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow,
                Status = "active",
            };
            await this._userManager.CreateAsync(newUser);
            if (!await _roleManager.RoleExistsAsync("USER"))
            {
                var role = new Role()
                {
                    Name = "USER",
                    Status = "active",
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                };
                await _roleManager.CreateAsync(role);
            }
            await this._userManager.AddToRoleAsync(newUser, "USER");
            userLogin = newUser;
        }
        EnsureActive(userLogin);
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        var refreshToken = Guid.NewGuid().ToString();
        var sessionId = Guid.NewGuid().ToString();

        var tokenModel = new JwtRefreshes
        {
            UserId = userLogin.Id,
            SessionId = sessionId,
            UserAgent = userAgent,
            IpAddress = ipAddress,
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _jwtTokenService.SaveRefreshTokenToRedisAsync(refreshToken, tokenModel, TimeSpan.FromDays(7));

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        context.Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        return _configuration["Authentication:Google:scRedirectUri"] ?? "https://cellphonesclonethanh.vercel.app/home";
    }

    public Task Logout(HttpContext context)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true, // JavaScript can't read inside
            Secure = context.Request.IsHttps, // Just pass through HTTPS
            SameSite = SameSiteMode.Lax, // Against CSRF
            Expires = DateTime.UtcNow.AddDays(-1) // Expire the cookie
        };
        context.Response.Cookies.Append("refreshToken", "", cookieOptions);
        return Task.CompletedTask;
    }

    public async Task<string> RefreshToken(HttpContext context)
    {
        return await _jwtTokenService.RefreshJwtToken(context.Request);
    }

    public Task<string> IsLoggedIn(string userId, HttpContext context)
    {
        return _jwtTokenService.RefreshJwtToken(context.Request);
    }

    public Task<string> GetUrlCallbackGoogle()
    {
        return Task.FromResult(_configuration["Authentication:Google:callbackUri"] ?? "http://localhost:5169/api/auth/callbackGoogle");
    }
}
