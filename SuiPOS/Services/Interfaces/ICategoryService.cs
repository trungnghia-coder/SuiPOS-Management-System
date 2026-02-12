using SuiPOS.ViewModels;

namespace SuiPOS.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryVM>> GetAllAsync();
        Task<CategoryVM?> GetByIdAsync(Guid id);
        Task<(bool Success, string Message)> CreateAsync(CategoryInputModel model);
        Task<(bool Success, string Message)> UpdateAsync(Guid id, CategoryInputModel model);
        Task<(bool Success, string Message)> DeleteAsync(Guid id);
    }
}
