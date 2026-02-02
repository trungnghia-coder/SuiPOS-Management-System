using SuiPOS.Models;

namespace SuiPOS.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(Guid id);
        Task AddAsync(Product product);
        Task Update(Product product);
        Task Delete(Product product);
        Task<bool> SaveChangesAsync();
    }
}
