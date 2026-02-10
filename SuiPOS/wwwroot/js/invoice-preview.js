// Invoice Preview Functions
window.openInvoicePreview = function() {
    console.log('Opening invoice preview...');
    const modal = document.getElementById('invoicePreviewModal');
    if (modal) {
        modal.classList.remove('hidden');
        
        // Generate barcode after modal is visible
        setTimeout(() => {
            try {
                if (typeof JsBarcode !== 'undefined') {
                    JsBarcode("#barcode", "SO04180001", {
                        format: "CODE128",
                        width: 2,
                        height: 40,
                        displayValue: false,
                        margin: 0
                    });
                } else {
                    console.error('JsBarcode library not loaded');
                }
            } catch (e) {
                console.error('Barcode generation error:', e);
            }
        }, 100);
    } else {
        console.error('Invoice modal not found! Make sure InvoicePreviewModal component is loaded.');
    }
};

window.closeInvoicePreview = function() {
    const modal = document.getElementById('invoicePreviewModal');
    if (modal) {
        modal.classList.add('hidden');
    }
};

window.printInvoicePDF = function () {
    const invoiceContent = document.getElementById('invoiceContent').innerHTML;

    // Tạo iframe ẩn để in (không hiện tab trắng đằng sau)
    let iframe = document.getElementById('printFrame');
    if (!iframe) {
        iframe = document.createElement('iframe');
        iframe.id = 'printFrame';
        iframe.style.visibility = 'hidden';
        iframe.style.position = 'fixed';
        iframe.style.right = '0';
        iframe.style.bottom = '0';
        document.body.appendChild(iframe);
    }

    const doc = iframe.contentWindow.document;
    doc.open();
    doc.write(`
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="utf-8">
            <script src="https://cdn.tailwindcss.com"><\/script>
            <script src="https://cdn.jsdelivr.net/npm/jsbarcode@3.11.5/dist/JsBarcode.all.min.js"><\/script>
        </head>
        <body>
            ${invoiceContent}
            <script>
                window.onload = function() {
                    try {
                        JsBarcode("#barcode", "SO04180001", {
                            format: "CODE128",
                            width: 2,
                            height: 40,
                            displayValue: false,
                            margin: 0
                        });
                    } catch(e) { console.error(e); }

                    setTimeout(() => {
                        window.focus();
                        window.print();
                    }, 600);
                };
            <\/script>
        </body>
        </html>
    `);
    doc.close();
};

// ESC to close
document.addEventListener('keydown', function(e) {
    if (e.key === 'Escape') {
        const modal = document.getElementById('invoicePreviewModal');
        if (modal && !modal.classList.contains('hidden')) {
            window.closeInvoicePreview();
        }
    }
});

// Debug: Log when script loads
console.log('Invoice preview functions loaded successfully');
