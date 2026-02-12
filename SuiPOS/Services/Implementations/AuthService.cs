using Microsoft.EntityFrameworkCore;
using SuiPOS.Data;
using SuiPOS.DTOs.Auth;
using SuiPOS.Models;
using SuiPOS.Services.Interfaces;

namespace SuiPOS.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly SuiPosDbContext _context;

        public AuthService(SuiPosDbContext context)
        {
            _context = context;
        }

        public async Task<Staff?> LoginAsync(LoginDto loginDto)
        {
            var staff = await _context.Staffs
                .Include(s => s.Role)
                .FirstOrDefaultAsync(s => s.Username == loginDto.Username);

            if (staff == null)
                return null;

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, staff.PasswordHash))
                return null;

            return staff;
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterDto registerDto)
        {
            if (await UsernameExistsAsync(registerDto.Username))
            {
                return (false, "Tên đăng nhập đã thành công!");
            }

            var defaultRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == "Staff" || r.Name == "Employee");

            if (defaultRole == null)
            {
                defaultRole = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Staff"
                };
                _context.Roles.Add(defaultRole);
                await _context.SaveChangesAsync();
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            var newStaff = new Staff
            {
                Id = Guid.NewGuid(),
                FullName = registerDto.FullName,
                Username = registerDto.Username,
                PasswordHash = passwordHash,
                RoleId = defaultRole.Id
            };

            _context.Staffs.Add(newStaff);
            await _context.SaveChangesAsync();

            return (true, "Đăng ký thành công");
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Staffs.AnyAsync(s => s.Username == username);
        }

        public async Task<Staff?> GetStaffByUsernameAsync(string username)
        {
            return await _context.Staffs
                .Include(s => s.Role)
                .FirstOrDefaultAsync(s => s.Username == username);
        }
    }
}
