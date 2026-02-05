using Microsoft.AspNetCore.Mvc;

namespace SuiPOS.ViewComponents
{
    public class PageHeaderViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string title, bool showTabs = false)
        {
            ViewBag.Title = title;
            ViewBag.ShowTabs = showTabs;
            return View();
        }
    }
}
