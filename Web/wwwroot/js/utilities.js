/**
 * UI Utilities - Spinner and Toast Management
 * Common utility functions for loading spinners and toast notifications
 */

window.UIUtils = (function () {
    'use strict';

    // Toast configuration
    const TOAST_CONFIG = {
        autohide: {
            success: true,
            info: true,
            warning: true,
            error: false // Error toasts stay visible until manually dismissed
        },
        delay: 5000,
        icons: {
            success: 'bi-check-circle',
            error: 'bi-exclamation-triangle',
            warning: 'bi-exclamation-triangle',
            info: 'bi-info-circle'
        },
        bgClasses: {
            success: 'bg-success',
            error: 'bg-danger',
            warning: 'bg-warning',
            info: 'bg-info'
        }
    };

    /**
     * Show loading spinner
     * @param {string} selector - Optional selector for specific spinner element
     */
    function showLoading(selector = '.loading-spinner') {
        const spinner = document.querySelector(selector);
        if (spinner) {
            spinner.classList.add('active');
        }
    }

    /**
     * Hide loading spinner
     * @param {string} selector - Optional selector for specific spinner element
     */
    function hideLoading(selector = '.loading-spinner') {
        const spinner = document.querySelector(selector);
        if (spinner) {
            spinner.classList.remove('active');
        }
    }

    /**
     * Show toast notification
     * @param {string} message - The message to display
     * @param {string} type - Type of toast (success, error, warning, info)
     * @param {string} title - Optional title for the toast
     * @param {Object} options - Additional options
     */
    function showToast(message, type = 'success', title = null, options = {}) {
        console.log(`${type}: ${message}`);

        // Merge with default options
        const config = {
            autohide: TOAST_CONFIG.autohide[type] !== false,
            delay: options.delay || TOAST_CONFIG.delay,
            position: options.position || 'bottom-end'
        };

        // Create toast container if it doesn't exist
        let container = getOrCreateToastContainer(config.position);

        // Create toast
        const toastId = `toast-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
        const iconClass = TOAST_CONFIG.icons[type] || 'bi-info-circle';
        const bgClass = TOAST_CONFIG.bgClasses[type] || 'bg-info';

        // Build toast HTML
        let toastContent = `<i class="bi ${iconClass} me-2"></i>`;
        if (title) {
            toastContent += `<strong>${title}</strong>: `;
        }
        toastContent += message;

        const toastHtml = `
            <div id="${toastId}" class="toast align-items-center text-white ${bgClass} border-0" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body">
                        ${toastContent}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>
        `;

        container.insertAdjacentHTML('beforeend', toastHtml);

        // Initialize and show toast
        const toastElement = document.getElementById(toastId);
        if (toastElement && typeof bootstrap !== 'undefined' && bootstrap.Toast) {
            const toast = new bootstrap.Toast(toastElement, {
                autohide: config.autohide,
                delay: config.delay
            });

            toast.show();

            // Remove toast element after it's hidden
            toastElement.addEventListener('hidden.bs.toast', () => {
                toastElement.remove();
            });

            // Return toast instance for manual control if needed
            return toast;
        }

        return null;
    }

    /**
     * Get or create toast container
     * @param {string} position - Position for the container
     * @returns {HTMLElement} The toast container element
     */
    function getOrCreateToastContainer(position = 'bottom-end') {
        const containerClass = `toast-container-${position}`;
        let container = document.querySelector(`.${containerClass}`);

        if (!container) {
            container = document.createElement('div');
            container.className = `toast-container ${containerClass} position-fixed p-3`;
            container.style.zIndex = '9999';

            // Set position based on parameter
            switch (position) {
                case 'top-start':
                    container.classList.add('top-0', 'start-0');
                    break;
                case 'top-center':
                    container.classList.add('top-0', 'start-50', 'translate-middle-x');
                    break;
                case 'top-end':
                    container.classList.add('top-0', 'end-0');
                    break;
                case 'bottom-start':
                    container.classList.add('bottom-0', 'start-0');
                    break;
                case 'bottom-center':
                    container.classList.add('bottom-0', 'start-50', 'translate-middle-x');
                    break;
                case 'bottom-end':
                default:
                    container.classList.add('bottom-0', 'end-0');
                    break;
            }

            document.body.appendChild(container);
        }

        return container;
    }

    /**
     * Show success toast
     * @param {string} message - Success message
     * @param {string} title - Optional title
     */
    function showSuccess(message, title = null) {
        return showToast(message, 'success', title);
    }

    /**
     * Show error toast
     * @param {string} message - Error message
     * @param {string} title - Optional title
     */
    function showError(message, title = null) {
        return showToast(message, 'error', title);
    }

    /**
     * Show warning toast
     * @param {string} message - Warning message
     * @param {string} title - Optional title
     */
    function showWarning(message, title = null) {
        return showToast(message, 'warning', title);
    }

    /**
     * Show info toast
     * @param {string} message - Info message
     * @param {string} title - Optional title
     */
    function showInfo(message, title = null) {
        return showToast(message, 'info', title);
    }

    /**
     * Clear all toasts
     */
    function clearAllToasts() {
        const containers = document.querySelectorAll('[class*="toast-container"]');
        containers.forEach(container => {
            const toasts = container.querySelectorAll('.toast');
            toasts.forEach(toast => {
                const toastInstance = bootstrap.Toast.getInstance(toast);
                if (toastInstance) {
                    toastInstance.hide();
                } else {
                    toast.remove();
                }
            });
        });
    }

    /**
     * Set loading state on a button
     * @param {HTMLElement|string} button - Button element or selector
     * @param {boolean} loading - Whether to show loading state
     * @param {string} loadingText - Text to show when loading
     */
    function setButtonLoading(button, loading = true, loadingText = 'Loading...') {
        const btn = typeof button === 'string' ? document.querySelector(button) : button;
        if (!btn) return;

        if (loading) {
            // Store original state
            btn.dataset.originalText = btn.innerHTML;
            btn.dataset.originalDisabled = btn.disabled;

            // Set loading state
            btn.disabled = true;
            btn.innerHTML = `<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>${loadingText}`;
        } else {
            // Restore original state
            btn.disabled = btn.dataset.originalDisabled === 'true';
            btn.innerHTML = btn.dataset.originalText || btn.innerHTML;

            // Clean up data attributes
            delete btn.dataset.originalText;
            delete btn.dataset.originalDisabled;
        }
    }

    /**
     * Create a loading overlay for an element
     * @param {HTMLElement|string} target - Target element or selector
     * @param {string} message - Loading message
     */
    function showElementLoading(target, message = 'Loading...') {
        const element = typeof target === 'string' ? document.querySelector(target) : target;
        if (!element) return;

        // Remove existing overlay
        hideElementLoading(element);

        // Create overlay
        const overlay = document.createElement('div');
        overlay.className = 'ui-loading-overlay position-absolute top-0 start-0 w-100 h-100 d-flex align-items-center justify-content-center bg-white bg-opacity-75';
        overlay.style.zIndex = '1000';
        overlay.innerHTML = `
            <div class="text-center">
                <div class="spinner-border text-primary mb-2" role="status" aria-hidden="true"></div>
                <div class="text-muted">${message}</div>
            </div>
        `;

        // Ensure target is positioned
        const position = window.getComputedStyle(element).position;
        if (position === 'static') {
            element.style.position = 'relative';
            overlay.dataset.positionModified = 'true';
        }

        element.appendChild(overlay);
    }

    /**
     * Hide loading overlay from an element
     * @param {HTMLElement|string} target - Target element or selector
     */
    function hideElementLoading(target) {
        const element = typeof target === 'string' ? document.querySelector(target) : target;
        if (!element) return;

        const overlay = element.querySelector('.ui-loading-overlay');
        if (overlay) {
            // Restore position if we modified it
            if (overlay.dataset.positionModified) {
                element.style.position = '';
            }
            overlay.remove();
        }
    }

    // Public API
    return {
        // Loading functions
        showLoading,
        hideLoading,
        setButtonLoading,
        showElementLoading,
        hideElementLoading,

        // Toast functions
        showToast,
        showSuccess,
        showError,
        showWarning,
        showInfo,
        clearAllToasts,

        // Utility functions
        getOrCreateToastContainer
    };
})();

// For backwards compatibility, expose commonly used functions globally
window.showLoading = window.UIUtils.showLoading;
window.hideLoading = window.UIUtils.hideLoading;
window.showToast = window.UIUtils.showToast;