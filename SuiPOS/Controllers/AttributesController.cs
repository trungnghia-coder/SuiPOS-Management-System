using Microsoft.AspNetCore.Mvc;
using SuiPOS.Services.Interfaces;

namespace SuiPOS.Controllers
{
    public class AttributesController : Controller
    {
        private readonly IAttributeService _attributeService;
        public AttributesController(IAttributeService attributeService) => _attributeService = attributeService;

        [HttpGet]
        public async Task<IActionResult> GetWithValues()
        {
            var data = await _attributeService.GetAllWithValuesAsync();
            return Json(data);
        }
    }
}
