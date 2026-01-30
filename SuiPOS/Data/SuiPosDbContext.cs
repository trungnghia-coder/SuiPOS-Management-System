using Microsoft.EntityFrameworkCore;
using SuiPOS.Models;

namespace SuiPOS.Data;

public class SuiPosDbContext : DbContext
{
    public SuiPosDbContext(DbContextOptions<SuiPosDbContext> options) : base(options) { }

    // Register your entities here 
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Staff> Staffs { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<ProductAttribute> ProductAttributes { get; set; }
    public DbSet<AttributeValue> AttributeValues { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>().Property(o => o.TotalAmount).HasPrecision(18, 2);
        modelBuilder.Entity<OrderDetail>().Property(od => od.UnitPrice).HasPrecision(18, 2);
        // Precision cho decimal trong ProductVariant
        modelBuilder.Entity<ProductVariant>()
            .Property(pv => pv.Price)
            .HasPrecision(18, 2);

        // Relationship giữa Product và ProductVariant
        modelBuilder.Entity<ProductVariant>()
            .HasOne(pv => pv.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(pv => pv.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship giữa ProductAttribute và AttributeValue
        modelBuilder.Entity<AttributeValue>()
            .HasOne(av => av.Attribute)
            .WithMany(a => a.Values)
            .HasForeignKey(av => av.AttributeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship giữa ProductVariant và AttributeValue 
        modelBuilder.Entity<ProductVariant>()
            .HasMany(pv => pv.SelectedValues)
            .WithMany()
            .UsingEntity(j => j.ToTable("ProductVariantAttributeValues"));

        // Relationship giữa Product và Category
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship giữa Order và Staff
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Staff)
            .WithMany()
            .HasForeignKey(o => o.StaffId)
            .OnDelete(DeleteBehavior.SetNull);

        // Relationship giữa Order và Customer
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany()
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        // Relationship giữa OrderDetail và Order
        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.Order)
            .WithMany(o => o.OrderDetails)
            .HasForeignKey(od => od.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship giữa OrderDetail và ProductVariant 
        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.ProductVariant)
            .WithMany()
            .HasForeignKey(od => od.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship giữa Staff và Role
        modelBuilder.Entity<Staff>()
            .HasOne(s => s.Role)
            .WithMany()
            .HasForeignKey(s => s.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index cho SKU trong ProductVariant
        modelBuilder.Entity<ProductVariant>()
            .HasIndex(pv => pv.SKU)
            .IsUnique();

        // Index cho Username trong Staff
        modelBuilder.Entity<Staff>()
            .HasIndex(s => s.Username)
            .IsUnique();

        // Index cho OrderCode trong Order
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.OrderCode)
            .IsUnique();

        // Index cho Phone trong Customer
        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.Phone);

        // Seed data cho Role
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Admin" },
            new Role { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Cashier" },
            new Role { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Manager" }
        );
    }
}