using Microsoft.AspNetCore.Mvc;

namespace SuiPOS.Controllers
{
    public class CustomersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
