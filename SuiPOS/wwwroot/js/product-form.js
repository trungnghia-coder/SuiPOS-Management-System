// ============================================
// STATE MANAGEMENT
// ============================================
const ProductForm = {
    attributes: [],
    selectedValues: {},
    variants: [],
    existingVariants: [], // Track existing variants
    rowIndex: 0,
    isLoading: false,
    isEditMode: false
};

// ============================================
// INIT
// ============================================
$(() => {
    loadAttributes();
    
    if (window.initialVariants?.length) {
        ProductForm.isEditMode = true;
        ProductForm.variants = window.initialVariants.map(normalizeVariant);
        ProductForm.existingVariants = [...ProductForm.variants]; // Store copy of original
        
        // Pre-select attributes and values based on existing variants
        restoreAttributeSelections();
        renderVariants();
    }
});

function normalizeVariant(v) {
    return {
        id: v.Id || v.id || null, // Keep existing variant ID
        combination: v.Combination || v.combination || '',
        sku: v.SKU || v.sku || '',
        price: v.Price || v.price || 0,
        stock: v.Stock || v.stock || 0,
        selectedAttributeValueIds: v.SelectedAttributeValueIds || v.selectedAttributeValueIds || [],
        isExisting: true // ✅ CRITICAL: Mark as existing variant
    };
}


function restoreAttributeSelections() {
    // Group variants by attributes to restore selection state
    if (!ProductForm.variants.length) {
        console.warn('⚠️ No variants to restore selections from');
        return;
    }
    
    console.log('🔄 Restoring attribute selections from variants:', ProductForm.variants);
    console.log('📋 Available attributes:', ProductForm.attributes.length);
    
    // Build selectedValues from existing variants
    const attributeValueMap = {};
    
    ProductForm.variants.forEach(variant => {
        const valueIds = variant.selectedAttributeValueIds || [];
        console.log(`  Variant "${variant.combination}": has ${valueIds.length} attribute values`, valueIds);
        
        valueIds.forEach(valueId => {
            // Find which attribute this value belongs to
            for (let attr of ProductForm.attributes) {
                const values = attr.values || attr.Values || [];
                const foundValue = values.find(v => (v.id || v.Id) === valueId);
                if (foundValue) {
                    const attrId = attr.id || attr.Id;
                    const attrName = attr.name || attr.Name;
                    
                    if (!attributeValueMap[attrId]) {
                        attributeValueMap[attrId] = new Set();
                    }
                    attributeValueMap[attrId].add(valueId);
                    
                    console.log(`    ✅ Found value "${foundValue.value || foundValue.Value}" in attribute "${attrName}"`);
                    break; // Found, move to next valueId
                }
            }
        });
    });
    
    console.log('📋 Attribute value map:', attributeValueMap);
    console.log('📋 Map has keys:', Object.keys(attributeValueMap));
    
    // ✅ CRITICAL: Set selections BEFORE adding rows
    Object.keys(attributeValueMap).forEach(attrId => {
        const values = Array.from(attributeValueMap[attrId]);
        ProductForm.selectedValues[attrId] = values;
        console.log(`  ✅ Set selections for ${attrId}:`, values);
    });
    
    console.log('✅ Selected values restored:', ProductForm.selectedValues);
    console.log('✅ Selected values keys:', Object.keys(ProductForm.selectedValues));
    
    // ✅ Add attribute rows AFTER selections are set
    Object.keys(attributeValueMap).forEach((attrId, index) => {
        // Add row
        addAttributeRow(attrId);
        
        // ✅ Force refresh the value buttons with correct selection state
        // Need to wait for DOM to render
        setTimeout(() => {
            const rowIdx = ProductForm.rowIndex - Object.keys(attributeValueMap).length + index;
            console.log(`🔄 Re-triggering selection display for row ${rowIdx}, attr ${attrId}`);
            onAttributeSelected(`attr-row-${rowIdx}`, attrId, rowIdx);
        }, 200); // Tăng từ 100ms lên 200ms
    });
}





