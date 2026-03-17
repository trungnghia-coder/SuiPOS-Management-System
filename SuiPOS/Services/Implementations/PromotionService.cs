using Dapper;
using SuiPOS.Data;
using SuiPOS.Models;
using SuiPOS.Services.Interfaces;
using SuiPOS.ViewModels;
using System.Data;

namespace SuiPOS.Services.Implementations
{
    public class PromotionService : IPromotionService
    {
        private readonly IDbConnectionFactory _dbFactory;

        public PromotionService(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<PromotionListVM>> GetAllAsync()
        {
            try
            {
                using var connection = await _dbFactory.CreateConnectionAsync();

                var promotions = await connection.QueryAsync<PromotionListVM>(
                    "sp_GetAllPromotions",
                    commandType: CommandType.StoredProcedure
                );

                return promotions.ToList();
            }
            catch (Exception ex)
            {
                return new List<PromotionListVM>();
            }
        }

        public async Task<List<PromotionListVM>> GetValidPromotionsForOrderAsync(decimal orderAmount)
        {
            try
            {
                using var connection = await _dbFactory.CreateConnectionAsync();

                var validPromotions = await connection.QueryAsync<PromotionListVM>(
                    "sp_GetValidPromotionsForOrder",
                    new { OrderAmount = orderAmount },
                    commandType: CommandType.StoredProcedure
                );

                return validPromotions.ToList();
            }
            catch (Exception ex)
            {
                return new List<PromotionListVM>();
            }
        }

        public async Task<PromotionVM?> GetByIdAsync(Guid id)
        {
            try
            {
                using var connection = await _dbFactory.CreateConnectionAsync();

                var promotion = await connection.QueryFirstOrDefaultAsync<PromotionVM>(
                    "sp_GetPromotionById",
                    new { Id = id },
                    commandType: CommandType.StoredProcedure
                );

                return promotion;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<(bool Success, string Message)> CreateAsync(PromotionVM model)
        {
            try
            {
                using var connection = await _dbFactory.CreateConnectionAsync();

                int promotionType = (int)Enum.Parse<Promotion.DiscountType>(model.Type);

                var parameters = new
                {
                    Id = Guid.NewGuid(),
                    model.Name,
                    model.Code,
                    Type = promotionType,
                    model.DiscountValue,
                    model.MinOrderAmount,
                    model.MaxDiscountAmount,
                    model.StartDate,
                    model.EndDate,
                    model.IsActive
                };

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_CreatePromotion",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                if (result == null) return (false, "Lỗi thực thi hệ thống.");

                return (result.Success == 1, (string)result.Message);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateAsync(PromotionVM model)
        {
            try
            {
                using var connection = await _dbFactory.CreateConnectionAsync();

                int promotionType = (int)Enum.Parse<Promotion.DiscountType>(model.Type);

                var parameters = new
                {
                    model.Id,
                    model.Name,
                    model.Code,
                    Type = promotionType,
                    model.DiscountValue,
                    model.MinOrderAmount,
                    model.MaxDiscountAmount,
                    model.StartDate,
                    model.EndDate,
                    model.IsActive
                };

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_UpdatePromotion",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                if (result == null) return (false, "Lỗi thực thi hệ thống.");

                return (result.Success == 1, (string)result.Message);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteAsync(Guid id)
        {
            try
            {
                using var connection = await _dbFactory.CreateConnectionAsync();

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_DeletePromotion",
                    new { Id = id },
                    commandType: CommandType.StoredProcedure
                );

                if (result == null) return (false, "Lỗi thực thi hệ thống.");

                return (result.Success == 1, (string)result.Message);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ToggleActiveAsync(Guid id)
        {
            try
            {
                using var connection = await _dbFactory.CreateConnectionAsync();

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_TogglePromotionActive",
                    new { Id = id },
                    commandType: CommandType.StoredProcedure
                );

                if (result == null) return (false, "Lỗi thực thi hệ thống.");

                return (result.Success == 1, (string)result.Message);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi hệ thống: {ex.Message}");
            }
        }
    }
}

