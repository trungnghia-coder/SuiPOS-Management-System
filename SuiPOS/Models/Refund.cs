using System.ComponentModel.DataAnnotations;

namespace SuiPOS.Models
{
    public class Refund
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public decimal RefundAmount { get; set; }

        [Required, StringLength(50)]
        public string Reason { get; set; } = string.Empty;

        public DateTime RefundDate { get; set; } = DateTime.Now;

        [Required]
        public Guid StaffId { get; set; }

        // Navigation
        public Order? Order { get; set; }
        public Staff? Staff { get; set; }
    }
}
