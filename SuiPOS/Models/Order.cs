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

        public Guid? StaffId { get; set; }
        public Staff? Staff { get; set; }

        public Guid? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
