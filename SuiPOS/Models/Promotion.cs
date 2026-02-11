using System.ComponentModel.DataAnnotations;

namespace SuiPOS.Models
{
    public class Promotion
    {
        [Key]
        public Guid Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        public DiscountType Type { get; set; }

        [Required]
        public decimal DiscountValue { get; set; }

        public decimal? MinOrderAmount { get; set; }
        public decimal? MaxDiscountAmount { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public enum DiscountType
        {
            Percentage,
            FixedAmount
        }
    }
}
