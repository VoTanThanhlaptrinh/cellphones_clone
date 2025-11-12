

using cellphones_backend.Data;

using cellphones_backend.Resources;
using cellphones_backend.Services;
using cellphones_backend.Services.Implement;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using cellphones_backend.Models;
using Microsoft.Extensions.Azure;
using cellPhoneS_backend.Services;
using cellPhoneS_backend.Services.Implement;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.AspNetCore.OpenApi;
using cellPhoneS_backend.DTOs;
using System.Text.Json;
using cellPhoneS_backend.Models;
using cellphones_backend.Repositories; // add repo registration extension

internal class Program
{
    private async static Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("dev")));
        builder.Services.AddIdentity<User, Role>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
        })
            .AddEntityFrameworkStores<ApplicationDbContext>()  // ⚡ cái này tạo UserManager/RoleManager
            .AddDefaultTokenProviders();

        builder.Services.AddAuthentication().AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwtOptions =>
        {
            jwtOptions.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidAudience = builder.Configuration["Jwt:Audience"],
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
            };
        });
        builder.Services.AddAuthentication().AddGoogle(googleOptions =>
        {
            googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
            googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
        });
        var cs = builder.Configuration.GetConnectionString("StorageAccount");
        builder.Services.AddAzureClients(clientBuilder => clientBuilder.AddBlobServiceClient(cs));
        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
        builder.Services.AddScoped<IResourceLocalizer, ResourceLocalizer>();
        builder.Services.AddControllers();
        builder.Services.AddScoped<AuthService, AuthServiceImpl>();
        builder.Services.AddScoped<UserService, UserServiceImpl>();
        builder.Services.AddScoped<AzuriteService, AzuriteServiceImpl>();
        builder.Services.AddScoped<ImageDownloaderService, ImageDownloaderServiceImpl>();
        builder.Services.AddScoped<CloneService, CloneServiceImpl>();
        builder.Services.AddScoped<JwtTokenService, JwtTokenServiceImpl>();
        builder.Services.AddScoped<InitService, InitServiceImpl>();
        builder.Services.AddScoped<ProductService, ProductServiceImpl>();
        builder.Services.AddHttpClient();
    // Register all repositories
    builder.Services.AddRepositories();
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
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var userManager = services.GetRequiredService<UserManager<User>>();
            var roleManager = services.GetRequiredService<RoleManager<Role>>();
            var dbContext = services.GetRequiredService<ApplicationDbContext>();
            var cloneService = services.GetRequiredService<CloneService>();
            // var categories = builder.Configuration.GetSection("DefaultSetting:CategoryInit").Get<string[]>();
            // var creater = await userManager.FindByEmailAsync("john.doe@example.com");
            // string createrId = creater?.Id.ToString() ?? "";

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRequestLocalization(app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>().Value);
            app.MapControllers();
            app.Run();
        }
    }
}
// string folder_path = @"D:\IT\node_js_ws\rs_1";
// string[] files = Directory.GetFiles(folder_path, "*.json", SearchOption.TopDirectoryOnly);
// Console.WriteLine("folder xong");
// string openStream = await File.ReadAllTextAsync(files[8], Encoding.UTF8);
// List<FileJson>? danhSach = JsonSerializer.Deserialize<List<FileJson>>(openStream);
// Console.WriteLine("Đọc file xong");
// var cloneService = services.GetRequiredService<CloneService>();
// Category? category = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == 11);
// using (var transaction = await dbContext.Database.BeginTransactionAsync())
// {
//     try
//     {
//         var index = 0;
//         List<CategoryProduct> categoryProducts = new List<CategoryProduct>();
//         foreach (var item in danhSach!)
//         {
//             var productDetail = item.product!.productBase[0];
//             var basePrice = 0.0;
//             var salePrice = 0.0;
//             try
//             {
//                 basePrice = double.Parse((productDetail.base_price ?? "0").Replace(".", "").Replace("đ", "").Trim());
//                 salePrice = double.Parse((productDetail.sale_price ?? "0").Replace(".", "").Replace("đ", "").Trim());
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine("Error parsing prices: " + ex.Message);
//             }
//             List<Image> images = new List<Image>();
//             if (productDetail.imgs != null)
//             {
//                 var productImgTask = productDetail.imgs.Select(imgUrl => cloneService.GetImageUrlFromOnlineAfterUploadOnAzurite(imgUrl)).ToList();
//                 Console.WriteLine($"==> Đang tải {productImgTask.Count} ảnh sản phẩm...");
//                 string[] productImgs = await Task.WhenAll(productImgTask);
//                 Console.WriteLine($"==> Tải ảnh sản phẩm XONG.");
//                 for (int i = 0; i < productImgs.Length; i++)
//                 {
//                     var img = new Image
//                     {
//                         BlobUrl = productImgs[i],
//                         OriginUrl = productDetail.imgs[i],
//                         MimeType = "image/png",
//                         Name = productDetail.name,
//                         Alt = $"Image of {productDetail.name}",
//                         CreateBy = createrId,
//                         CreateDate = DateTime.UtcNow,
//                         UpdateBy = createrId,
//                         UpdateDate = DateTime.UtcNow
//                     };
//                     images.Add(img);
//                 }
//             }
//             var product = new Product
//             {
//                 Name = productDetail.name,
//                 Version = productDetail.ver ?? productDetail.name,
//                 BasePrice = basePrice,
//                 SalePrice = salePrice,
//                 CreateBy = createrId,
//                 CreateDate = DateTime.UtcNow,
//                 UpdateBy = createrId,
//                 UpdateDate = DateTime.UtcNow
//             };
//             categoryProducts.Add(new CategoryProduct
//             {
//                 Product = product,
//                 Category = category,
//                 CreateBy = createrId,
//                 CreateDate = DateTime.UtcNow,
//                 UpdateBy = createrId,
//                 UpdateDate = DateTime.UtcNow
//             });

