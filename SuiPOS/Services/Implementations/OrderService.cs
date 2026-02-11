using SuiPOS.Models;
using SuiPOS.Repositories.Interfaces;
using SuiPOS.Services.Interfaces;
using SuiPOS.ViewModels;

namespace SuiPOS.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;

        public async Task<Order> ProcessCheckoutAsync(OrderViewModel model)
        {
            var totalItemsPrice = model.Items.Sum(x => x.Quantity * x.UnitPrice);
            var finalAmount = totalItemsPrice - model.DiscountAmount + model.ShippingFee;
            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = model.CustomerId,
                TotalAmount = model.Items.Sum(x => x.Quantity * x.UnitPrice),
                OrderDate = DateTime.Now,
                Note = model.Note,
                Status = "Completed",
                AmountReceived = model.AmountReceived,
                ChangeAmount = (decimal)(model.AmountReceived - finalAmount),
                StaffId = Guid.Parse("F82975BD-B0F2-4FCD-B42C-EAD375BBD334"),
                Discount = model.DiscountAmount,

            };

            foreach (var item in model.Items)
            {
                order.OrderDetails.Add(new OrderDetail
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductVariantId = item.VariantId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                });
            }

            foreach (var item in model.Payments)
            {
                order.Payments.Add(new Payment
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    PaymentMethod = item.Method,
                    Amount = item.Amount,
                    TransactionReference = item.Reference,
                    PaymentDate = DateTime.Now
                });
            }

            return await _orderRepo.CreateOrderAsync(order);
        }
    }
}
