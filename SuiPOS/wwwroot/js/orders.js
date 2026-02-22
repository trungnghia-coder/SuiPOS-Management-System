// ============================================
// ORDERS PAGE - ORDER MANAGEMENT
// ============================================

let currentOrders = [];
let selectedOrderId = null;

// ========== LOAD ORDERS ==========
async function loadOrders() {
    try {
        const fromDate = document.getElementById('fromDate').value;
        const toDate = document.getElementById('toDate').value;
        
        let url = '/Orders/GetOrders';
        const params = new URLSearchParams();
        if (fromDate) params.append('fromDate', fromDate);
        if (toDate) params.append('toDate', toDate);
        
        if (params.toString()) {
            url += '?' + params.toString();
        }

        const response = await fetch(url);
        currentOrders = await response.json();
        
        renderOrderList(currentOrders);
    } catch (error) {
        console.error('Error loading orders:', error);
        alert('Lỗi khi tải danh sách đơn hàng');
    }
}

// ========== RENDER ORDER LIST ==========
function renderOrderList(orders) {
    const container = document.getElementById('orderList');
    
    if (!orders.length) {
        container.innerHTML = '<div class="p-8 text-center text-gray-500">Không có đơn hàng nào</div>';
        return;
    }

    container.innerHTML = orders.map(order => {
        const statusClass = getStatusClass(order.status);
        const statusText = getStatusText(order.status);
        
        return `
            <div class="order-item p-4 border-b border-gray-200 hover:bg-gray-50 cursor-pointer ${selectedOrderId === order.id ? 'bg-blue-50' : ''}" 
                 data-order-id="${order.id}"
                 onclick="selectOrder('${order.id}')">
                <div class="flex items-start justify-between mb-2">
                    <div>
                        <div class="font-medium text-sm text-gray-800">${order.orderCode}</div>
                        <div class="text-xs text-gray-500">${order.customerName || 'Khách lẻ'}</div>
                    </div>
                    <div class="text-right">
                        <div class="font-semibold text-sm">${order.totalAmount.toLocaleString()} ₫</div>
                        <span class="order-status-badge ${statusClass}">${statusText}</span>
                    </div>
                </div>
                <div class="text-xs text-gray-600">${formatDateTime(order.orderDate)}</div>
            </div>
        `;
    }).join('');
}

// ========== SELECT ORDER & SHOW DETAIL ==========
async function selectOrder(orderId) {
    selectedOrderId = orderId;
    
    // Highlight selected
    document.querySelectorAll('.order-item').forEach(item => {
        item.classList.remove('bg-blue-50');
    });
    document.querySelector(`[data-order-id="${orderId}"]`)?.classList.add('bg-blue-50');
    
    try {
        const response = await fetch(`/Orders/GetOrderDetail?id=${orderId}`);
        const order = await response.json();
        
        renderOrderDetail(order);
    } catch (error) {
        console.error('Error loading order detail:', error);
        alert('Lỗi khi tải chi tiết đơn hàng');
    }
}

