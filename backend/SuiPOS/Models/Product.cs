using System.ComponentModel.DataAnnotations;

namespace SuiPOS.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;
        [Required, StringLength(50)]
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
