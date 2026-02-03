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
            // Use explicit transaction to ensure atomicity
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var product = await _context.Products
                    .Include(p => p.Variants)
                        .ThenInclude(v => v.SelectedValues)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null) return false;

                // Update basic info
                product.Name = model.Name;
                product.CategoryId = model.CategoryId;

                // Save current image URL BEFORE any changes
                var originalImageUrl = product.ImageUrl;

                // Update image ONLY if new file uploaded
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(originalImageUrl))
                    {
                        await _fileService.DeleteImageAsync(originalImageUrl);
                    }

                    // Upload new image
                    product.ImageUrl = await _fileService.UploadImageAsync(model.ImageFile, "products");
                }
                // ELSE: Keep original image URL (don't change)

                // Check if any new SKU already exists in OTHER products
                var newSKUs = model.Variants.Select(v => v.SKU).ToList();
                var existingSKUs = await _context.ProductVariants
                    .AsNoTracking() // Don't track this query
                    .Where(pv => pv.ProductId != id && newSKUs.Contains(pv.SKU))
                    .Select(pv => pv.SKU)
                    .ToListAsync();

                if (existingSKUs.Any())
                {
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException($"SKU already exists in other products: {string.Join(", ", existingSKUs)}");
                }

                // Get old variant IDs
                var oldVariantIds = product.Variants.Select(v => v.Id).ToList();

                // Check if any old variant is used in orders
                var hasOrders = await _context.OrderDetails
                    .AsNoTracking()
                    .AnyAsync(od => oldVariantIds.Contains(od.ProductVariantId));

                if (hasOrders)
                {
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException("Cannot update variants because they are used in existing orders.");
                }

                // STEP 1: Clear many-to-many relationships
                foreach (var variant in product.Variants.ToList())
                {
                    variant.SelectedValues.Clear();
                }
                await _context.SaveChangesAsync(); // Save to delete junction table entries

                // STEP 2: Remove old variants
                var variantsToRemove = product.Variants.ToList();
                foreach (var variant in variantsToRemove)
                {
                    _context.ProductVariants.Remove(variant);
                }
                product.Variants.Clear();
                await _context.SaveChangesAsync(); // Save to delete ProductVariants

                // STEP 3: Clear change tracker to avoid tracking conflicts
                _context.ChangeTracker.Clear();

                // STEP 4: Re-attach product (without variants)
                var freshProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == id);
                
                if (freshProduct == null)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                // Update basic info
                freshProduct.Name = product.Name;
                freshProduct.CategoryId = product.CategoryId;
                
                // Update ImageUrl ONLY if it was changed (new file uploaded)
                if (product.ImageUrl != originalImageUrl)
                {
                    freshProduct.ImageUrl = product.ImageUrl;
                }
                // ELSE: Keep existing ImageUrl in database (don't overwrite)

                // STEP 5: Add new variants with fresh context
                foreach (var variantInput in model.Variants)
                {
                    var selectedValues = await _context.AttributeValues
                        .Where(av => variantInput.SelectedAttributeValueIds.Contains(av.Id))
                        .ToListAsync();

                    var combination = string.Join(", ", selectedValues.Select(av => av.Value));

                    var newVariant = new ProductVariant
                    {
                        ProductId = freshProduct.Id,
                        SKU = variantInput.SKU,
                        Price = variantInput.Price,
                        Stock = variantInput.Stock,
                        VariantCombination = combination
                    };

                    // Add to context first
                    _context.ProductVariants.Add(newVariant);
                    
                    // Attach selected values
                    foreach (var selectedValue in selectedValues)
                    {
                        newVariant.SelectedValues.Add(selectedValue);
                    }
                }

                // STEP 6: Final save
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
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
