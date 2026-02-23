using Microsoft.AspNetCore.Mvc;
using SuiPOS.Services.Interfaces;
using SuiPOS.ViewModels;

namespace SuiPOS.Controllers
{
    public class AttributesController : Controller
    {
        private readonly IAttributeService _attributeService;

        public AttributesController(IAttributeService attributeService)
        {
            _attributeService = attributeService;
        }

        // View for managing attributes
        public async Task<IActionResult> Index()
        {
            var attributes = await _attributeService.GetAllAsync();
            return View(attributes);
        }

        // API endpoints
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _attributeService.GetAllAsync();
            return Json(new { success = true, data });
        }

        [HttpGet]
        public async Task<IActionResult> GetWithValues()
        {
            var data = await _attributeService.GetAllWithValuesAsync();
            return Json(new { success = true, data });
        }

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id)
        {
            var attribute = await _attributeService.GetByIdAsync(id);
            if (attribute == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thuộc tính" });
            }
            return Json(new { success = true, data = attribute });
        }

        [HttpGet]
        public async Task<IActionResult> GetValues(Guid attributeId)
        {
            var values = await _attributeService.GetValuesByAttributeIdAsync(attributeId);
            return Json(new { success = true, data = values });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CreateAttribute([FromBody] Dictionary<string, string> data)
        {
            if (!data.ContainsKey("name") || string.IsNullOrWhiteSpace(data["name"]))
            {
                return Json(new { success = false, message = "Tên thuộc tính là bắt buộc" });
            }

            var (success, message) = await _attributeService.CreateAttributeAsync(data["name"]);
            return Json(new { success, message });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateAttribute([FromBody] Dictionary<string, string> data)
        {
            if (!data.ContainsKey("id") || !Guid.TryParse(data["id"], out var id))
            {
                return Json(new { success = false, message = "ID không hợp lệ" });
            }

            if (!data.ContainsKey("name") || string.IsNullOrWhiteSpace(data["name"]))
            {
                return Json(new { success = false, message = "Tên thuộc tính là bắt buộc" });
            }

            var (success, message) = await _attributeService.UpdateAttributeAsync(id, data["name"]);
            return Json(new { success, message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAttribute(Guid id)
        {
            var (success, message) = await _attributeService.DeleteAttributeAsync(id);
            return Json(new { success, message });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AddValue([FromBody] CreateAttributeValueRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            var (success, message) = await _attributeService.AddValueAsync(request.AttributeId, request.Value);
            return Json(new { success, message });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateValue([FromBody] Dictionary<string, string> data)
        {
            if (!data.ContainsKey("id") || !Guid.TryParse(data["id"], out var id))
            {
                return Json(new { success = false, message = "ID không hợp lệ" });
            }

            if (!data.ContainsKey("value") || string.IsNullOrWhiteSpace(data["value"]))
            {
                return Json(new { success = false, message = "Giá trị là bắt buộc" });
            }

            var (success, message) = await _attributeService.UpdateValueAsync(id, data["value"]);
            return Json(new { success, message });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteValue(Guid id)
        {
            var (success, message) = await _attributeService.DeleteValueAsync(id);
            return Json(new { success, message });
        }
    }
}

