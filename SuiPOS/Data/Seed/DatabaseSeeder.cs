using Microsoft.EntityFrameworkCore;
using SuiPOS.Models;

namespace SuiPOS.Data.Seed
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(SuiPosDbContext context, bool forceReseed = false)
        {
            Console.WriteLine("Starting database seeding...");



            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // 1. Seed Customers (50)
                Console.WriteLine("Seeding 50 customers...");
                var customers = CustomerSeeder.GenerateCustomers(50);
                await context.Customers.AddRangeAsync(customers);
                await context.SaveChangesAsync();
                Console.WriteLine("50 customers seeded");

                // 2. Seed Products & Variants (100)
                Console.WriteLine("Seeding 100 products with variants...");
                var (products, variants) = ProductSeeder.GenerateProducts(100);
                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
                await context.ProductVariants.AddRangeAsync(variants);
                await context.SaveChangesAsync();
                Console.WriteLine($"100 products and {variants.Count} variants seeded");

                // 3. Seed Promotions (10)
                Console.WriteLine("Seeding 10 promotions...");
                var promotions = PromotionSeeder.GeneratePromotions(10);
                await context.Promotions.AddRangeAsync(promotions);
                await context.SaveChangesAsync();
                Console.WriteLine("10 promotions seeded");

                // 4. Get default staff for orders
                var staff = await context.Staffs.FirstOrDefaultAsync();
                if (staff == null)
                {
                    Console.WriteLine("No staff found. Creating default staff...");
                    staff = new Staff
                    {
                        Id = Guid.NewGuid(),
                        Username = "admin",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                        FullName = "Administrator",
                        RoleId = Guid.Parse("00000000-0000-0000-0000-000000000001")
                    };
                    await context.Staffs.AddAsync(staff);
                    await context.SaveChangesAsync();
                }

                // 5. Seed Orders (100)
                Console.WriteLine("Seeding 100 orders...");
                var (orders, orderDetails, paymentsData) = OrderSeeder.GenerateOrders(
                    100,
                    customers,
                    variants,
                    staff.Id
                );
                await context.Orders.AddRangeAsync(orders);
                await context.SaveChangesAsync();
                await context.OrderDetails.AddRangeAsync(orderDetails);
                await context.SaveChangesAsync();
                await context.Payments.AddRangeAsync(paymentsData);
                await context.SaveChangesAsync();
                Console.WriteLine($"100 orders with {orderDetails.Count} items and {paymentsData.Count} payments seeded");

                // 6. Update customer debt and total spent
                Console.WriteLine("Updating customer financials...");
                var customerOrders = orders
                    .Where(o => o.CustomerId.HasValue)
                    .GroupBy(o => o.CustomerId!.Value)
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var customer in customers)
                {
                    if (customerOrders.TryGetValue(customer.Id, out var custOrders))
                    {
                        customer.TotalSpent = custOrders.Sum(o => o.TotalAmount - (o.Discount ?? 0));
                        customer.DebtAmount = custOrders.Sum(o =>
                            Math.Max(0, (o.TotalAmount - (o.Discount ?? 0)) - o.AmountReceived)
                        );
                    }
                }

                await context.SaveChangesAsync();
                Console.WriteLine("Customer financials updated");

                await transaction.CommitAsync();
                Console.WriteLine("Database seeding completed successfully!");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error during seeding: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}
