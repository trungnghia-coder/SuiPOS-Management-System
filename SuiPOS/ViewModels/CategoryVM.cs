using System.ComponentModel.DataAnnotations;

namespace SuiPOS.ViewModels
{
    public class CategoryVM
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CategoryInputModel
    {
        [Required(ErrorMessage = "Tên loại không được để trống")]
        public string Name { get; set; } = string.Empty;
    }
}
