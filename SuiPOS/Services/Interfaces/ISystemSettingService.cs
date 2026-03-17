namespace SuiPOS.Services.Interfaces
{
    public interface ISystemSettingService
    {
        Task<Dictionary<string, string>> GetSettingsByCategoryAsync(string category);
        Task<bool> UpdateSettingsAsync(Dictionary<string, string> settings, Guid? updatedBy = null);
    }
}
