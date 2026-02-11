using Microsoft.AspNetCore.Mvc;
using SuiPOS.Services.Interfaces;
using SuiPOS.ViewModels;

namespace SuiPOS.Controllers
{
    public class POSController : Controller
    {
        private readonly IOrderService _orderService;

        public POSController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Checkout([FromBody] OrderViewModel model)
        {
            if (model == null || !model.Items.Any())
            {
                return Json(new { success = false, message = "Empty Cart!!" });
            }

            try
            {
                var result = await _orderService.ProcessCheckoutAsync(model);
                return Json(new { success = true, orderId = result.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}
