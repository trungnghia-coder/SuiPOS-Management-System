// ============================================
// PROMOTION SELECTOR FOR POS
// ============================================

let selectedPromotion = null;

// Open promotion selector modal
async function openPromotionSelector() {
    const cartData = Cart.getOrderData();
    const totalAmount = cartData.totalAmount || 0;
    
    if (totalAmount === 0) {
        alert('Vui lòng thêm sản phẩm vào giỏ hàng trước!');
        return;
    }
    
    document.getElementById('promotionModal').classList.remove('hidden');
    
    // Load valid promotions for this order amount
    await loadValidPromotions(totalAmount);
}

// Close promotion selector
function closePromotionSelector() {
    document.getElementById('promotionModal').classList.add('hidden');
}

// Load valid promotions from API
async function loadValidPromotions(orderAmount) {
    try {
        const response = await fetch(`/Promotions/GetValidPromotions?orderAmount=${orderAmount}`);
        const result = await response.json();
        
        if (result.success && result.data && result.data.length > 0) {
            renderPromotions(result.data, orderAmount);
        } else {
            const today = new Date().toLocaleDateString('vi-VN');
            document.getElementById('promotionsList').innerHTML = `
                <div class="text-center text-gray-500 py-8">
                    <i class="fas fa-gift text-4xl mb-2 text-gray-300"></i>
                    <p class="font-medium">Không có khuyến mãi nào khả dụng</p>
                </div>
            `;
        }
    } catch (error) {
        document.getElementById('promotionsList').innerHTML = `
            <div class="text-center text-red-500 py-8">
                <i class="fas fa-exclamation-circle text-4xl mb-2"></i>
                <p>Có lỗi khi tải khuyến mãi</p>
                <p class="text-xs mt-2">${error.message}</p>
            </div>
        `;
    }
}




// Render promotions list
function renderPromotions(promotions, orderAmount) {
    if (!promotions || promotions.length === 0) {
        document.getElementById('promotionsList').innerHTML = `
            <div class="text-center text-gray-500 py-8">
                <i class="fas fa-gift text-4xl mb-2"></i>
                <p>Không có khuyến mãi nào khả dụng</p>
            </div>
        `;
        return;
    }
    
    // Sort: Valid ones first, then invalid ones (grayed out)
    const validPromotions = promotions.filter(p => p.isValid);
    const invalidPromotions = promotions.filter(p => !p.isValid);
    const sortedPromotions = [...validPromotions, ...invalidPromotions];
    
    const html = sortedPromotions.map(promo => {
        const isValid = promo.isValid;
        const discount = calculateDiscount(promo, orderAmount);
        
        return `
            <div class="p-4 border rounded-lg cursor-pointer transition ${isValid ? 'border-gray-300 hover:border-blue-500 hover:bg-blue-50' : 'border-gray-200 bg-gray-50 opacity-60 cursor-not-allowed'}"
                 ${isValid ? `onclick="selectPromotion('${promo.id}', '${promo.name}', '${promo.code}', '${promo.type}', ${promo.discountValue}, ${promo.maxDiscountAmount || 'null'})"` : ''}>
                <div class="flex items-start justify-between">
                    <div class="flex-1">
                        <div class="flex items-center gap-2 mb-1">
                            <h3 class="font-semibold text-gray-900">${promo.name}</h3>
                            ${isValid 
                                ? '<span class="px-2 py-0.5 bg-green-100 text-green-800 rounded-full text-xs font-medium">Khả dụng</span>'
                                : '<span class="px-2 py-0.5 bg-gray-200 text-gray-600 rounded-full text-xs font-medium">Không đủ điều kiện</span>'
                            }
                        </div>
                        <p class="text-sm text-gray-600 mb-2">
                            <span class="px-2 py-0.5 bg-blue-100 text-blue-800 rounded text-xs font-mono">${promo.code}</span>
                        </p>
                        <div class="text-sm text-gray-700">
                            <i class="fas fa-tag text-blue-600"></i>
                            Giảm ${promo.type === 'Percentage' ? promo.discountValue + '%' : promo.discountValue.toLocaleString() + 'đ'}
                            ${promo.maxDiscountAmount ? ` (tối đa ${promo.maxDiscountAmount.toLocaleString()}đ)` : ''}
                        </div>
                        ${promo.minOrderAmount 
                            ? `<div class="text-xs text-gray-500 mt-1">
                                <i class="fas fa-info-circle"></i>
                                Đơn tối thiểu: ${promo.minOrderAmount.toLocaleString()}đ
                            </div>`
                            : ''
                        }
                    </div>
                    ${isValid 
                        ? `<div class="text-right">
                            <div class="text-lg font-bold text-green-600">-${discount.toLocaleString()}đ</div>
                            <div class="text-xs text-gray-500">Tiết kiệm</div>
                        </div>`
                        : ''
                    }
                </div>
            </div>
        `;
    }).join('');
    
    document.getElementById('promotionsList').innerHTML = html;
}

