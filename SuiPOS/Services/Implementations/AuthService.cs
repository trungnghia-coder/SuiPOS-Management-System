using Dapper;
using SuiPOS.Data;
using SuiPOS.DTOs.Auth;
using SuiPOS.Models;
using SuiPOS.Services.Interfaces;
using System.Data;

namespace SuiPOS.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IDbConnectionFactory _dbFactory;

        public AuthService(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<Staff?> GetStaffByUsernameAsync(string username)
        {
            using var conn = await _dbFactory.CreateConnectionAsync();

            var row = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "spStaff_GetByUsername",
                new { Username = username },
                commandType: CommandType.StoredProcedure
            );

            if (row == null) return null;

            return new Staff
            {
                Id = row.Id,
                FullName = row.FullName,
                Username = row.Username,
                PasswordHash = row.PasswordHash,
                Role = new Role { Name = row.RoleName ?? "N/A" }
            };
        }

        public async Task<Staff?> LoginAsync(LoginDto loginDto)
        {
            var staff = await GetStaffByUsernameAsync(loginDto.Username);

            if (staff != null && BCrypt.Net.BCrypt.Verify(loginDto.Password, staff.PasswordHash))
            {
                return staff;
            }

            return null;
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterDto dto)
        {
            using var conn = await _dbFactory.CreateConnectionAsync();

            var parameters = new
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleName = "Staff"
            };

            var result = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "spStaff_Register",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return (result.Success == true || result.Success == 1, (string)result.Message);
        }
    }
}