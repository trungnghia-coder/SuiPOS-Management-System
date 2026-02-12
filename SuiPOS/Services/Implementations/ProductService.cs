using Microsoft.EntityFrameworkCore;
using SuiPOS.Data;
using SuiPOS.Models;
using SuiPOS.Services.Interfaces;
using SuiPOS.ViewModels;

namespace SuiPOS.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly SuiPosDbContext _context;
        private readonly IFileService _fileService;

        public ProductService(SuiPosDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<(bool Success, string Message)> CreateAsync(ProductInputVM model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string? uploadedUrl = null;
                if (model.ImageFile != null)
                {
                    uploadedUrl = await _fileService.UploadImageAsync(model.ImageFile);
                }

                var product = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name,
                    CategoryId = model.CategoryId,
                    ImageUrl = uploadedUrl
                };

                if (model.Variants != null && model.Variants.Any())
                {
                    foreach (var v in model.Variants)
                    {
                        var variant = new ProductVariant
                        {
                            Id = Guid.NewGuid(),
                            ProductId = product.Id,
                            SKU = v.SKU,
                            Price = v.Price,
                            Stock = v.Stock,
                            VariantCombination = v.Combination ?? ""
                        };
                        _context.ProductVariants.Add(variant);
                    }
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return (true, "Sản phẩm và các phiên bản đã được thêm thành công.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return (false, $"Lỗi hệ thống: {innerMessage}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteAsync(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return (false, "Sản phẩm không tồn tại.");

            product.isActive = false;

            _context.Products.Update(product);

            await _context.SaveChangesAsync();
            return (true, "Xóa thành công.");
        }

        public async Task<List<ProductVM>> GetAllAsync()
        {
            return await _context.Database
                .SqlQueryRaw<ProductVM>("EXEC GetProductList")
                .ToListAsync();
        }

        public async Task<ProductVM?> GetByIdAsync(Guid id)
        {
            return await _context.Products
                .Include(p => p.Variants)
                .Include(p => p.Category)
                .Where(p => p.Id == id)
                .Select(p => new ProductVM
                {
                    Id = p.Id,
                    ProductName = p.Name,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : "",
                    ImageUrl = p.ImageUrl,
                    isActive = p.isActive,
                    Variants = p.Variants.Select(v => new VariantDisplayVM
                    {
                        Id = v.Id,
                        SKU = v.SKU,
                        Price = v.Price,
                        Stock = v.Stock,
                        Combination = v.VariantCombination
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<(bool Success, string Message)> UpdateAsync(Guid id, ProductInputVM model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var product = await _context.Products
                    .Include(p => p.Variants)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null) return (false, "Sản phẩm không tồn tại.");

                if (model.ImageFile != null)
                {
                    if (!string.IsNullOrEmpty(product.ImageUrl))
                        await _fileService.DeleteImageAsync(product.ImageUrl);
                    product.ImageUrl = await _fileService.UploadImageAsync(model.ImageFile);
                }

                product.Name = model.Name;
                product.CategoryId = model.CategoryId;

                var updatedSkuList = model.Variants.Select(v => v.SKU).ToList();
                var variantsToRemove = product.Variants
                    .Where(v => !updatedSkuList.Contains(v.SKU)).ToList();
                _context.ProductVariants.RemoveRange(variantsToRemove);

                foreach (var vModel in model.Variants)
                {
                    var existingVariant = product.Variants.FirstOrDefault(v => v.SKU == vModel.SKU);
                    if (existingVariant != null)
                    {
                        existingVariant.Price = vModel.Price;
                        existingVariant.Stock = vModel.Stock;

                        if (!string.IsNullOrWhiteSpace(vModel.Combination))
                        {
                            existingVariant.VariantCombination = vModel.Combination;
                        }
                    }
                    else
                    {
                        _context.ProductVariants.Add(new ProductVariant
                        {
                            Id = Guid.NewGuid(),
                            ProductId = product.Id,
                            SKU = vModel.SKU,
                            Price = vModel.Price,
                            Stock = vModel.Stock,
                            VariantCombination = vModel.Combination ?? ""
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (true, "Cập nhật thành công.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Lỗi: {ex.Message}");
            }
        }
    }
}
