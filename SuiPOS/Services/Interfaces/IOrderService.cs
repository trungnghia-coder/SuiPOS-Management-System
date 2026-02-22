using SuiPOS.ViewModels;

namespace SuiPOS.Services.Interfaces
{
    public interface IOrderService
    {
        Task<(bool Success, string Message, Guid? OrderId)> CreateOrderAsync(OrderViewModel model);
        Task<OrderDetailVM?> GetOrderByIdAsync(Guid id);
        Task<List<OrderListItemVM>> GetOrdersAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<(bool Success, string Message)> CancelOrderAsync(Guid orderId);
        Task<(bool Success, string Message)> RefundOrderAsync(Guid orderId);
        Task<ValidateStockResult> ValidateStockForReorderAsync(List<(Guid VariantId, int Quantity)> items);
    }
}
