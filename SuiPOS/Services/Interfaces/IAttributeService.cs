using SuiPOS.ViewModels;

namespace SuiPOS.Services.Interfaces
{
    public interface IAttributeService
    {
        // Attribute CRUD
        Task<List<AttributeListVM>> GetAllAsync();
        Task<List<AttributeVM>> GetAllWithValuesAsync();
        Task<AttributeVM?> GetByIdAsync(Guid id);
        Task<(bool Success, string Message)> CreateAttributeAsync(string name);
        Task<(bool Success, string Message)> UpdateAttributeAsync(Guid id, string name);
        Task<(bool Success, string Message)> DeleteAttributeAsync(Guid id);

        // AttributeValue CRUD
        Task<(bool Success, string Message)> AddValueAsync(Guid attributeId, string value);
        Task<(bool Success, string Message)> UpdateValueAsync(Guid valueId, string value);
        Task<(bool Success, string Message)> DeleteValueAsync(Guid valueId);

        // Helper methods
        Task<List<AttributeValueVM>> GetValuesByAttributeIdAsync(Guid attributeId);
    }
}

