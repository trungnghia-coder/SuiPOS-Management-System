using Microsoft.AspNetCore.Mvc;

namespace SuiPOS.Controllers
{
    public class POSController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
