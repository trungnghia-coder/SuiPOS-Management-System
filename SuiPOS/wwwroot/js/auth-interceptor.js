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
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                }
            });

            if (response.ok) {
                const data = await response.json();
                console.log('Token refreshed successfully');
                return data.success;
            }
            console.warn('Token refresh failed with status:', response.status);
            return false;
        } catch (error) {
            console.error('Token refresh failed:', error);
            return false;
        }
    },

    async handleUnauthorized() {
        if (this.isRefreshing) {
            return new Promise((resolve, reject) => {
                this.failedQueue.push({ resolve, reject });
            });
        }

        console.log('Attempting to refresh token...');
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
            console.log('Redirecting to login...');
            window.location.href = '/Auth/Login';
            return false;
        }
    }
};

// Override fetch globally
const originalFetch = window.fetch;
window.fetch = async function(...args) {
    let response = await originalFetch(...args);

    // If 401 Unauthorized
    if (response.status === 401) {
        const refreshed = await AuthInterceptor.handleUnauthorized();
        
        if (refreshed) {
            // Retry original request
            response = await originalFetch(...args);
        }
    }

    return response;
};

// Check token expiry periodically (every 5 minutes)
setInterval(async () => {
    const response = await fetch('/Auth/CheckToken');
    if (!response.ok) {
        await AuthInterceptor.refreshAccessToken();
    }
}, 5 * 60 * 1000);