// ========== RENDER ORDER DETAIL ==========
function renderOrderDetail(order) {
    const detailContainer = document.getElementById('orderDetailContainer');
    
    const statusClass = getStatusClass(order.status);
    const statusText = getStatusText(order.status);
    
    detailContainer.innerHTML = `
        <!-- Order Header -->
        <div class="p-3 border-b border-gray-200">
            <div class="flex items-center justify-between mb-4">
                <div>
                    <h2 class="text-sm font-bold text-gray-800">Đơn ${order.orderCode}</h2>
                    <div class="flex items-center gap-3 mt-2">
                        <span class="order-status-badge ${statusClass}">${statusText}</span>
                    </div>
                </div>
                <div class="flex items-center gap-2">
                    <button class="p-2 hover:bg-gray-100 rounded-lg">
                        <i class="fas fa-print text-gray-600"></i>
                    </button>
                    <div class="relative">
                        <select id="orderActionDropdown" class="px-4 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 cursor-pointer bg-white">
                            <option value="" selected disabled>Thao tác</option>
                            ${order.status === 'Completed' ? `
                                <option value="reset">Đặt lại</option>
                                <option value="refund">Hoàn trả</option>
                                <option value="cancel">Hủy đơn hàng</option>
                            ` : ''}
                        </select>
                    </div>
                </div>
            </div>
        </div>

        <!-- Order Items -->
        <div class="flex overflow-y-auto h-full">
            <div class="rounded-lg flex-1 overflow-y-auto">
                <div class="grid grid-cols-2 gap-4 text-sm p-3 border-b sticky top-0 bg-white z-10">
                    <div>
                        <span class="text-gray-600">Người bán:</span>
                        <span class="font-medium ml-2">${order.staffName || '--'}</span>
                    </div>
                </div>
                
                <!-- Products List -->
                <div class="overflow-y-auto">
                    ${order.items.map(item => `
                        <div class="flex items-center gap-4 p-4 border-b border-gray-200">
                            <img src="${item.imageUrl || 'https://placehold.co/100x100/webp?text=No+Image'}" 
                                 alt="${item.productName}" 
                                 class="w-16 h-16 object-cover rounded-lg">
                            <div class="flex-1">
                                <h4 class="font-medium text-sm mb-1">${item.productName}</h4>
                                <p class="text-xs text-gray-500">${item.variantName}</p>
                                <p class="text-xs text-gray-400">${item.sku}</p>
                                <div class="text-bottom flex w-auto gap-20 mt-2">
                                    <div class="text-sm mb-1">${item.unitPrice.toLocaleString()} ₫</div>
                                    <div class="text-sm mb-1">x ${item.quantity}</div>
                                    <div class="text-sm font-semibold">${item.totalPrice.toLocaleString()} ₫</div>
                                </div>
                            </div>
                        </div>
                    `).join('')}
                </div>
            </div>

            <div class="p-3 h-auto border-l bg-white">
                <div class="max-w-md ml-auto space-y-2 text-sm">
                    <div class="flex justify-between">
                        <span class="text-gray-600">Tổng tiền hàng</span>
                        <div class="flex items-center gap-2">
                            <span class="text-xs bg-gray-200 px-2 py-1 rounded">${order.items.length} sản phẩm</span>
                            <span class="font-semibold">${order.totalAmount.toLocaleString()} ₫</span>
                        </div>
                    </div>
                    ${order.discount > 0 ? `
                        <div class="flex justify-between">
                            <span class="text-gray-600">Giảm giá đơn hàng</span>
                            <span class="font-semibold text-red-600">-${order.discount.toLocaleString()} ₫</span>
                        </div>
                    ` : ''}
                    <div class="flex justify-between">
                        <span class="text-gray-600">Ngày</span>
                        <span class="font-semibold">${formatDate(order.orderDate)}</span>
                    </div>
                    <div class="h-px bg-gray-300 my-2"></div>
                    
                    <!-- Payment Methods -->
                    ${order.payments.map(p => `
                        <div class="flex justify-between">
                            <span class="text-gray-600">${getPaymentMethodText(p.method)}</span>
                            <span class="font-semibold">${p.amount.toLocaleString()} ₫</span>
                        </div>
                    `).join('')}
                    
                    <div class="flex justify-between">
                        <span class="text-gray-600">Tiền khách đã thanh toán</span>
                        <span class="font-semibold">${order.amountReceived.toLocaleString()} ₫</span>
                    </div>
                    <div class="flex justify-between text-base font-bold text-blue-600">
                        <span>Tiền thừa</span>
                        <span>${order.changeAmount.toLocaleString()} ₫</span>
                    </div>
                </div>

                <!-- Note -->
                <div class="mt-4 p-3 bg-white rounded-lg border border-gray-200">
                    <div class="flex items-center gap-2 text-gray-600">
                        <i class="fas fa-sticky-note"></i>
                        <span class="text-sm">${order.note || 'Không có ghi chú cho đơn hàng này'}</span>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    // Attach event listener to dropdown
    document.getElementById('orderActionDropdown')?.addEventListener('change', function(e) {
        handleOrderAction(e.target.value, order.id);
        e.target.value = '';
    });
}

function handleOrderAction(action, orderId) {
    if (action === 'reset') {
        reorder(orderId);
    } else if (action === 'refund') {
        refundOrder(orderId);
    } else if (action === 'cancel') {
        cancelOrder(orderId);
    }
}

function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('vi-VN');
}

// ========== SEARCH ORDERS ==========
function searchOrders() {
    const searchTerm = document.getElementById('searchInput').value.toLowerCase();
    
    if (!searchTerm) {
        renderOrderList(currentOrders);
        return;
    }
    
    const filtered = currentOrders.filter(order => 
        order.orderCode.toLowerCase().includes(searchTerm) ||
        (order.customerName && order.customerName.toLowerCase().includes(searchTerm))
    );
    
    renderOrderList(filtered);
}

// ========== REORDER ==========
async function reorder(orderId) {
    if (!confirm('Đặt lại đơn hàng này?\nGiỏ hàng hiện tại sẽ bị xóa.')) {
        return;
    }
    
    try {
        // Load order detail
        const response = await fetch(`/Orders/GetOrderDetail?id=${orderId}`);
        const order = await response.json();
        
        if (!order || !order.items || !order.items.length) {
            alert('Không thể tải thông tin đơn hàng!');
            return;
        }
        
        // Validate stock for all items
        const validateResponse = await fetch('/Orders/ValidateStock', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify({
                items: order.items.map(item => ({
                    variantId: item.variantId,
                    quantity: item.quantity
                }))
            })
        });
        
        const validateResult = await validateResponse.json();
        
        if (!validateResult.success) {
            alert(validateResult.message || 'Một số sản phẩm không còn đủ hàng trong kho!');
            return;
        }
        
        // Convert order items to cart format (only available items)
        const cartItems = validateResult.availableItems.map(item => ({
            variantId: item.variantId,
            sku: item.sku,
            name: `${item.productName} - ${item.variantName}`,
            productName: item.productName,
            variantName: item.variantName,
            price: item.unitPrice,
            quantity: item.quantity,
            imageUrl: item.imageUrl || 'https://placehold.co/100x100/webp?text=No+Image'
        }));
        
        // Clear old cart and save new cart
        sessionStorage.removeItem('pos_cart_data');
        sessionStorage.setItem('reorder_cart', JSON.stringify(cartItems));
        
        // Show warning if some items are unavailable
        if (validateResult.unavailableItems && validateResult.unavailableItems.length > 0) {
            alert(`Đã thêm ${cartItems.length} sản phẩm vào giỏ hàng.\n\nCảnh báo: ${validateResult.unavailableItems.length} sản phẩm không thể thêm do hết hàng hoặc đã xóa.`);
        }
        
        // Redirect to POS
        window.location.href = '/POS/Index';
    } catch (error) {
        console.error('Error loading order for reorder:', error);
        alert('Lỗi khi tải đơn hàng!');
    }
}

// ========== REFUND ORDER ==========
async function refundOrder(orderId) {
    if (!confirm('Xác nhận hoàn trả đơn hàng này?\nTồn kho sẽ được cộng lại.')) {
        return;
    }
    
    try {
        const response = await fetch(`/Orders/Refund?id=${orderId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            }
        });
        
        const result = await response.json();
        
        if (result.success) {
            alert('Hoàn trả đơn hàng thành công!');
            await loadOrders();
            if (selectedOrderId === orderId) {
                await selectOrder(orderId);
            }
        } else {
            alert(result.message || 'Lỗi khi hoàn trả đơn hàng');
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Lỗi khi hoàn trả đơn hàng');
    }
}

