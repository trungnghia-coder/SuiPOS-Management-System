using Microsoft.AspNetCore.Mvc;

namespace SuiPOS.ViewComponents
{
    public class InvoicePreviewModalViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
