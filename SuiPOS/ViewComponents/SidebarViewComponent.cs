using Microsoft.AspNetCore.Mvc;

namespace SuiPOS.ViewComponents
{
    public class SidebarViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var menuItems = new List<MenuItem>
            {
                new MenuItem { Name = "Bán hàng", Icon = "shopping-cart", Url = "/POS", IsActive = false },
                new MenuItem { Name = "Tra cứu bán hàng", Icon = "receipt", Url = "/Orders", IsActive = false },
                new MenuItem { Name = "Tra cứu bán hàng", Icon = "tag", Url = "/Products", IsActive = false },
                new MenuItem { Name = "Tạo phiếu thu", Icon = "arrow-down-left", Url = "/Receipt", IsActive = false },
                new MenuItem { Name = "Tạo phiếu chi", Icon = "arrow-up-right", Url = "/Payment", IsActive = false },
                new MenuItem { Name = "Báo cáo cuối ngày", Icon = "bar-chart", Url = "/Reports/Daily", IsActive = false },
                new MenuItem { Name = "Báo cáo bán hàng", Icon = "bar-chart-2", Url = "/Reports/Sales", IsActive = false },
                new MenuItem { Name = "Cấu hình", Icon = "settings", Url = "/Settings", IsActive = false }
            };

            // Set active based on current route
            var currentPath = ViewContext.HttpContext.Request.Path.Value;
            foreach (var item in menuItems)
            {
                item.IsActive = currentPath?.StartsWith(item.Url, StringComparison.OrdinalIgnoreCase) ?? false;
            }

            return View(menuItems);
        }
    }

    public class MenuItem
    {
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}