// ============================================
// ATTRIBUTES
// ============================================
function loadAttributes() {
    if (ProductForm.isLoading) return;
    
    ProductForm.isLoading = true;
    
    console.log('🔄 Loading attributes...');
    
    $.ajax({
        url: '/Attributes/GetWithValues',
        type: 'GET',
        dataType: 'json',
        success: function(response) {
            console.log('📦 Raw API response:', response);
            console.log('📦 Response type:', typeof response);
            
            // jQuery already parsed JSON
            if (response && response.success && response.data) {
                ProductForm.attributes = response.data;
                console.log(`✅ Loaded ${ProductForm.attributes.length} attributes`);
            } else if (Array.isArray(response)) {
                // Fallback if response is directly array
                ProductForm.attributes = response;
                console.log(`✅ Loaded ${ProductForm.attributes.length} attributes (direct array)`);
            } else {
                console.error('❌ Unexpected response format:', response);
                ProductForm.attributes = [];
            }
            
            console.log('📋 Attributes:', ProductForm.attributes);
            
            // ✅ CRITICAL: Restore selections AFTER attributes are loaded
            if (ProductForm.isEditMode && window.initialVariants?.length) {
                console.log('🔄 Edit mode detected, restoring selections NOW');
                restoreAttributeSelections();
            }
        },

        error: function(xhr, status, error) {
            console.error('❌ Failed to load attributes:', error);
            console.error('Status:', status);
            console.error('Response:', xhr.responseText);
            alert('Không thể tải thuộc tính! Vui lòng kiểm tra kết nối.');
            ProductForm.attributes = [];
        },
        complete: function() {
            ProductForm.isLoading = false;
        }
    });
}



function addAttributeRow(preselectedAttrId = null) {
    console.log('➕ Adding attribute row, current attributes:', ProductForm.attributes.length);
    
    if (!ProductForm.attributes || !ProductForm.attributes.length) {
        console.warn('⚠️ Attributes not loaded yet, waiting...');
        alert('Vui lòng đợi tải danh sách thuộc tính!');
        
        // Retry after 1 second if attributes are being loaded
        if (!ProductForm.isLoading) {
            loadAttributes();
        }
        
        setTimeout(() => {
            if (ProductForm.attributes && ProductForm.attributes.length) {
                addAttributeRow(preselectedAttrId);
            }
        }, 1000);
        return;
    }

    const rowId = `attr-row-${ProductForm.rowIndex}`;
    const currentRowIdx = ProductForm.rowIndex;
    const options = ProductForm.attributes.map(a => {
        const attrId = a.id || a.Id;
        const attrName = a.name || a.Name;
        const selected = attrId === preselectedAttrId ? 'selected' : '';
        return `<option value="${attrId}" ${selected}>${attrName}</option>`;
    }).join('');

    $('<div>', { 
        id: rowId,
        class: 'mb-6 pb-6 border-b border-gray-200',
        html: `
            <div class="grid grid-cols-2 gap-6">
                <div>
                    <label class="block text-sm font-medium text-gray-700 mb-2">Thuộc tính</label>
                    <div class="flex gap-3">
                        <select id="attr-select-${currentRowIdx}" 
                                onchange="onAttributeSelected('${rowId}', this.value, ${currentRowIdx})" 
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
                    <div id="values-container-${currentRowIdx}">
                        <input type="text" placeholder="Chọn thuộc tính trước" class="w-full px-4 py-3 border rounded-lg bg-gray-50" disabled />
                    </div>
                </div>
            </div>
            <div id="selected-values-${currentRowIdx}" class="mt-4 flex flex-wrap gap-2"></div>`
    }).appendTo('#attributesContainer');
    
    ProductForm.rowIndex++;
    
    // If preselected, trigger the change event
    if (preselectedAttrId) {
        onAttributeSelected(rowId, preselectedAttrId, currentRowIdx);
    }
}



function removeAttributeRow(rowId) {
    const idx = rowId.split('-')[2];
    const attrId = $(`#attr-select-${idx}`).val();
    if (attrId) delete ProductForm.selectedValues[attrId];
    $(`#${rowId}`).remove();
    generateVariants();
}

