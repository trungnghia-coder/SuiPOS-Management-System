using Microsoft.AspNetCore.Mvc;
using SuiPOS.Services.Interfaces;
using SuiPOS.ViewModels;

namespace SuiPOS.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ISystemSettingService _settingService;

        public SettingsController(ISystemSettingService settingService)
        {
            _settingService = settingService;
        }

        public async Task<IActionResult> Index()
        {
            var storeSettings = await _settingService.GetSettingsByCategoryAsync("Store");
            var invoiceSettings = await _settingService.GetSettingsByCategoryAsync("Invoice");
            var printerSettings = await _settingService.GetSettingsByCategoryAsync("Printer");

            var viewModel = new SystemSettingsVM
            {
                StoreName = storeSettings.GetValueOrDefault("store_name", ""),
                StorePhone = storeSettings.GetValueOrDefault("store_phone", ""),
                StoreAddress = storeSettings.GetValueOrDefault("store_address", ""),
                InvoiceFooterMessage = storeSettings.GetValueOrDefault("invoice_footer_message", ""),

                ShowBarcode = bool.Parse(invoiceSettings.GetValueOrDefault("show_barcode", "true")),
                ShowLogo = bool.Parse(invoiceSettings.GetValueOrDefault("show_logo", "true")),
                ShowQRCode = bool.Parse(invoiceSettings.GetValueOrDefault("show_qr_code", "false")),
                AutoPrint = bool.Parse(invoiceSettings.GetValueOrDefault("auto_print", "true")),
                PaperSize = invoiceSettings.GetValueOrDefault("paper_size", "K80"),

                PrinterName = printerSettings.GetValueOrDefault("printer_name", "")
            };

            return View(viewModel);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Save([FromBody] SystemSettingsVM model)
        {
            try
            {
                var settings = new Dictionary<string, string>
                {
                    { "store_name", model.StoreName },
                    { "store_phone", model.StorePhone },
                    { "store_address", model.StoreAddress },
                    { "invoice_footer_message", model.InvoiceFooterMessage },
                    { "show_barcode", model.ShowBarcode.ToString().ToLower() },
                    { "show_logo", model.ShowLogo.ToString().ToLower() },
                    { "show_qr_code", model.ShowQRCode.ToString().ToLower() },
                    { "auto_print", model.AutoPrint.ToString().ToLower() },
                    { "paper_size", model.PaperSize },
                    { "printer_name", model.PrinterName }
                };

                // Get staff ID from cookie
                var staffId = Request.Cookies["staff_id"];
                Guid? updatedBy = null;
                if (Guid.TryParse(staffId, out var parsedStaffId))
                {
                    updatedBy = parsedStaffId;
                }

                await _settingService.UpdateSettingsAsync(settings, updatedBy);

                return Json(new { success = true, message = "L?u c?u hình thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "L?i: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            try
            {
                var storeSettings = await _settingService.GetSettingsByCategoryAsync("Store");
                var invoiceSettings = await _settingService.GetSettingsByCategoryAsync("Invoice");
                var printerSettings = await _settingService.GetSettingsByCategoryAsync("Printer");

                var settings = new
                {
                    storeName = storeSettings.GetValueOrDefault("store_name", ""),
                    storePhone = storeSettings.GetValueOrDefault("store_phone", ""),
                    storeAddress = storeSettings.GetValueOrDefault("store_address", ""),
                    invoiceFooterMessage = storeSettings.GetValueOrDefault("invoice_footer_message", ""),
                    showBarcode = bool.Parse(invoiceSettings.GetValueOrDefault("show_barcode", "true")),
                    showLogo = bool.Parse(invoiceSettings.GetValueOrDefault("show_logo", "true")),
                    showQRCode = bool.Parse(invoiceSettings.GetValueOrDefault("show_qr_code", "false")),
                    autoPrint = bool.Parse(invoiceSettings.GetValueOrDefault("auto_print", "true")),
                    paperSize = invoiceSettings.GetValueOrDefault("paper_size", "K80"),
                    printerName = printerSettings.GetValueOrDefault("printer_name", "")
                };

                return Json(new { success = true, data = settings });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

