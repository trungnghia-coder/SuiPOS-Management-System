namespace SuiPOS.ViewModels
{
    public class OrderViewModel
    {
        public Guid? CustomerId { get; set; }
        public Guid? StaffId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountReceived { get; set; }
        public decimal? DiscountAmount { get; set; }
        public string? Note { get; set; }
        public List<CartItemViewModel> Items { get; set; } = new();
        public List<PaymentViewModel> Payments { get; set; } = new();
    }

    public class CartItemViewModel
    {
        public Guid VariantId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class PaymentViewModel
    {
        public string Method { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Reference { get; set; }
    }
}
