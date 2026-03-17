using Dapper;
using Microsoft.EntityFrameworkCore;
using SuiPOS.Data;
using SuiPOS.Services.Interfaces;
using System.Data;
using System.Text.Json;

namespace SuiPOS.Services.Implementations
{
    public class SystemSettingService : ISystemSettingService
    {
        private readonly SuiPosDbContext _context;
        private readonly IDbConnectionFactory _dbFactory;

        public SystemSettingService(SuiPosDbContext context, IDbConnectionFactory dbFactory)
        {
            _context = context;
            _dbFactory = dbFactory;
        }
        public async Task<Dictionary<string, string>> GetSettingsByCategoryAsync(string category)
        {
            using var connection = await _dbFactory.CreateConnectionAsync();

            var result = await connection.QueryAsync<(string Key, string Value)>(

                "sp_GetSettingsByCategory",
                new { Category = category },
                commandType: CommandType.StoredProcedure
            );

            return result.ToDictionary(x => x.Key, x => x.Value);
        }

        public async Task<bool> UpdateSettingsAsync(Dictionary<string, string> settings, Guid? updatedBy = null)
        {
            try
            {
                using var connection = await _dbFactory.CreateConnectionAsync();

                var jsonData = JsonSerializer.Serialize(settings.Select(kvp => new
                {
                    key = kvp.Key,
                    value = kvp.Value
                }));

                await connection.ExecuteAsync(
                    "sp_UpdateSystemSettings",
                    new { JsonData = jsonData, UpdatedBy = updatedBy },
                    commandType: CommandType.StoredProcedure
                );

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
