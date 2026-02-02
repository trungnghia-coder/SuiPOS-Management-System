using Microsoft.EntityFrameworkCore;
using SuiPOS.Data;
using SuiPOS.Models;
using SuiPOS.Repositories.Interfaces;
using SuiPOS.Services.Interfaces;
using SuiPOS.ViewModels;

namespace SuiPOS.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepo;
        private readonly SuiPosDbContext _context;
        private readonly IFileService _fileService;

        public ProductService(IProductRepository productRepo, SuiPosDbContext context, IFileService fileService)
        {
            _productRepo = productRepo;
            _context = context;
            _fileService = fileService;
        }

        // Create a new product with variants
        public async Task<bool> CreateAsync(ProductInputVM model)
        {
            // Upload product image nếu có
            string? productImageUrl = null;
            if (model.ImageFile != null)
            {
                productImageUrl = await _fileService.UploadImageAsync(model.ImageFile, "products");
            }

            var product = new Product
            {
                Name = model.Name,
                CategoryId = model.CategoryId,
                ImageUrl = productImageUrl,
                Variants = new List<ProductVariant>()
            };

            foreach (var variantInput in model.Variants)
            {
                var selectedValues = await _context.AttributeValues
                    .Where(av => variantInput.SelectedAttributeValueIds.Contains(av.Id))
                    .ToListAsync();

                var combination = string.Join(", ", selectedValues.Select(av => av.Value));

                var variant = new ProductVariant
                {
                    SKU = variantInput.SKU,
                    Price = variantInput.Price,
                    Stock = variantInput.Stock,
                    VariantCombination = combination,
                    SelectedValues = selectedValues
                };

                product.Variants.Add(variant);
            }

            await _productRepo.AddAsync(product);
            await _productRepo.SaveChangesAsync();
            return true;
        }

        // Delete a product by ID
        public async Task<bool> DeleteAsync(Guid id)
        {
            Product product = await _productRepo.GetByIdAsync(id);
            if (product == null) return false;

            // Xóa ảnh product nếu có
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                await _fileService.DeleteImageAsync(product.ImageUrl);
            }

            await _productRepo.Delete(product);
            await _productRepo.SaveChangesAsync();
            return true;
        }

        // Get all products with their variants
        public async Task<List<ProductVM>> GetAllAsync()
        {
            var products = await _productRepo.GetAllAsync();
            return products.Select(p => MapToVM(p)).ToList();
        }

        // Get a product by ID with its variants
        public async Task<ProductVM?> GetByIdAsync(Guid id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            return product != null ? MapToVM(product) : null;
        }

        // Update an existing product
        public async Task<bool> UpdateAsync(Guid id, ProductInputVM model)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product == null) return false;

            product.Name = model.Name;
            product.CategoryId = model.CategoryId;
            await _productRepo.Update(product);
            await _productRepo.SaveChangesAsync();
            return true;
        }

        private ProductVM MapToVM(Product p)
        {
            return new ProductVM
            {
                Id = p.Id,
                Name = p.Name,
                ImageUrl = p.ImageUrl,
                CategoryName = p.Category?.Name ?? "No category",
                Variants = p.Variants.Select(v => new VariantDisplayVM
                {
                    Id = v.Id,
                    SKU = v.SKU,
                    Combination = v.VariantCombination,
                    Price = v.Price,
                    Stock = v.Stock
                }).ToList()
            };
        }
    }
}
