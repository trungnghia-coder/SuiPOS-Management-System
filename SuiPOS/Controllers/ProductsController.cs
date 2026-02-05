using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SuiPOS.Data;
using SuiPOS.Services.Interfaces;
using SuiPOS.ViewModels;

namespace SuiPOS.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly SuiPosDbContext _context;

        public ProductsController(IProductService productService, SuiPosDbContext context)
        {
            _productService = productService;
            _context = context;
        }

        // GET: /Products
        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _productService.GetAllAsync();
                return View(products);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải danh sách sản phẩm: {ex.Message}";
                return View(new List<ProductVM>());
            }
        }

        // GET: /Products/GetCategories - API endpoint for categories tab
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _context.Categories
                    .Include(c => c.Products)
                    .Select(c => new
                    {
                        id = c.Id,
                        name = c.Name,
                        productCount = c.Products.Count,
                        description = "",
                        status = "Hoạt động"
                    })
                    .ToListAsync();

                return Json(categories);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // POST: /Products/CreateCategory
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryInputModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    return Json(new { success = false, message = "Tên loại sản phẩm không được để trống" });
                }

                var category = new Models.Category
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Tạo loại sản phẩm thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // DELETE: /Products/DeleteCategory/id
        [HttpPost]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy loại sản phẩm" });
                }

                if (category.Products.Any())
                {
                    return Json(new { success = false, message = "Không thể xóa loại sản phẩm đang có sản phẩm" });
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa loại sản phẩm thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // GET: /Product/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Không tìm thấy sản phẩm!";
                    return RedirectToAction(nameof(Index));
                }

                return View(product);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tải sản phẩm: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Product/Create
        public async Task<IActionResult> Create()
        {
            await LoadDropdownData();
            return View("ProductForm", new ProductInputVM());
        }

        // POST: /Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductInputVM model)
        {
            // Validate SKU unique
            if (model.Variants != null && model.Variants.Any())
            {
                var duplicateSKUs = model.Variants
                    .GroupBy(v => v.SKU)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key);

                if (duplicateSKUs.Any())
                {
                    ModelState.AddModelError("Variants", $"Duplicate SKU: {string.Join(", ", duplicateSKUs)}");
                }

                // Check if SKU already exists in DB
                var existingSKUs = await _context.ProductVariants
                    .Where(pv => model.Variants.Select(v => v.SKU).Contains(pv.SKU))
                    .Select(pv => pv.SKU)
                    .ToListAsync();

                if (existingSKUs.Any())
                {
                    ModelState.AddModelError("Variants", $"SKU already exists in system: {string.Join(", ", existingSKUs)}");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdownData();
                return View("ProductForm", model);
            }

            try
            {
                var result = await _productService.CreateAsync(model);

                if (result)
                {
                    TempData["Success"] = "Thêm sản phẩm thành công!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Không thể lưu sản phẩm. Vui lòng thử lại.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi: {ex.Message}");
            }

            await LoadDropdownData();
            return View(model);
        }

        // GET: /Product/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                // Get product from database with Category and Variants
                var product = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Variants)
                        .ThenInclude(v => v.SelectedValues)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    TempData["Error"] = "Product not found!";
                    return RedirectToAction(nameof(Index));
                }

                // Map Product to ProductInputVM
                var inputModel = new ProductInputVM
                {
                    Id = product.Id,
                    Name = product.Name,
                    CategoryId = product.CategoryId,
                    ExistingImageUrl = product.ImageUrl,
                    Variants = product.Variants.Select(v => new VariantInputVM
                    {
                        SKU = v.SKU,
                        Price = v.Price,
                        Stock = v.Stock,
                        SelectedAttributeValueIds = v.SelectedValues.Select(av => av.Id).ToList()
                    }).ToList()
                };

                await LoadDropdownData();
                return View(inputModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading product: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ProductInputVM model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            // Validate at least 1 variant
            if (model.Variants == null || !model.Variants.Any())
            {
                ModelState.AddModelError("Variants", "Product must have at least 1 variant");
            }

            // Validate SKU unique
            if (model.Variants != null && model.Variants.Any())
            {
                var duplicateSKUs = model.Variants
                    .GroupBy(v => v.SKU)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key);

                if (duplicateSKUs.Any())
                {
                    ModelState.AddModelError("Variants", $"Duplicate SKU: {string.Join(", ", duplicateSKUs)}");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdownData();
                return View(model);
            }

            try
            {
                var result = await _productService.UpdateAsync(id, model);

                if (result)
                {
                    TempData["Success"] = "Product updated successfully!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Could not update product. Please try again.");
            }
            catch (DbUpdateException dbEx)
            {
                // Log detailed database error
                var innerException = dbEx.InnerException?.Message ?? dbEx.Message;
                ModelState.AddModelError("", $"Database error: {innerException}");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }

            await LoadDropdownData();
            return View(model);
        }

        // POST: /Product/Delete/5 (AJAX)
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _productService.DeleteAsync(id);

                if (success)
                {
                    return Json(new { success = true, message = "Xóa sản phẩm thành công!" });
                }

                return Json(new { success = false, message = "Không tìm thấy sản phẩm!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // GET: /Product/GetAttributes (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetAttributes()
        {
            try
            {
                var attributes = await _context.ProductAttributes
                    .Include(a => a.Values)
                    .Select(a => new
                    {
                        id = a.Id,
                        name = a.Name,
                        values = a.Values.Select(v => new
                        {
                            id = v.Id,
                            value = v.Value
                        }).ToList()
                    })
                    .ToListAsync();
                return Json(attributes);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Helper method để load dropdown data
        private async Task LoadDropdownData()
        {
            ViewBag.Categories = new SelectList(
                await _context.Categories.ToListAsync(),
                "Id",
                "Name"
            );

            // Load attributes cho JavaScript
            var attributes = await _context.ProductAttributes
                .Include(a => a.Values)
                .ToListAsync();
            ViewBag.Attributes = attributes;
        }
    }

    // Input model for creating category
    public class CategoryInputModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
