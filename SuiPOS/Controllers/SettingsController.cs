using Microsoft.AspNetCore.Mvc;

namespace SuiPOS.Controllers
{
    public class SettingsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
