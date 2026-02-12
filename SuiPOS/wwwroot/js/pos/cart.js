// ============================================
// CART STATE MANAGEMENT
// ============================================
const Cart = {
    data: {}, // { orderId: [items] }
    
    getActiveOrderId() {
        const activeTab = document.querySelector('.order-tab.active');
        return activeTab?.dataset.orderId || '1';
    },
    
    getItems() {
        const orderId = this.getActiveOrderId();
        return this.data[orderId] || [];
    },
    
    findItem(variantId) {
        return this.getItems().find(item => item.variantId === variantId);
    },
    
    addItem(product) {
        const orderId = this.getActiveOrderId();
        if (!this.data[orderId]) this.data[orderId] = [];
        
        const existing = this.findItem(product.variantId);
        if (existing) {
            existing.quantity++;
        } else {
            this.data[orderId].push({
                variantId: product.variantId,     
                sku: product.sku,                
                name: product.name,              
                productName: product.productName, 
                variantName: product.variantName, 
                price: product.price,             
                quantity: 1,                      
                image: product.image || 'https://placehold.co/100x100/webp?text=100x100'
            });
        }
    },
    
    updateQuantity(variantId, delta) {
        const item = this.findItem(variantId);
        if (item) {
            item.quantity += delta;
            if (item.quantity <= 0) {
                this.removeItem(variantId);
            }
        }
    },
    
    removeItem(variantId) {
        const orderId = this.getActiveOrderId();
        this.data[orderId] = this.getItems().filter(item => item.variantId !== variantId);
    },
    
    clear() {
        const orderId = this.getActiveOrderId();
        if (this.getItems().length > 0) {
            if (confirm('Xóa tất cả sản phẩm?')) {
                this.data[orderId] = [];
            }
        }
    },
    
    // ✅ Get data theo format OrderViewModel
    getOrderData() {
        const items = this.getItems();
        return {
            items: items.map(item => ({
                variantId: item.variantId,  // ✅ Guid
                quantity: item.quantity,    // ✅ int
                unitPrice: item.price       // ✅ decimal
            })),
            totalAmount: items.reduce((sum, item) => sum + (item.price * item.quantity), 0),
            totalItems: items.reduce((sum, item) => sum + item.quantity, 0)
        };
    }
};

// ============================================
// PUBLIC API
// ============================================
function addToCart(product) {
    if (!product || !product.variantId) {
        alert('Dữ liệu sản phẩm không hợp lệ!');
        return;
    }
    
    Cart.addItem(product);
    updateCartDisplay();
    
    // Close modal if exists
    const modal = document.getElementById('variantModal');
    const sheet = document.getElementById('productListSheet');
    if (modal) modal.classList.add('hidden');
    if (sheet) {
        const bottomSheet = sheet.querySelector('.bottom-sheet');
        if (bottomSheet) bottomSheet.classList.remove('active');
        setTimeout(() => sheet.classList.add('hidden'), 300);
    }
}

function increaseQuantity(variantId) {
    Cart.updateQuantity(variantId, 1);
    updateCartDisplay();
}

function decreaseQuantity(variantId) {
    Cart.updateQuantity(variantId, -1);
    updateCartDisplay();
}

function removeFromCart(variantId) {
    Cart.removeItem(variantId);
    updateCartDisplay();
}

function clearAllCart() {
    Cart.clear();
    updateCartDisplay();
}

// ============================================
// UI UPDATE
// ============================================
function updateCartDisplay() {
    const items = Cart.getItems();
    const emptyCart = document.getElementById('emptyCart');
    const cartItemsContainer = document.getElementById('cartItems');

    if (!items.length) {
        emptyCart?.classList.remove('hidden');
        cartItemsContainer?.classList.add('hidden');
        updateOrderSummary();
        return;
    }

    emptyCart?.classList.add('hidden');
    cartItemsContainer?.classList.remove('hidden');
    
    const html = items.map(item => `
        <div class="bg-white border border-gray-200 rounded-lg p-3 flex items-center gap-3">
            <button onclick="removeFromCart('${item.variantId}')" 
                    class="text-red-500 hover:text-red-700 flex-shrink-0">
                <i class="fas fa-trash"></i>
            </button>
            <img src="${item.image}" alt="${item.name}" 
                 class="w-16 h-16 object-cover rounded-lg flex-shrink-0">
            <div class="flex-1 min-w-0">
                <h4 class="font-medium text-sm text-gray-800 line-clamp-2 mb-1">${item.name}</h4>
                <p class="text-xs text-gray-500 mb-1">${item.productName}</p>
                <p class="text-xs text-gray-400">SKU: ${item.sku}</p>
            </div>
            <div class="flex items-center gap-4 flex-shrink-0">
                <span class="font-semibold text-blue-600">${item.price.toLocaleString()} ₫</span>
                <div class="flex items-center gap-2 border border-gray-300 rounded-lg">
                    <button onclick="decreaseQuantity('${item.variantId}')" 
                            class="px-2 py-1 hover:bg-gray-100">
                        <i class="fas fa-minus text-xs"></i>
                    </button>
                    <span class="px-3 py-1 border-l border-r min-w-[40px] text-center">${item.quantity}</span>
                    <button onclick="increaseQuantity('${item.variantId}')" 
                            class="px-2 py-1 hover:bg-gray-100">
                        <i class="fas fa-plus text-xs"></i>
                    </button>
                </div>
                <span class="font-bold">${(item.price * item.quantity).toLocaleString()} ₫</span>
            </div>
        </div>
    `).join('');
    
    cartItemsContainer.innerHTML = html;

    updateOrderSummary();
}

function updateOrderSummary() {
    const orderData = Cart.getOrderData();

    document.getElementById('totalItems').textContent = `${orderData.totalItems} sản phẩm`;
    document.getElementById('totalAmount').textContent = orderData.totalAmount.toLocaleString() + ' ₫';
    document.getElementById('customerPay').textContent = orderData.totalAmount.toLocaleString() + ' ₫';
}