function onAttributeSelected(rowId, attrId, rowIdx) {
    console.log(`🎯 Attribute selected: rowId=${rowId}, attrId=${attrId}, rowIdx=${rowIdx}`);
    
    const container = $(`#values-container-${rowIdx}`);
    const selectedContainer = $(`#selected-values-${rowIdx}`);
    
    if (!attrId) {
        container.html('<input type="text" class="w-full px-4 py-3 border rounded-lg bg-gray-50" disabled />');
        selectedContainer.empty();
        return;
    }

    const attr = ProductForm.attributes.find(a => (a.id || a.Id) === attrId);
    if (!attr) {
        console.warn(`⚠️ Attribute ${attrId} not found`);
        return;
    }

    console.log(`📋 Attribute:`, attr.name || attr.Name);

    // Input for adding new values
    container.html(`
        <div class="flex gap-2">
            <input type="text" id="new-value-input-${rowIdx}" placeholder="Thêm giá trị mới" 
                   class="flex-1 px-4 py-3 border rounded-lg" />
            <button type="button" onclick="addNewValue('${attrId}', ${rowIdx})" 
                    class="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700">Thêm</button>
        </div>`);

    const values = attr.values || attr.Values || [];
    if (values.length) {
        // ✅ Get current selections for this attribute (if any)
        const selectedValueIds = ProductForm.selectedValues[attrId] || [];
        
        console.log(`  📌 Current selections for ${attrId}:`, selectedValueIds);
        
        const buttons = values.map(v => {
            const vId = v.id || v.Id;
            const vVal = v.value || v.Value;
            const isSelected = selectedValueIds.includes(vId);
            const selectedClass = isSelected ? 'bg-blue-600 text-white border-blue-600' : '';
            
            if (isSelected) {
                console.log(`    ✅ Value ${vVal} (${vId}) is selected`);
            }
            
            return `<button type="button" id="value-btn-${rowIdx}-${vId}"
                            class="px-4 py-2 border rounded-lg hover:bg-blue-50 ${selectedClass}"
                            onclick="toggleAttributeValue('${attrId}', '${vId}', '${vVal}', ${rowIdx})">${vVal}</button>`;
        }).join('');
        
        selectedContainer.html(`
            <div class="w-full">
                <div class="text-sm text-gray-600 mb-2">Chọn nhanh:</div>
                <div class="flex flex-wrap gap-2">${buttons}</div>
            </div>
        `);
    } else {
        selectedContainer.empty();
    }
}



function toggleAttributeValue(attrId, valueId, valueName, rowIdx) {
    console.log(`🔘 Toggle: attr=${attrId}, value=${valueId} (${valueName}), row=${rowIdx}`);
    
    $(`#value-btn-${rowIdx}-${valueId}`).toggleClass('bg-blue-600 text-white border-blue-600');
    
    if (!ProductForm.selectedValues[attrId]) ProductForm.selectedValues[attrId] = [];
    const idx = ProductForm.selectedValues[attrId].indexOf(valueId);
    
    if (idx === -1) {
        ProductForm.selectedValues[attrId].push(valueId);
        console.log(`  ✅ Selected ${valueName}, now have:`, ProductForm.selectedValues[attrId].length);
    } else {
        ProductForm.selectedValues[attrId].splice(idx, 1);
        console.log(`  ❌ Deselected ${valueName}, now have:`, ProductForm.selectedValues[attrId].length);
    }
    
    console.log(`📋 All selections:`, ProductForm.selectedValues);
    
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
        // If no selections but we have existing variants, keep them
        if (ProductForm.isEditMode && ProductForm.existingVariants.length) {
            ProductForm.variants = [...ProductForm.existingVariants];
        } else {
            ProductForm.variants = [];
        }
        renderVariants();
        return;
    }

    // ✅ Store current variants BEFORE generating new ones
    const currentVariants = [...ProductForm.variants];
    
    // Generate ALL possible combinations from selected values
    const combos = cartesianProduct(...keys.map(k => ProductForm.selectedValues[k]));
    
    console.log(`🔄 Generating ${combos.length} variant combinations...`);
    
    // ✅ Map to variants, preserving existing ones BY COMBINATION STRING
    const newVariants = combos.map(combo => {
        const ids = Array.isArray(combo) ? combo : [combo];
        const names = ids.map(vid => {
            for (let attr of ProductForm.attributes) {
                const vals = attr.values || attr.Values;
                const v = vals?.find(val => (val.id || val.Id) === vid);
                if (v) return v.value || v.Value;
            }
        }).filter(Boolean);
        
        const combination = names.join(' / ');
        
        // ✅ CRITICAL: Check by COMBINATION STRING instead of IDs
        // This handles the case where IDs are different but combination is same
        const existing = currentVariants.find(ev => {
            const evCombination = ev.combination || ev.Combination || '';
            // Normalize: trim spaces, lowercase for comparison
            const normalized1 = combination.toLowerCase().trim().replace(/\s+/g, ' ');
            const normalized2 = evCombination.toLowerCase().trim().replace(/\s+/g, ' ');
            return normalized1 === normalized2;
        });
        
        if (existing) {
            // ✅ Keep existing variant data (preserve ID, price, stock, SKU)
            console.log(`  ✅ Keeping existing: ${combination} (ID: ${existing.id || 'new'}, isExisting=${existing.isExisting})`);
            return { 
                ...existing,
                isExisting: true, // Mark as existing
                selectedAttributeValueIds: ids // Update IDs to current selection
            };
        }
        
        // ✅ New variant - use basePrice
        console.log(`  ➕ Creating new: ${combination}`);
        return {
            id: null,
            combination: combination,
            selectedAttributeValueIds: ids,
            sku: '',
            price: $('#basePrice').val() || 0,
            stock: 0,
            isExisting: false
        };
    });
    
    const existingCount = newVariants.filter(v => v.isExisting || v.id).length;
    const newCount = newVariants.length - existingCount;
    console.log(`✅ Result: ${newVariants.length} total (${existingCount} existing + ${newCount} new)`);
    
    ProductForm.variants = newVariants;
    renderVariants();
}



