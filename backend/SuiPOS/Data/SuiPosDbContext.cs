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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().Property(p => p.Price).HasPrecision(18, 2);
        modelBuilder.Entity<Order>().Property(o => o.TotalAmount).HasPrecision(18, 2);
        modelBuilder.Entity<OrderDetail>().Property(od => od.UnitPrice).HasPrecision(18, 2);
    }
}