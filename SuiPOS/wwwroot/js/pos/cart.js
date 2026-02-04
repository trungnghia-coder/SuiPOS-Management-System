// ============================================
// CART MANAGEMENT
// ============================================

let cart = {}; // Store cart items by order ID

// Sample products data
const sampleProducts = [
    { id: 1, name: 'Bộ Tole Lạnh Quần Đùi Tay Nhí Họa Tiết Hàn Châu Co Rút Đô Bền Cao Nitimo', price: 185000, image: 'https://cdn.hstatic.net/products/200000810809/upload_f1c96ea215334df49d1383b25e91f422_medium.jpg', color: 'Xanh đen', size: '2XL (65Kg-70Kg)', barcode: '8935245531694' },
    { id: 2, name: 'Bộ Dài Cổ Kiểu Phối Sát Nách Họa Tiết', price: 165000, image: 'https://cdn.hstatic.net/products/200000810809/upload_f1c96ea215334df49d1383b25e91f422_medium.jpg', color: 'Hồng', size: 'L', barcode: '8935245531701' },
    { id: 3, name: 'Bộ Tole Quần 9 Tấc Sát Nách Họa Tiết Hoa', price: 175000, image: 'https://cdn.hstatic.net/products/200000810809/upload_f1c96ea215334df49d1383b25e91f422_medium.jpg', color: 'Trắng', size: 'M', barcode: '8935245531718' },
    { id: 4, name: 'Bộ Tole Dài Tay Lớn Họa Tiết Hoa Hàn Châu', price: 195000, image: 'https://cdn.hstatic.net/products/200000810809/upload_f1c96ea215334df49d1383b25e91f422_medium.jpg', color: 'Xanh', size: 'XL', barcode: '8935245531725' },
    { id: 5, name: 'Bộ Dài Cổ Pijama Tay Con Chất Liệu Lụa', price: 155000, image: 'https://cdn.hstatic.net/products/200000810809/upload_f1c96ea215334df49d1383b25e91f422_medium.jpg', color: 'Hồng phấn', size: 'L', barcode: '8935245531732' },
    { id: 6, name: 'Bộ Pijama Dài Tay Con Nữ Lụa Cổ Ve Nitimo', price: 185000, image: 'https://cdn.hstatic.net/products/200000810809/upload_f1c96ea215334df49d1383b25e91f422_medium.jpg', color: 'Xanh navy', size: 'M', barcode: '8935245531749' },
    { id: 7, name: 'Pijama Tole Lửng Sát Nách Tay Nhún In Hoa', price: 165000, image: 'https://cdn.hstatic.net/products/200000810809/upload_f1c96ea215334df49d1383b25e91f422_medium.jpg', color: 'Vàng', size: 'L', barcode: '8935245531756' },
    { id: 8, name: 'Bộ Lụa Pijama Dài Tay Dài Nữ Họa Tiết', price: 175000, image: 'https://cdn.hstatic.net/products/200000810809/upload_f1c96ea215334df49d1383b25e91f422_medium.jpg', color: 'Tím', size: 'XL', barcode: '8935245531763' },
    { id: 9, name: 'Bộ Lụa Kiểu Quần 9 Tấc Sát Nách 011231', price: 145000, image: 'https://cdn.hstatic.net/products/200000810809/upload_f1c96ea215334df49d1383b25e91f422_medium.jpg', color: 'Đỏ đô', size: 'M', barcode: '8935245531770' },
    { id: 10, name: 'Bộ Tole Nhung Quần Ống Suông Tay Con', price: 195000, image: 'https://cdn.hstatic.net/products/200000810809/upload_f1c96ea215334df49d1383b25e91f422_medium.jpg', color: 'Xanh rêu', size: 'L', barcode: '8935245531787' },
    { id: 11, name: 'Bộ Tole dài tay con Nitimo họa tiết Bông', price: 185000, image: 'https://cdn.hstatic.net/products/200000810809/upload_f1c96ea215334df49d1383b25e91f422_medium.jpg', color: 'Hồng sen', size: '2XL', barcode: '8935245531794' },
    { id: 12, name: 'Bộ Dài Cổ Pijama Tay Con Nitimo Cổ Điển', price: 175000, image: 'https://cdn.hstatic.net/products/200000810809/upload_f1c96ea215334df49d1383b25e91f422_medium.jpg', color: 'Hồng cánh sen', size: 'L', barcode: '8935245531800' }
];

