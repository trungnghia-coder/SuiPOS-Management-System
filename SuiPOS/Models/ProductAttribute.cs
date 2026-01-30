using System.ComponentModel.DataAnnotations;

namespace SuiPOS.Models
{
    public class ProductAttribute
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public List<AttributeValue> Values { get; set; } = new();
    }
}
