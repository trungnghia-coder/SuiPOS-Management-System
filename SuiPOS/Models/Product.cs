using System.ComponentModel.DataAnnotations;

namespace SuiPOS.Models
{
    public class Product
    {
        [Key]
        public Guid Id { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public Guid CategoryId { get; set; }

        public Category? Category { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public List<ProductVariant> Variants { get; set; } = new();
    }
}
