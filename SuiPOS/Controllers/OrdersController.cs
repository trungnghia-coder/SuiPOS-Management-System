using Microsoft.AspNetCore.Mvc;
using SuiPOS.Services.Interfaces;

namespace SuiPOS.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders(DateTime? fromDate, DateTime? toDate)
        {
            var orders = await _orderService.GetOrdersAsync(fromDate, toDate);
            return Json(orders);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrderDetail(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return Json(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var (success, message) = await _orderService.CancelOrderAsync(id);
            return Json(new { success, message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refund(Guid id)
        {
            var (success, message) = await _orderService.RefundOrderAsync(id);
            return Json(new { success, message });
        }

        [HttpPost]
        public async Task<IActionResult> ValidateStock([FromBody] ValidateStockRequest request)
        {
            var items = request.Items.Select(i => (i.VariantId, i.Quantity)).ToList();
            var result = await _orderService.ValidateStockForReorderAsync(items);
            return Json(result);
        }
    }

    public class ValidateStockRequest
    {
        public List<ValidateStockItem> Items { get; set; } = new();
    }

    public class ValidateStockItem
    {
        public Guid VariantId { get; set; }
        public int Quantity { get; set; }
    }
}
