// ============================================
// UI EVENTS & TAB MANAGEMENT
// ============================================

const MAX_TABS = 8;
let orderCount = 1;
let usedOrderIds = new Set([1]);

// ========== START: Initialize ==========
document.addEventListener('DOMContentLoaded', function() {
    loadProductsToModal();
    
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

function loadProductsToModal() {
    const grid = document.getElementById('productsGrid');
    grid.innerHTML = sampleProducts.map(product => `
        <div class="bg-white rounded-lg overflow-hidden cursor-pointer border" onclick="event.stopPropagation(); showVariantModal(${product.id})">
            <div class="p-2 flex">
                <img src="${product.image}" alt="${product.name}" class="w-10 h-10 border-[1px] rounded-lg object-cover cursor-pointer">
                <div class="px-2">
                    <h3 class="text-sm font-small text-gray-800 line-clamp-2 mb-1 min-h-[40px]">${product.name}</h3>
                    <button class="px-2 py-1 bg-gray-100 rounded hover:bg-gray-200 text-xs">
                        <i class="fas fa-ellipsis-h"></i>
                    </button>
                </div>
            </div>
            
        </div>
    `).join('');
}

function showVariantModal(productId) {
    const product = sampleProducts.find(p => p.id === productId);
    if (!product) return;
    
    const variants = [
        { id: 1, name: `${product.color} / 2XL (65Kg-70Kg)`, stock: 1, barcode: '8935245531694' },
        { id: 2, name: `${product.color} / L (53Kg-57Kg)`, stock: 1, barcode: '8935245531701' },
        { id: 3, name: `${product.color} / XL (58Kg-64Kg)`, stock: 0, barcode: '8935245531718' },
        { id: 4, name: `${product.color} / M (43Kg-52Kg)`, stock: 1, barcode: '8935245531725' },
        { id: 5, name: `Kem sữa / 2XL (65Kg-70Kg)`, stock: 0, barcode: '8935245531732' },
        { id: 6, name: `Kem sữa / L (53Kg-57Kg)`, stock: 1, barcode: '8935245531739' }
    ];
    
    const variantsList = document.getElementById('variantsList');
    variantsList.innerHTML = variants.map(variant => `
        <div class="flex items-center justify-between p-3 border border-gray-200 rounded-lg hover:bg-gray-50 cursor-pointer ${variant.stock === 0 ? 'opacity-50' : ''}" 
             onclick="${variant.stock > 0 ? `selectVariant(${product.id}, ${variant.id})` : ''}">
            <div class="flex-1">
                <p class="text-sm font-medium">${variant.name}</p>
                <p class="text-xs text-gray-500">${variant.barcode}</p>
            </div>
            <div class="text-right">
                ${variant.stock > 0 
                    ? `<span class="inline-block px-2 py-1 text-xs bg-green-100 text-green-700 rounded">Còn ${variant.stock}</span>` 
                    : `<span class="inline-block px-2 py-1 text-xs bg-red-100 text-red-700 rounded">Hết hàng</span>`
                }
            </div>
        </div>
    `).join('');
    
    document.getElementById('variantModal').classList.remove('hidden');
}

function selectVariant(productId, variantId) {
    addToCart(productId);
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
        
        delete cart[orderId];
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
