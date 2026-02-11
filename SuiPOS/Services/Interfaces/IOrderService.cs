using SuiPOS.Models;
using SuiPOS.ViewModels;

namespace SuiPOS.Services.Interfaces
{
    public interface IOrderService
    {
        Task<Order> ProcessCheckoutAsync(OrderViewModel model);
    }
}
