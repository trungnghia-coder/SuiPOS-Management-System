using Microsoft.AspNetCore.Mvc;
using SuiPOS.Services.Interfaces;
using SuiPOS.ViewModels;

namespace SuiPOS.Controllers
{
    public class POSController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;

        public POSController(IOrderService orderService, IProductService productService)
        {
            _orderService = orderService;
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllAsync();
            return View(products);
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
                var (success, message, orderId) = await _orderService.CreateOrderAsync(model);
                return Json(new { success, message, orderId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderDetail(Guid orderId)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found" });
                }
                return Json(new { success = true, data = order });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
