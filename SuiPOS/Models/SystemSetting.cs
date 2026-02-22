using System.ComponentModel.DataAnnotations;

namespace SuiPOS.Models
{
    public class SystemSetting
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Key { get; set; } = string.Empty;

        [Required]
        public string Value { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(20)]
        public string DataType { get; set; } = "string"; // string, bool, int, decimal

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = "General"; // Store, Invoice, Printer, General

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public Guid? UpdatedBy { get; set; }
        public Staff? UpdatedByStaff { get; set; }
    }
}
