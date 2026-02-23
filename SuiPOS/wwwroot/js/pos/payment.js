// ============================================
// PAYMENT METHODS - SINGLE ORDER
// ============================================
const MAX_PAYMENT_METHODS = 3;

const Payment = {
    STORAGE_KEY: 'pos_payment_methods',
    counter: 0,
    
    get methods() {
        const stored = sessionStorage.getItem(this.STORAGE_KEY);
        return stored ? JSON.parse(stored) : [];
    },
    
    set methods(value) {
        sessionStorage.setItem(this.STORAGE_KEY, JSON.stringify(value));
    },
    
    addMethod(type = 'cash', amount = 0) {
        const methods = this.methods;
        this.counter++;
        methods.push({
            id: this.counter,
            type: type,
            amount: amount
        });
        this.methods = methods;
    },
    
    removeMethod(id) {
        this.methods = this.methods.filter(m => m.id !== id);
    },
    
    updateMethod(id, field, value) {
        const methods = this.methods;
        const method = methods.find(m => m.id === id);
        if (method) {
            method[field] = value;
            this.methods = methods;
        }
    },
    
    clear() {
        sessionStorage.removeItem(this.STORAGE_KEY);
        this.counter = 0;
    }
};

function initializeDefaultPaymentMethod() {
    if (Payment.methods.length === 0) {
        Payment.addMethod('cash', 0);
        renderPaymentMethods();
    }
}

function addPaymentMethod() {
    const methods = Payment.methods;
    
    // Giới hạn tối đa 3 phương thức thanh toán
    if (methods.length >= MAX_PAYMENT_METHODS) {
        alert(`Chỉ được thêm tối đa ${MAX_PAYMENT_METHODS} phương thức thanh toán!`);
        return;
    }
    
    // Tìm phương thức chưa được sử dụng
    const usedTypes = methods.map(m => m.type);
    const availableTypes = ['cash', 'card', 'transfer'].filter(t => !usedTypes.includes(t));
    
    if (availableTypes.length === 0) {
        return;
    }
    
    // Thêm phương thức đầu tiên còn available
    Payment.addMethod(availableTypes[0], 0);
    renderPaymentMethods();
}

function removePaymentMethod(methodId) {
    Payment.removeMethod(methodId);
    renderPaymentMethods();
}

function updatePaymentMethod(methodId, field, value) {
    Payment.updateMethod(methodId, field, value);
    renderPaymentMethods();
}

function handlePaymentAmountInput(methodId, inputElement) {
    const rawValue = inputElement.value.replace(/\D/g, '');
    
    const cleanValue = rawValue.replace(/^0+/, '') || '0';
    
    const amount = parseInt(cleanValue, 10) || 0;
    
    Payment.updateMethod(methodId, 'amount', amount);
    
    inputElement.value = cleanValue === '0' ? '' : cleanValue;
    
    updatePaymentSummary();
}

function formatPaymentOnBlur(methodId, inputElement) {
    const amount = Payment.methods.find(m => m.id === methodId)?.amount || 0;
    inputElement.value = amount > 0 ? amount.toLocaleString('en-US') : '';
}

function renderPaymentMethods() {
    const methods = Payment.methods;
    const container = document.getElementById('paymentMethods');
    
    if (!container) return;
    
    if (methods.length === 0) {
        initializeDefaultPaymentMethod();
        return;
    }
    
    const selectedTypes = methods.map(m => m.type);
    
    container.innerHTML = methods.map(method => `
        <div class="flex gap-2 items-center">
            <select class="flex-1 px-3 py-2 text-sm border border-gray-300 rounded-lg" 
                    onchange="updatePaymentMethod(${method.id}, 'type', this.value)">
                <option value="cash" ${method.type === 'cash' ? 'selected' : ''} ${selectedTypes.includes('cash') && method.type !== 'cash' ? 'disabled' : ''}>Tiền mặt</option>
                <option value="card" ${method.type === 'card' ? 'selected' : ''} ${selectedTypes.includes('card') && method.type !== 'card' ? 'disabled' : ''}>Thẻ</option>
                <option value="transfer" ${method.type === 'transfer' ? 'selected' : ''} ${selectedTypes.includes('transfer') && method.type !== 'transfer' ? 'disabled' : ''}>Chuyển khoản</option>
            </select>
            <input type="text" 
                   value="${method.amount > 0 ? method.amount.toLocaleString() : ''}" 
                   placeholder="0"
                   class="w-32 px-3 py-2 text-sm border border-gray-300 rounded-lg text-right font-semibold"
                   oninput="handlePaymentAmountInput(${method.id}, this)"
                   onblur="formatPaymentOnBlur(${method.id}, this)">
            ${methods.length > 1 ? `
                <button onclick="removePaymentMethod(${method.id})" class="p-2 text-gray-400 hover:text-red-500">
                    <i class="fas fa-times"></i>
                </button>
            ` : '<div class="w-10"></div>'}
        </div>
    `).join('');

    updatePaymentSummary();
}

