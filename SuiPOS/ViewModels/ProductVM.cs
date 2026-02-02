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

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Tên sản phẩm phải từ 3 đến 200 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public Guid CategoryId { get; set; }

        [Display(Name = "Hình ảnh sản phẩm")]
        public IFormFile? ImageFile { get; set; }

        public string? ExistingImageUrl { get; set; }

        [MinimumCount(1, ErrorMessage = "Sản phẩm phải có ít nhất 1 phiên bản")]
        public List<VariantInputVM> Variants { get; set; } = new();
    }

    public class VariantInputVM
    {
        [Required(ErrorMessage = "SKU không được để trống")]
        [StringLength(50, ErrorMessage = "SKU không được vượt quá 50 ký tự")]
        [RegularExpression(@"^[A-Za-z0-9-_]+$", ErrorMessage = "SKU chỉ được chứa chữ cái, số, dấu gạch ngang và gạch dưới")]
        public string SKU { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá là bắt buộc")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm")]
        public int Stock { get; set; }

        [MinimumCount(1, ErrorMessage = "Phải chọn ít nhất 1 thuộc tính")]
        public List<Guid> SelectedAttributeValueIds { get; set; } = new();
    }
}


