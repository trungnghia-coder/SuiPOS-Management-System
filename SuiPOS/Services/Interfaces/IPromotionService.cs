using SuiPOS.ViewModels;

namespace SuiPOS.Services.Interfaces
{
    public interface IPromotionService
    {
        Task<List<PromotionListVM>> GetAllAsync();
        Task<List<PromotionListVM>> GetActivePromotionsAsync();
        Task<List<PromotionListVM>> GetValidPromotionsForOrderAsync(decimal orderAmount);
        Task<PromotionVM?> GetByIdAsync(Guid id);
        Task<(bool Success, string Message)> CreateAsync(PromotionVM model);
        Task<(bool Success, string Message)> UpdateAsync(PromotionVM model);
        Task<(bool Success, string Message)> DeleteAsync(Guid id);
        Task<(bool Success, string Message)> ToggleActiveAsync(Guid id);
    }
}
