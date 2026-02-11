using Microsoft.AspNetCore.Mvc;
using SuiPOS.Services.Interfaces;
using SuiPOS.ViewModels;

namespace SuiPOS.Controllers
{
    public class CustomersController : Controller
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var customers = await _customerService.GetAllAsync();
            return View(customers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu nhập vào không hợp lệ." });
            }

            var (success, message) = await _customerService.CreateAsync(model);
            return Json(new { success = success, message = message });
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomer(Guid id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null) return NotFound();

            return Json(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Guid id, CustomerViewModel model)
        {
            if (!ModelState.IsValid) return Json(new { success = false, message = "Thông tin không hợp lệ." });

            var (success, message) = await _customerService.UpdateAsync(id, model);
            return Json(new { success = success, message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _customerService.DeleteAsync(id);
            if (success)
            {
                return Json(new { success = true, message = "Đã xóa khách hàng thành công." });
            }
            return Json(new { success = false, message = "Không thể xóa khách hàng này." });
        }
    }
}
