// ============================================
// UI EVENTS & TAB MANAGEMENT
// ============================================

const MAX_TABS = 8;
let orderCount = 1;
let usedOrderIds = new Set([1]);

// ========== START: Initialize ==========
document.addEventListener('DOMContentLoaded', function() {
    
    document.querySelectorAll('.order-tab').forEach(tab => {
        tab.addEventListener('click', function(e) {
            if (!e.target.closest('button')) {
                switchTab(this);
            }
        });
    });

    document.getElementById('addPaymentMethodBtn').addEventListener('click', addPaymentMethod);
    document.getElementById('openProductListBtn').addEventListener('click', openProductList);
    document.getElementById('addCustomerBtn').addEventListener('click', () => {
        document.getElementById('addCustomerModal').classList.remove('hidden');
    });
    document.getElementById('clearAllBtn').addEventListener('click', clearAllCart);
    document.getElementById('addOrderBtn').addEventListener('click', addNewTab);

    initializeDefaultPaymentMethod();
    updateAddButtonVisibility();
});
// ========== END: Initialize ==========

// ========== START: Product Bottom Sheet ==========
function openProductList() {
    const sheet = document.getElementById('productListSheet');
    sheet.classList.remove('hidden');
    
    setTimeout(() => {
        sheet.querySelector('.bottom-sheet').classList.add('active');
    }, 10);
}

async function showVariantModal(productId) {
    try {
        const variantsList = document.getElementById('variantsList');
        variantsList.innerHTML = `
            <div class="text-center py-8">
                <i class="fas fa-spinner fa-spin text-2xl text-gray-400"></i>
                <p class="text-sm text-gray-500 mt-2">Đang tải biến thể...</p>
            </div>`;
        document.getElementById('variantModal').classList.remove('hidden');

        const response = await fetch(`/Products/GetById?id=${productId}`);
        
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        const product = await response.json();

        if (!product || !product.id) {
            alert("Không thể load thông tin sản phẩm!");
            closeVariantModal();
            return;
        }

        const variants = product.variants || product.Variants || [];

        if (variants.length === 0) {
            alert("Sản phẩm này không có biến thể!");
            closeVariantModal();
            return;
        }

        renderVariantsList(variants, product.productName || product.ProductName);

    } catch (error) {
        console.error('❌ Error loading variants:', error);
        alert(`Lỗi khi tải biến thể: ${error.message}`);
        closeVariantModal();
    }
}

function renderVariantsList(variants, productName) {
    const variantsList = document.getElementById('variantsList');

    variantsList.innerHTML = variants.map(v => {
        const variantId = v.id || v.Id; // ✅ GUID của variant
        const sku = v.sku || v.SKU || '';
        const name = v.combination || v.Combination || 'Mặc định';
        const price = v.price || v.Price || 0;
        const stock = v.stock || v.Stock || 0;

        return `
            <div class="flex items-center justify-between p-4 border border-gray-200 rounded-lg hover:bg-blue-50 transition cursor-pointer ${stock <= 0 ? 'opacity-60 bg-gray-50' : ''}" 
                 onclick="${stock > 0 ? `selectVariant('${variantId}', '${sku}', '${name}', ${price}, '${productName || 'Sản phẩm'}')` : "alert('Sản phẩm đã hết hàng!')"}">
                
                <div class="flex-1">
                    <div class="flex items-center gap-2">
                        <p class="text-sm font-bold text-gray-800">${name}</p>
                        <span class="text-[10px] px-1.5 py-0.5 bg-gray-100 text-gray-500 rounded font-mono">${sku}</span>
                    </div>
                    <p class="text-xs text-blue-600 font-semibold mt-1">${price.toLocaleString('vi-VN')} ₫</p>
                </div>

                <div class="text-right">
                    <p class="text-xs mb-1 ${stock > 0 ? 'text-gray-500' : 'text-red-500 font-bold'}">
                        Tồn: <strong>${stock}</strong>
                    </p>
                    ${stock > 0
                        ? `<span class="px-2 py-1 text-[10px] bg-green-100 text-green-700 rounded-full font-medium">Sẵn sàng</span>`
                        : `<span class="px-2 py-1 text-[10px] bg-red-100 text-red-700 rounded-full font-medium">Hết hàng</span>`
                    }
                </div>
            </div>
        `;
    }).join('');
}


function selectVariant(variantId, sku, variantName, price, productName) {
    if (typeof addToCart === 'function') {
        addToCart({
            variantId: variantId, 
            sku: sku,              
            name: `${productName} - ${variantName}`,
            productName: productName,
            variantName: variantName,
            price: price,         
            quantity: 1
        });
    }
    
    closeVariantModal();
}