// Add product to cart
function addToCart(productId) {
    const product = sampleProducts.find(p => p.id === productId);
    if (!product) return;

    const activeTab = document.querySelector('.order-tab.active');
    const orderId = activeTab.dataset.orderId;

    if (!cart[orderId]) {
        cart[orderId] = [];
    }

    const existingItem = cart[orderId].find(item => item.id === productId);
    if (existingItem) {
        existingItem.quantity++;
    } else {
        cart[orderId].push({ ...product, quantity: 1 });
    }

    updateCartDisplay();
    closeProductList();
}

// Update cart display
function updateCartDisplay() {
    const activeTab = document.querySelector('.order-tab.active');
    const orderId = activeTab.dataset.orderId;
    const cartItems = cart[orderId] || [];

    const emptyCart = document.getElementById('emptyCart');
    const cartItemsContainer = document.getElementById('cartItems');

    if (cartItems.length === 0) {
        emptyCart.classList.remove('hidden');
        cartItemsContainer.classList.add('hidden');
    } else {
        emptyCart.classList.add('hidden');
        cartItemsContainer.classList.remove('hidden');
        
        cartItemsContainer.innerHTML = cartItems.map(item => `
            <div class="bg-white border border-gray-200 rounded-lg p-3 flex items-center gap-3">
                <button onclick="removeFromCart(${item.id})" class="text-[#4b5563] hover:opacity flex-shrink-0">
                    <i class="fas fa-trash"></i>
                </button>
                <img src="${item.image}" alt="${item.name}" class="flex w-16 h-16 object-cover rounded-lg flex-shrink-0">
                <div class="flex-1 min-w-0">
                    <h4 class="font-medium text-sm text-gray-800 line-clamp-2 mb-1">${item.name}</h4>
                    <h4 class="font-medium text-sm text-gray-500">${item.color} / ${item.size}</h4>
                    <h4 class="font-medium text-sm text-gray-400">${item.barcode}</h4>
                </div>
                <div class="flex items-center gap-5 justify-between flex-shrink-0">
                    <span class="font-semibold text-blue-600">${item.price.toLocaleString()}</span>
                    <div class="flex items-center gap-2 border border-gray-300 rounded-lg">
                        <button onclick="decreaseQuantity(${item.id})" class="px-2 py-1 hover:bg-gray-100 text-gray-600">
                            <i class="fas fa-minus text-xs"></i>
                        </button>
                        <span class="px-3 py-1 border-l border-r border-gray-300 min-w-[40px] text-center">${item.quantity}</span>
                        <button onclick="increaseQuantity(${item.id})" class="px-2 py-1 hover:bg-gray-100 text-gray-600">
                            <i class="fas fa-plus text-xs"></i>
                        </button>
                    </div>
                    <span class="font-bold text-sm">${(item.price * item.quantity).toLocaleString()}</span>
                </div>
            </div>
        `).join('');
    }

    updateOrderSummary();
}

// Increase quantity
function increaseQuantity(productId) {
    const activeTab = document.querySelector('.order-tab.active');
    const orderId = activeTab.dataset.orderId;
    const item = cart[orderId].find(item => item.id === productId);
    if (item) {
        item.quantity++;
        updateCartDisplay();
    }
}

// Decrease quantity
function decreaseQuantity(productId) {
    const activeTab = document.querySelector('.order-tab.active');
    const orderId = activeTab.dataset.orderId;
    const item = cart[orderId].find(item => item.id === productId);
    if (item && item.quantity > 1) {
        item.quantity--;
        updateCartDisplay();
    }
}

// Remove from cart
function removeFromCart(productId) {
    const activeTab = document.querySelector('.order-tab.active');
    const orderId = activeTab.dataset.orderId;
    cart[orderId] = cart[orderId].filter(item => item.id !== productId);
    updateCartDisplay();
}

// Clear all items
function clearAllCart() {
    const activeTab = document.querySelector('.order-tab.active');
    const orderId = activeTab.dataset.orderId;
    
    if (cart[orderId] && cart[orderId].length > 0) {
        if (confirm('Bạn có chắc muốn xóa tất cả sản phẩm trong giỏ hàng?')) {
            cart[orderId] = [];
            updateCartDisplay();
        }
    }
}