function cartesianProduct(...arrays) {
    return arrays.reduce((acc, arr) => 
        acc.flatMap(x => arr.map(y => [...(Array.isArray(x) ? x : [x]), y])), [[]]);
}

function renderVariants() {
    $('#variantCount').text(`${ProductForm.variants.length} Biến thể`);
    
    if (!ProductForm.variants.length) {
        $('#variantsContainer').html('<p class="text-gray-500 text-sm text-center py-4">Chọn thuộc tính để tạo biến thể</p>');
        return;
    }

    const html = ProductForm.variants.map((v, i) => {
        const combo = v.combination || v.Combination || `Biến thể ${i + 1}`;
        const sku = v.sku || v.SKU || '';
        const price = v.price || v.Price || 0;
        const stock = v.stock || v.Stock || 0;
        const attrIds = v.selectedAttributeValueIds || v.SelectedAttributeValueIds || [];
        const variantId = v.id || v.Id || '';
        const isExisting = v.isExisting || false;
        
        const statusBadge = isExisting 
            ? '<span class="text-xs px-2 py-1 bg-green-100 text-green-800 rounded-full">Đã có</span>'
            : '<span class="text-xs px-2 py-1 bg-blue-100 text-blue-800 rounded-full">Mới</span>';
        
        return `
        <div class="flex gap-4 p-4 border rounded-lg hover:shadow-sm transition">
            <div class="flex-1">
                <div class="flex items-center gap-2">
                    <div class="font-bold text-blue-600">${combo}</div>
                    ${statusBadge}
                </div>
                <div class="text-sm mt-1">SKU: 
                    <input type="text" name="Variants[${i}].SKU" value="${sku}" 
                           class="border-b focus:border-blue-500 max-w-[200px]" required />
                </div>
                ${variantId ? `<input type="hidden" name="Variants[${i}].Id" value="${variantId}" />` : ''}
                <input type="hidden" name="Variants[${i}].Combination" value="${combo}" />
                ${attrIds.map((id, j) => `<input type="hidden" name="Variants[${i}].SelectedAttributeValueIds[${j}]" value="${id}" />`).join('')}
            </div>
            <div class="flex gap-4 items-center">
                <div>
                    <label class="block text-xs text-gray-500">Giá bán</label>
                    <input type="number" name="Variants[${i}].Price" value="${price}" 
                           class="w-24 border-b text-right" min="0" step="1000" required /> ₫
                </div>
                <div>
                    <label class="block text-xs text-gray-500">Tồn kho</label>
                    <input type="number" name="Variants[${i}].Stock" value="${stock}" 
                           class="w-16 border-b text-right" min="0" required />
                </div>
                <button type="button" onclick="removeVariant(${i})" 
                        class="text-red-600 hover:bg-red-50 p-2 rounded" title="Xóa biến thể">
                    <i class="fas fa-trash"></i>
                </button>
            </div>
        </div>`;
    }).join('');
    
    $('#variantsContainer').html(html);
}

function removeVariant(index) {
    if (!confirm('Bạn có chắc chắn muốn xóa biến thể này?')) {
        return;
    }
    
    ProductForm.variants.splice(index, 1);
    renderVariants();
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
