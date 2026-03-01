using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SuiPOS.Services.Interfaces;
using SuiPOS.ViewModels;

namespace SuiPOS.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductsController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(int page = 1, int size = 50)
        {
            var products = await _productService.GetAllAsync(page, size);

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = size;

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _categoryService.GetAllAsync(), "Id", "Name");
            return View("ProductForm", new ProductInputVM());
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var product = await _productService.GetByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            var model = new ProductInputVM
            {
                Id = product.Id,
                Name = product.ProductName,
                CategoryId = product.CategoryId,
                ExistingImageUrl = product.ImageUrl,
                // ✅ Map variants with SelectedAttributeValueIds from SelectedValues
                Variants = product.Variants?.Select(v => new VariantInputVM
                {
                    Id = v.Id,
                    SKU = v.SKU,
                    Price = v.Price,
                    Stock = v.Stock,
                    Combination = v.Combination,
                    // ✅ CRITICAL: Extract IDs from SelectedValues
                    SelectedAttributeValueIds = v.SelectedValues?.Select(sv => sv.Id).ToList() ?? new List<Guid>()
                }).ToList() ?? new List<VariantInputVM>()
            };

            ViewBag.Categories = new SelectList(await _categoryService.GetAllAsync(), "Id", "Name", model.CategoryId);
            return View("ProductForm", model);
        }


        [HttpGet]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound(new { message = "Không tìm thấy sản phẩm" });

            return Json(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] ProductInputVM model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = $"Dữ liệu không hợp lệ: {string.Join(", ", errors)}" });
            }

            var (success, message) = await _productService.CreateAsync(model);
            return Json(new { success, message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Guid id, [FromForm] ProductInputVM model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = $"Dữ liệu không hợp lệ: {string.Join(", ", errors)}" });
            }

            var (success, message) = await _productService.UpdateAsync(id, model);
            return Json(new { success, message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var (success, message) = await _productService.DeleteAsync(id);
            return Json(new { success, message });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryService.GetAllAsync();
            return Json(categories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryInputModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = $"Dữ liệu không hợp lệ: {string.Join(", ", errors)}" });
            }

            var (success, message) = await _categoryService.CreateAsync(model);
            return Json(new { success, message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var (success, message) = await _categoryService.DeleteAsync(id);
            return Json(new { success, message });
        }

        private async Task LoadDropdownData()
        {
            var categories = await _categoryService.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
        }
    }
}