function updatePaymentSummary() {
    const methods = Payment.methods;
    const cartItems = Cart.getItems();

    const totalAmount = cartItems.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    
    // ✅ Get discount from promotion
    const discountAmount = getCurrentDiscount ? getCurrentDiscount() : 0;
    const finalAmount = totalAmount - discountAmount;
    
    const totalPaid = methods.reduce((sum, method) => sum + method.amount, 0);
    const change = totalPaid - finalAmount; // Use finalAmount (after discount)

    document.getElementById('customerGive').textContent = totalPaid.toLocaleString() + 'đ';
    
    const changeElement = document.getElementById('changeAmount');
    const changeLabel = document.getElementById('changeLabel');
    
    if (change >= 0) {
        changeLabel.textContent = 'Tiền thừa trả khách';
        changeElement.textContent = change.toLocaleString() + 'đ';
    } else {
        changeLabel.textContent = 'Khách cần trả thêm';
        changeElement.textContent = Math.abs(change).toLocaleString() + 'đ';
    }
}

// Update order summary
function updateOrderSummary() {
    const cartItems = Cart.getItems();

    const totalItems = cartItems.reduce((sum, item) => sum + item.quantity, 0);
    const totalAmount = cartItems.reduce((sum, item) => sum + (item.price * item.quantity), 0);

    document.getElementById('totalItems').textContent = `${totalItems} sản phẩm`;
    document.getElementById('totalAmount').textContent = totalAmount.toLocaleString() + 'đ';
    
    // ✅ Calculate final amount after discount
    const discountAmount = getCurrentDiscount ? getCurrentDiscount() : 0;
    const finalAmount = totalAmount - discountAmount;
    document.getElementById('customerPay').textContent = finalAmount.toLocaleString() + 'đ';

    // ONLY auto-set first payment if it's the ONLY payment and amount is 0
    const methods = Payment.methods;
    if (methods.length === 1 && methods[0].amount === 0 && finalAmount > 0) {
        Payment.updateMethod(methods[0].id, 'amount', finalAmount);
        renderPaymentMethods();
    } else {
        // Don't re-render if multiple payments exist (to preserve user input)
        updatePaymentSummary();
    }
}


