function switchTab(tabName) {
    document.querySelectorAll('.tab-content').forEach(tab => {
        tab.classList.remove('active');
        tab.style.display = 'none';
    });

    document.querySelectorAll('.tab-button').forEach(btn => {
        btn.classList.remove('active');
    });

    const targetTab = document.getElementById(`${tabName}-tab`);
    if (targetTab) {
        targetTab.classList.add('active');
        targetTab.style.display = 'block';
    }

    const targetBtn = document.querySelector(`button[onclick*="'${tabName}'"]`);
    if (targetBtn) {
        targetBtn.classList.add('active');
    }

    if (tabName === 'categories') {
        if (typeof loadCategories === 'function') {
            loadCategories();
        } else {
            console.warn("Hàm loadCategories chưa được nạp. Kiểm tra file category-management.js");
        }
    }
}

document.getElementById('productSearch')?.addEventListener('input', function (e) {
    const term = e.target.value.toLowerCase();
    const rows = document.querySelectorAll('#products-tab tbody tr');
    
    rows.forEach(row => {
        const match = row.innerText.toLowerCase().includes(term);
        row.style.display = match ? '' : 'none';
    });
});

async function deleteProduct(id, name) {
    if (!confirm(`Bạn có chắc chắn muốn xóa sản phẩm "${name}" không?`)) {
        return;
    }

    try {
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        const response = await fetch(`/Products/Delete/${id}`, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': token
            }
        });

        const result = await response.json();

        if (result.success) {
            alert(result.message);
            window.location.reload();
        } else {
            alert("Lỗi: " + result.message);
        }
    } catch (error) {
        console.error("Lỗi khi xóa:", error);
        alert("Không thể kết nối đến máy chủ.");
    }
}

let selectedFile = null;

function showImportModal() {
    document.getElementById('importModal')?.classList.remove('hidden');
}

function closeImportModal() {
    document.getElementById('importModal')?.classList.add('hidden');
    selectedFile = null;
    document.getElementById('importFile').value = '';
    document.getElementById('selectedFileName')?.classList.add('hidden');
    document.getElementById('importButton').disabled = true;
    document.getElementById('importButton').className = 'px-6 py-2 bg-gray-300 text-gray-500 rounded-lg cursor-not-allowed';
}

function handleFileSelect(event) {
    const file = event.target.files[0];
    if (file) {
        selectedFile = file;
        document.getElementById('selectedFileName').textContent = `Đã chọn: ${file.name}`;
        document.getElementById('selectedFileName').classList.remove('hidden');
        
        const importButton = document.getElementById('importButton');
        importButton.disabled = false;
        importButton.className = 'px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700';
    }
}

async function importProducts() {
    if (!selectedFile) {
        alert('Vui lòng chọn file!');
        return;
    }

    const formData = new FormData();
    formData.append('file', selectedFile);

    try {
        const response = await fetch('/Products/Import', {
            method: 'POST',
            body: formData
        });

        const result = await response.json();

        if (result.success) {
            alert('Import thành công!');
            closeImportModal();
            window.location.reload();
        } else {
            alert('Lỗi: ' + result.message);
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Lỗi kết nối máy chủ!');
    }
}