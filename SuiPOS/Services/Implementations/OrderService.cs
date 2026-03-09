using Dapper;
using SuiPOS.Data;
using SuiPOS.DTOs;
using SuiPOS.Services.Interfaces;
using SuiPOS.ViewModels;
using System.Data;
using System.Text.Json;

namespace SuiPOS.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IDbConnectionFactory _dbFactory;

        public OrderService(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<(bool Success, string Message, Guid? OrderId)> CreateOrderAsync(OrderViewModel model)
        {
            using var conn = await _dbFactory.CreateConnectionAsync();

            string jsonOrder = JsonSerializer.Serialize(model);

            var result = await conn.QueryFirstOrDefaultAsync<dynamic>(
                "sp_CreateOrder",
                new { OrderData = jsonOrder },
                commandType: CommandType.StoredProcedure
            );

            return (
                result.Success == 1,
                (string)result.Message,
                result.Success == 1 ? (Guid?)result.OrderId : null
            );
        }

        public async Task<OrderDetailVM?> GetOrderByIdAsync(Guid id)
        {
            using var conn = await _dbFactory.CreateConnectionAsync();

            using var multi = await conn.QueryMultipleAsync(
                "sp_GetOrderById",
                new { Id = id },
                commandType: CommandType.StoredProcedure
            );

            var order = await multi.ReadFirstOrDefaultAsync<OrderDetailVM>();
            if (order == null) return null;

            order.Items = (await multi.ReadAsync<OrderItemDetailVM>()).ToList();

            order.Payments = (await multi.ReadAsync<PaymentDetailVM>()).ToList();

            return order;
        }

        public async Task<List<OrderListItemVM>> GetOrdersAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            using var conn = await _dbFactory.CreateConnectionAsync();

            var orders = await conn.QueryAsync<OrderListItemVM>(
                "sp_GetOrdersList",
                new
                {
                    FromDate = fromDate,
                    ToDate = toDate
                },
                commandType: CommandType.StoredProcedure
            );

            return orders.ToList();
        }

        public async Task<(bool Success, string Message)> CancelOrderAsync(Guid orderId)
        {
            using var conn = await _dbFactory.CreateConnectionAsync();

            var result = await conn.QueryFirstOrDefaultAsync<DbResponse>(
                "sp_CancelOrder",
                new { OrderId = orderId },
                commandType: CommandType.StoredProcedure
            );

            if (result == null) return (false, "Lỗi kết nối hệ thống.");

            return (result.Success == 1, result.Message);
        }

        public async Task<(bool Success, string Message)> RefundOrderAsync(Guid orderId)
        {
            using var conn = await _dbFactory.CreateConnectionAsync();

            var result = await conn.QueryFirstOrDefaultAsync<DbResponse>(
                "sp_RefundOrder",
                new { OrderId = orderId },
                commandType: CommandType.StoredProcedure
            );

            if (result == null) return (false, "Lỗi hệ thống.");

            return (result.Success == 1, result.Message);
        }

        public async Task<ValidateStockResult> ValidateStockForReorderAsync(List<(Guid VariantId, int Quantity)> items)
        {
            using var conn = await _dbFactory.CreateConnectionAsync();
            string jsonItems = JsonSerializer.Serialize(items);

            using var multi = await conn.QueryMultipleAsync(
                "sp_ValidateStockForReorder",
                new { JsonItems = jsonItems },
                commandType: CommandType.StoredProcedure
            );

            var status = await multi.ReadFirstAsync<dynamic>();
            var result = new ValidateStockResult
            {
                Success = status.Success,
                Message = status.Message
            };

            var dbItems = (await multi.ReadAsync<dynamic>()).ToList();

            foreach (var item in dbItems)
            {
                if (!(bool)item.IsAvailable)
                {
                    result.UnavailableItems.Add($"{item.ProductName} - {item.VariantName}: {item.Note}");
                }
                else
                {
                    result.AvailableItems.Add(new OrderItemDetailVM
                    {
                        VariantId = item.VariantId,
                        ProductName = item.ProductName,
                        VariantName = item.VariantName,
                        SKU = item.SKU,
                        ImageUrl = item.ImageUrl,
                        Quantity = item.RequestedQuantity,
                        UnitPrice = (decimal)item.UnitPrice
                    });
                }
            }

            return result;
        }
    }
}
