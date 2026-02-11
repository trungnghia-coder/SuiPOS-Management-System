using SuiPOS.Models;

namespace SuiPOS.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> CreateOrderAsync(Order order);
    }
}
