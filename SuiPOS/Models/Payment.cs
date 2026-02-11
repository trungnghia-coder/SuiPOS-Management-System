using System.ComponentModel.DataAnnotations;

namespace SuiPOS.Models
{
    public class Payment
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid OrderId { get; set; }

        [Required, StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        public string? TransactionReference { get; set; }

        public Order? Order { get; set; }
    }
}
