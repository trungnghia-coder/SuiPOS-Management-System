namespace SuiPOS.ViewModels
{
    public class SystemSettingsVM
    {
        // Store Settings
        public string StoreName { get; set; } = string.Empty;
        public string StorePhone { get; set; } = string.Empty;
        public string StoreAddress { get; set; } = string.Empty;
        public string InvoiceFooterMessage { get; set; } = string.Empty;

        // Invoice Settings
        public bool ShowBarcode { get; set; }
        public bool ShowLogo { get; set; }
        public bool ShowQRCode { get; set; }
        public bool AutoPrint { get; set; }
        public string PaperSize { get; set; } = "K80";

        // Printer Settings
        public string PrinterName { get; set; } = string.Empty;
    }
}
