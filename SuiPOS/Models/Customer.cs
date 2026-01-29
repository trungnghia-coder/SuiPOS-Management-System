using System.ComponentModel.DataAnnotations;

namespace SuiPOS.Models
{
    public class Customer
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;
        [StringLength(15)]
        public string? Phone { get; set; }
        public int Points { get; set; } = 0;
    }
}
