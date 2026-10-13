using cellphones_backend.Data;
using cellphones_backend.Models;
using cellphones_backend.Repositories;
using cellphones_backend.Resources;
using cellPhoneS_backend.Services;
using cellPhoneS_backend.Services.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.RateLimiting;
using Hangfire;
using Hangfire.PostgreSql;

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
        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
        builder.Services.AddScoped<IResourceLocalizer, ResourceLocalizer>();
        builder.Services.AddControllers(options => options.Filters.Add<cellPhoneS_backend.Filters.ApiExceptionFilter>());
        builder.Services.AddCustomServices();
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
        app.Run();
    }
}
