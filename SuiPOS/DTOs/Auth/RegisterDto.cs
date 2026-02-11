using System.ComponentModel.DataAnnotations;

namespace SuiPOS.DTOs.Auth
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "H? tên không ???c ?? tr?ng")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên ??ng nh?p không ???c ?? tr?ng")]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "M?t kh?u không ???c ?? tr?ng")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "M?t kh?u ph?i có ít nh?t 6 ký t?")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nh?n m?t kh?u không ???c ?? tr?ng")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "M?t kh?u xác nh?n không kh?p")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
