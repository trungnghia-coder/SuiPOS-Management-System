-- Seed default system settings
INSERT INTO [SystemSettings] ([Id], [Key], [Value], [Description], [DataType], [Category], [CreatedAt])
VALUES 
    (NEWID(), 'store_name', 'thietbisieuthi.bi', N'Tên c?a hàng', 'string', 'Store', GETUTCDATE()),
    (NEWID(), 'store_phone', '0971902631', N'S? ?i?n tho?i c?a hàng', 'string', 'Store', GETUTCDATE()),
    (NEWID(), 'store_address', N'Tr?n ??i Ngh?a, Hoài ??c, Xã S?n Tây, Huy?n Hoài ??c, Hà N?i', N'??a ch? c?a hàng', 'string', 'Store', GETUTCDATE()),
    (NEWID(), 'invoice_footer_message', N'Quý khách ???c phép ??i tr? hàng trong vòng 7 ngày k? t? ngày mua hàng', N'L?i nh?n cu?i hóa ??n', 'string', 'Store', GETUTCDATE()),
    (NEWID(), 'show_barcode', 'true', N'Hi?n th? barcode trên hóa ??n', 'bool', 'Invoice', GETUTCDATE()),
    (NEWID(), 'show_logo', 'true', N'Hi?n th? logo trên hóa ??n', 'bool', 'Invoice', GETUTCDATE()),
    (NEWID(), 'show_qr_code', 'false', N'Hi?n th? QR code trên hóa ??n', 'bool', 'Invoice', GETUTCDATE()),
    (NEWID(), 'auto_print', 'true', N'T? ??ng in hóa ??n sau khi thanh toán', 'bool', 'Invoice', GETUTCDATE()),
    (NEWID(), 'paper_size', 'K80', N'Kích th??c gi?y in (K57, K80, A4)', 'string', 'Invoice', GETUTCDATE()),
    (NEWID(), 'printer_name', '', N'Tên máy in', 'string', 'Printer', GETUTCDATE()),
    (NEWID(), 'app_name', 'SuiPOS', N'Tên ?ng d?ng', 'string', 'General', GETUTCDATE()),
    (NEWID(), 'app_version', '1.0.0', N'Phiên b?n ?ng d?ng', 'string', 'General', GETUTCDATE()),
    (NEWID(), 'timezone', 'Asia/Ho_Chi_Minh', N'Múi gi?', 'string', 'General', GETUTCDATE());
