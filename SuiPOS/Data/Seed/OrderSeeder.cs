using SuiPOS.Models;

namespace SuiPOS.Data.Seed
{
    public static class OrderSeeder
    {
        private static readonly string[] PaymentMethods = { "cash", "card", "transfer" };

        public static (List<Order>, List<OrderDetail>, List<Payment>) GenerateOrders(
            int count,
            List<Customer> customers,
            List<ProductVariant> variants,
            Guid staffId)
        {
            var orders = new List<Order>();
            var orderDetails = new List<OrderDetail>();
            var payments = new List<Payment>();
            var random = new Random();

            var startDate = new DateTime(2025, 7, 9);
            var now = DateTime.UtcNow;
            var totalDays = (now - startDate).Days;

            for (int i = 0; i < count; i++)
            {
                var orderId = Guid.NewGuid();
                var orderDate = startDate.AddDays(random.Next(0, totalDays + 1))
                    .AddHours(random.Next(8, 20))
                    .AddMinutes(random.Next(0, 60));

                var customer = random.Next(0, 100) > 20
                    ? customers[random.Next(customers.Count)]
                    : null;

                var numItems = random.Next(1, 6);
                var selectedVariants = variants
                    .OrderBy(x => random.Next())
                    .Take(numItems)
                    .ToList();

                decimal orderTotal = 0;

                foreach (var variant in selectedVariants)
                {
                    var quantity = random.Next(1, 4);
                    var itemTotal = variant.Price * quantity;
                    orderTotal += itemTotal;

                    orderDetails.Add(new OrderDetail
                    {
                        Id = Guid.NewGuid(),
                        OrderId = orderId,
                        ProductVariantId = variant.Id,
                        Quantity = quantity,
                        UnitPrice = variant.Price
                    });
                }

                var discount = random.Next(0, 100) > 70
                    ? random.Next(10000, 50000)
                    : 0;

                var finalAmount = orderTotal - discount;

                var amountReceived = customer != null && random.Next(0, 100) > 60
                    ? finalAmount - random.Next(10000, 50000)
                    : finalAmount;

                amountReceived = Math.Max(0, amountReceived);

                var changeAmount = Math.Max(0, amountReceived - finalAmount);

                var order = new Order
                {
                    Id = orderId,
                    OrderCode = $"ORD{orderDate:yyyyMMddHHmmss}{i:D3}",
                    CustomerId = customer?.Id,
                    StaffId = staffId,
                    TotalAmount = orderTotal,
                    OrderDate = orderDate,
                    Note = random.Next(0, 100) > 80 ? "Khách hàng đặt hàng trước" : null,
                    Status = "Completed",
                    AmountReceived = amountReceived,
                    ChangeAmount = changeAmount,
                    Discount = discount
                };

                orders.Add(order);

                var paymentMethod = PaymentMethods[random.Next(PaymentMethods.Length)];
                payments.Add(new Payment
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    PaymentMethod = paymentMethod,
                    Amount = amountReceived,
                    TransactionReference = paymentMethod != "cash"
                        ? $"TXN{DateTime.UtcNow.Ticks}{random.Next(1000, 9999)}"
                        : null,
                    PaymentDate = orderDate
                });
            }

            return (orders, orderDetails, payments);
        }
    }
}
