// ============================================
// UI EVENTS - SINGLE ORDER
// ============================================

document.addEventListener('DOMContentLoaded', function() {
    document.getElementById('addPaymentMethodBtn')?.addEventListener('click', addPaymentMethod);
    document.getElementById('openProductListBtn')?.addEventListener('click', openProductList);
    document.getElementById('addCustomerBtn')?.addEventListener('click', () => {
        document.getElementById('addCustomerModal').classList.remove('hidden');
    });
    document.getElementById('clearAllBtn')?.addEventListener('click', clearAllCart);

    // Initialize product search
    const productSearchInput = document.getElementById('productSearchInput');
    if (productSearchInput) {
        productSearchInput.addEventListener('input', filterProducts);
    }

    // Load reorder cart if exists
    if (typeof Cart !== 'undefined' && typeof Cart.loadReorderCart === 'function') {
        Cart.loadReorderCart();
    }

    if (typeof initializeDefaultPaymentMethod === 'function') {
        initializeDefaultPaymentMethod();
    }
    if (typeof updateCartDisplay === 'function') {
        updateCartDisplay();
    }
});

// ========== START: Product Search ==========
function filterProducts() {
    const searchTerm = document.getElementById('productSearchInput').value.toLowerCase();
    const productCards = document.querySelectorAll('#productsGrid > div');
    
    productCards.forEach(card => {
        const productName = card.querySelector('h3').textContent.toLowerCase();
        if (productName.includes(searchTerm)) {
            card.style.display = '';
        } else {
            card.style.display = 'none';
        }
    });
}
// ========== END: Product Search ==========

// ========== START: Product Bottom Sheet ==========
function openProductList() {
    const sheet = document.getElementById('productListSheet');
    if (!sheet) return;
    
    sheet.classList.remove('hidden');
    setTimeout(() => {
        sheet.querySelector('.bottom-sheet')?.classList.add('active');
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

        const productName = product.productName || product.ProductName;
        const imageUrl = product.imageUrl || product.ImageUrl;
        renderVariantsList(variants, productName, imageUrl);

    } catch (error) {
        console.error('Error loading variants:', error);
        alert(`Lỗi khi tải biến thể: ${error.message}`);
        closeVariantModal();
    }
}

function renderVariantsList(variants, productName, imageUrl) {
    const variantsList = document.getElementById('variantsList');

    variantsList.innerHTML = variants.map(v => {
        const variantId = v.id || v.Id;
        const sku = v.sku || v.SKU || '';
        const name = v.combination || v.Combination || 'Mặc định';
        const price = v.price || v.Price || 0;
        const stock = v.stock || v.Stock || 0;

        return `
            <div class="flex items-center justify-between p-4 border border-gray-200 rounded-lg hover:bg-blue-50 transition cursor-pointer ${stock <= 0 ? 'opacity-60 bg-gray-50' : ''}" 
                 data-variant-id="${variantId}"
                 data-sku="${sku}"
                 data-name="${name}"
                 data-price="${price}"
                 data-product-name="${productName || 'Sản phẩm'}"
                 data-image-url="${imageUrl || ''}"
                 data-stock="${stock}"
                 onclick="handleVariantClick(this)">
                
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

function handleVariantClick(element) {
    const stock = parseInt(element.dataset.stock);
    
    if (stock <= 0) {
        alert('Sản phẩm đã hết hàng!');
        return;
    }
    
    selectVariant(
        element.dataset.variantId,
        element.dataset.sku,
        element.dataset.name,
        parseFloat(element.dataset.price),
        element.dataset.productName,
        element.dataset.imageUrl
    );
}


function selectVariant(variantId, sku, variantName, price, productName, imageUrl) {
    if (typeof addToCart === 'function') {
        addToCart({
            variantId: variantId, 
            sku: sku,              
            name: `${productName} - ${variantName}`,
            productName: productName,
            variantName: variantName,
            price: price,         
            quantity: 1,
            imageUrl: imageUrl
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
