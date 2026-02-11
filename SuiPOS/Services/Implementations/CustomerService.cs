using Microsoft.EntityFrameworkCore;
using SuiPOS.Data;
using SuiPOS.DTOs.Customers;
using SuiPOS.Models;
using SuiPOS.Services.Interfaces;
using SuiPOS.ViewModels;

namespace SuiPOS.Services.Implementations
{
    public class CustomerService : ICustomerService
    {
        private readonly SuiPosDbContext _context;

        public CustomerService(SuiPosDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message)> CreateAsync(CustomerViewModel model)
        {
            if (await _context.Customers.AnyAsync(c => c.Phone == model.PhoneNumber))
            {
                return (false, "Số điện thoại đã tồn tại trong hệ thống.");
            }

            var newCustomer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                Phone = model.PhoneNumber,
                IsActive = true,
                DebtAmount = 0,
                TotalSpent = 0,
                Points = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.Customers.Add(newCustomer);
            await _context.SaveChangesAsync();
            return (true, "Khách hàng đã được tạo thành công.");
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return false;

            customer.IsActive = false;

            _context.Customers.Update(customer);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<CustomerTableDto>> GetAllAsync()
        {
            return await _context.Database
                .SqlQueryRaw<CustomerTableDto>("EXEC GetCustomerList")
                .ToListAsync();
        }

        public async Task<CustomerViewModel?> GetByIdAsync(Guid id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null) return null;

            return new CustomerViewModel
            {
                Id = customer.Id,
                Name = customer.Name,
                PhoneNumber = customer.Phone ?? string.Empty
            };
        }

        public async Task<(bool Success, string Message)> UpdateAsync(Guid id, CustomerViewModel model)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return (false, "Không tìm tháy khách hàng");

            customer.Name = model.Name;
            customer.Phone = model.PhoneNumber;

            _context.Customers.Update(customer);

            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return (true, "Khách hàng đã cập nhật thành công.");
            }

            return (false, "Cập nhật thất bại, vui lòng thử lại.");
        }
    }
}