async function submitOrder() {
const cartData = Cart.getOrderData();
const customer = CustomerState.customer;
const payments = Payment.methods;

if (!cartData.items.length) {
    alert('Giỏ hàng trống!');
    return;
}

if (!payments.length || payments.every(p => p.amount === 0)) {
    alert('Vui lòng nhập số tiền thanh toán!');
    return;
}

// ✅ Validate: Khách phải đưa đủ tiền
const discountAmount = getCurrentDiscount ? getCurrentDiscount() : 0;
const finalAmount = cartData.totalAmount - discountAmount;
const totalPaid = payments.reduce((sum, p) => sum + p.amount, 0);
    
if (totalPaid < finalAmount) {
    const shortage = finalAmount - totalPaid;
    alert(`Khách chưa đưa đủ tiền!\nCòn thiếu: ${shortage.toLocaleString()}đ`);
    return;
}

    // ✅ Get staff ID from cookie and validate
    const getCookie = (name) => {
        const value = `; ${document.cookie}`;
        const parts = value.split(`; ${name}=`);
        if (parts.length === 2) return parts.pop().split(';').shift();
        return null;
    };
    
    // ✅ Debug: Log all cookies
    console.log('🍪 All cookies:', document.cookie);
    
    const staffIdString = getCookie('staff_id');
    const staffName = getCookie('staff_name');
    
    console.log('🍪 staff_id cookie:', staffIdString);
    console.log('🍪 staff_name cookie:', staffName);
    
    let staffId = null;
    
    // ✅ Validate if it's a valid GUID format
    const guidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
    
    if (staffIdString && staffIdString !== 'null' && staffIdString !== 'undefined') {
        if (guidRegex.test(staffIdString)) {
            staffId = staffIdString;
            console.log('✅ Valid Staff ID:', staffId);
        } else {
            console.error('❌ Invalid GUID format for staff_id:', staffIdString);
            console.error('❌ Testing GUID regex on value:', staffIdString, 'Result:', guidRegex.test(staffIdString));
        }
    } else {
        console.warn('⚠️ No staff_id cookie found or invalid value:', staffIdString);
        console.warn('⚠️ Please check if you are logged in');
    }
    
    // Get order note from textarea
    const orderNote = document.getElementById('orderNote')?.value.trim() || null;

    const orderData = {
        customerId: customer ? customer.id : null,
        staffId: staffId, // ✅ Send as Guid string or null
        items: cartData.items,
        payments: payments.map(p => ({
            method: p.type,
            amount: p.amount,
            reference: null
        })),
        totalAmount: cartData.totalAmount,
        amountReceived: totalPaid,
        discountAmount: discountAmount,
        note: orderNote
    };

    console.log('📦 Submitting Order Data:', orderData);
    console.log('📦 StaffId:', { type: typeof staffId, value: staffId, isNull: staffId === null });




    try {
        const response = await fetch('/POS/Checkout', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify(orderData)
        });

        const result = await response.json();

        if (result.success) {
            const orderId = result.orderId;
            
            // Clear cart and customer first
            Cart.clear();
            CustomerState.customer = null;
            Payment.clear();
            
            // Show success message and ask for print
            if (confirm('Thanh toán thành công! Bạn có muốn in hóa đơn không?')) {
                // Load order data and print immediately
                await loadOrderAndPrint(orderId);
            } else {
                // Just reload if user doesn't want to print
                location.reload();
            }
        } else {
            alert(result.message || 'Lỗi khi tạo đơn hàng!');
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Không thể kết nối với server!');
    }
}

document.querySelector('.submitOrder').addEventListener('click', function () {
    submitOrder();
});

// Function to load order data, update invoice modal and print
async function loadOrderAndPrint(orderId) {
    try {
        // Load order data
        const orderResponse = await fetch(`/POS/GetOrderDetail?orderId=${orderId}`);
        const orderResult = await orderResponse.json();

        if (!orderResult.success || !orderResult.data) {
            alert('Không thể tải thông tin hóa đơn!');
            location.reload();
            return;
        }

        // Load system settings for store info
        const settingsResponse = await fetch('/Settings/GetSettings');
        const settingsResult = await settingsResponse.json();
        
        // Update invoice modal with real data
        await updateInvoiceModalWithData(orderResult.data, settingsResult.data || {});
        
        // Show modal briefly then print
        openInvoicePreview();
        
        // Wait a bit for modal to render, then print
        setTimeout(() => {
            printInvoice();
            
            // Close modal and reload after printing
            setTimeout(() => {
                closeInvoicePreview();
                location.reload();
            }, 500);
        }, 500);
        
    } catch (error) {
        console.error('Error loading order:', error);
        alert('Có lỗi khi tải hóa đơn!');
        location.reload();
    }
}

