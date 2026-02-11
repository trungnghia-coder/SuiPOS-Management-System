using System.ComponentModel.DataAnnotations;

namespace SuiPOS.Models
{
    public class Order
    {
        [Key]
        public Guid Id { get; set; }

        [Required, StringLength(20)]
        public string OrderCode { get; set; } = string.Empty;

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        public decimal TotalAmount { get; set; }

        public decimal? Discount { get; set; } = 0;

        [StringLength(500)]
        public string? Note { get; set; }

        [Required, StringLength(50)]
        public string Status { get; set; } = "Completed";

        public decimal AmountReceived { get; set; } = 0;

        public decimal ChangeAmount { get; set; } = 0;

        public Guid? StaffId { get; set; }

        public Staff? Staff { get; set; }

        public Guid? CustomerId
        {
            get; set;
        }
        public Customer? Customer { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
