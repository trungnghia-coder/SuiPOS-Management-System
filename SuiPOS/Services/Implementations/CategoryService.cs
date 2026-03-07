using Dapper;
using SuiPOS.Data;
using SuiPOS.DTOs;
using SuiPOS.Services.Interfaces;
using SuiPOS.ViewModels;
using System.Data;

namespace SuiPOS.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly IDbConnectionFactory _dbFactory;

        public CategoryService(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<CategoryVM>> GetAllAsync()
        {
            using var conn = await _dbFactory.CreateConnectionAsync();

            var categories = await conn.QueryAsync<CategoryVM>(
                "sp_GetAllCategories",
                commandType: CommandType.StoredProcedure
            );

            return categories.ToList();
        }

        public async Task<CategoryVM?> GetByIdAsync(Guid id)
        {
            using var conn = await _dbFactory.CreateConnectionAsync();

            return await conn.QueryFirstOrDefaultAsync<CategoryVM>(
                "sp_GetCategoryById",
                new { Id = id },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<(bool Success, string Message)> CreateAsync(CategoryInputModel model)
        {
            using var conn = await _dbFactory.CreateConnectionAsync();

            var result = await conn.QueryFirstOrDefaultAsync<DbResponse>(
                "sp_CreateCategory",
                new { Name = model.Name },
                commandType: CommandType.StoredProcedure
            );

            if (result == null) return (false, "Lỗi kết nối hệ thống.");

            return (result.Success == 1, result.Message);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(Guid id, CategoryInputModel model)
        {
            using var conn = await _dbFactory.CreateConnectionAsync();

            var result = await conn.QueryFirstOrDefaultAsync<DbResponse>(
                "sp_UpdateCategory",
                new { Id = id, Name = model.Name },
                commandType: CommandType.StoredProcedure
            );

            if (result == null) return (false, "Lỗi kết nối hệ thống.");

            return (result.Success == 1, result.Message);
        }

        public async Task<(bool Success, string Message)> DeleteAsync(Guid id)
        {
            using var conn = await _dbFactory.CreateConnectionAsync();

            var result = await conn.QueryFirstOrDefaultAsync<DbResponse>(
                "sp_DeleteCategory",
                new { Id = id },
                commandType: CommandType.StoredProcedure
            );

            if (result == null) return (false, "Lỗi kết nối hệ thống.");

            return (result.Success == 1, result.Message);
        }
    }
}
