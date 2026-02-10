using Microsoft.AspNetCore.Mvc;

namespace SuiPOS.Controllers
{
    public class ReportsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
