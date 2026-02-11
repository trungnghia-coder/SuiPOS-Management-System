using System.ComponentModel.DataAnnotations;

namespace SuiPOS.ViewModels
{
    public class CustomerViewModel
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Tên khách hàng là bắt buộc")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên phải từ 2 đến 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [RegularExpression(@"^(0|84)(3|5|7|8|9)([0-9]{8})$", ErrorMessage = "Số điện thoại không đúng định dạng Việt Nam")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }
    }
}
