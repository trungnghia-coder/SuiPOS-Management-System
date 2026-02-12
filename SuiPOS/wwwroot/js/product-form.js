// ============================================
// STATE MANAGEMENT
// ============================================
const ProductForm = {
    attributes: [],
    selectedValues: {},
    variants: [],
    rowIndex: 0,
    isLoading: false
};

// ============================================
// INIT
// ============================================
$(() => {
    loadAttributes();
    
    if (window.initialVariants?.length) {
        ProductForm.variants = window.initialVariants.map(normalizeVariant);
        renderVariants();
    }
});

function normalizeVariant(v) {
    return {
        combination: v.Combination || v.combination || '',
        sku: v.SKU || v.sku || '',
        price: v.Price || v.price || 0,
        stock: v.Stock || v.stock || 0,
        selectedAttributeValueIds: v.SelectedAttributeValueIds || v.selectedAttributeValueIds || []
    };
}

// ============================================
// ATTRIBUTES
// ============================================
function loadAttributes() {
    if (ProductForm.isLoading || ProductForm.attributes.length) return;
    
    ProductForm.isLoading = true;
    $.get('/Attributes/GetWithValues')
        .done(data => {
            ProductForm.attributes = data;
            console.log(`✅ Loaded ${data.length} attributes`);
        })
        .fail(() => alert('Không thể tải thuộc tính!'))
        .always(() => ProductForm.isLoading = false);
}

function addAttributeRow() {
    if (!ProductForm.attributes.length) {
        alert('Vui lòng đợi tải danh sách thuộc tính!');
        setTimeout(() => ProductForm.attributes.length && addAttributeRow(), 1000);
        return;
    }

    const rowId = `attr-row-${ProductForm.rowIndex}`;
    const options = ProductForm.attributes.map(a => 
        `<option value="${a.id || a.Id}">${a.name || a.Name}</option>`
    ).join('');

    $('<div>', { 
        id: rowId,
        class: 'mb-6 pb-6 border-b border-gray-200',
        html: `
            <div class="grid grid-cols-2 gap-6">
                <div>
                    <label class="block text-sm font-medium text-gray-700 mb-2">Thuộc tính</label>
                    <div class="flex gap-3">
                        <select id="attr-select-${ProductForm.rowIndex}" 
                                onchange="onAttributeSelected('${rowId}', this.value, ${ProductForm.rowIndex})" 
                                class="flex-1 px-4 py-3 border rounded-lg">
                            <option value="">Chọn thuộc tính</option>${options}
                        </select>
                        <button type="button" onclick="removeAttributeRow('${rowId}')" 
                                class="text-red-600 p-2 hover:bg-red-50 rounded-lg">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </div>
                <div>
                    <label class="block text-sm font-medium text-gray-700 mb-2">Giá trị</label>
                    <div id="values-container-${ProductForm.rowIndex}">
                        <input type="text" placeholder="Chọn thuộc tính trước" class="w-full px-4 py-3 border rounded-lg bg-gray-50" disabled />
                    </div>
                </div>
            </div>
            <div id="selected-values-${ProductForm.rowIndex}" class="mt-4 flex flex-wrap gap-2"></div>`
    }).appendTo('#attributesContainer');
    
    ProductForm.rowIndex++;
}

function removeAttributeRow(rowId) {
    const idx = rowId.split('-')[2];
    const attrId = $(`#attr-select-${idx}`).val();
    if (attrId) delete ProductForm.selectedValues[attrId];
    $(`#${rowId}`).remove();
    generateVariants();
}

function onAttributeSelected(rowId, attrId, rowIdx) {
    const container = $(`#values-container-${rowIdx}`);
    const selectedContainer = $(`#selected-values-${rowIdx}`);
    
    if (!attrId) {
        container.html('<input type="text" class="w-full px-4 py-3 border rounded-lg bg-gray-50" disabled />');
        selectedContainer.empty();
        return;
    }

    const attr = ProductForm.attributes.find(a => (a.id || a.Id) === attrId);
    if (!attr) return;

    container.html(`
        <div class="flex gap-2">
            <input type="text" id="new-value-input-${rowIdx}" placeholder="Thêm giá trị mới" 
                   class="flex-1 px-4 py-3 border rounded-lg" />
            <button type="button" onclick="addNewValue('${attrId}', ${rowIdx})" 
                    class="px-6 py-3 bg-blue-600 text-white rounded-lg">Thêm</button>
        </div>`);

    const values = attr.values || attr.Values || [];
    if (values.length) {
        const buttons = values.map(v => {
            const vId = v.id || v.Id;
            const vVal = v.value || v.Value;
            return `<button type="button" id="value-btn-${rowIdx}-${vId}"
                            class="px-4 py-2 border rounded-lg hover:bg-blue-50"
                            onclick="toggleAttributeValue('${attrId}', '${vId}', '${vVal}', ${rowIdx})">${vVal}</button>`;
        }).join('');
        selectedContainer.html(`<div class="w-full"><div class="text-sm text-gray-600 mb-2">Chọn nhanh:</div><div class="flex flex-wrap gap-2">${buttons}</div></div>`);
    }
}

