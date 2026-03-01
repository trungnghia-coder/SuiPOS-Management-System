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

        public async Task<List<AttributeListVM>> GetAllAsync()
        {
            return await _context.ProductAttributes
                .Include(a => a.Values)
                .Select(a => new AttributeListVM
                {
                    Id = a.Id,
                    Name = a.Name,
                    ValueCount = a.Values.Count,
                    SampleValues = a.Values.Take(3).Select(v => v.Value).ToList()
                })
                .OrderBy(a => a.Name)
                .ToListAsync();
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
                        Value = v.Value,
                        AttributeId = v.AttributeId
                    }).ToList()
                })
                .OrderBy(a => a.Name)
                .ToListAsync();
        }

        public async Task<AttributeVM?> GetByIdAsync(Guid id)
        {
            var attribute = await _context.ProductAttributes
                .Include(a => a.Values)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attribute == null) return null;

            return new AttributeVM
            {
                Id = attribute.Id,
                Name = attribute.Name,
                Values = attribute.Values.Select(v => new AttributeValueVM
                {
                    Id = v.Id,
                    Value = v.Value,
                    AttributeId = v.AttributeId
                }).ToList()
            };
        }

        public async Task<(bool Success, string Message)> CreateAttributeAsync(string name)
        {
            try
            {
                if (await _context.ProductAttributes.AnyAsync(a => a.Name == name))
                    return (false, "Thuộc tính này đã tồn tại");

                var attribute = new ProductAttribute
                {
                    Id = Guid.NewGuid(),
                    Name = name
                };

                _context.ProductAttributes.Add(attribute);
                await _context.SaveChangesAsync();

                return (true, "Tạo thuộc tính thành công");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateAttributeAsync(Guid id, string name)
        {
            try
            {
                var attribute = await _context.ProductAttributes.FindAsync(id);
                if (attribute == null)
                    return (false, "Không tìm thấy thuộc tính");

                // Check duplicate name (except current)
                if (await _context.ProductAttributes.AnyAsync(a => a.Name == name && a.Id != id))
                    return (false, "Tên thuộc tính đã tồn tại");

                attribute.Name = name;
                await _context.SaveChangesAsync();

                return (true, "Cập nhật thuộc tính thành công");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteAttributeAsync(Guid id)
        {
            try
            {
                var attribute = await _context.ProductAttributes
                    .Include(a => a.Values)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (attribute == null)
                    return (false, "Không tìm thấy thuộc tính");

                // Check if any product is using this attribute
                var isUsed = await _context.ProductVariants
                    .AnyAsync(pv => pv.SelectedValues.Any(sv => sv.AttributeId == id));

                if (isUsed)
                    return (false, "Không thể xóa thuộc tính đang được sử dụng bởi sản phẩm");

                _context.ProductAttributes.Remove(attribute);
                await _context.SaveChangesAsync();

                return (true, "Xóa thuộc tính thành công");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> AddValueAsync(Guid attributeId, string value)
        {
            try
            {
                var attribute = await _context.ProductAttributes.FindAsync(attributeId);
                if (attribute == null)
                    return (false, "Không tìm thấy thuộc tính");

                // Check duplicate value
                var exists = await _context.AttributeValues
                    .AnyAsync(v => v.AttributeId == attributeId && v.Value == value);

                if (exists)
                    return (false, "Giá trị này đã tồn tại");

                var attrValue = new AttributeValue
                {
                    Id = Guid.NewGuid(),
                    AttributeId = attributeId,
                    Value = value
                };

                _context.AttributeValues.Add(attrValue);
                await _context.SaveChangesAsync();

                return (true, "Thêm giá trị thành công");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateValueAsync(Guid valueId, string value)
        {
            try
            {
                var attrValue = await _context.AttributeValues.FindAsync(valueId);
                if (attrValue == null)
                    return (false, "Không tìm thấy giá trị");

                // Check duplicate value (except current)
                var exists = await _context.AttributeValues
                    .AnyAsync(v => v.AttributeId == attrValue.AttributeId && v.Value == value && v.Id != valueId);

                if (exists)
                    return (false, "Giá trị này đã tồn tại");

                attrValue.Value = value;
                await _context.SaveChangesAsync();

                return (true, "Cập nhật giá trị thành công");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteValueAsync(Guid valueId)
        {
            try
            {
                var val = await _context.AttributeValues.FindAsync(valueId);
                if (val == null)
                    return (false, "Không tìm thấy giá trị");

                // Check if any product variant is using this value
                var isUsed = await _context.ProductVariants
                    .AnyAsync(pv => pv.SelectedValues.Any(sv => sv.Id == valueId));

                if (isUsed)
                    return (false, "Không thể xóa giá trị đang được sử dụng bởi sản phẩm");

                _context.AttributeValues.Remove(val);
                await _context.SaveChangesAsync();

                return (true, "Xóa giá trị thành công");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<List<AttributeValueVM>> GetValuesByAttributeIdAsync(Guid attributeId)
        {
            return await _context.AttributeValues
                .Where(v => v.AttributeId == attributeId)
                .Select(v => new AttributeValueVM
                {
                    Id = v.Id,
                    Value = v.Value,
                    AttributeId = v.AttributeId
                })
                .OrderBy(v => v.Value)
                .ToListAsync();
        }
    }
}