// ========== CANCEL ORDER ==========
async function cancelOrder(orderId) {
    if (!confirm('Xác nhận hủy đơn hàng này?\nTồn kho sẽ được cộng lại.\nThao tác này không thể hoàn tác!')) {
        return;
    }
    
    try {
        const response = await fetch(`/Orders/Cancel?id=${orderId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            }
        });
        
        const result = await response.json();
        
        if (result.success) {
            alert('Hủy đơn hàng thành công!');
            await loadOrders();
            if (selectedOrderId === orderId) {
                await selectOrder(orderId);
            }
        } else {
            alert(result.message || 'Lỗi khi hủy đơn hàng');
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Lỗi khi hủy đơn hàng');
    }
}

// ========== HELPERS ==========
function getStatusClass(status) {
    const statusMap = {
        'Completed': 'status-payed',
        'Cancelled': 'bg-red-100 text-red-700',
        'Refunded': 'bg-yellow-100 text-yellow-700'
    };
    return statusMap[status] || 'bg-gray-100 text-gray-700';
}

function getStatusText(status) {
    const textMap = {
        'Completed': 'Đã thanh toán',
        'Cancelled': 'Đã hủy',
        'Refunded': 'Đã hoàn trả'
    };
    return textMap[status] || status;
}

function getPaymentMethodText(method) {
    const methodMap = {
        'cash': 'Tiền mặt',
        'card': 'Thẻ',
        'transfer': 'Chuyển khoản'
    };
    return methodMap[method] || method;
}

function formatDateTime(dateString) {
    const date = new Date(dateString);
    return date.toLocaleString('vi-VN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit'
    });
}

// ========== INIT ==========
document.addEventListener('DOMContentLoaded', function() {
    // Set default date range (last 30 days)
    const today = new Date();
    const last30Days = new Date();
    last30Days.setDate(today.getDate() - 30);
    
    document.getElementById('fromDate').value = last30Days.toISOString().split('T')[0];
    document.getElementById('toDate').value = today.toISOString().split('T')[0];
    
    // Load orders
    loadOrders();
    
    // Event listeners
    document.getElementById('searchInput').addEventListener('input', searchOrders);
    document.getElementById('fromDate').addEventListener('change', loadOrders);
    document.getElementById('toDate').addEventListener('change', loadOrders);
});
