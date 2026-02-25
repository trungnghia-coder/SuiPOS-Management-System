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
    }

};

// Override fetch globally
const originalFetch = window.fetch;
window.fetch = async function(...args) {
    let response = await originalFetch(...args);

    // If 401 Unauthorized and not already a refresh request
    if (response.status === 401 && !args[0].includes('/Auth/RefreshToken')) {
        const refreshed = await AuthInterceptor.handleUnauthorized();
        
        if (refreshed) {
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
                await AuthInterceptor.refreshAccessToken();
            }
        } catch (error) {
        }
    }, 2 * 60 * 1000);
}


if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', startTokenCheck);
} else {
    startTokenCheck();
}



