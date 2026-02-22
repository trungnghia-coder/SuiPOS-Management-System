using Microsoft.EntityFrameworkCore;
using SuiPOS.Data;
using SuiPOS.Models;
using SuiPOS.Services.Interfaces;
using SuiPOS.ViewModels;

namespace SuiPOS.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly SuiPosDbContext _context;

        public OrderService(SuiPosDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message, Guid? OrderId)> CreateOrderAsync(OrderViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (model.Items == null || !model.Items.Any())
                {
                    return (false, "Giỏ hàng trống", null);
                }

                var totalItemsPrice = model.Items.Sum(x => x.Quantity * x.UnitPrice);
                var finalAmount = totalItemsPrice - (model.DiscountAmount ?? 0);

                // Generate unique order code
                var orderCode = "ORD" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");

                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    OrderCode = orderCode,
                    CustomerId = model.CustomerId,
                    StaffId = model.StaffId,
                    TotalAmount = totalItemsPrice,
                    OrderDate = DateTime.UtcNow,
                    Note = model.Note,
                    Status = "Completed",
                    AmountReceived = model.AmountReceived,
                    ChangeAmount = model.AmountReceived - finalAmount,
                    Discount = model.DiscountAmount ?? 0
                };

                _context.Orders.Add(order);

                foreach (var item in model.Items)
                {
                    var variant = await _context.ProductVariants.FindAsync(item.VariantId);
                    if (variant == null)
                    {
                        await transaction.RollbackAsync();
                        return (false, $"Sản phẩm không tồn tại", null);
                    }

                    if (variant.Stock < item.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return (false, $"Không đủ tồn kho cho {variant.SKU}", null);
                    }

                    order.OrderDetails.Add(new OrderDetail
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order.Id,
                        ProductVariantId = item.VariantId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    });

                    variant.Stock -= item.Quantity;
                    _context.ProductVariants.Update(variant);
                }

                if (model.Payments != null)
                {
                    foreach (var payment in model.Payments)
                    {
                        order.Payments.Add(new Payment
                        {
                            Id = Guid.NewGuid(),
                            OrderId = order.Id,
                            PaymentMethod = payment.Method,
                            Amount = payment.Amount,
                            TransactionReference = payment.Reference,
                            PaymentDate = DateTime.UtcNow
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Tạo đơn hàng thành công", order.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Lỗi: {ex.Message}", null);
            }
        }

        public async Task<OrderDetailVM?> GetOrderByIdAsync(Guid id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Staff)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return null;

            return new OrderDetailVM
            {
                Id = order.Id,
                OrderCode = order.OrderCode,
                CustomerName = order.Customer?.Name,
                CustomerPhone = order.Customer?.Phone,
                StaffName = order.Staff?.FullName,
                TotalAmount = order.TotalAmount,
                AmountReceived = order.AmountReceived,
                ChangeAmount = order.ChangeAmount,
                Discount = order.Discount ?? 0,
                Status = order.Status,
                Note = order.Note,
                OrderDate = order.OrderDate,
                Items = order.OrderDetails.Select(od => new OrderItemDetailVM
                {
                    VariantId = od.ProductVariantId,
                    ProductName = od.ProductVariant?.Product?.Name ?? "N/A",
                    VariantName = od.ProductVariant?.VariantCombination ?? "Default",
                    SKU = od.ProductVariant?.SKU ?? "N/A",
                    ImageUrl = od.ProductVariant?.Product?.ImageUrl,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice
                }).ToList(),
                Payments = order.Payments.Select(p => new PaymentDetailVM
                {
                    Method = p.PaymentMethod,
                    Amount = p.Amount,
                    Reference = p.TransactionReference
                }).ToList()
            };
        }

        public async Task<List<OrderListItemVM>> GetOrdersAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Orders
                .Include(o => o.Customer)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                var endOfDay = toDate.Value.Date.AddDays(1).AddSeconds(-1);
                query = query.Where(o => o.OrderDate <= endOfDay);
            }

            return await query
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderListItemVM
                {
                    Id = o.Id,
                    OrderCode = o.OrderCode,
                    CustomerName = o.Customer != null ? o.Customer.Name : "Khách lẻ",
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    OrderDate = o.OrderDate
                })
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> CancelOrderAsync(Guid orderId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return (false, "Không tìm thấy đơn hàng");
                }

                if (order.Status == "Cancelled")
                {
                    return (false, "Đơn hàng đã bị hủy trước đó");
                }

                // Restore stock
                foreach (var item in order.OrderDetails)
                {
                    if (item.ProductVariant != null)
                    {
                        item.ProductVariant.Stock += item.Quantity;
                        _context.ProductVariants.Update(item.ProductVariant);
                    }
                }

                order.Status = "Cancelled";
                _context.Orders.Update(order);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Hủy đơn hàng thành công");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> RefundOrderAsync(Guid orderId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return (false, "Không tìm thấy đơn hàng");
                }

                if (order.Status == "Refunded")
                {
                    return (false, "Đơn hàng đã được hoàn trả");
                }

                // Restore stock
                foreach (var item in order.OrderDetails)
                {
                    if (item.ProductVariant != null)
                    {
                        item.ProductVariant.Stock += item.Quantity;
                        _context.ProductVariants.Update(item.ProductVariant);
                    }
                }

                order.Status = "Refunded";
                _context.Orders.Update(order);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Hoàn trả đơn hàng thành công");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<ValidateStockResult> ValidateStockForReorderAsync(List<(Guid VariantId, int Quantity)> items)
        {
            var result = new ValidateStockResult { Success = true };
            
            try
            {
                foreach (var item in items)
                {
                    var variant = await _context.ProductVariants
                        .Include(v => v.Product)
                        .FirstOrDefaultAsync(v => v.Id == item.VariantId);

                    if (variant == null || !variant.Product.isActive)
                    {
                        result.UnavailableItems.Add($"{item.VariantId} - Sản phẩm không tồn tại hoặc đã bị xóa");
                        continue;
                    }

                    if (variant.Stock < item.Quantity)
                    {
                        result.UnavailableItems.Add($"{variant.Product.Name} - {variant.VariantCombination} (Còn {variant.Stock}, yêu cầu {item.Quantity})");
                        continue;
                    }

                    // Add to available items
                    result.AvailableItems.Add(new OrderItemDetailVM
                    {
                        VariantId = variant.Id,
                        ProductName = variant.Product.Name,
                        VariantName = variant.VariantCombination,
                        SKU = variant.SKU,
                        ImageUrl = variant.Product.ImageUrl,
                        Quantity = item.Quantity,
                        UnitPrice = variant.Price
                    });
                }

                if (result.UnavailableItems.Any())
                {
                    result.Success = false;
                    result.Message = $"{result.UnavailableItems.Count} sản phẩm không khả dụng";
                }

                return result;
            }
            catch (Exception ex)
            {
                return new ValidateStockResult
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }
    }
}
