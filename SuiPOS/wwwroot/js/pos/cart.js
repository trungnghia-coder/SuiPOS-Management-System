// ============================================
// CART STATE MANAGEMENT WITH SESSIONSTORAGE
// ============================================
const Cart = {
    STORAGE_KEY: 'pos_cart_data',
    
    get data() {
        const stored = sessionStorage.getItem(this.STORAGE_KEY);
        return stored ? JSON.parse(stored) : [];
    },
    
    set data(value) {
        sessionStorage.setItem(this.STORAGE_KEY, JSON.stringify(value));
    },

    getItems() {
        return this.data;
    },

    findItem(variantId) {
        return this.data.find(item => item.variantId === variantId);
    },

    addItem(product) {
        const allData = this.data;
        const existing = allData.find(item => item.variantId === product.variantId);

        if (existing) {
            existing.quantity += product.quantity || 1;
        } else {
            allData.push({
                variantId: product.variantId,
                sku: product.sku,
                name: product.name,
                productName: product.productName,
                variantName: product.variantName,
                price: product.price,
                quantity: product.quantity || 1,
                image: product.imageUrl || product.image || 'https://placehold.co/100x100/webp?text=100x100'
            });
        }
        
        this.data = allData;
    },

    loadReorderCart() {
        const reorderCart = sessionStorage.getItem('reorder_cart');
        if (reorderCart) {
            try {
                // Clear old cart first
                sessionStorage.removeItem('pos_cart_data');
                
                const items = JSON.parse(reorderCart);
                items.forEach(item => this.addItem(item));
                sessionStorage.removeItem('reorder_cart');
                updateCartDisplay();
            } catch (error) {
                console.error('Error loading reorder cart:', error);
            }
        }
    },

    updateQuantity(variantId, delta) {
        const allData = this.data;
        const item = allData.find(i => i.variantId === variantId);
        
        if (item) {
            item.quantity += delta;
            if (item.quantity <= 0) {
                this.removeItem(variantId);
            } else {
                this.data = allData;
            }
        }
    },

    removeItem(variantId) {
        this.data = this.data.filter(item => item.variantId !== variantId);
    },

    clear() {
        if (this.data.length > 0 && confirm('Xóa tất cả sản phẩm?')) {
            this.data = [];
        }
    },

    getOrderData() {
        const items = this.data;
        return {
            items: items.map(item => ({
                variantId: item.variantId,
                quantity: item.quantity,
                unitPrice: item.price
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