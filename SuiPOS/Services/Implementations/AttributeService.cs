using Dapper;
using SuiPOS.Data;
using SuiPOS.DTOs;
using SuiPOS.Services.Interfaces;
using SuiPOS.ViewModels;
using System.Data;

namespace SuiPOS.Services.Implementations
{
    public class AttributeService : IAttributeService
    {
        private readonly IDbConnectionFactory _dbFactory;

        public AttributeService(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<AttributeListVM>> GetAllAsync()
        {
            using var conn = await _dbFactory.CreateConnectionAsync();
            var result = await conn.QueryAsync<dynamic>("sp_GetAllProductAttributes", commandType: CommandType.StoredProcedure);

            return result.Select(MapToAttributeListVM).ToList();
        }

        public async Task<List<AttributeVM>> GetAllWithValuesAsync()
        {
            using var conn = await _dbFactory.CreateConnectionAsync();

            var result = await conn.QueryAsync<dynamic>(
                "sp_GetAllAttributesWithValues",
                commandType: CommandType.StoredProcedure
            );

            return result.Select(a => new AttributeVM
            {
                Id = a.Id,
                Name = a.Name,
                Values = string.IsNullOrEmpty((string)a.ValuesJson)
                         ? new List<AttributeValueVM>()
                         : Newtonsoft.Json.JsonConvert.DeserializeObject<List<AttributeValueVM>>((string)a.ValuesJson)
            }).ToList();
        }

        public async Task<AttributeVM?> GetByIdAsync(Guid id)
        {
            using var conn = await _dbFactory.CreateConnectionAsync();

            var a = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "sp_GetAttributeById",
                new { Id = id },
                commandType: CommandType.StoredProcedure
            );

            if (a == null) return null;

            return new AttributeVM
            {
                Id = a.Id,
                Name = a.Name,
                Values = string.IsNullOrEmpty((string)a.ValuesJson)
                         ? new List<AttributeValueVM>()
                         : Newtonsoft.Json.JsonConvert.DeserializeObject<List<AttributeValueVM>>((string)a.ValuesJson)
            };
        }

        public async Task<(bool Success, string Message)> CreateAttributeAsync(string name)
        {
            using var conn = await _dbFactory.CreateConnectionAsync();

            var result = await conn.QueryFirstOrDefaultAsync<DbResponse>(
                "sp_CreateProductAttribute",
                new { Name = name },
                commandType: CommandType.StoredProcedure
            );

            return (result?.Success == 1, result?.Message ?? "Lỗi");
        }

        public async Task<(bool Success, string Message)> UpdateAttributeAsync(Guid id, string name)
        {
            using var conn = await _dbFactory.CreateConnectionAsync();

            var result = await conn.QueryFirstOrDefaultAsync<DbResponse>(
                "sp_UpdateProductAttribute",
                new { Id = id, Name = name },
                commandType: CommandType.StoredProcedure
            );

            return (result?.Success == 1, result?.Message ?? "Lỗi");
        }

        public async Task<(bool Success, string Message)> DeleteAttributeAsync(Guid id)
        {
            using var conn = await _dbFactory.CreateConnectionAsync();

            var result = await conn.QueryFirstOrDefaultAsync<DbResponse>(
                "sp_DeleteProductAttribute",
                new { Id = id },
                commandType: CommandType.StoredProcedure
            );

            if (result == null) return (false, "Không nhận được phản hồi từ hệ thống");

            return (result.Success == 1, result.Message ?? "Không có thông báo");
        }

        public async Task<(bool Success, string Message)> AddValueAsync(Guid attributeId, string value)
        {
            using var conn = await _dbFactory.CreateConnectionAsync();

            var result = await conn.QueryFirstOrDefaultAsync<DbResponse>(
                "sp_AddAttributeValue",
                new { AttributeId = attributeId, Value = value },
                commandType: CommandType.StoredProcedure
            );

            if (result == null) return (false, "Lỗi kết nối");

            return (result.Success == 1, result.Message);
        }

        public async Task<(bool Success, string Message)> UpdateValueAsync(Guid valueId, string value)
        {
            using var conn = await _dbFactory.CreateConnectionAsync();

            var result = await conn.QueryFirstOrDefaultAsync<DbResponse>(
                "sp_UpdateAttributeValue",
                new { Id = valueId, Value = value },
                commandType: CommandType.StoredProcedure
            );

            if (result == null) return (false, "Lỗi kết nối hệ thống");

            return (result.Success == 1, result.Message);
        }

        public async Task<(bool Success, string Message)> DeleteValueAsync(Guid valueId)
        {
            using var conn = await _dbFactory.CreateConnectionAsync();

            var result = await conn.QueryFirstOrDefaultAsync<DbResponse>(
                "sp_DeleteAttributeValue",
                new { Id = valueId },
                commandType: CommandType.StoredProcedure
            );

            if (result == null) return (false, "Lỗi kết nối hệ thống");

            return (result.Success == 1, result.Message);
        }

        public async Task<List<AttributeValueVM>> GetValuesByAttributeIdAsync(Guid attributeId)
        {
            using var conn = await _dbFactory.CreateConnectionAsync();

            var values = await conn.QueryAsync<AttributeValueVM>(
                "sp_GetValuesByAttributeId",
                new { AttributeId = attributeId },
                commandType: CommandType.StoredProcedure
            );

            return values.ToList();
        }

        private AttributeListVM MapToAttributeListVM(dynamic a)
        {
            string rawValues = a.SampleValuesRaw?.ToString();
            return new AttributeListVM
            {
                Id = a.Id,
                Name = a.Name,
                ValueCount = (int)(a.ValueCount ?? 0),
                SampleValues = string.IsNullOrEmpty(rawValues)
                    ? new List<string>()
                    : rawValues.Split(", ").ToList()
            };
        }
    }
}