//             var pi = new List<ProductImage>();
//             for (int i = 0; i < images.Count; i++)
//             {
//                 pi.Add(new ProductImage
//                 {
//                     Product = product,
//                     Image = images[i],
//                     CreateBy = createrId,
//                     CreateDate = DateTime.UtcNow,
//                     UpdateBy = createrId,
//                     UpdateDate = DateTime.UtcNow
//                 });
//             }

//             List<Image> colorImgs = new List<Image>();
//             List<Color> colos = new List<Color>();
//             if (productDetail.variants != null)
//             {
//                 var variants = productDetail.variants.Select(c => cloneService.GetImageUrlFromOnlineAfterUploadOnAzurite(c.src!)).ToList();
//                 string[] colors = await Task.WhenAll(variants);
//                 for (int i = 0; i < variants.Count; i++)
//                 {
//                     var im = new Image
//                     {
//                         BlobUrl = colors[i],
//                         OriginUrl = productDetail.variants[i].src,
//                         MimeType = "image/png",
//                         Name = productDetail.name,
//                         Alt = $"Image of {productDetail.name}",
//                         CreateBy = createrId,
//                         CreateDate = DateTime.UtcNow,
//                         UpdateBy = createrId,
//                         UpdateDate = DateTime.UtcNow
//                     };
//                     colorImgs.Add(im);
//                     var c = new Color
//                     {
//                         Name = productDetail.variants[i].name,
//                         Image = im,
//                         CreateBy = createrId,
//                         CreateDate = DateTime.UtcNow,
//                         UpdateBy = createrId,
//                         UpdateDate = DateTime.UtcNow
//                     };
//                     colos.Add(c);
//                 }
//             }
//             List<Commitment> commitments = new List<Commitment>();
//             if (productDetail.commitList != null)
//             {
//                 commitments = productDetail.commitList.Select(c =>
//                 new Commitment
//                 {
//                     Product = product,
//                     Context = c,
//                     CreateBy = createrId,
//                     CreateDate = DateTime.UtcNow,
//                     UpdateBy = createrId,
//                     UpdateDate = DateTime.UtcNow
//                 }
//             ).ToList();
//             }
//             // Technical
//             List<Specification> technicals = new List<Specification>();
//             List<ProductSpecification> productSpecifications = new List<ProductSpecification>();
//             if (item.product.techs != null)
//             {
//                 item.product.techs.ForEach(t =>
//             {
//                 var spec = new Specification
//                 {
//                     Name = t.title,
//                     CreateBy = createrId,
//                     CreateDate = DateTime.UtcNow,
//                     UpdateBy = createrId,
//                     UpdateDate = DateTime.UtcNow
//                 };
//                 technicals.Add(spec);
//                 productSpecifications.Add(new ProductSpecification
//                 {
//                     Product = product,
//                     Specification = spec,
//                     CreateBy = createrId,
//                     CreateDate = DateTime.UtcNow,
//                     UpdateBy = createrId,
//                     UpdateDate = DateTime.UtcNow
//                 });
//                 List<SpecificationDetail> technicalDetails = new List<SpecificationDetail>();
//                 if (t.fields != null)
//                 {
//                     t.fields.ForEach(f =>
//                 {
//                     var field = new SpecificationDetail
//                     {
//                         Specification = spec,
//                         Name = f.key,
//                         Value = f.value,
//                         CreateBy = createrId,
//                         CreateDate = DateTime.UtcNow,
//                         UpdateBy = createrId,
//                         UpdateDate = DateTime.UtcNow
//                     };
//                     technicalDetails.Add(field);
//                 });
//                 }
//                 dbContext.SpecificationDetails.AddRangeAsync(technicalDetails);
//             }
//             );
//             }
//             await dbContext.Products.AddAsync(product);
//             await dbContext.Images.AddRangeAsync(images);
//             await dbContext.AddRangeAsync(pi);
//             await dbContext.Images.AddRangeAsync(colorImgs);
//             await dbContext.Colors.AddRangeAsync(colos);
//             await dbContext.Commitments.AddRangeAsync(commitments);
//             await dbContext.Specifications.AddRangeAsync(technicals);
//             await dbContext.CategoryProducts.AddRangeAsync(categoryProducts);
//             await dbContext.ProductSpecifications.AddRangeAsync(productSpecifications);
//             index++;
//             Console.WriteLine("Sản phẩm thứ:" + index + " tiến độ:" + index / danhSach.Count * 100);

//         }
//         await dbContext.SaveChangesAsync();
//         await transaction.CommitAsync();
//         Console.WriteLine("Hoàn Thành");
//     }
//     catch (Exception ex)
//     {
//         await transaction.RollbackAsync();
//         Console.WriteLine("Error occurred: " + ex.Message);

//     }
// }
// var dbContext = services.GetRequiredService<ApplicationDbContext>();
// dbContext.Categories.AddRange(categories);
// await dbContext.SaveChangesAsync();
// var cloneService = services.GetRequiredService<CloneService>();
// string url = await cloneService.GetImageUrlFromOnlineAfterUploadOnAzurite();