// Calculate discount amount
function calculateDiscount(promo, orderAmount) {
    let discount = 0;
    
    if (promo.type === 'Percentage') {
        discount = (orderAmount * promo.discountValue) / 100;
    } else {
        discount = promo.discountValue;
    }
    
    // Apply max discount if exists
    if (promo.maxDiscountAmount && discount > promo.maxDiscountAmount) {
        discount = promo.maxDiscountAmount;
    }
    
    return discount;
}

// Select a promotion
function selectPromotion(id, name, code, type, value, maxDiscount) {
    const cartData = Cart.getOrderData();
    const totalAmount = cartData.totalAmount || 0;
    
    selectedPromotion = {
        id: id,
        name: name,
        code: code,
        type: type,
        value: value,
        maxDiscount: maxDiscount
    };
    
    // Calculate discount
    let discount = 0;
    if (type === 'Percentage') {
        discount = (totalAmount * value) / 100;
    } else {
        discount = value;
    }
    
    // Apply max discount
    if (maxDiscount && discount > maxDiscount) {
        discount = maxDiscount;
    }
    
    // Update UI
    document.getElementById('appliedPromotionName').textContent = `(${name})`;
    document.getElementById('discountAmount').textContent = `-${discount.toLocaleString()}đ`;
    document.getElementById('discountRow').classList.remove('hidden');
    
    // Update customer pay amount
    const finalAmount = totalAmount - discount;
    document.getElementById('customerPay').textContent = finalAmount.toLocaleString() + 'đ';
    
    // Reset payment methods to match new amount
    resetPaymentMethodsAfterDiscount(finalAmount);
    
    // Close modal
    closePromotionSelector();
}

// Reset payment methods when discount applied
function resetPaymentMethodsAfterDiscount(newTotalAmount) {

    // Clear existing payment methods
    if (typeof window.clearAllPaymentMethods === 'function') {
        window.clearAllPaymentMethods();
    }
    
    // Add default payment method with new amount
    if (typeof window.addDefaultPaymentMethod === 'function') {
        window.addDefaultPaymentMethod(newTotalAmount);
    }
}


// Remove applied promotion
function removePromotion() {
    selectedPromotion = null;
    
    document.getElementById('appliedPromotionName').textContent = '';
    document.getElementById('discountAmount').textContent = '0';
    document.getElementById('discountRow').classList.add('hidden');
    
    // Reset customer pay to total amount
    const cartData = Cart.getOrderData();
    const totalAmount = cartData.totalAmount || 0;
    document.getElementById('customerPay').textContent = totalAmount.toLocaleString() + 'đ';
    
    // Reset payment methods
    resetPaymentMethodsAfterDiscount(totalAmount);
}


// Get current discount amount for order submission
function getCurrentDiscount() {
    if (!selectedPromotion) return 0;
    
    const cartData = Cart.getOrderData();
    const totalAmount = cartData.totalAmount || 0;
    
    let discount = 0;
    if (selectedPromotion.type === 'Percentage') {
        discount = (totalAmount * selectedPromotion.value) / 100;
    } else {
        discount = selectedPromotion.value;
    }
    
    if (selectedPromotion.maxDiscount && discount > selectedPromotion.maxDiscount) {
        discount = selectedPromotion.maxDiscount;
    }
    
    return discount;
}

// Expose helper functions globally
window.clearAllPaymentMethods = function() {
    if (typeof Payment !== 'undefined' && Payment.clear) {
        Payment.clear();
        // Reinitialize with one default method
        Payment.addMethod('cash', 0);
        if (typeof renderPaymentMethods === 'function') {
            renderPaymentMethods();
        }
    }
};

window.addDefaultPaymentMethod = function(amount) {
    if (typeof Payment !== 'undefined' && Payment.methods) {
        const methods = Payment.methods;
        if (methods.length === 1) {
            // Update existing method
            Payment.updateMethod(methods[0].id, 'amount', amount);
            if (typeof renderPaymentMethods === 'function') {
                renderPaymentMethods();
            }
        }
    }
};

// Make getCurrentDiscount global
window.getCurrentDiscount = getCurrentDiscount;

