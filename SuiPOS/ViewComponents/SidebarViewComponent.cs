using Microsoft.AspNetCore.Mvc;

namespace SuiPOS.ViewComponents
{
    public class SidebarViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var menuItems = new List<MenuItem>
            {
                new MenuItem { Name = "Bán hàng", Icon = "shopping-cart", Url = "/POS/Index", IsActive = false },
                new MenuItem { Name = "Tra cứu bán hàng", Icon = "receipt", Url = "/Orders/Index", IsActive = false },
                new MenuItem { Name = "Sản phẩm", Icon = "product", Url = "/Products/Index", IsActive = false },
                new MenuItem { Name = "Khách hàng", Icon = "customer", Url = "/Customers/Index", IsActive = false },
                new MenuItem { Name = "Báo cáo", Icon = "chart-bar", Url = "/Reports/Index", IsActive = false },
                new MenuItem { Name = "Cấu hình", Icon = "settings", Url = "/Settings/Index", IsActive = false }
            };

            // Set active based on current route
            var currentPath = ViewContext.HttpContext.Request.Path.Value;
            foreach (var item in menuItems)
            {
                // Check if current path matches the controller (not just exact match)
                var controllerName = item.Url.Split('/')[1];
                item.IsActive = currentPath?.Contains($"/{controllerName}", StringComparison.OrdinalIgnoreCase) ?? false;
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

