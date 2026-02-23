using Microsoft.EntityFrameworkCore;
using SuiPOS.Data;
using SuiPOS.Models;
using SuiPOS.Services.Interfaces;
using SuiPOS.ViewModels;

namespace SuiPOS.Services.Implementations
{
    public class PromotionService : IPromotionService
    {
        private readonly SuiPosDbContext _context;

        public PromotionService(SuiPosDbContext context)
        {
            _context = context;
        }

        public async Task<List<PromotionListVM>> GetAllAsync()
        {
            var now = DateTime.Now;
            
            var promotions = await _context.Promotions
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PromotionListVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Code = p.Code,
                    Type = p.Type.ToString(),
                    DiscountValue = p.DiscountValue,
                    MinOrderAmount = p.MinOrderAmount,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    IsActive = p.IsActive,
                    IsValid = p.IsActive && p.StartDate <= now && p.EndDate >= now
                })
                .ToListAsync();

            return promotions;
        }

        public async Task<List<PromotionListVM>> GetActivePromotionsAsync()
        {
            var now = DateTime.Now;
            
            var promotions = await _context.Promotions
                .Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now)
                .OrderBy(p => p.Name)
                .Select(p => new PromotionListVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Code = p.Code,
                    Type = p.Type.ToString(),
                    DiscountValue = p.DiscountValue,
                    MinOrderAmount = p.MinOrderAmount,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    IsActive = p.IsActive,
                    IsValid = true
                })
                .ToListAsync();

            return promotions;
        }

        public async Task<List<PromotionListVM>> GetValidPromotionsForOrderAsync(decimal orderAmount)
        {
            var now = DateTime.Now;
            
            Console.WriteLine($"?? Current time: {now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"?? Order amount: {orderAmount}");

            var validPromotions = await _context.Promotions
                .Where(p => p.IsActive
                    && p.StartDate <= now
                    && p.EndDate >= now
                    && (!p.MinOrderAmount.HasValue || p.MinOrderAmount.Value <= orderAmount))
                .OrderByDescending(p => p.MinOrderAmount.HasValue && p.MinOrderAmount.Value <= orderAmount ? 1 : 0)
                .ThenBy(p => p.Name)
                .Select(p => new PromotionListVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Code = p.Code,
                    Type = p.Type.ToString(),
                    DiscountValue = p.DiscountValue,
                    MinOrderAmount = p.MinOrderAmount,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    IsActive = p.IsActive,
                    IsValid = (!p.MinOrderAmount.HasValue || p.MinOrderAmount.Value <= orderAmount)
                })
                .ToListAsync();
            
            Console.WriteLine($"? Found {validPromotions.Count} valid promotions");
            foreach (var p in validPromotions)
            {
                Console.WriteLine($"   - {p.Name} ({p.Code}): {p.StartDate:yyyy-MM-dd} ~ {p.EndDate:yyyy-MM-dd}");
            }

            return validPromotions;
        }

        public async Task<PromotionVM?> GetByIdAsync(Guid id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null) return null;

            return new PromotionVM
            {
                Id = promotion.Id,
                Name = promotion.Name,
                Code = promotion.Code,
                Type = promotion.Type.ToString(),
                DiscountValue = promotion.DiscountValue,
                MinOrderAmount = promotion.MinOrderAmount,
                MaxDiscountAmount = promotion.MaxDiscountAmount,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                IsActive = promotion.IsActive
            };
        }

        public async Task<(bool Success, string Message)> CreateAsync(PromotionVM model)
        {
            try
            {
                var exists = await _context.Promotions.AnyAsync(p => p.Code == model.Code);
                if (exists)
                {
                    return (false, "Mã khuy?n mãi ?ã t?n t?i");
                }

                if (model.EndDate <= model.StartDate)
                {
                    return (false, "Ngày k?t thúc ph?i sau ngày b?t ??u");
                }

                var promotion = new Promotion
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name,
                    Code = model.Code,
                    Type = Enum.Parse<Promotion.DiscountType>(model.Type),
                    DiscountValue = model.DiscountValue,
                    MinOrderAmount = model.MinOrderAmount,
                    MaxDiscountAmount = model.MaxDiscountAmount,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Promotions.Add(promotion);
                await _context.SaveChangesAsync();

                return (true, "Thêm khuy?n mãi thành công");
            }
            catch (Exception ex)
            {
                return (false, $"L?i: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateAsync(PromotionVM model)
        {
            try
            {
                var promotion = await _context.Promotions.FindAsync(model.Id);
                if (promotion == null)
                {
                    return (false, "Không tìm th?y khuy?n mãi");
                }

                var exists = await _context.Promotions
                    .AnyAsync(p => p.Code == model.Code && p.Id != model.Id);
                if (exists)
                {
                    return (false, "Mã khuy?n mãi ?ã t?n t?i");
                }

                if (model.EndDate <= model.StartDate)
                {
                    return (false, "Ngày k?t thúc ph?i sau ngày b?t ??u");
                }

                promotion.Name = model.Name;
                promotion.Code = model.Code;
                promotion.Type = Enum.Parse<Promotion.DiscountType>(model.Type);
                promotion.DiscountValue = model.DiscountValue;
                promotion.MinOrderAmount = model.MinOrderAmount;
                promotion.MaxDiscountAmount = model.MaxDiscountAmount;
                promotion.StartDate = model.StartDate;
                promotion.EndDate = model.EndDate;
                promotion.IsActive = model.IsActive;
                promotion.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return (true, "C?p nh?t khuy?n mãi thành công");
            }
            catch (Exception ex)
            {
                return (false, $"L?i: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteAsync(Guid id)
        {
            try
            {
                var promotion = await _context.Promotions.FindAsync(id);
                if (promotion == null)
                {
                    return (false, "Không tìm th?y khuy?n mãi");
                }

                _context.Promotions.Remove(promotion);
                await _context.SaveChangesAsync();

                return (true, "Xóa khuy?n mãi thành công");
            }
            catch (Exception ex)
            {
                return (false, $"L?i: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ToggleActiveAsync(Guid id)
        {
            try
            {
                var promotion = await _context.Promotions.FindAsync(id);
                if (promotion == null)
                {
                    return (false, "Không tìm th?y khuy?n mãi");
                }

                promotion.IsActive = !promotion.IsActive;
                promotion.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var status = promotion.IsActive ? "kích ho?t" : "vô hi?u hóa";
                return (true, $"?ã {status} khuy?n mãi");
            }
            catch (Exception ex)
            {
                return (false, $"L?i: {ex.Message}");
            }
        }
    }
}

