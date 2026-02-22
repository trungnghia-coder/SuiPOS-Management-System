using Microsoft.EntityFrameworkCore;
using SuiPOS.Data;
using SuiPOS.Services.Interfaces;
using SuiPOS.ViewModels;

namespace SuiPOS.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly SuiPosDbContext _context;

        public ReportService(SuiPosDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardReportVM> GetDashboardReportAsync(
            DateTime? fromDate = null, 
            DateTime? toDate = null, 
            string chartGroupBy = "day")
        {
            // Set default date range (last 30 days)
            var from = fromDate ?? DateTime.Today.AddDays(-30);
            var to = toDate ?? DateTime.Today;
            var endOfDay = to.Date.AddDays(1).AddSeconds(-1);

            var report = new DashboardReportVM
            {
                TotalRevenue = await GetTotalRevenueAsync(from, endOfDay),
                TotalReceived = await GetTotalReceivedAsync(from, endOfDay),
                ActualReceived = await GetActualReceivedAsync(from, endOfDay),
                TotalOrders = await GetTotalOrdersAsync(from, endOfDay),
                TotalProducts = await GetTotalProductsSoldAsync(from, endOfDay)
            };

            // Get revenue by date
            report.RevenueByDate = await GetRevenueByDateAsync(from, endOfDay, chartGroupBy);

            // Get order count by date
            report.OrderCountByDate = await GetOrderCountByDateAsync(from, endOfDay, chartGroupBy);

            // Get top selling products
            report.TopSellingProducts = await GetTopSellingProductsAsync(10, from, endOfDay);

            return report;
        }

        private async Task<decimal> GetTotalRevenueAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.Orders
                .Where(o => o.Status == "Completed" && o.OrderDate >= fromDate && o.OrderDate <= toDate)
                .SumAsync(o => o.TotalAmount);
        }

        private async Task<decimal> GetTotalReceivedAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.Orders
                .Where(o => o.Status == "Completed" && o.OrderDate >= fromDate && o.OrderDate <= toDate)
                .SumAsync(o => o.AmountReceived);
        }

        private async Task<decimal> GetActualReceivedAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.Orders
                .Where(o => o.Status == "Completed" && o.OrderDate >= fromDate && o.OrderDate <= toDate)
                .SumAsync(o => o.AmountReceived - o.ChangeAmount);
        }

        private async Task<int> GetTotalOrdersAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.Orders
                .Where(o => o.Status == "Completed" && o.OrderDate >= fromDate && o.OrderDate <= toDate)
                .CountAsync();
        }

        private async Task<int> GetTotalProductsSoldAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.OrderDetails
                .Where(od => od.Order.Status == "Completed" 
                    && od.Order.OrderDate >= fromDate 
                    && od.Order.OrderDate <= toDate)
                .SumAsync(od => od.Quantity);
        }

        private async Task<List<RevenueByDateVM>> GetRevenueByDateAsync(DateTime fromDate, DateTime toDate, string groupBy)
        {
            var orders = await _context.Orders
                .Where(o => o.Status == "Completed" && o.OrderDate >= fromDate && o.OrderDate <= toDate)
                .ToListAsync();

            var grouped = groupBy.ToLower() switch
            {
                "month" => orders
                    .GroupBy(o => o.OrderDate.ToString("yyyy-MM"))
                    .Select(g => new RevenueByDateVM { Date = g.Key, Revenue = g.Sum(o => o.TotalAmount) }),
                "year" => orders
                    .GroupBy(o => o.OrderDate.ToString("yyyy"))
                    .Select(g => new RevenueByDateVM { Date = g.Key, Revenue = g.Sum(o => o.TotalAmount) }),
                _ => orders
                    .GroupBy(o => o.OrderDate.ToString("yyyy-MM-dd"))
                    .Select(g => new RevenueByDateVM { Date = g.Key, Revenue = g.Sum(o => o.TotalAmount) })
            };

            return grouped.OrderBy(r => r.Date).ToList();
        }

        private async Task<List<OrderCountByDateVM>> GetOrderCountByDateAsync(DateTime fromDate, DateTime toDate, string groupBy)
        {
            var orders = await _context.Orders
                .Where(o => o.Status == "Completed" && o.OrderDate >= fromDate && o.OrderDate <= toDate)
                .ToListAsync();

            var grouped = groupBy.ToLower() switch
            {
                "month" => orders
                    .GroupBy(o => o.OrderDate.ToString("yyyy-MM"))
                    .Select(g => new OrderCountByDateVM { Date = g.Key, OrderCount = g.Count() }),
                "year" => orders
                    .GroupBy(o => o.OrderDate.ToString("yyyy"))
                    .Select(g => new OrderCountByDateVM { Date = g.Key, OrderCount = g.Count() }),
                _ => orders
                    .GroupBy(o => o.OrderDate.ToString("yyyy-MM-dd"))
                    .Select(g => new OrderCountByDateVM { Date = g.Key, OrderCount = g.Count() })
            };

            return grouped.OrderBy(o => o.Date).ToList();
        }

        private async Task<List<TopSellingProductVM>> GetTopSellingProductsAsync(int top, DateTime fromDate, DateTime toDate)
        {
            var result = await _context.OrderDetails
                .Include(od => od.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .Include(od => od.Order)
                .Where(od => od.Order.Status == "Completed" 
                    && od.Order.OrderDate >= fromDate 
                    && od.Order.OrderDate <= toDate
                    && od.ProductVariant != null 
                    && od.ProductVariant.Product.isActive)
                .GroupBy(od => new
                {
                    ProductName = od.ProductVariant.Product.Name,
                    ImageUrl = od.ProductVariant.Product.ImageUrl
                })
                .Select(g => new TopSellingProductVM
                {
                    ProductName = g.Key.ProductName,
                    ImageUrl = g.Key.ImageUrl,
                    TotalSold = g.Sum(od => od.Quantity),
                    TotalRevenue = g.Sum(od => od.Quantity * od.UnitPrice)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(top)
                .ToListAsync();

            return result;
        }
    }
}

