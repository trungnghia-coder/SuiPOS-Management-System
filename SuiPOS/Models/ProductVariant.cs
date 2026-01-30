using System.ComponentModel.DataAnnotations;

namespace SuiPOS.Models
{
    public class ProductVariant
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ProductId { get; set; }

        public Product? Product { get; set; }

        [Required, StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int Stock { get; set; }

        [StringLength(200)]
        public string VariantCombination { get; set; } = string.Empty;

        public List<AttributeValue> SelectedValues { get; set; } = new();
    }
}
