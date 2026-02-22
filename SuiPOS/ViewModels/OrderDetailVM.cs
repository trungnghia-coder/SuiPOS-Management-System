namespace SuiPOS.ViewModels
{
    public class OrderListItemVM
    {
        public Guid Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
    }

    public class OrderDetailVM
    {
        public Guid Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? StaffName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountReceived { get; set; }
        public decimal ChangeAmount { get; set; }
        public decimal Discount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime OrderDate { get; set; }
        public List<OrderItemDetailVM> Items { get; set; } = new();
        public List<PaymentDetailVM> Payments { get; set; } = new();
    }

    public class OrderItemDetailVM
    {
        public Guid VariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string VariantName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
    }

    public class PaymentDetailVM
    {
        public string Method { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Reference { get; set; }
    }

    public class ValidateStockResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<OrderItemDetailVM> AvailableItems { get; set; } = new();
        public List<string> UnavailableItems { get; set; } = new();
    }
}

