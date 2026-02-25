using Dapper;
using SuiPOS.Data;
using SuiPOS.Services.Interfaces;
using SuiPOS.ViewModels;
using System.Data;

namespace SuiPOS.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public ReportService(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<DashboardReportVM> GetDashboardReportAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string chartGroupBy = "day")
        {

            var from = fromDate ?? DateTime.Today.AddDays(-30);
            var to = toDate ?? DateTime.Today;
            var endOfDay = to.Date.AddDays(1).AddSeconds(-1);

            var parameters = new DynamicParameters();
            parameters.Add("@FromDate", from);
            parameters.Add("@ToDate", endOfDay);
            parameters.Add("@GroupBy", chartGroupBy.ToLower());

            using var connection = await _dbConnectionFactory.CreateConnectionAsync();

            using var multi = await connection.QueryMultipleAsync(
                "sp_GetDashboardReport",
                parameters,
                commandType: CommandType.StoredProcedure);

            var report = new DashboardReportVM();

            var summary = await multi.ReadSingleOrDefaultAsync();
            if (summary != null)
            {
                report.TotalRevenue = (decimal)summary.TotalRevenue;
                report.TotalReceived = (decimal)summary.TotalReceived;
                report.ActualReceived = (decimal)summary.ActualReceived;
                report.TotalOrders = (int)summary.TotalOrders;
                report.TotalProducts = (int)summary.TotalProducts;
            }

            report.RevenueByDate = (await multi.ReadAsync<RevenueByDateVM>()).ToList();

            report.OrderCountByDate = (await multi.ReadAsync<OrderCountByDateVM>()).ToList();

            report.TopSellingProducts = (await multi.ReadAsync<TopSellingProductVM>()).ToList();

            return report;
        }
    }
}