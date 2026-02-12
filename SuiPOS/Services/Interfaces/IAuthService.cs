using SuiPOS.DTOs.Auth;
using SuiPOS.Models;

namespace SuiPOS.Services.Interfaces
{
    public interface IAuthService
    {
        Task<Staff?> LoginAsync(LoginDto loginDto);
        Task<(bool Success, string Message)> RegisterAsync(RegisterDto registerDto);
        Task<bool> UsernameExistsAsync(string username);
        Task<Staff?> GetStaffByUsernameAsync(string username);
    }
}