// Function to update invoice modal with real order and settings data
function updateInvoiceModalWithData(orderData, settings) {
    const invoiceContent = document.getElementById('invoiceContent');
    if (!invoiceContent) return;

    // Update store info from settings
    const storeName = settings.storeName || 'SuiPOS';
    const storePhone = settings.storePhone || '';
    const storeAddress = settings.storeAddress || '';
    const invoiceFooter = settings.invoiceFooterMessage || '';

    // Build invoice HTML
    const invoiceHTML = `
        <!-- Store Info -->
        <div class="text-center mb-4">
            <div class="text-lg font-bold">${storeName}</div>
            ${storeAddress ? `<div class="text-xs text-gray-600">${storeAddress}</div>` : ''}
            ${storePhone ? `<div class="text-xs text-gray-600">${storePhone}</div>` : ''}
        </div>

        <!-- Invoice Title -->
        <div class="text-center mb-3">
            <div class="text-base font-bold">HÓA ĐƠN BÁN HÀNG</div>
            <div class="text-xs mt-2">${orderData.orderCode}</div>
        </div>

        <!-- Invoice Details -->
        <div class="text-xs space-y-1 mb-3">
            <div class="flex justify-between">
                <span>Ngày bán</span>
                <span>${new Date(orderData.orderDate).toLocaleString('vi-VN')}</span>
            </div>
            ${orderData.staffName ? `
                <div class="flex justify-between">
                    <span>Thu ngân</span>
                    <span>${orderData.staffName}</span>
                </div>
            ` : ''}
            ${orderData.customerName ? `
                <div class="flex justify-between">
                    <span>Khách hàng</span>
                    <span>${orderData.customerName}</span>
                </div>
            ` : ''}
            ${orderData.customerPhone ? `
                <div class="flex justify-between">
                    <span>SĐT</span>
                    <span>${orderData.customerPhone}</span>
                </div>
            ` : ''}
        </div>

        <!-- Separator -->
        <div class="border-t border-dashed border-gray-400 my-2"></div>

        <!-- Products Table -->
        <table class="w-full text-xs mb-2">
            <thead>
                <tr class="border-b border-gray-300">
                    <th class="text-left pb-1">SL</th>
                    <th class="text-left pb-1">ĐG</th>
                    <th class="text-right pb-1">T.Tiền</th>
                </tr>
            </thead>
            <tbody>
                ${orderData.items.map(item => `
                    <tr>
                        <td colspan="3" class="pt-2 pb-1 font-medium">${item.productName}</td>
                    </tr>
                    ${item.variantName ? `
                        <tr>
                            <td colspan="3" class="text-gray-500 pb-1">${item.variantName}</td>
                        </tr>
                    ` : ''}
                    <tr class="border-gray-200">
                        <td>${item.quantity}</td>
                        <td>${item.unitPrice.toLocaleString()}</td>
                        <td class="text-right">${item.totalPrice.toLocaleString()}</td>
                    </tr>
                `).join('')}
            </tbody>
        </table>

        <!-- Separator -->
        <div class="border-t border-dashed border-gray-400 my-2"></div>

        <!-- Summary -->
        <div class="text-xs space-y-1">
            <div class="flex justify-between">
                <span>Tổng số lượng</span>
                <span class="text-right">${orderData.items.reduce((sum, item) => sum + item.quantity, 0)}</span>
            </div>
            <div class="flex justify-between">
                <span>Tổng tiền hàng</span>
                <span class="text-right">${orderData.totalAmount.toLocaleString()}</span>
            </div>
            ${orderData.discount > 0 ? `
                <div class="flex justify-between">
                    <span>Giảm giá</span>
                    <span class="text-right">-${orderData.discount.toLocaleString()}</span>
                </div>
            ` : ''}
            <div class="flex justify-between font-bold text-sm">
                <span>Khách phải trả</span>
                <span class="text-right">${(orderData.totalAmount - orderData.discount).toLocaleString()}</span>
            </div>
            ${orderData.payments.map(payment => `
                <div class="flex justify-between">
                    <span>${getPaymentMethodName(payment.method)}</span>
                    <span class="text-right">${payment.amount.toLocaleString()}</span>
                </div>
            `).join('')}
            <div class="flex justify-between">
                <span>Tiền khách đưa</span>
                <span class="text-right">${orderData.amountReceived.toLocaleString()}</span>
            </div>
            <div class="flex justify-between">
                <span>Tiền trả lại</span>
                <span class="text-right">${orderData.changeAmount.toLocaleString()}</span>
            </div>
        </div>

        <!-- Separator -->
        <div class="border-t border-dashed border-gray-400 my-3"></div>

        <!-- Footer Message -->
        ${invoiceFooter ? `
            <div class="text-center text-xs text-gray-600">
                ${invoiceFooter}
            </div>
        ` : ''}

        <!-- Thank you -->
        <div class="text-center text-xs font-bold mt-2">
            Cảm ơn quý khách!
        </div>
        
        <!-- Barcode -->
        <div class="text-center mt-3">
            <svg id="barcode-${orderData.orderCode}"></svg>
        </div>
    `;

    invoiceContent.innerHTML = invoiceHTML;
    
    // Generate barcode if JsBarcode is available
    if (typeof JsBarcode !== 'undefined') {
        try {
            JsBarcode(`#barcode-${orderData.orderCode}`, orderData.orderCode, {
                format: "CODE128",
                width: 1,
                height: 30,
                displayValue: true,
                fontSize: 10
            });
        } catch (e) {
            console.error('Barcode generation failed:', e);
        }
    }
}