function closeVariantModal() {
    document.getElementById('variantModal').classList.add('hidden');
}

function closeProductList() {
    const sheet = document.getElementById('productListSheet');
    const bottomSheet = sheet.querySelector('.bottom-sheet');
    bottomSheet.classList.remove('active');
    
    setTimeout(() => {
        sheet.classList.add('hidden');
    }, 300);
}

function closeAddCustomer() {
    document.getElementById('addCustomerModal').classList.add('hidden');
}
// ========== END: Product Bottom Sheet ==========

// ========== START: Tab Management ==========
function switchTab(tabElement) {
    document.querySelectorAll('.order-tab').forEach(tab => {
        tab.classList.remove('active');
    });

    tabElement.classList.add('active');

    initializeDefaultPaymentMethod();
    updateCartDisplay();
    renderPaymentMethods();
}

function getNextAvailableOrderId() {
    let id = 1;
    while (usedOrderIds.has(id)) {
        id++;
    }
    return id;
}

function addNewTab() {
    const totalTabs = document.querySelectorAll('.order-tab').length;
    if (totalTabs >= MAX_TABS) {
        alert(`Chỉ được mở tối đa ${MAX_TABS} đơn hàng cùng lúc!`);
        return;
    }

    const tabsContainer = document.getElementById('tabs-container');
    const addButton = document.getElementById('addOrderBtn');
    
    document.querySelectorAll('.order-tab').forEach(tab => {
        tab.classList.remove('active');
    });
    
    const newOrderId = getNextAvailableOrderId();
    usedOrderIds.add(newOrderId);
    
    const newTab = document.createElement('div');
    newTab.className = 'order-tab active flex items-center gap-2 px-4 py-2 rounded-lg cursor-pointer';
    newTab.dataset.orderId = newOrderId;
    newTab.innerHTML = `
        <span class="font-medium text-gray-700">Đơn ${newOrderId}</span>
        <button class="text-gray-400 hover:text-red-500" onclick="event.stopPropagation(); removeTab(this)">
            <i class="fas fa-times"></i>
        </button>
    `;
    
    newTab.addEventListener('click', function(e) {
        if (!e.target.closest('button')) {
            switchTab(this);
        }
    });
    
    tabsContainer.insertBefore(newTab, addButton);
    updateCartDisplay();
    updateAddButtonVisibility();
}

function removeTab(button) {
    const tab = button.closest('.order-tab');
    const orderId = parseInt(tab.dataset.orderId);
    
    if (document.querySelectorAll('.order-tab').length > 1) {
        const wasActive = tab.classList.contains('active');
        
        usedOrderIds.delete(orderId);
        
        // ✅ FIX: Dùng Cart.data thay vì cart
        delete Cart.data[orderId];
        delete paymentMethods[orderId];
        
        if (wasActive) {
            const nextTab = tab.nextElementSibling?.classList.contains('order-tab') 
                ? tab.nextElementSibling 
                : tab.previousElementSibling;
            if (nextTab && nextTab.classList.contains('order-tab')) {
                switchTab(nextTab);
            }
        }
        
        tab.remove();
        updateAddButtonVisibility();
    } else {
        alert('Phải có ít nhất 1 đơn hàng!');
    }
}

function updateAddButtonVisibility() {
    const addButton = document.getElementById('addOrderBtn');
    const totalTabs = document.querySelectorAll('.order-tab').length;
    
    if (totalTabs >= MAX_TABS) {
        addButton.classList.add('hidden');
    } else {
        addButton.classList.remove('hidden');
    }
}
// ========== END: Tab Management ==========

// ========== START: Keyboard Shortcuts ==========
document.addEventListener('keydown', function(e) {
    if (e.key === 'F2') {
        e.preventDefault();
        document.getElementById('searchProductInput').focus();
    }
    if (e.key === 'F3') {
        e.preventDefault();
        document.querySelector('input[placeholder*="Tìm khách hàng"]').focus();
    }
    if (e.key === 'F8') {
        e.preventDefault();
        const modal = document.getElementById('addCustomerModal');
        if (!modal.classList.contains('hidden')) {
            alert('Lưu khách hàng!');
        }
    }
    if (e.key === 'F9') {
        e.preventDefault();
        alert('Thanh toán đơn hàng!');
    }
    if (e.key === 'Escape') {
        closeProductList();
        closeAddCustomer();
        closeVariantModal();
    }
});
// ========== END: Keyboard Shortcuts ==========
