using System;
using cellphones_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace cellphones_backend.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    protected ApplicationDbContext()
    {
    }

    public DbSet<Brand> Brands { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartDetail> CartDetails { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Color> Colors { get; set; }
    public DbSet<ColorImage> ColorImages { get; set; }
    public DbSet<Commitment> Commitments { get; set; }
    public DbSet<Criterion> Criteria { get; set; }
    public DbSet<CriterionDetail> CriterionDetails { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<Info> Infos { get; set; }
    public DbSet<Oauth2> Oauth2s { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<ProductSpecification> ProductSpecifications { get; set; }
    public DbSet<Series> Series { get; set; }
    public DbSet<Specification> Specifications { get; set; }
    public DbSet<SpecificationDetail> SpecificationDetails { get; set; }
    public DbSet<Store> Stores { get; set; }
    public DbSet<StoreHouse> StoreHouses { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<GrantRole> GrantRoles { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProductImage>().HasKey(pi => new { pi.ProductId, pi.ImageId });
        modelBuilder.Entity<ProductSpecification>().HasKey(ps => new { ps.ProductId, ps.SpecificationId });
        modelBuilder.Entity<GrantRole>().HasKey(g => new { g.UserId, g.RoleId });

        modelBuilder.Entity<User>()
            .HasOne(u => u.Cart).WithOne(c => c.CreateUser)
            .HasForeignKey<Cart>(c => c.CreateBy).IsRequired();
        modelBuilder.Entity<User>()
            .HasOne(u => u.Info).WithOne(i => i.CreateUser)
            .HasForeignKey<Info>(i => i.CreateBy).IsRequired();
    }
}
