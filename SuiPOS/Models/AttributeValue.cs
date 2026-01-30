using System.ComponentModel.DataAnnotations;

namespace SuiPOS.Models
{
    public class AttributeValue
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, StringLength(100)]
        public string Value { get; set; } = string.Empty;

        [Required]
        public Guid AttributeId { get; set; }

        public ProductAttribute? Attribute { get; set; }
    }
}
