// Hàm nạp danh sách loại sản phẩm từ Server
function loadCategories() {
    const tableBody = $('#categoriesTableBody');
    // Hiển thị trạng thái đang tải
    tableBody.html('<tr><td colspan="6" class="px-4 py-8 text-center text-gray-500"><i class="fas fa-spinner fa-spin mr-2"></i> Đang tải dữ liệu...</td></tr>');

    $.get('/Products/GetAllCategories', function (data) {
        let html = '';
        if (data && data.length > 0) {
            data.forEach(item => {
                html += `
                    <tr class="hover:bg-gray-50">
                        <td class="px-4 py-4"><input type="checkbox" class="w-4 h-4 rounded border-gray-300"></td>
                        <td class="px-4 py-4 text-sm font-medium text-gray-900">${item.name}</td>
                        <td class="px-4 py-4 text-center text-sm">${item.productCount || 0}</td>
                        <td class="px-4 py-4 text-center">
                            <span class="px-2 py-1 text-xs font-medium ${item.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'} rounded-full">
                                ${item.isActive ? 'Hoạt động' : 'Ngừng'}
                            </span>
                        </td>
                        <td class="px-4 py-4 text-center">
                            <button onclick="deleteCategory('${item.id}')" class="text-red-600 hover:text-red-800">
                                <i class="fas fa-trash-alt"></i>
                            </button>
                        </td>
                    </tr>`;
            });
        } else {
            html = '<tr><td colspan="6" class="px-4 py-8 text-center text-gray-500">Chưa có loại sản phẩm nào</td></tr>';
        }
        tableBody.html(html);
    }).fail(function(jqXHR, textStatus, errorThrown) {
        console.error('Error loading categories:', textStatus, errorThrown);
        tableBody.html(`<tr><td colspan="6" class="px-4 py-8 text-center text-red-500">
            <i class="fas fa-exclamation-triangle mr-2"></i> Lỗi tải dữ liệu: ${textStatus}
        </td></tr>`);
    });
}

// Hàm mở Modal
function showCreateCategoryModal() {
    $('#createCategoryModal').removeClass('hidden');
    $('#categoryName').val('').focus();
}

// Hàm đóng Modal
function closeCreateCategoryModal() {
    $('#createCategoryModal').addClass('hidden');
}

// Hàm gọi API tạo mới Category
async function createCategory() {
    const name = $('#categoryName').val().trim();
    if (!name) {
        alert("Vui lòng nhập tên loại sản phẩm");
        return;
    }

    try {
        const token = $('input[name="__RequestVerificationToken"]').val();
        
        const response = await fetch('/Products/CreateCategory', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({ Name: name })
        });

        const result = await response.json();

        if (result.success) {
            alert("Thành công: " + result.message);
            closeCreateCategoryModal();
            loadCategories();
        } else {
            alert("Lỗi: " + result.message);
        }
    } catch (error) {
        console.error("Error:", error);
        alert("Lỗi kết nối máy chủ");
    }
}

// Hàm xóa Category
async function deleteCategory(id) {
    if (!confirm('Bạn có chắc chắn muốn xóa loại sản phẩm này?')) {
        return;
    }

    try {
        const token = $('input[name="__RequestVerificationToken"]').val();
        
        const response = await fetch(`/Products/DeleteCategory?id=${id}`, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': token
            }
        });

        const result = await response.json();

        if (result.success) {
            alert("Thành công: " + result.message);
            loadCategories();
        } else {
            alert("Lỗi: " + result.message);
        }
    } catch (error) {
        console.error("Error:", error);
        alert("Lỗi kết nối máy chủ");
    }
}