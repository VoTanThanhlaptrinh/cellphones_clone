using cellphones_backend.Data;
using cellphones_backend.Resources;
using cellphones_backend.Services;
using cellphones_backend.Services.Implement;
using Microsoft.EntityFrameworkCore;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("dev")));

        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
        builder.Services.AddScoped<IResourceLocalizer, ResourceLocalizer>();
        builder.Services.AddControllers();
        builder.Services.AddScoped<AuthService, AuthServiceImpl>();
        builder.Services.AddOpenApi();

        var supportedCultures = new[] { "vi", "en" }; // start với vi, sau thêm en
        builder.Services.Configure<RequestLocalizationOptions>(opts =>
        {
            opts.SetDefaultCulture("vi");
            opts.AddSupportedCultures(supportedCultures);
            opts.AddSupportedUICultures(supportedCultures);

            // Các provider xác định culture theo thứ tự ưu tiên:
            // ?culture=vi, Header Accept-Language, Route/Path (nếu dùng), Cookie
            opts.RequestCultureProviders.Insert(0, new Microsoft.AspNetCore.Localization.QueryStringRequestCultureProvider { QueryStringKey = "culture" });
            // CookieProvider và AcceptLanguageHeaderProvider đã có sẵn
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUi(options => {
                    options.DocumentPath = "/openapi/v1.json";
                });
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRequestLocalization(app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>().Value);
        app.MapControllers();
        app.Run();
    }
}