using SuiPOS.ViewModels;

namespace SuiPOS.Services.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductVM>> GetAllAsync();
        Task<ProductVM?> GetByIdAsync(Guid id);
        Task<bool> CreateAsync(ProductInputVM model);
        Task<bool> UpdateAsync(Guid id, ProductInputVM model);
        Task<bool> DeleteAsync(Guid id);
    }
}
