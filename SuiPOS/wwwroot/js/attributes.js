// ============================================
// ATTRIBUTES MANAGEMENT
// ============================================

let currentAttributeId = null;

// Open modal for adding/editing attribute
function openAttributeModal(id = null, name = '') {
    currentAttributeId = id;
    document.getElementById('attributeModalTitle').textContent = id ? 'Sửa biến thể' : 'Thêm biến thể';
    document.getElementById('attributeId').value = id || '';
    document.getElementById('attributeName').value = name;
    document.getElementById('attributeModal').classList.remove('hidden');
}

// Close attribute modal
function closeAttributeModal() {
    document.getElementById('attributeModal').classList.add('hidden');
    document.getElementById('attributeForm').reset();
}

// Edit attribute
function editAttribute(id, name) {
    // Decode HTML entities
    const decodedName = document.createElement('textarea');
    decodedName.innerHTML = name;
    openAttributeModal(id, decodedName.value);
}

// Delete attribute
async function deleteAttribute(id) {
    if (!confirm('Bạn có chắc chắn muốn xóa biến thể này?\nLưu ý: Tất cả giá trị của biến thể cũng sẽ bị xóa.')) {
        return;
    }
    
    try {
        const formData = new FormData();
        formData.append('id', id);
        
        const response = await fetch('/Attributes/DeleteAttribute', {
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
        alert('Có lỗi xảy ra khi xóa biến thể!');
    }
}

// Form submit for attribute
document.getElementById('attributeForm')?.addEventListener('submit', async function(e) {
    e.preventDefault();
    
    const id = document.getElementById('attributeId').value;
    const name = document.getElementById('attributeName').value;
    
    try {
        const url = id ? '/Attributes/UpdateAttribute' : '/Attributes/CreateAttribute';
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ id, name })
        });
        
        const result = await response.json();
        
        if (result.success) {
            alert(result.message);
            closeAttributeModal();
            location.reload();
        } else {
            alert(result.message);
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Có lỗi xảy ra khi lưu biến thể!');
    }
});

// ============================================
// VALUES MANAGEMENT
// ============================================

let currentManagingAttributeId = null;

// Open values management modal
async function manageValues(attributeId, attributeName) {
    currentManagingAttributeId = attributeId;
    
    // Decode HTML entities
    const decodedName = document.createElement('textarea');
    decodedName.innerHTML = attributeName;
    
    document.getElementById('valuesAttributeName').textContent = decodedName.value;
    document.getElementById('valuesModal').classList.remove('hidden');
    
    // Load values
    await loadValues(attributeId);
}

// Close values modal
function closeValuesModal() {
    document.getElementById('valuesModal').classList.add('hidden');
    document.getElementById('newValueInput').value = '';
    currentManagingAttributeId = null;
}

// Load values for an attribute
async function loadValues(attributeId) {
    try {
        const response = await fetch(`/Attributes/GetValues?attributeId=${attributeId}`);
        const result = await response.json();
        
        if (result.success && result.data) {
            renderValues(result.data);
        } else {
            document.getElementById('valuesList').innerHTML = `
                <div class="text-center text-gray-500 py-8">
                    <i class="fas fa-inbox text-4xl mb-2"></i>
                    <p>Chưa có giá trị nào</p>
                </div>
            `;
        }
    } catch (error) {
        console.error('Error:', error);
        document.getElementById('valuesList').innerHTML = `
            <div class="text-center text-red-500 py-8">
                <i class="fas fa-exclamation-circle text-4xl mb-2"></i>
                <p>Có lỗi khi tải giá trị</p>
            </div>
        `;
    }
}

// Render values list
function renderValues(values) {
    if (!values || values.length === 0) {
        document.getElementById('valuesList').innerHTML = `
            <div class="text-center text-gray-500 py-8">
                <i class="fas fa-inbox text-4xl mb-2"></i>
                <p>Chưa có giá trị nào</p>
                <p class="text-sm">Nhập giá trị mới ở trên để thêm</p>
            </div>
        `;
        return;
    }
    
    const html = values.map(val => `
        <div class="flex items-center justify-between p-3 border border-gray-200 rounded-lg hover:bg-gray-50">
            <div class="flex items-center gap-3 flex-1">
                <i class="fas fa-tag text-gray-400"></i>
                <span class="text-gray-900">${val.value}</span>
            </div>
            <div class="flex items-center gap-2">
                <button onclick="editValue('${val.id}', '${val.value.replace(/'/g, "\\'")}', '${val.attributeId}')" 
                        class="p-2 text-blue-600 hover:bg-blue-50 rounded" title="Sửa">
                    <i class="fas fa-edit"></i>
                </button>
                <button onclick="deleteValue('${val.id}')" 
                        class="p-2 text-red-600 hover:bg-red-50 rounded" title="Xóa">
                    <i class="fas fa-trash"></i>
                </button>
            </div>
        </div>
    `).join('');
    
    document.getElementById('valuesList').innerHTML = html;
}

// Add new value
async function addNewValue() {
    const value = document.getElementById('newValueInput').value.trim();
    
    if (!value) {
        alert('Vui lòng nhập giá trị!');
        return;
    }
    
    if (!currentManagingAttributeId) {
        alert('Lỗi: Không xác định được biến thể!');
        return;
    }
    
    try {
        const response = await fetch('/Attributes/AddValue', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                attributeId: currentManagingAttributeId,
                value: value
            })
        });
        
        const result = await response.json();
        
        if (result.success) {
            document.getElementById('newValueInput').value = '';
            await loadValues(currentManagingAttributeId);
        } else {
            alert(result.message);
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Có lỗi xảy ra khi thêm giá trị!');
    }
}

// Edit value
function editValue(id, value, attributeId) {
    const newValue = prompt('Nhập giá trị mới:', value);
    
    if (newValue === null || newValue.trim() === '') {
        return;
    }
    
    updateValue(id, newValue.trim(), attributeId);
}

// Update value
async function updateValue(id, value, attributeId) {
    try {
        const response = await fetch('/Attributes/UpdateValue', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ id, value })
        });
        
        const result = await response.json();
        
        if (result.success) {
            await loadValues(attributeId);
        } else {
            alert(result.message);
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Có lỗi xảy ra khi cập nhật giá trị!');
    }
}

// Delete value
async function deleteValue(id) {
    if (!confirm('Bạn có chắc chắn muốn xóa giá trị này?')) {
        return;
    }
    
    try {
        const formData = new FormData();
        formData.append('id', id);
        
        const response = await fetch('/Attributes/DeleteValue', {
            method: 'POST',
            body: formData
        });
        
        const result = await response.json();
        
        if (result.success) {
            await loadValues(currentManagingAttributeId);
        } else {
            alert(result.message);
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Có lỗi xảy ra khi xóa giá trị!');
    }
}

// Search attributes
document.getElementById('searchInput')?.addEventListener('input', function(e) {
    const searchTerm = e.target.value.toLowerCase();
    const rows = document.querySelectorAll('#attributesTableBody tr');
    
    rows.forEach(row => {
        const text = row.textContent.toLowerCase();
        row.style.display = text.includes(searchTerm) ? '' : 'none';
    });
});

// Allow Enter key to add value
document.getElementById('newValueInput')?.addEventListener('keypress', function(e) {
    if (e.key === 'Enter') {
        e.preventDefault();
        addNewValue();
    }
});
