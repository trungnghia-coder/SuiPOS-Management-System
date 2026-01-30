using Microsoft.EntityFrameworkCore;
using SuiPOS.Data;
using SuiPOS.Models;
using SuiPOS.Repositories.Interfaces;

namespace SuiPOS.Repositories.Implementations
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly SuiPosDbContext _context;

        public CategoryRepository(SuiPosDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
        }

        public void Delete(Category category)
        {
            _context.Categories.Remove(category);
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public Task<Category?> GetByIdAsync(int id)
        {
            return _context.Categories.FindAsync(id).AsTask();
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() >= 0;
        }

        public void Update(Category category)
        {
            _context.Categories.Update(category);
        }
    }
}