function getPaymentMethodName(method) {
    const methods = {
        'cash': 'Tiền mặt',
        'card': 'Thẻ',
        'transfer': 'Chuyển khoản'
    };
    return methods[method] || method;
}

function printInvoice() {
    const invoiceContent = document.getElementById('invoiceContent');
    if (!invoiceContent) return;

    // Create a new window for printing
    const printWindow = window.open('', '_blank');
    printWindow.document.write(`
        <html>
            <head>
                <title>Hóa đơn</title>
                <style>
                    @media print {
                        @page { margin: 0; size: 80mm auto; }
                        body { margin: 0; padding: 10mm; }
                    }
                    body {
                        font-family: 'Courier New', monospace;
                        font-size: 12px;
                        max-width: 80mm;
                        margin: 0 auto;
                    }
                    .text-center { text-align: center; }
                    .text-right { text-align: right; }
                    .font-bold { font-weight: bold; }
                    .text-xs { font-size: 11px; }
                    .text-sm { font-size: 12px; }
                    .text-base { font-size: 14px; }
                    .text-lg { font-size: 16px; }
                    .mb-2 { margin-bottom: 8px; }
                    .mb-3 { margin-bottom: 12px; }
                    .mb-4 { margin-bottom: 16px; }
                    .mt-2 { margin-top: 8px; }
                    .my-2 { margin-top: 8px; margin-bottom: 8px; }
                    .my-3 { margin-top: 12px; margin-bottom: 12px; }
                    .pb-1 { padding-bottom: 4px; }
                    .pt-2 { padding-top: 8px; }
                    .space-y-1 > * + * { margin-top: 4px; }
                    table { width: 100%; border-collapse: collapse; }
                    th { text-align: left; border-bottom: 1px solid #333; padding-bottom: 4px; }
                    td { padding: 2px 0; }
                    .border-dashed { border-top: 1px dashed #999; }
                    .text-gray-600 { color: #666; }
                    .text-gray-500 { color: #888; }
                </style>
            </head>
            <body>
                ${invoiceContent.innerHTML}
            </body>
        </html>
    `);
    printWindow.document.close();
    
    // Wait for content to load then print
    setTimeout(() => {
        printWindow.print();
        printWindow.close();
    }, 250);
}

// Legacy function - keep for compatibility with Settings page
async function openInvoicePreviewWithOrder(orderId) {
    try {
        const response = await fetch(`/POS/GetOrderDetail?orderId=${orderId}`);
        const result = await response.json();

        if (result.success && result.data) {
            // Update invoice modal with order data
            const settingsResponse = await fetch('/Settings/GetSettings');
            const settingsResult = await settingsResponse.json();
            updateInvoiceModalWithData(result.data, settingsResult.data || {});
            // Show modal
            openInvoicePreview();
        } else {
            // Fallback: just show modal with default data
            openInvoicePreview();
        }
    } catch (error) {
        console.error('Error loading order:', error);
        // Fallback: just show modal with default data
        openInvoicePreview();
    }
}

function updateInvoiceModal(orderData) {
    // Deprecated - use updateInvoiceModalWithData instead
    console.warn('updateInvoiceModal is deprecated, use updateInvoiceModalWithData');
}




