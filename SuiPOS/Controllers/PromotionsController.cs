using Microsoft.AspNetCore.Mvc;
using SuiPOS.Services.Interfaces;
using SuiPOS.ViewModels;

namespace SuiPOS.Controllers
{
    public class PromotionsController : Controller
    {
        private readonly IPromotionService _promotionService;

        public PromotionsController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        public async Task<IActionResult> Index()
        {
            var promotions = await _promotionService.GetAllAsync();
            return View(promotions);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var promotions = await _promotionService.GetAllAsync();
            return Json(new { success = true, data = promotions });
        }

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id)
        {
            var promotion = await _promotionService.GetByIdAsync(id);
            if (promotion == null)
            {
                return Json(new { success = false, message = "Không tìm th?y khuy?n mãi" });
            }
            return Json(new { success = true, data = promotion });
        }

        [HttpGet]
        public async Task<IActionResult> GetValidPromotions(decimal orderAmount)
        {
            var promotions = await _promotionService.GetValidPromotionsForOrderAsync(orderAmount);
            return Json(new { success = true, data = promotions });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Create([FromBody] PromotionVM model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            var (success, message) = await _promotionService.CreateAsync(model);
            return Json(new { success, message });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Update([FromBody] PromotionVM model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            var (success, message) = await _promotionService.UpdateAsync(model);
            return Json(new { success, message });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var (success, message) = await _promotionService.DeleteAsync(id);
            return Json(new { success, message });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            var (success, message) = await _promotionService.ToggleActiveAsync(id);
            return Json(new { success, message });
        }
    }
}
