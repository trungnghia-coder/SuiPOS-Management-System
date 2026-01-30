using System.ComponentModel.DataAnnotations;

namespace SuiPOS.Models
{
    public class OrderDetail
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public Guid ProductVariantId { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        public Order? Order { get; set; }
        public ProductVariant? ProductVariant { get; set; }
    }
}