function toggleAttributeValue(attrId, valueId, valueName, rowIdx) {
    $(`#value-btn-${rowIdx}-${valueId}`).toggleClass('bg-blue-600 text-white border-blue-600');
    
    if (!ProductForm.selectedValues[attrId]) ProductForm.selectedValues[attrId] = [];
    const idx = ProductForm.selectedValues[attrId].indexOf(valueId);
    idx === -1 ? ProductForm.selectedValues[attrId].push(valueId) : ProductForm.selectedValues[attrId].splice(idx, 1);
    
    generateVariants();
}

function addNewValue(attrId, rowIdx) {
    const val = $(`#new-value-input-${rowIdx}`).val().trim();
    if (!val) return;
    
    const newId = `new-${Date.now()}`;
    if (!ProductForm.selectedValues[attrId]) ProductForm.selectedValues[attrId] = [];
    ProductForm.selectedValues[attrId].push(newId);
    
    const attr = ProductForm.attributes.find(a => (a.id || a.Id) === attrId);
    if (attr) (attr.values || attr.Values).push({ id: newId, value: val });
    
    generateVariants();
    onAttributeSelected(`attr-row-${rowIdx}`, attrId, rowIdx);
}

// ============================================
// VARIANTS
// ============================================
function generateVariants() {
    const keys = Object.keys(ProductForm.selectedValues).filter(k => ProductForm.selectedValues[k].length);
    if (!keys.length) {
        ProductForm.variants = [];
        renderVariants();
        return;
    }

    const combos = cartesianProduct(...keys.map(k => ProductForm.selectedValues[k]));
    ProductForm.variants = combos.map(combo => {
        const ids = Array.isArray(combo) ? combo : [combo];
        const names = ids.map(vid => {
            for (let attr of ProductForm.attributes) {
                const vals = attr.values || attr.Values;
                const v = vals?.find(val => (val.id || val.Id) === vid);
                if (v) return v.value || v.Value;
            }
        }).filter(Boolean);
        
        return {
            combination: names.join(' / '),
            selectedAttributeValueIds: ids,
            sku: '',
            price: $('#basePrice').val() || 0,
            stock: 0
        };
    });
    
    renderVariants();
}

function cartesianProduct(...arrays) {
    return arrays.reduce((acc, arr) => 
        acc.flatMap(x => arr.map(y => [...(Array.isArray(x) ? x : [x]), y])), [[]]);
}

function renderVariants() {
    $('#variantCount').text(`${ProductForm.variants.length} Biến thể`);
    
    if (!ProductForm.variants.length) {
        $('#variantsContainer').html('<p class="text-gray-500 text-sm">Chọn thuộc tính để tạo biến thể</p>');
        return;
    }

    const html = ProductForm.variants.map((v, i) => {
        const combo = v.combination || v.Combination || `Biến thể ${i + 1}`;
        const sku = v.sku || v.SKU || '';
        const price = v.price || v.Price || 0;
        const stock = v.stock || v.Stock || 0;
        const attrIds = v.selectedAttributeValueIds || v.SelectedAttributeValueIds || [];
        
        return `
        <div class="flex gap-4 p-4 border rounded-lg hover:shadow-sm transition">
            <div class="flex-1">
                <div class="font-bold text-blue-600">${combo}</div>
                <div class="text-sm mt-1">SKU: 
                    <input type="text" name="Variants[${i}].SKU" value="${sku}" 
                           class="border-b focus:border-blue-500 max-w-[200px]" required />
                </div>
                <input type="hidden" name="Variants[${i}].Combination" value="${combo}" />
                ${attrIds.map((id, j) => `<input type="hidden" name="Variants[${i}].SelectedAttributeValueIds[${j}]" value="${id}" />`).join('')}
            </div>
            <div class="flex gap-4">
                <div>
                    <label class="block text-xs text-gray-500">Giá bán</label>
                    <input type="number" name="Variants[${i}].Price" value="${price}" 
                           class="w-24 border-b text-right" min="0" step="1000" /> ₫
                </div>
                <div>
                    <label class="block text-xs text-gray-500">Tồn kho</label>
                    <input type="number" name="Variants[${i}].Stock" value="${stock}" 
                           class="w-16 border-b text-right" min="0" />
                </div>
            </div>
        </div>`;
    }).join('');
    
    $('#variantsContainer').html(html);
}

// ============================================
// IMAGE
// ============================================
function previewImage(e) {
    const file = e.target.files[0];
    if (!file) return;
    
    const reader = new FileReader();
    reader.onload = ev => {
        $('#previewImg').attr('src', ev.target.result);
        $('#uploadArea').addClass('hidden');
        $('#imagePreview').removeClass('hidden');
    };
    reader.readAsDataURL(file);
}

function removeImage() {
    $('#imageFile').val('');
    $('#uploadArea').removeClass('hidden');
    $('#imagePreview').addClass('hidden');
}

// ============================================
// SUBMIT
// ============================================
$('#productForm').on('submit', async function (e) {
    e.preventDefault();
    
    const formData = new FormData(this);
    const id = $('#Id').val();
    const isCreate = !id || id === '00000000-0000-0000-0000-000000000000';
    const url = isCreate ? '/Products/Create' : `/Products/Update/${id}`;

    try {
        const res = await fetch(url, { method: 'POST', body: formData });
        const result = await res.json();
        
        if (result.success) {
            alert(result.message);
            window.location.href = '/Products/Index';
        } else {
            alert(`Lỗi: ${result.message}`);
        }
    } catch (error) {
        console.error('Submission error:', error);
        alert('Lỗi kết nối!');
    }
});
