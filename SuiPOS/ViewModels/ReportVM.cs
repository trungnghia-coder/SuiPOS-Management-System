namespace SuiPOS.ViewModels
{
    public class DashboardReportVM
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalReceived { get; set; }
        public decimal ActualReceived { get; set; }
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
        public List<RevenueByDateVM> RevenueByDate { get; set; } = new();
        public List<OrderCountByDateVM> OrderCountByDate { get; set; } = new();
        public List<TopSellingProductVM> TopSellingProducts { get; set; } = new();
    }

    public class RevenueByDateVM
    {
        public string Date { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }

    public class OrderCountByDateVM
    {
        public string Date { get; set; } = string.Empty;
        public int OrderCount { get; set; }
    }

    public class TopSellingProductVM
    {
        public string ProductName { get; set; } = string.Empty;
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public string? ImageUrl { get; set; }
    }
}
