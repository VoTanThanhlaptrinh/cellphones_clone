using System;
using cellphones_backend.Models;
using cellPhoneS_backend.Data.View;
using cellPhoneS_backend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace cellphones_backend.Data;

public class ApplicationDbContext : IdentityDbContext<User, Role, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Brand> Brands { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartDetail> CartDetails { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Color> Colors { get; set; }
    public DbSet<Commitment> Commitments { get; set; }
    public DbSet<Criterion> Criteria { get; set; }
    public DbSet<CriterionDetail> CriterionDetails { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<Info> Infos { get; set; }
    public DbSet<Oauth2> Oauth2s { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductSpecification> ProductSpecifications { get; set; }
    public DbSet<Series> Series { get; set; }
    public DbSet<Specification> Specifications { get; set; }
    public DbSet<SpecificationDetail> SpecificationDetails { get; set; }
    public DbSet<Store> Stores { get; set; }
    public DbSet<StoreHouse> StoreHouses { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<GrantRole> GrantRoles { get; set; }
    public DbSet<Demand> Demands { get; set; }
    public DbSet<DemandImage> DemandImages { get; set; }
    public DbSet<CategoryProduct> CategoryProducts { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<ProductColorStockView> ProductColorStockView { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProductSpecification>().HasKey(ps => new { ps.ProductId, ps.SpecificationId });
        modelBuilder.Entity<GrantRole>().HasKey(g => new { g.UserId, g.RoleId });

        modelBuilder.Entity<DemandImage>(entity =>
        {
            entity.HasKey(di => new { di.DemandId, di.ImageId });
        });
        modelBuilder.Entity<ProductImage>(pi =>
        {
            pi.HasKey(x => new { x.ProductId, x.ImageId });
        });
        modelBuilder.Entity<Color>(entity =>
        {
            entity.HasKey(ci => ci.Id);
            entity.Property(ci => ci.Id).ValueGeneratedOnAdd();

        });
        modelBuilder.Entity<CategoryProduct>(entity =>
        {
            entity.HasKey(cp => new { cp.CategoryId, cp.ProductId });
        });
        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasKey(s => new { s.StoreHouseId, s.ColorId, s.ProductId });
        });
        modelBuilder.Entity<User>()
            .HasOne(u => u.Cart).WithOne(c => c.CreateUser)
            .HasForeignKey<Cart>(c => c.CreateBy).IsRequired();
        modelBuilder.Entity<User>()
            .HasOne(u => u.Info).WithOne(i => i.CreateUser)
            .HasForeignKey<Info>(i => i.CreateBy).IsRequired();

        modelBuilder.Entity<ProductColorStockView>()
        .ToView("ProductColorStockView")
        .HasNoKey();
    }
}

