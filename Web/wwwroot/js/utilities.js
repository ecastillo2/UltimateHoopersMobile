/**
 * Basic Utility Functions for Run Management
 * Provides common functionality used across the application
 */

window.UIUtils = {
    // Toast notification system
    showToast: function (message, type = 'info', duration = 5000) {
        console.log(`${type.toUpperCase()}: ${message}`);

        // Create toast element
        const toast = document.createElement('div');
        toast.className = `alert alert-${this.getBootstrapAlertClass(type)} alert-dismissible fade show position-fixed`;
        toast.style.cssText = `
            top: 20px;
            right: 20px;
            z-index: 9999;
            min-width: 300px;
            max-width: 500px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        `;

        toast.innerHTML = `
            <div class="d-flex align-items-center">
                <i class="bi bi-${this.getToastIcon(type)} me-2"></i>
                <div class="flex-grow-1">${message}</div>
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;

        document.body.appendChild(toast);

        // Auto remove after duration
        setTimeout(() => {
            if (toast.parentElement) {
                toast.remove();
            }
        }, duration);

        return toast;
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

    // Loading state management
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
        }
        loader.style.display = 'flex';
    },

    hideLoading: function () {
        const loader = document.getElementById('globalLoader');
        if (loader) {
            loader.style.display = 'none';
        }
    },

    // Button loading state
    setButtonLoading: function (button, loading, originalText = null) {
        if (!button) return;

        if (loading) {
            if (!originalText) {
                button.dataset.originalText = button.textContent;
            } else {
                button.dataset.originalText = originalText;
            }
            button.disabled = true;
            button.innerHTML = `
                <span class="spinner-border spinner-border-sm me-2" role="status"></span>
                Loading...
            `;
        } else {
            button.disabled = false;
            button.textContent = button.dataset.originalText || 'Submit';
        }
    },

    // Date/Time utilities
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

    // Form utilities
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

    // Validation utilities
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

    // API utilities
    getAntiForgeryToken: function () {
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        return token ? token.value : '';
    },

    // URL utilities
    buildUrl: function (path, params = {}) {
        const url = new URL(path, window.location.origin);

        Object.keys(params).forEach(key => {
            if (params[key] !== null && params[key] !== undefined) {
                url.searchParams.append(key, params[key]);
            }
        });

        return url.toString();
    },

    // Local storage utilities (with error handling)
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

    // Debug utilities
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

// Make utilities available globally
window.Utils = window.UIUtils;

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    console.log('🔧 Utilities loaded successfully');

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