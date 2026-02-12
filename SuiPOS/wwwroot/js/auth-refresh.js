// ============================================
// AUTO REFRESH ACCESS TOKEN
// ============================================

let refreshTimer = null;

// Check access token và tự động refresh nếu gần hết hạn
async function checkAndRefreshToken() {
    const cookies = document.cookie.split(';');
    const hasAccessToken = cookies.some(c => c.trim().startsWith('suipos_ac='));
    const hasRefreshToken = cookies.some(c => c.trim().startsWith('suipos_rf='));

    // Nếu có refresh token nhưng không có access token → refresh ngay
    if (!hasAccessToken && hasRefreshToken) {
        console.log('🔄 Access token expired, refreshing...');
        await refreshAccessToken();
    }
}

async function refreshAccessToken() {
    try {
        const response = await fetch('/Auth/RefreshToken', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        const result = await response.json();

        if (result.success) {
            console.log('✅ Token refreshed successfully');
            return true;
        } else {
            console.warn('⚠️ Refresh failed:', result.message);
            // Redirect to login if refresh fails
            window.location.href = '/Auth/Login';
            return false;
        }
    } catch (error) {
        console.error('❌ Error refreshing token:', error);
        window.location.href = '/Auth/Login';
        return false;
    }
}

// Check token mỗi 5 phút
function startTokenRefreshMonitor() {
    // Check ngay khi load page
    checkAndRefreshToken();

    // Check mỗi 5 phút
    if (refreshTimer) {
        clearInterval(refreshTimer);
    }
    
    refreshTimer = setInterval(() => {
        checkAndRefreshToken();
    }, 5 * 60 * 1000); // 5 minutes
}

// Intercept fetch để tự động retry khi token expired
const originalFetch = window.fetch;
window.fetch = async function(...args) {
    try {
        const response = await originalFetch(...args);
        
        // Nếu 401 Unauthorized → thử refresh token
        if (response.status === 401) {
            console.log('🔐 401 Unauthorized, attempting token refresh...');
            const refreshed = await refreshAccessToken();
            
            if (refreshed) {
                // Retry request với token mới
                return await originalFetch(...args);
            }
        }
        
        return response;
    } catch (error) {
        throw error;
    }
};

// Start monitor khi DOM ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', startTokenRefreshMonitor);
} else {
    startTokenRefreshMonitor();
}

// Cleanup khi unload
window.addEventListener('beforeunload', () => {
    if (refreshTimer) {
        clearInterval(refreshTimer);
    }
});
