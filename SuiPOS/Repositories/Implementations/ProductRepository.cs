using Microsoft.EntityFrameworkCore;
using SuiPOS.Data;
using SuiPOS.Models;
using SuiPOS.Repositories.Interfaces;

namespace SuiPOS.Repositories.Implementations
{
    public class ProductRepository : IProductRepository
    {
        private readonly SuiPosDbContext _context;

        public ProductRepository(SuiPosDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
        }

        public async Task Delete(Product product)
        {
            _context.Products.Remove(product);
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Variants)
                .ToListAsync();
        }

        public Task<Product?> GetByIdAsync(Guid id)
        {
            return _context.Products
                .Include(p => p.Category)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.SelectedValues)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() >= 0);
        }

        public async Task Update(Product product)
        {
            _context.Products.Update(product);
        }
    }
}
