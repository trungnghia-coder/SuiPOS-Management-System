// ============================================
// AUTO TOKEN REFRESH INTERCEPTOR
// ============================================

const AuthInterceptor = {
    isRefreshing: false,
    failedQueue: [],

    async refreshAccessToken() {
        try {
            const response = await fetch('/Auth/RefreshToken', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'include' // Important: send cookies
            });

            if (response.ok) {
                const data = await response.json();
                console.log('? Token refreshed successfully');
                return data.success;
            }
            console.warn('?? Token refresh failed with status:', response.status);
            return false;
        } catch (error) {
            console.error('? Token refresh error:', error);
            return false;
        }
    },

    async handleUnauthorized() {
        if (this.isRefreshing) {
            return new Promise((resolve, reject) => {
                this.failedQueue.push({ resolve, reject });
            });
        }

        console.log('?? Attempting to refresh token...');
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
            console.log('?? Redirecting to login...');
            window.location.href = '/Auth/Login';
            return false;
        }
    }
};

// Override fetch globally
const originalFetch = window.fetch;
window.fetch = async function(...args) {
    let response = await originalFetch(...args);

    // If 401 Unauthorized and not already a refresh request
    if (response.status === 401 && !args[0].includes('/Auth/RefreshToken')) {
        console.log('?? 401 Unauthorized detected, attempting refresh...');
        const refreshed = await AuthInterceptor.handleUnauthorized();
        
        if (refreshed) {
            // Retry original request
            console.log('?? Retrying original request...');
            response = await originalFetch(...args);
        }
    }

    return response;
};

// ? Check token every 2 minutes (more frequent)
let tokenCheckInterval;

function startTokenCheck() {
    // Clear existing interval if any
    if (tokenCheckInterval) {
        clearInterval(tokenCheckInterval);
    }
    
    tokenCheckInterval = setInterval(async () => {
        try {
            const response = await fetch('/Auth/CheckToken', {
                credentials: 'include'
            });
            
            if (!response.ok) {
                console.log('? Token check failed, refreshing...');
                await AuthInterceptor.refreshAccessToken();
            } else {
                console.log('? Token still valid');
            }
        } catch (error) {
            console.error('? Token check error:', error);
        }
    }, 2 * 60 * 1000); // ? Every 2 minutes instead of 5
}

// Start checking when page loads
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', startTokenCheck);
} else {
    startTokenCheck();
}

console.log('?? Auth interceptor initialized - checking token every 2 minutes');


