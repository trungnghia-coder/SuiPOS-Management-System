namespace SuiPOS.Services.Interfaces
{
    public interface ISystemSettingService
    {
        Task<string?> GetSettingAsync(string key);
        Task<T?> GetSettingAsync<T>(string key);
        Task<Dictionary<string, string>> GetSettingsByCategoryAsync(string category);
        Task<bool> UpdateSettingAsync(string key, string value, Guid? updatedBy = null);
        Task<bool> UpdateSettingsAsync(Dictionary<string, string> settings, Guid? updatedBy = null);
    }
}
