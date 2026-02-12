using Microsoft.EntityFrameworkCore;
using SuiPOS.Data;
using SuiPOS.Models;
using SuiPOS.Services.Interfaces;
using SuiPOS.ViewModels;

namespace SuiPOS.Services.Implementations
{
    public class AttributeService : IAttributeService
    {
        private readonly SuiPosDbContext _context;

        public AttributeService(SuiPosDbContext context)
        {
            _context = context;
        }

        public async Task<List<AttributeVM>> GetAllWithValuesAsync()
        {
            return await _context.ProductAttributes
                .Include(a => a.Values)
                .Select(a => new AttributeVM
                {
                    Id = a.Id,
                    Name = a.Name,
                    Values = a.Values.Select(v => new AttributeValueVM
                    {
                        Id = v.Id,
                        Value = v.Value
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> CreateAttributeAsync(string name)
        {
            if (await _context.ProductAttributes.AnyAsync(a => a.Name == name))
                return (false, "Thuộc tính này đã có trong hệ thống.");

            var attribute = new ProductAttribute { Id = Guid.NewGuid(), Name = name };
            _context.ProductAttributes.Add(attribute);

            return await _context.SaveChangesAsync() > 0
                ? (true, "Tạo nhóm thuộc tính thành công.")
                : (false, "Lỗi khi tạo.");
        }

        public async Task<(bool Success, string Message)> AddValueAsync(Guid attributeId, string value)
        {
            var attribute = await _context.ProductAttributes.FindAsync(attributeId);
            if (attribute == null) return (false, "Nhóm thuộc tính không tồn tại.");

            var attrValue = new AttributeValue
            {
                Id = Guid.NewGuid(),
                AttributeId = attributeId,
                Value = value
            };

            _context.AttributeValues.Add(attrValue);
            return await _context.SaveChangesAsync() > 0
                ? (true, $"Đã thêm giá trị '{value}' thành công.")
                : (false, "Thêm giá trị thất bại.");
        }

        public async Task<(bool Success, string Message)> DeleteAttributeAsync(Guid id)
        {
            var attribute = await _context.ProductAttributes.FindAsync(id);
            if (attribute == null) return (false, "Không tìm thấy.");

            _context.ProductAttributes.Remove(attribute);
            return await _context.SaveChangesAsync() > 0 ? (true, "Xóa nhóm thành công.") : (false, "Lỗi.");
        }

        public async Task<(bool Success, string Message)> DeleteValueAsync(Guid valueId)
        {
            var val = await _context.AttributeValues.FindAsync(valueId);
            if (val == null) return (false, "Giá trị không tồn tại.");

            _context.AttributeValues.Remove(val);
            return await _context.SaveChangesAsync() > 0 ? (true, "Đã xóa giá trị.") : (false, "Lỗi.");
        }
    }
}
