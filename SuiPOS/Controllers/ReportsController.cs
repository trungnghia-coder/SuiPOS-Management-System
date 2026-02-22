using Microsoft.AspNetCore.Mvc;
using SuiPOS.Services.Interfaces;

namespace SuiPOS.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardData(DateTime? fromDate, DateTime? toDate, string groupBy = "day")
        {
            var report = await _reportService.GetDashboardReportAsync(fromDate, toDate, groupBy);
            return Json(report);
        }
    }
}

