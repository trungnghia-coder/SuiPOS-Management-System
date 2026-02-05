using Microsoft.AspNetCore.Mvc;

namespace SuiPOS.Controllers
{
    public class OrdersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Refund(string orderId)
        {
            ViewData["OrderId"] = orderId;
            return View();
        }
    }
}
