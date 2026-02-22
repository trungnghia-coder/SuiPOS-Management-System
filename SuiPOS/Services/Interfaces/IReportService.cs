using SuiPOS.ViewModels;

namespace SuiPOS.Services.Interfaces
{
    public interface IReportService
    {
        Task<DashboardReportVM> GetDashboardReportAsync(DateTime? fromDate = null, DateTime? toDate = null, string chartGroupBy = "day");
    }
}
