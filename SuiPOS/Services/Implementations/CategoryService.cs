using Microsoft.EntityFrameworkCore;
using SuiPOS.Data;
using SuiPOS.Models;
using SuiPOS.Services.Interfaces;
using SuiPOS.ViewModels;

namespace SuiPOS.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly SuiPosDbContext _context;

        public CategoryService(SuiPosDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryVM>> GetAllAsync()
        {
            return await _context.Categories
                .Select(c => new CategoryVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    ProductCount = c.Products.Count,
                    IsActive = true
                })
                .ToListAsync();
        }

        public async Task<CategoryVM?> GetByIdAsync(Guid id)
        {
            var category = await _context.Categories
                .Where(c => c.Id == id)
                .Select(c => new CategoryVM { Id = c.Id, Name = c.Name })
                .FirstOrDefaultAsync();
            return category;
        }

        public async Task<(bool Success, string Message)> CreateAsync(CategoryInputModel model)
        {
            if (await _context.Categories.AnyAsync(c => c.Name == model.Name))
                return (false, "Tên loại sản phẩm này đã tồn tại.");

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = model.Name
            };

            _context.Categories.Add(category);
            var result = await _context.SaveChangesAsync();
            return result > 0 ? (true, "Thêm loại sản phẩm thành công.") : (false, "Lỗi khi lưu dữ liệu.");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(Guid id, CategoryInputModel model)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return (false, "Không tìm thấy loại sản phẩm.");

            category.Name = model.Name;
            var result = await _context.SaveChangesAsync();
            return result > 0 ? (true, "Cập nhật thành công.") : (false, "Cập nhật thất bại.");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(Guid id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return (false, "Loại sản phẩm không tồn tại.");

            if (category.Products.Any())
                return (false, "Không thể xóa vì loại này đang chứa sản phẩm. Hãy xóa sản phẩm trước.");

            _context.Categories.Remove(category);
            var result = await _context.SaveChangesAsync();
            return result > 0 ? (true, "Xóa thành công.") : (false, "Xóa thất bại.");
        }
    }
}
