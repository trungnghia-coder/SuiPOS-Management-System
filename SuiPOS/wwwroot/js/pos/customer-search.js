// ============================================
// CUSTOMER SEARCH AUTOCOMPLETE WITH SESSIONSTORAGE
// ============================================

const CustomerState = {
    STORAGE_KEY: 'pos_selected_customer',
    
    get customer() {
        const stored = sessionStorage.getItem(this.STORAGE_KEY);
        return stored ? JSON.parse(stored) : null;
    },
    
    set customer(value) {
        if (value) {
            sessionStorage.setItem(this.STORAGE_KEY, JSON.stringify(value));
        } else {
            sessionStorage.removeItem(this.STORAGE_KEY);
        }
    }
};

let searchTimeout = null;

function initCustomerSearch() {
    const input = document.getElementById('customerSearchInput');
    const dropdown = document.getElementById('customerDropdown');
    
    if (!input || !dropdown) return;

    // Restore customer from sessionStorage
    const savedCustomer = CustomerState.customer;
    if (savedCustomer) {
        displaySelectedCustomer(savedCustomer);
    }

    input.addEventListener('input', function() {
        const query = this.value.trim();
        
        clearTimeout(searchTimeout);
        
        if (query.length < 2) {
            dropdown.classList.add('hidden');
            return;
        }

        searchTimeout = setTimeout(() => searchCustomers(query), 300);
    });

    input.addEventListener('focus', function() {
        if (this.value.trim().length >= 2) {
            searchCustomers(this.value.trim());
        }
    });

    document.addEventListener('click', function(e) {
        if (!input.contains(e.target) && !dropdown.contains(e.target)) {
            dropdown.classList.add('hidden');
        }
    });
}

async function searchCustomers(query) {
    const dropdown = document.getElementById('customerDropdown');
    
    try {
        dropdown.innerHTML = '<div class="p-3 text-center text-gray-500"><i class="fas fa-spinner fa-spin"></i> ?ang tìm...</div>';
        dropdown.classList.remove('hidden');

        const response = await fetch(`/Customers/Search?query=${encodeURIComponent(query)}`);
        const customers = await response.json();

        if (!customers.length) {
            dropdown.innerHTML = '<div class="p-3 text-center text-gray-500">Không tìm th?y khách hàng</div>';
            return;
        }

        dropdown.innerHTML = customers.map(c => `
            <div class="p-3 hover:bg-gray-50 cursor-pointer border-b last:border-b-0 customer-item"
                 onclick='selectCustomer(${JSON.stringify(c)})'>
                <div class="flex items-center gap-3">
                    <div class="w-10 h-10 rounded-full bg-blue-500 text-white flex items-center justify-center font-bold">
                        ${c.avatar}
                    </div>
                    <div class="flex-1">
                        <div class="font-medium text-gray-800">${c.name}</div>
                        <div class="text-sm text-gray-500">${c.phone || 'Ch?a có S?T'}</div>
                    </div>
                </div>
            </div>
        `).join('');

    } catch (error) {
        console.error('Error searching customers:', error);
        dropdown.innerHTML = '<div class="p-3 text-center text-red-500">L?i khi tìm ki?m</div>';
    }
}

function selectCustomer(customer) {
    CustomerState.customer = customer;
    displaySelectedCustomer(customer);
    document.getElementById('customerDropdown').classList.add('hidden');
    document.getElementById('customerSearchInput').value = '';
}

function displaySelectedCustomer(customer) {
    const searchContainer = document.getElementById('customerSearchContainer');
    const selectedDisplay = document.getElementById('selectedCustomer');

    searchContainer.classList.add('hidden');
    selectedDisplay.classList.remove('hidden');

    document.getElementById('selectedAvatar').textContent = customer.avatar;
    document.getElementById('selectedName').textContent = customer.name;
    document.getElementById('selectedPhone').textContent = customer.phone || 'Ch?a có S?T';
}

function clearSelectedCustomer() {
    CustomerState.customer = null;
    
    const searchContainer = document.getElementById('customerSearchContainer');
    const selectedDisplay = document.getElementById('selectedCustomer');
    
    selectedDisplay.classList.add('hidden');
    searchContainer.classList.remove('hidden');
    
    document.getElementById('customerSearchInput').focus();
}

document.addEventListener('DOMContentLoaded', function() {
    initCustomerSearch();
});
