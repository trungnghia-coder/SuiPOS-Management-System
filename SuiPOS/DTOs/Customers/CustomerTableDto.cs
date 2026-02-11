namespace SuiPOS.DTOs.Customers
{
    public class CustomerTableDto
    {
        public Guid Id { get; set; }
        public string Customer { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? LastestOrder { get; set; }
        public int CountOrder { get; set; }
        public decimal Debt { get; set; }
        public decimal SpentTotal { get; set; }
    }
}
