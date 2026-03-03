using System.ComponentModel.DataAnnotations;

namespace SuiPOS.DTOs.Auth
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Tên đăng nhập không được trống")]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được trống")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;
    }
}
