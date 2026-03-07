// ============================================
// AUTO TOKEN REFRESH INTERCEPTOR
// ============================================

const AuthInterceptor = {
    isRefreshing: false,
    failedQueue: [],

    async refreshAccessToken() {
        try {
            const response = await originalFetch('/Auth/RefreshToken', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include'
            });

            if (response.ok) {
                const data = await response.json();
                return data.success;
            }
            return false;
        } catch (error) {
            return false;
        }
    },

    async handleUnauthorized() {
        if (this.isRefreshing) {
            return new Promise((resolve, reject) => {
                this.failedQueue.push({ resolve, reject });
            });
        }

        this.isRefreshing = true;
        const refreshed = await this.refreshAccessToken();
        this.isRefreshing = false;

        if (refreshed) {
            this.failedQueue.forEach(promise => promise.resolve());
            this.failedQueue = [];
            return true;
        } else {
            this.failedQueue.forEach(promise => promise.reject());
            this.failedQueue = [];
            window.location.href = '/Auth/Login';
            return false;
        }
    },

    getStaffData() {
        const name = "staff_data=";
        const decodedCookie = decodeURIComponent(document.cookie);
        const ca = decodedCookie.split(';');
        for (let i = 0; i < ca.length; i++) {
            let c = ca[i].trim();
            if (c.indexOf(name) == 0) {
                try {
                    return JSON.parse(c.substring(name.length, c.length));
                } catch (e) { return null; }
            }
        }
        return null;
    },

    renderStaffUI() {
        const staff = this.getStaffData();
        console.log('Staff Data:', staff); 

        if (!staff) {
            console.warn('No staff data found in cookie');
            return;
        }

        const initials = staff.name.split(' ').filter(n => n).map(n => n[0]).join('').toUpperCase();
        console.log('Initials:', initials); 

        const elements = {
            'userInitials': initials,
            'userName': staff.name,
            'userRole': staff.role,
            'dropdown-auth-name': staff.name,
            'dropdown-auth-role': staff.role,
            'posStaffName': staff.name
        };

        for (const [id, value] of Object.entries(elements)) {
            const el = document.getElementById(id);
            if (el) {
                el.textContent = value;
                console.log(`Updated #${id}:`, value);
            } else {
                console.warn(`Element #${id} not found`);
            }
        }

        if (staff.role !== 'Admin') {
            document.querySelectorAll('.admin-only').forEach(el => el.remove());
        }
    },

    setupDropdownToggle() {
        const btn = document.getElementById('userMenuBtn');
        const dropdown = document.getElementById('userMenuDropdown');

        if (!btn || !dropdown) return;

        btn.addEventListener('click', function(e) {
            e.stopPropagation();
            dropdown.classList.toggle('hidden');
        });

        document.addEventListener('click', function(e) {
            if (!dropdown.contains(e.target) && !btn.contains(e.target)) {
                dropdown.classList.add('hidden');
            }
        });
    }
};

const originalFetch = window.fetch;
window.fetch = async function (...args) {
    let response = await originalFetch(...args);

    const isRefreshRequest = args[0].includes('/Auth/RefreshToken') || args[0].includes('/Auth/CheckToken');

    if (response.status === 401 && !isRefreshRequest) {
        const refreshed = await AuthInterceptor.handleUnauthorized();

        if (refreshed) {
            return await originalFetch(...args);
        }
    }

    return response;
};

let tokenCheckInterval;
function startTokenCheck() {
    if (tokenCheckInterval) clearInterval(tokenCheckInterval);

    tokenCheckInterval = setInterval(async () => {
        try {
            await fetch('/Auth/CheckToken', { credentials: 'include' });
        } catch (error) {
        }
    }, 5 * 60 * 1000); 
}

function initAuth() {
    AuthInterceptor.renderStaffUI();
    AuthInterceptor.setupDropdownToggle();
    startTokenCheck();
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initAuth);
} else {
    initAuth();
}


