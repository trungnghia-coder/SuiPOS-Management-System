using Microsoft.EntityFrameworkCore;
using SuiPOS.Data;
using SuiPOS.Services.Interfaces;

namespace SuiPOS.Services.Implementations
{
    public class SystemSettingService : ISystemSettingService
    {
        private readonly SuiPosDbContext _context;

        public SystemSettingService(SuiPosDbContext context)
        {
            _context = context;
        }

        public async Task<string?> GetSettingAsync(string key)
        {
            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Key == key);

            return setting?.Value;
        }

        public async Task<T?> GetSettingAsync<T>(string key)
        {
            var value = await GetSettingAsync(key);
            if (value == null) return default;

            try
            {
                if (typeof(T) == typeof(bool))
                {
                    return (T)(object)bool.Parse(value);
                }
                else if (typeof(T) == typeof(int))
                {
                    return (T)(object)int.Parse(value);
                }
                else if (typeof(T) == typeof(decimal))
                {
                    return (T)(object)decimal.Parse(value);
                }
                else if (typeof(T) == typeof(string))
                {
                    return (T)(object)value;
                }
            }
            catch
            {
                return default;
            }

            return default;
        }

        public async Task<Dictionary<string, string>> GetSettingsByCategoryAsync(string category)
        {
            var settings = await _context.SystemSettings
                .Where(s => s.Category == category)
                .ToDictionaryAsync(s => s.Key, s => s.Value);

            return settings;
        }

        public async Task<bool> UpdateSettingAsync(string key, string value, Guid? updatedBy = null)
        {
            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Key == key);

            if (setting == null)
            {
                return false;
            }

            setting.Value = value;
            setting.UpdatedAt = DateTime.UtcNow;
            setting.UpdatedBy = updatedBy;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateSettingsAsync(Dictionary<string, string> settings, Guid? updatedBy = null)
        {
            foreach (var kvp in settings)
            {
                var setting = await _context.SystemSettings
                    .FirstOrDefaultAsync(s => s.Key == kvp.Key);

                if (setting != null)
                {
                    // Update existing setting
                    setting.Value = kvp.Value;
                    setting.UpdatedAt = DateTime.UtcNow;
                    setting.UpdatedBy = updatedBy;
                }
                else
                {
                    // Create new setting if not exists
                    var category = GetCategoryFromKey(kvp.Key);
                    var dataType = GetDataTypeFromValue(kvp.Value);
                    
                    _context.SystemSettings.Add(new Models.SystemSetting
                    {
                        Id = Guid.NewGuid(),
                        Key = kvp.Key,
                        Value = kvp.Value,
                        Category = category,
                        DataType = dataType,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedBy = updatedBy
                    });
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private string GetCategoryFromKey(string key)
        {
            if (key.StartsWith("store_") || key.Contains("invoice_footer"))
                return "Store";
            if (key.StartsWith("show_") || key.Contains("print") || key.Contains("paper"))
                return "Invoice";
            if (key.Contains("printer"))
                return "Printer";
            return "General";
        }

        private string GetDataTypeFromValue(string value)
        {
            if (bool.TryParse(value, out _))
                return "bool";
            if (int.TryParse(value, out _))
                return "int";
            if (decimal.TryParse(value, out _))
                return "decimal";
            return "string";
        }
    }
}
