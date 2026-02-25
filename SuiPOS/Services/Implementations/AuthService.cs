using Microsoft.Data.SqlClient;
using SuiPOS.Data;
using SuiPOS.DTOs.Auth;
using SuiPOS.Models;
using SuiPOS.Services.Interfaces;

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

            const string sql = @"
            SELECT s.*, r.Name as RoleName 
            FROM Staffs s 
            LEFT JOIN Roles r ON s.RoleId = r.Id 
            WHERE s.Username = @Username";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Username", username);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapReaderToStaff(reader);
            }

            return null;
        }

        public async Task<Staff?> LoginAsync(LoginDto loginDto)
        {
            using var conn = await _dbFactory.CreateConnectionAsync();
            const string sql = @"
            SELECT s.*, r.Name as RoleName 
            FROM Staffs s 
            LEFT JOIN Roles r ON s.RoleId = r.Id 
            WHERE s.Username = @Username";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Username", loginDto.Username);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var staff = MapReaderToStaff(reader);

                if (BCrypt.Net.BCrypt.Verify(loginDto.Password, staff.PasswordHash))
                    return staff;
            }
            return null;
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterDto dto)
        {
            if (await UsernameExistsAsync(dto.Username))
                return (false, "Tên đăng nhập đã tồn tại!");

            using var conn = await _dbFactory.CreateConnectionAsync();
            const string sql = @"
            INSERT INTO Staffs (Id, FullName, Username, PasswordHash, RoleId) 
            VALUES (@Id, @FullName, @Username, @PasswordHash, @RoleId)";

            using var cmd = new SqlCommand(sql, conn);
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            cmd.Parameters.AddWithValue("@Id", Guid.NewGuid());
            cmd.Parameters.AddWithValue("@FullName", dto.FullName);
            cmd.Parameters.AddWithValue("@Username", dto.Username);
            cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
            cmd.Parameters.AddWithValue("@RoleId", await GetDefaultRoleId(conn));

            await cmd.ExecuteNonQueryAsync();
            return (true, "Đăng ký thành công");
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            using var conn = await _dbFactory.CreateConnectionAsync();
            const string sql = "SELECT COUNT(1) FROM Staffs WHERE Username = @Username";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Username", username);
            return (int)await cmd.ExecuteScalarAsync()! > 0;
        }

        private Staff MapReaderToStaff(SqlDataReader reader)
        {
            return new Staff
            {
                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                Role = new Role { Name = reader.GetString(reader.GetOrdinal("RoleName")) }
            };
        }

        private async Task<Guid> GetDefaultRoleId(SqlConnection conn)
        {
            const string sql = "SELECT TOP 1 Id FROM Roles WHERE Name = 'Staff'";
            using var cmd = new SqlCommand(sql, conn);
            return (Guid)(await cmd.ExecuteScalarAsync() ?? Guid.Empty);
        }
    }
}