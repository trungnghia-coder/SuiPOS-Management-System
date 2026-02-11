using SuiPOS.Data;
using SuiPOS.Models;
using SuiPOS.Repositories.Interfaces;

namespace SuiPOS.Repositories.Implementations
{
    public class OrderRepository : IOrderRepository
    {
        private readonly SuiPosDbContext _context;
        public OrderRepository(SuiPosDbContext context)
        {
            _context = context;
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            return order;
        }
    }
}
