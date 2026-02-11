using SuiPOS.DTOs.Customers;
using SuiPOS.ViewModels;

namespace SuiPOS.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<List<CustomerTableDto>> GetAllAsync();
        Task<CustomerViewModel?> GetByIdAsync(Guid id);
        Task<bool> DeleteAsync(Guid id);
        Task<(bool Success, string Message)> CreateAsync(CustomerViewModel model);
        Task<(bool Success, string Message)> UpdateAsync(Guid id, CustomerViewModel model);
    }
}
