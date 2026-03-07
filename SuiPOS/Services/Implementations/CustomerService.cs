using Dapper;
using SuiPOS.Data;
using SuiPOS.DTOs;
using SuiPOS.DTOs.Customers;
using SuiPOS.Services.Interfaces;
using SuiPOS.ViewModels;
using System.Data;

namespace SuiPOS.Services.Implementations
{
    public class CustomerService : ICustomerService
    {
        private readonly IDbConnectionFactory _dbFactory;

        public CustomerService(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<(bool Success, string Message)> CreateAsync(CustomerViewModel model)
        {
            using var conn = await _dbFactory.CreateConnectionAsync();

            var result = await conn.QueryFirstOrDefaultAsync<DbResponse>(
                "sp_CreateCustomer",
                new
                {
                    Name = model.Name,
                    Phone = model.PhoneNumber
                },
                commandType: CommandType.StoredProcedure
            );

            if (result == null) return (false, "Lỗi kết nối hệ thống.");

            return (result.Success == 1, result.Message);
        }

        public async Task<(bool Success, string Message)> DeleteAsync(Guid id)
        {
            using var conn = await _dbFactory.CreateConnectionAsync();

            var result = await conn.QueryFirstOrDefaultAsync<DbResponse>(
                "sp_DeleteCustomer",
                new { Id = id },
                commandType: CommandType.StoredProcedure
            );

            return (result?.Success == 1, result?.Message ?? "Lỗi hệ thống");
        }

        public async Task<List<CustomerTableDto>> GetAllAsync()
        {
            using var conn = await _dbFactory.CreateConnectionAsync();

            var customers = await conn.QueryAsync<CustomerTableDto>(
                "GetCustomerList",
                commandType: CommandType.StoredProcedure
            );

            return customers.ToList();
        }

        public async Task<CustomerViewModel?> GetByIdAsync(Guid id)
        {
            using var conn = await _dbFactory.CreateConnectionAsync();

            return await conn.QueryFirstOrDefaultAsync<CustomerViewModel>(
                "sp_GetCustomerById",
                new { Id = id },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<(bool Success, string Message)> UpdateAsync(Guid id, CustomerViewModel model)
        {
            using var conn = await _dbFactory.CreateConnectionAsync();

            var result = await conn.QueryFirstOrDefaultAsync<DbResponse>(
                "sp_UpdateCustomer",
                new
                {
                    Id = id,
                    Name = model.Name,
                    Phone = model.PhoneNumber,
                    DebtAmount = model.DebtAmount,
                    TotalSpent = model.TotalSpent
                },
                commandType: CommandType.StoredProcedure
            );

            if (result == null) return (false, "Lỗi hệ thống.");

            return (result.Success == 1, result.Message);
        }

        public async Task<List<CustomerSearchVM>> SearchAsync(string query)
        {
            using var conn = await _dbFactory.CreateConnectionAsync();

            var results = await conn.QueryAsync<CustomerSearchVM>(
                "sp_SearchCustomers",
                new { Query = query },
                commandType: CommandType.StoredProcedure
            );

            return results.ToList();
        }
    }
}
