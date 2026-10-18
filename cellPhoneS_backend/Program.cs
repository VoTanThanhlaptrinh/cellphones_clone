using System.Net;
using System.Text;
using cellphones_backend.Data;
using cellphones_backend.Models;
using cellphones_backend.Repositories;
using cellphones_backend.Resources;
using cellPhoneS_backend.Services;
using cellPhoneS_backend.Services.Interface;
using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

internal partial class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("db")));
        builder.Services.AddMemoryCache();
        builder.Services.AddIdentity<User, cellphones_backend.Models.Role>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
        })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", policyBuilder =>
            {
                policyBuilder.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithOrigins("http://localhost:4200", "https://cellphonesclonethanh.vercel.app");
            });
        });

        builder.Services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("GlobalLimit", opt =>
            {
                opt.Window = TimeSpan.FromMinutes(1);
                opt.PermitLimit = 100;
                opt.QueueLimit = 2;
                opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
            });

            // Bị chặn thì trả 429 (mặc định là 503, frontend không phân biệt được với lỗi server)
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Chỉ áp dụng cho login/register; tính riêng theo IP (trước đây cả server dùng chung 1 bộ đếm 5 lần/phút)
            options.AddPolicy("AuthLimit", httpContext =>
                System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(1),
                        PermitLimit = 10,
                        QueueLimit = 0
                    }));
        });

        builder.Services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("db"))));

        builder.Services.AddHangfireServer();

        string redisConnectionString = builder.Configuration.GetConnectionString("Redis")!;
        var configuration = ConfigurationOptions.Parse(redisConnectionString, true);
        configuration.AbortOnConnectFail = false;
        // Redis cloud (Upstash...) bắt buộc TLS; Redis local (localhost) thì không hỗ trợ TLS
        bool isLocalRedis = configuration.EndPoints.All(e => e switch
        {
            DnsEndPoint dns => dns.Host is "localhost" or "127.0.0.1" or "::1",
            IPEndPoint ip => IPAddress.IsLoopback(ip.Address),
            _ => false
        });
        configuration.Ssl = !isLocalRedis;
        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(configuration));

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(jwtOptions =>
            {
                jwtOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                };
                jwtOptions.Events = new JwtBearerEvents
                {
                    // Token hợp lệ về chữ ký/thời hạn nhưng JTI đã bị thu hồi (Redis blacklist), hoặc cấp trước mốc
                    // thu hồi phiên của user (khoá tài khoản / đổi role: Redis user-revoked:{userId}) thì từ chối
                    OnTokenValidated = async context =>
                    {
                        var blacklist = context.HttpContext.RequestServices.GetRequiredService<IJwtBlacklistService>();
                        var jti = context.SecurityToken?.Id;
                        if (!string.IsNullOrEmpty(jti) && await blacklist.IsTokenBlacklistedAsync(jti))
                        {
                            context.Fail("Token has been revoked.");
                            return;
                        }

                        var userId = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                        if (string.IsNullOrEmpty(userId)) return;
                        long? issuedAt = long.TryParse(context.Principal!.FindFirst("iat")?.Value, out var iat) ? iat : null;
                        if (await blacklist.IsUserSessionRevokedAsync(userId, issuedAt))
                        {
                            context.Fail("User session has been revoked.");
                        }
                    }
                };
            })
            .AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
                googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
                googleOptions.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
                {
                    OnRemoteFailure = context =>
                    {
                        context.HandleResponse();
                        context.Response.Redirect("http://localhost:4200/login");
                        return Task.CompletedTask;
                    }
                };
            });

        var cs = builder.Configuration.GetConnectionString("StorageAccount");
        builder.Services.AddAzureClients(clientBuilder => clientBuilder.AddBlobServiceClient(cs));
        // Không đặt ResourcesPath: resx được nhúng với tên "cellphones_backend.Resources.ShareResource" (namespace của class ShareResource),
        // đúng tên mà IStringLocalizer<ShareResource> tìm. Có ResourcesPath thì nó tìm "cellPhoneS_backend.Resources.cellphones_backend.Resources..."
        // và mọi thông báo của validator trả về nguyên khoá (vd. "EmailInvalid").
        builder.Services.AddLocalization();
        builder.Services.AddScoped<IResourceLocalizer, ResourceLocalizer>();
        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<cellPhoneS_backend.Filters.ApiExceptionFilter>();
            options.Filters.Add<cellPhoneS_backend.Filters.AdminAuditFilter>();        // ghi AuditLog cho request ghi dưới /api/admin
            options.Filters.Add<cellPhoneS_backend.Filters.ValidationActionFilter>();   // chạy validator FluentValidation, lỗi -> 400 ApiResponse
        });
        // Validator FluentValidation (Scoped): quét assembly chứa LoginDTOValidator; ValidationActionFilter tìm validator theo kiểu argument của action.
        builder.Services.AddValidatorsFromAssemblyContaining<cellphones_backend.Validation.Account.LoginDTOValidator>();
        // OpenAPI chỉ mô tả khu admin (hợp đồng cho frontend admin). Một số action storefront nhận/trả thẳng entity EF
        // (vòng tham chiếu Order -> User -> ...) làm bộ sinh schema vượt MaxDepth và cả document trả 500, nên không đưa vào.
        builder.Services.AddOpenApi(options =>
            options.ShouldInclude = description =>
                description.RelativePath?.StartsWith("api/admin", StringComparison.OrdinalIgnoreCase) == true);
        builder.Services.AddCustomServices();
        // Payment gateway (payment-module): khoá API chỉ nằm ở backend, cấu hình qua PaymentGateway:BaseUrl|ApiKey (user-secrets/biến môi trường).
        builder.Services.AddHttpClient<cellPhoneS_backend.Services.Interface.IPaymentGatewayClient, cellPhoneS_backend.Services.Implement.PaymentGatewayClientImpl>(client =>
        {
            var baseUrl = builder.Configuration["PaymentGateway:BaseUrl"];
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
            }
            client.DefaultRequestHeaders.Add("x-api-key", builder.Configuration["PaymentGateway:ApiKey"] ?? "");
            // Gateway có thể phải gọi cổng thanh toán nên cần timeout tối thiểu 15 giây.
            client.Timeout = TimeSpan.FromSeconds(20);
        });
        builder.Services.AddHttpClient("VietnamAdministrativeApi", client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["VietnamAdministrativeApi:BaseUrl"] ?? "https://provinces.open-api.vn/api/");
            client.Timeout = TimeSpan.FromSeconds(20);
        });
        builder.Services.AddRepositories();

        var supportedCultures = new[] { "vi", "en" };
        builder.Services.Configure<RequestLocalizationOptions>(opts =>
        {
            opts.SetDefaultCulture("vi");
            opts.AddSupportedCultures(supportedCultures);
            opts.AddSupportedUICultures(supportedCultures);
            opts.RequestCultureProviders.Insert(0, new Microsoft.AspNetCore.Localization.QueryStringRequestCultureProvider
            {
                QueryStringKey = "culture"
            });
        });

        var app = builder.Build();
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                await cellphones_backend.Data.AdminSeeder.SeedAsync(services, builder.Configuration);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi seed role/permission/tai khoan ADMIN: " + ex.Message);
            }

            try
            {
                var searchService = services.GetRequiredService<ProductSearchService>();
                await searchService.InitializeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi Init Cache ProductSearch: " + ex.Message);
            }

            try
            {
                var vietnamLocationService = services.GetRequiredService<IVietnamLocationService>();
                await vietnamLocationService.WarmUpAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi khoi tao cache dia gioi hanh chinh Viet Nam: " + ex.Message);
            }
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }
        app.UseRouting();
        app.UseCors("CorsPolicy");
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseMiddleware<cellPhoneS_backend.Auth.CentralizedAuthMiddleware>(cellPhoneS_backend.Auth.RouteConfig.GetPolicies());
        app.UseAuthorization();
        app.UseHangfireDashboard("/hangfire");
        app.UseRequestLocalization(app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>().Value);
        app.MapControllers();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi(); // GET /openapi/v1.json (RouteConfig: /openapi/* AllowAnonymous)

            // Fail-closed: endpoint admin quên [RequirePermission] chỉ còn được bảo vệ bởi admin.access (STAFF gọi được API ghi).
            var missingPermission = app.Services
                .GetRequiredService<Microsoft.AspNetCore.Mvc.Infrastructure.IActionDescriptorCollectionProvider>()
                .ActionDescriptors.Items
                .Where(a => a.AttributeRouteInfo?.Template?.StartsWith("api/admin", StringComparison.OrdinalIgnoreCase) == true)
                .Where(a => !a.EndpointMetadata.OfType<cellPhoneS_backend.Filters.RequirePermissionAttribute>().Any())
                .Select(a => a.DisplayName);
            foreach (var name in missingPermission)
            {
                app.Logger.LogWarning("Admin endpoint thiếu [RequirePermission]: {Action}", name);
            }
        }

        // Dự phòng khi khách trả tiền rồi đóng trang: định kỳ hỏi gateway các thanh toán online còn chờ.
        try
        {
            app.Services.GetRequiredService<IRecurringJobManager>().AddOrUpdate<cellPhoneS_backend.Services.Interface.OrderService>(
                "sync-online-payments", service => service.SyncPendingOnlinePayments(), "*/5 * * * *");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Không đăng ký được job đồng bộ thanh toán online");
        }

        app.Run();
    }
}
