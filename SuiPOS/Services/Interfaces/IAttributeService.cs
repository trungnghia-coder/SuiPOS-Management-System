using SuiPOS.ViewModels;

namespace SuiPOS.Services.Interfaces
{
    public interface IAttributeService
    {
        Task<List<AttributeVM>> GetAllWithValuesAsync();
        Task<(bool Success, string Message)> CreateAttributeAsync(string name);
        Task<(bool Success, string Message)> DeleteAttributeAsync(Guid id);
        Task<(bool Success, string Message)> AddValueAsync(Guid attributeId, string value);
        Task<(bool Success, string Message)> DeleteValueAsync(Guid valueId);
    }
}
