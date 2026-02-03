using SuiPOS.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SuiPOS.ViewModels
{
    public class ProductVM
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public List<VariantDisplayVM> Variants { get; set; } = new();
    }

    public class VariantDisplayVM
    {
        public Guid Id { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string Combination { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }

    public class ProductInputVM
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Product name must be between 3 and 200 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a category")]
        public Guid CategoryId { get; set; }

        [Display(Name = "Product Image")]
        // ✅ REMOVED: [Required] - Image is optional when editing
        public IFormFile? ImageFile { get; set; }

        public string? ExistingImageUrl { get; set; }

        [MinimumCount(1, ErrorMessage = "Product must have at least 1 variant")]
        public List<VariantInputVM> Variants { get; set; } = new();
    }

    public class VariantInputVM
    {
        [Required(ErrorMessage = "SKU is required")]
        [StringLength(50, ErrorMessage = "SKU cannot exceed 50 characters")]
        [RegularExpression(@"^[A-Za-z0-9-_]+$", ErrorMessage = "SKU can only contain letters, numbers, hyphens and underscores")]
        public string SKU { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int Stock { get; set; }

        [MinimumCount(1, ErrorMessage = "Must select at least 1 attribute")]
        public List<Guid> SelectedAttributeValueIds { get; set; } = new();
    }
}


