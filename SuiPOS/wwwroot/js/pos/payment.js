// ============================================
// PAYMENT MANAGEMENT
// ============================================

let paymentMethods = {}; // Store payment methods by order ID
let paymentMethodCounter = 0;

// Initialize default payment method
function initializeDefaultPaymentMethod() {
    const activeTab = document.querySelector('.order-tab.active');
    const orderId = activeTab.dataset.orderId;
    
    if (!paymentMethods[orderId] || paymentMethods[orderId].length === 0) {
        if (!paymentMethods[orderId]) {
            paymentMethods[orderId] = [];
        }
        
        paymentMethodCounter++;
        paymentMethods[orderId].push({
            id: paymentMethodCounter,
            type: 'cash',
            amount: 0
        });
        
        renderPaymentMethods();
    }
}

// Add payment method
function addPaymentMethod() {
    const activeTab = document.querySelector('.order-tab.active');
    const orderId = activeTab.dataset.orderId;

    if (!paymentMethods[orderId]) {
        paymentMethods[orderId] = [];
    }

    paymentMethodCounter++;
    const methodId = paymentMethodCounter;

    paymentMethods[orderId].push({
        id: methodId,
        type: 'cash',
        amount: 0
    });

    renderPaymentMethods();
}

// Remove payment method
function removePaymentMethod(methodId) {
    const activeTab = document.querySelector('.order-tab.active');
    const orderId = activeTab.dataset.orderId;

    paymentMethods[orderId] = paymentMethods[orderId].filter(m => m.id !== methodId);
    renderPaymentMethods();
}

// Update payment method type
function updatePaymentMethod(methodId, field, value) {
    const activeTab = document.querySelector('.order-tab.active');
    const orderId = activeTab.dataset.orderId;

    const method = paymentMethods[orderId].find(m => m.id === methodId);
    if (method) {
        method[field] = value;
        renderPaymentMethods();
    }
}

// Handle payment amount input
function handlePaymentAmountInput(methodId, inputElement) {
    const value = inputElement.value;
    const numericValue = value.replace(/\D/g, '');
    
    const activeTab = document.querySelector('.order-tab.active');
    const orderId = activeTab.dataset.orderId;
    const method = paymentMethods[orderId].find(m => m.id === methodId);
    if (method) {
        method.amount = parseInt(numericValue) || 0;
        
        const formattedValue = numericValue ? parseInt(numericValue).toLocaleString() : '';
        
        const start = inputElement.selectionStart;
        const end = inputElement.selectionEnd;
        const oldLength = value.length;
        
        inputElement.value = formattedValue;
        
        const newLength = formattedValue.length;
        const diff = newLength - oldLength;
        inputElement.setSelectionRange(start + diff, end + diff);
        
        updatePaymentSummary();
    }
}

// Render payment methods
function renderPaymentMethods() {
    const activeTab = document.querySelector('.order-tab.active');
    const orderId = activeTab.dataset.orderId;
    const methods = paymentMethods[orderId] || [];

    const container = document.getElementById('paymentMethods');
    
    if (methods.length === 0) {
        initializeDefaultPaymentMethod();
    } else {
        const selectedTypes = methods.map(m => m.type);
        
        container.innerHTML = methods.map((method, index) => `
            <div class="flex gap-2 items-center">
                <select class="flex-1 px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500" 
                        onchange="updatePaymentMethod(${method.id}, 'type', this.value)">
                    <option value="cash" ${method.type === 'cash' ? 'selected' : ''} ${selectedTypes.includes('cash') && method.type !== 'cash' ? 'disabled' : ''}>Tiền mặt</option>
                    <option value="card" ${method.type === 'card' ? 'selected' : ''} ${selectedTypes.includes('card') && method.type !== 'card' ? 'disabled' : ''}>Thẻ</option>
                    <option value="transfer" ${method.type === 'transfer' ? 'selected' : ''} ${selectedTypes.includes('transfer') && method.type !== 'transfer' ? 'disabled' : ''}>Chuyển khoản</option>
                </select>
                <input type="text" 
                       value="${method.amount > 0 ? method.amount.toLocaleString() : ''}" 
                       placeholder="0"
                       inputmode="numeric"
                       data-method-id="${method.id}"
                       class="w-32 px-3 py-2 text-sm border border-gray-300 rounded-lg text-right font-semibold focus:outline-none focus:ring-2 focus:ring-blue-500"
                       oninput="handlePaymentAmountInput(${method.id}, this)">
                ${methods.length > 1 ? `
                    <button onclick="removePaymentMethod(${method.id})" class="p-2 text-gray-400 hover:text-red-500">
                        <i class="fas fa-times"></i>
                    </button>
                ` : '<div class="w-10"></div>'}
            </div>
        `).join('');
    }

    updatePaymentSummary();
}

// Update payment summary
function updatePaymentSummary() {
    const activeTab = document.querySelector('.order-tab.active');
    const orderId = activeTab.dataset.orderId;
    const methods = paymentMethods[orderId] || [];
    const cartItems = cart[orderId] || [];

    const totalAmount = cartItems.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    const totalPaid = methods.reduce((sum, method) => sum + method.amount, 0);
    const change = totalPaid - totalAmount;

    document.getElementById('customerGive').textContent = totalPaid.toLocaleString();
    
    const changeElement = document.getElementById('changeAmount');
    const changeLabel = document.getElementById('changeLabel');
    
    if (change >= 0) {
        changeLabel.textContent = 'Tiền thừa trả khách';
        changeElement.textContent = change.toLocaleString();
    } else {
        changeLabel.textContent = 'Khách cần trả thêm';
        changeElement.textContent = Math.abs(change).toLocaleString();
    }
}

// Update order summary
function updateOrderSummary() {
    const activeTab = document.querySelector('.order-tab.active');
    const orderId = activeTab.dataset.orderId;
    const cartItems = cart[orderId] || [];

    const totalItems = cartItems.reduce((sum, item) => sum + item.quantity, 0);
    const totalAmount = cartItems.reduce((sum, item) => sum + (item.price * item.quantity), 0);

    document.getElementById('totalItems').textContent = `${totalItems} sản phẩm`;
    document.getElementById('totalAmount').textContent = totalAmount.toLocaleString();
    document.getElementById('customerPay').textContent = totalAmount.toLocaleString();

    // Auto-update first payment method with total amount
    if (paymentMethods[orderId] && paymentMethods[orderId].length > 0 && totalAmount > 0) {
        paymentMethods[orderId][0].amount = totalAmount;
        renderPaymentMethods();
    }

    updatePaymentSummary();
}

