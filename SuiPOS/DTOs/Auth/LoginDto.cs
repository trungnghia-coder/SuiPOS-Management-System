using System.ComponentModel.DataAnnotations;

namespace SuiPOS.DTOs.Auth
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Tên ??ng nh?p không ???c ?? tr?ng")]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "M?t kh?u không ???c ?? tr?ng")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;
    }
}
