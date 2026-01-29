using System.ComponentModel.DataAnnotations;

namespace SuiPOS.Models
{
    public class Staff
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid(); // Tự tạo mã duy nhất toàn cầu
        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;
        [Required, StringLength(50)]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public Role? Role { get; set; }
    }
}
