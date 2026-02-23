// ============================================
// PROMOTIONS MANAGEMENT
// ============================================

let currentPromotionId = null;

// Open modal for adding new promotion
function openPromotionModal() {
    currentPromotionId = null;
    document.getElementById('modalTitle').textContent = 'Thêm khuyến mãi';
    document.getElementById('promotionForm').reset();
    document.getElementById('promotionId').value = '';
    
    // Set default dates
    const now = new Date();
    const nextMonth = new Date(now.setMonth(now.getMonth() + 1));
    document.getElementById('startDate').value = formatDateTimeLocal(new Date());
    document.getElementById('endDate').value = formatDateTimeLocal(nextMonth);
    
    document.getElementById('promotionModal').classList.remove('hidden');
}

// Close modal
function closePromotionModal() {
    document.getElementById('promotionModal').classList.add('hidden');
}

// Format date for datetime-local input
function formatDateTimeLocal(date) {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
}

// Update discount value label
function updateDiscountLabel() {
    const type = document.getElementById('discountType').value;
    const label = document.getElementById('discountValueLabel');
    label.textContent = type === 'Percentage' ? 'Giá trị giảm (%)' : 'Giá trị giảm (đ)';
}

// Edit promotion
async function editPromotion(id) {
    try {
        const response = await fetch(`/Promotions/GetById?id=${id}`);
        const result = await response.json();
        
        if (result.success && result.data) {
            const promo = result.data;
            
            currentPromotionId = promo.id;
            document.getElementById('modalTitle').textContent = 'Sửa khuyến mãi';
            document.getElementById('promotionId').value = promo.id;
            document.getElementById('promotionName').value = promo.name;
            document.getElementById('promotionCode').value = promo.code;
            document.getElementById('discountType').value = promo.type;
            document.getElementById('discountValue').value = promo.discountValue;
            document.getElementById('minOrderAmount').value = promo.minOrderAmount || '';
            document.getElementById('maxDiscountAmount').value = promo.maxDiscountAmount || '';
            document.getElementById('startDate').value = formatDateTimeLocal(new Date(promo.startDate));
            document.getElementById('endDate').value = formatDateTimeLocal(new Date(promo.endDate));
            document.getElementById('isActive').checked = promo.isActive;
            
            updateDiscountLabel();
            document.getElementById('promotionModal').classList.remove('hidden');
        } else {
            alert('Không tìm thấy khuyến mãi!');
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Có lỗi xảy ra khi tải thông tin khuyến mãi!');
    }
}

// Delete promotion
async function deletePromotion(id) {
    if (!confirm('Bạn có chắc chắn muốn xóa khuyến mãi này?')) {
        return;
    }
    
    try {
        const formData = new FormData();
        formData.append('id', id);
        
        const response = await fetch('/Promotions/Delete', {
            method: 'POST',
            body: formData
        });
        
        const result = await response.json();
        
        if (result.success) {
            alert(result.message);
            location.reload();
        } else {
            alert(result.message);
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Có lỗi xảy ra khi xóa khuyến mãi!');
    }
}

// Toggle active status
async function togglePromotionActive(id) {
    try {
        const formData = new FormData();
        formData.append('id', id);
        
        const response = await fetch('/Promotions/ToggleActive', {
            method: 'POST',
            body: formData
        });
        
        const result = await response.json();
        
        if (result.success) {
            alert(result.message);
            location.reload();
        } else {
            alert(result.message);
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Có lỗi xảy ra!');
    }
}

// Form submit
document.getElementById('promotionForm')?.addEventListener('submit', async function(e) {
    e.preventDefault();
    
    const id = document.getElementById('promotionId').value;
    const data = {
        id: id || '00000000-0000-0000-0000-000000000000',
        name: document.getElementById('promotionName').value,
        code: document.getElementById('promotionCode').value,
        type: document.getElementById('discountType').value,
        discountValue: parseFloat(document.getElementById('discountValue').value),
        minOrderAmount: document.getElementById('minOrderAmount').value ? parseFloat(document.getElementById('minOrderAmount').value) : null,
        maxDiscountAmount: document.getElementById('maxDiscountAmount').value ? parseFloat(document.getElementById('maxDiscountAmount').value) : null,
        startDate: document.getElementById('startDate').value,
        endDate: document.getElementById('endDate').value,
        isActive: document.getElementById('isActive').checked
    };
    
    try {
        const url = id ? '/Promotions/Update' : '/Promotions/Create';
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        });
        
        const result = await response.json();
        
        if (result.success) {
            alert(result.message);
            closePromotionModal();
            location.reload();
        } else {
            alert(result.message);
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Có lỗi xảy ra khi lưu khuyến mãi!');
    }
});

// Search promotions
document.getElementById('searchInput')?.addEventListener('input', function(e) {
    const searchTerm = e.target.value.toLowerCase();
    const rows = document.querySelectorAll('#promotionsTableBody tr');
    
    rows.forEach(row => {
        const text = row.textContent.toLowerCase();
        row.style.display = text.includes(searchTerm) ? '' : 'none';
    });
});
