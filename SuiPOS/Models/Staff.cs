using System.ComponentModel.DataAnnotations;

namespace SuiPOS.Models
{
    public class Staff
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public Guid RoleId { get; set; }

        public Role? Role { get; set; }
    }
}
