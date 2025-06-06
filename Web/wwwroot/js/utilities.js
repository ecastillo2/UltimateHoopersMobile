/**
 * Enhanced Utility Functions - Centralized Toast and Spinner System
 * Provides common functionality used across the entire application
 */

window.UIUtils = {
    // ========== TOAST NOTIFICATION SYSTEM ==========
    showToast: function (message, type = 'info', title = '', duration = 5000) {
        console.log(`${type.toUpperCase()}: ${title ? title + ' - ' : ''}${message}`);

        const alertClass = this.getBootstrapAlertClass(type);
        const icon = this.getToastIcon(type);

        const toast = document.createElement('div');
        toast.className = `alert alert-${alertClass} alert-dismissible fade show position-fixed`;
        toast.style.cssText = `
            top: 20px;
            right: 20px;
            z-index: 9999;
            min-width: 300px;
            max-width: 500px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            animation: slideInRight 0.3s ease-out;
        `;

        toast.innerHTML = `
            <div class="d-flex align-items-center">
                <i class="bi bi-${icon} me-2"></i>
                <div class="flex-grow-1">
                    ${title ? `<strong>${title}:</strong> ` : ''}${message}
                </div>
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;

        document.body.appendChild(toast);

        // Auto remove after duration
        setTimeout(() => {
            if (toast.parentElement) {
                toast.style.animation = 'slideOutRight 0.3s ease-in';
                setTimeout(() => {
                    if (toast.parentElement) {
                        toast.remove();
                    }
                }, 300);
            }
        }, duration);

        return toast;
    },

    // Convenience methods for different toast types
    showSuccess: function (message, title = 'Success') {
        return this.showToast(message, 'success', title);
    },

    showError: function (message, title = 'Error') {
        return this.showToast(message, 'error', title);
    },

    showWarning: function (message, title = 'Warning') {
        return this.showToast(message, 'warning', title);
    },

    showInfo: function (message, title = 'Info') {
        return this.showToast(message, 'info', title);
    },

    getBootstrapAlertClass: function (type) {
        const classMap = {
            'success': 'success',
            'error': 'danger',
            'warning': 'warning',
            'info': 'info'
        };
        return classMap[type] || 'info';
    },

    getToastIcon: function (type) {
        const iconMap = {
            'success': 'check-circle',
            'error': 'x-circle',
            'warning': 'exclamation-triangle',
            'info': 'info-circle'
        };
        return iconMap[type] || 'info-circle';
    },

    // ========== LOADING STATE MANAGEMENT ==========
    showLoading: function (message = 'Loading...') {
        let loader = document.getElementById('globalLoader');
        if (!loader) {
            loader = document.createElement('div');
            loader.id = 'globalLoader';
            loader.className = 'position-fixed top-0 start-0 w-100 h-100 d-flex align-items-center justify-content-center';
            loader.style.cssText = `
                background: rgba(255, 255, 255, 0.9);
                z-index: 9999;
                backdrop-filter: blur(2px);
            `;
            loader.innerHTML = `
                <div class="text-center">
                    <div class="spinner-border text-primary mb-3" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    <div class="text-muted">${message}</div>
                </div>
            `;
            document.body.appendChild(loader);
        } else {
            // Update message if loader already exists
            const messageEl = loader.querySelector('.text-muted');
            if (messageEl) {
                messageEl.textContent = message;
            }
        }
        loader.style.display = 'flex';
    },

    hideLoading: function () {
        const loader = document.getElementById('globalLoader');
        if (loader) {
            loader.style.display = 'none';
        }
    },

    // ========== BUTTON LOADING STATE ==========
    setButtonLoading: function (button, isLoading, loadingText = 'Loading...') {
        if (!button) return;

        if (isLoading) {
            button.disabled = true;
            if (!button.dataset.originalText) {
                button.dataset.originalText = button.innerHTML;
            }
            button.innerHTML = `<span class="spinner-border spinner-border-sm me-2" role="status"></span>${loadingText}`;
        } else {
            button.disabled = false;
            if (button.dataset.originalText) {
                button.innerHTML = button.dataset.originalText;
                delete button.dataset.originalText;
            }
        }
    },

    // ========== MODAL LOADING STATE ==========
    showModalLoading: function (modalId, message = 'Loading...') {
        const modal = document.getElementById(modalId);
        if (!modal) return;

        const modalBody = modal.querySelector('.modal-body');
        if (!modalBody) return;

        let loadingDiv = modal.querySelector('.modal-loading-overlay');
        if (!loadingDiv) {
            loadingDiv = document.createElement('div');
            loadingDiv.className = 'modal-loading-overlay';
            loadingDiv.style.cssText = `
                position: absolute;
                top: 0;
                left: 0;
                right: 0;
                bottom: 0;
                background: rgba(255, 255, 255, 0.8);
                display: flex;
                align-items: center;
                justify-content: center;
                z-index: 1000;
            `;
            loadingDiv.innerHTML = `
                <div class="text-center">
                    <div class="spinner-border text-primary mb-2" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    <div class="text-muted">${message}</div>
                </div>
            `;
            modalBody.style.position = 'relative';
            modalBody.appendChild(loadingDiv);
        }
        loadingDiv.style.display = 'flex';
    },

    hideModalLoading: function (modalId) {
        const modal = document.getElementById(modalId);
        if (!modal) return;

        const loadingDiv = modal.querySelector('.modal-loading-overlay');
        if (loadingDiv) {
            loadingDiv.style.display = 'none';
        }
    },

    // ========== SPINNER UTILITIES ==========
    createSpinner: function (size = 'md', color = 'primary') {
        const sizeClass = size === 'sm' ? 'spinner-border-sm' : '';
        return `<div class="spinner-border ${sizeClass} text-${color}" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>`;
    },

    // ========== DATE/TIME UTILITIES ==========
    formatDate: function (date, format = 'short') {
        if (!date) return '';

        if (typeof date === 'string') {
            date = new Date(date);
        }

        if (isNaN(date.getTime())) return '';

        const options = {
            short: { month: 'short', day: 'numeric', year: 'numeric' },
            long: { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' },
            time: { hour: 'numeric', minute: '2-digit', hour12: true }
        };

        return date.toLocaleDateString('en-US', options[format] || options.short);
    },

    formatTime: function (time) {
        if (!time) return '';

        // Handle TimeSpan format (HH:mm:ss)
        if (typeof time === 'string' && time.includes(':')) {
            const parts = time.split(':');
            if (parts.length >= 2) {
                const hours = parseInt(parts[0]);
                const minutes = parseInt(parts[1]);
                const date = new Date();
                date.setHours(hours, minutes, 0, 0);
                return date.toLocaleTimeString('en-US', {
                    hour: 'numeric',
                    minute: '2-digit',
                    hour12: true
                });
            }
        }

        // Handle Date object
        if (time instanceof Date) {
            return time.toLocaleTimeString('en-US', {
                hour: 'numeric',
                minute: '2-digit',
                hour12: true
            });
        }

        return time.toString();
    },

    // ========== FORM UTILITIES ==========
    getFormData: function (form) {
        const formData = new FormData(form);
        const data = {};

        for (let [key, value] of formData.entries()) {
            if (data[key]) {
                // Handle multiple values (checkboxes, etc.)
                if (Array.isArray(data[key])) {
                    data[key].push(value);
                } else {
                    data[key] = [data[key], value];
                }
            } else {
                data[key] = value;
            }
        }

        return data;
    },

    // ========== VALIDATION UTILITIES ==========
    validateRequired: function (fields) {
        const errors = [];

        fields.forEach(field => {
            const element = typeof field === 'string' ? document.getElementById(field) : field;
            if (element && !element.value.trim()) {
                const label = element.labels && element.labels[0] ?
                    element.labels[0].textContent.replace('*', '').trim() :
                    element.name || element.id;
                errors.push(`${label} is required`);
                element.classList.add('is-invalid');
            } else if (element) {
                element.classList.remove('is-invalid');
            }
        });

        return errors;
    },

    // ========== API UTILITIES ==========
    getAntiForgeryToken: function () {
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        return token ? token.value : '';
    },

    // ========== URL UTILITIES ==========
    buildUrl: function (path, params = {}) {
        const url = new URL(path, window.location.origin);

        Object.keys(params).forEach(key => {
            if (params[key] !== null && params[key] !== undefined) {
                url.searchParams.append(key, params[key]);
            }
        });

        return url.toString();
    },

    // ========== LOCAL STORAGE UTILITIES ==========
    saveToStorage: function (key, data) {
        try {
            localStorage.setItem(key, JSON.stringify(data));
            return true;
        } catch (error) {
            console.warn('Could not save to localStorage:', error);
            return false;
        }
    },

    loadFromStorage: function (key, defaultValue = null) {
        try {
            const item = localStorage.getItem(key);
            return item ? JSON.parse(item) : defaultValue;
        } catch (error) {
            console.warn('Could not load from localStorage:', error);
            return defaultValue;
        }
    },

    // ========== DEBUG UTILITIES ==========
    log: function (message, data = null) {
        if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
            console.log(`🔧 ${message}`, data);
        }
    },

    error: function (message, error = null) {
        console.error(`❌ ${message}`, error);
    },

    warn: function (message, data = null) {
        console.warn(`⚠️ ${message}`, data);
    }
};

// Make utilities available globally with multiple aliases
window.Utils = window.UIUtils;

// Add CSS animations for toasts
const toastStyles = document.createElement('style');
toastStyles.textContent = `
    @keyframes slideInRight {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    @keyframes slideOutRight {
        from {
            transform: translateX(0);
            opacity: 1;
        }
        to {
            transform: translateX(100%);
            opacity: 0;
        }
    }
`;
document.head.appendChild(toastStyles);

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    console.log('🔧 Enhanced Utilities loaded successfully');

    // Auto-remove old alerts/toasts
    document.querySelectorAll('.alert').forEach(alert => {
        if (alert.classList.contains('auto-dismiss')) {
            setTimeout(() => {
                if (alert.parentElement) {
                    alert.remove();
                }
            }, 5000);
        }
    });
});