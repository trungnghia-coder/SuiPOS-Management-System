using System.ComponentModel.DataAnnotations;

namespace SuiPOS.ViewModels
{
    public class AttributeVM
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Tên thuộc tính là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        public List<AttributeValueVM> Values { get; set; } = new();
    }

    public class AttributeValueVM
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Giá trị là bắt buộc")]
        [StringLength(100, ErrorMessage = "Giá trị không được vượt quá 100 ký tự")]
        public string Value { get; set; } = string.Empty;

        public Guid AttributeId { get; set; }
    }

    public class AttributeInputVM
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class AttributeListVM
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ValueCount { get; set; }
        public List<string> SampleValues { get; set; } = new();
    }

    public class CreateAttributeValueRequest
    {
        [Required(ErrorMessage = "Giá trị là bắt buộc")]
        public string Value { get; set; } = string.Empty;

        [Required(ErrorMessage = "AttributeId là bắt buộc")]
        public Guid AttributeId { get; set; }
    }
}

