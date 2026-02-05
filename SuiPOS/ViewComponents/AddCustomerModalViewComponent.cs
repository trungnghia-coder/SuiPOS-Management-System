using Microsoft.AspNetCore.Mvc;

namespace SuiPOS.ViewComponents
{
    public class AddCustomerModalViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
