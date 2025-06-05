/**
 * Fixed Login System with Working Spinner - Simplified Version
 * Focus on reliability and getting login working
 */

document.addEventListener('DOMContentLoaded', function () {
    console.log('🔐 Initializing Simplified Login System');

    // Initialize the login system
    initializeLoginHandlers();
    initializeModalHandlers();

    console.log('✅ Login system initialized');

    // ========== SIMPLIFIED LOGIN FORM HANDLERS ==========
    function initializeLoginHandlers() {
        // Find and attach handlers to both login forms
        const clientForm = document.querySelector('#client-login form');
        const adminForm = document.querySelector('#admin-login form');

        if (clientForm) {
            clientForm.addEventListener('submit', function (e) {
                handleFormSubmit(e, 'client');
            });
            console.log('✅ Client form handler attached');
        }

        if (adminForm) {
            adminForm.addEventListener('submit', function (e) {
                handleFormSubmit(e, 'admin');
            });
            console.log('✅ Admin form handler attached');
        }
    }

    function handleFormSubmit(e, loginType) {
        const form = e.target;
        const submitBtn = form.querySelector('button[type="submit"]');

        console.log(`🔐 ${loginType} login form submitted`);

        // Basic validation only - don't prevent submission for minor issues
        const emailInput = form.querySelector('input[type="email"]');
        const passwordInput = form.querySelector('input[type="password"]');

        if (!emailInput.value.trim() || !passwordInput.value.trim()) {
            console.log('❌ Empty fields detected');
            showQuickError(form, 'Please fill in both email and password.');
            e.preventDefault();
            return false;
        }

        // Show visual feedback immediately
        console.log('🟣 Showing login feedback');
        setButtonLoadingState(submitBtn, true, loginType);
        showSimpleSpinner(`Logging in as ${loginType}...`);

        // Let the form submit naturally - don't prevent it
        // The spinner will be hidden when the page changes or if login fails

        console.log('✅ Form submitting normally');
        return true;
    }

    // ========== SIMPLE SPINNER SYSTEM ==========
    function showSimpleSpinner(message = 'Loading...') {
        console.log('🟣 Showing simple spinner:', message);

        // Try global spinner first
        if (window.showSpinner && typeof window.showSpinner === 'function') {
            window.showSpinner(message);
            return;
        }

        // Fallback to our own spinner
        createSimpleSpinner(message);
    }

    function createSimpleSpinner(message) {
        // Remove any existing spinner
        hideSimpleSpinner();

        const spinner = document.createElement('div');
        spinner.id = 'simpleLoginSpinner';
        spinner.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0, 0, 0, 0.7);
            display: flex;
            align-items: center;
            justify-content: center;
            z-index: 9999;
            flex-direction: column;
        `;

        spinner.innerHTML = `
            <div class="spinner-border text-light" style="width: 3rem; height: 3rem;" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
            <div class="text-light mt-3">${message}</div>
        `;

        document.body.appendChild(spinner);
        console.log('✅ Simple spinner created');
    }

    function hideSimpleSpinner() {
        // Hide global spinner
        if (window.hideSpinner && typeof window.hideSpinner === 'function') {
            window.hideSpinner();
        }

        // Hide our spinner
        const spinner = document.getElementById('simpleLoginSpinner');
        if (spinner) {
            spinner.remove();
            console.log('✅ Simple spinner removed');
        }
    }

    // ========== BUTTON STATE MANAGEMENT ==========
    function setButtonLoadingState(button, loading, loginType) {
        if (!button) return;

        if (loading) {
            button.disabled = true;
            const originalText = button.textContent;
            button.setAttribute('data-original-text', originalText);
            button.innerHTML = `
                <span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                Signing in...
            `;
        } else {
            button.disabled = false;
            const originalText = button.getAttribute('data-original-text') ||
                `Login as ${loginType.charAt(0).toUpperCase() + loginType.slice(1)}`;
            button.textContent = originalText;
        }
    }

    // ========== SIMPLE ERROR HANDLING ==========
    function showQuickError(form, message) {
        // Remove any existing errors
        const existingError = form.querySelector('.quick-error');
        if (existingError) {
            existingError.remove();
        }

        const errorDiv = document.createElement('div');
        errorDiv.className = 'alert alert-danger quick-error mt-2';
        errorDiv.innerHTML = `<i class="bi bi-exclamation-triangle me-2"></i>${message}`;

        const submitButton = form.querySelector('button[type="submit"]');
        form.insertBefore(errorDiv, submitButton);

        // Auto-remove error after 5 seconds
        setTimeout(() => {
            if (errorDiv.parentNode) {
                errorDiv.remove();
            }
        }, 5000);
    }

    // ========== MODAL HANDLERS ==========
    function initializeModalHandlers() {
        const loginModal = document.getElementById('loginModal');
        if (!loginModal) return;

        loginModal.addEventListener('hidden.bs.modal', function () {
            console.log('🚪 Modal closed - cleaning up');
            hideSimpleSpinner();
            resetAllForms();
        });
    }

    function resetAllForms() {
        const forms = document.querySelectorAll('#loginModal form');
        forms.forEach(form => {
            // Reset form
            form.reset();

            // Remove errors
            const errors = form.querySelectorAll('.quick-error');
            errors.forEach(error => error.remove());

            // Reset button state
            const submitBtn = form.querySelector('button[type="submit"]');
            if (submitBtn) {
                const loginType = form.closest('#client-login') ? 'client' : 'admin';
                setButtonLoadingState(submitBtn, false, loginType);
            }
        });
    }

    // ========== CLEANUP ==========
    window.addEventListener('beforeunload', hideSimpleSpinner);
    window.addEventListener('pagehide', hideSimpleSpinner);

    // ========== DEBUG FUNCTIONS ==========
    window.loginDebug = {
        testSpinner: function () {
            console.log('🧪 Testing spinner...');
            showSimpleSpinner('Test spinner');
            setTimeout(() => {
                hideSimpleSpinner();
                console.log('✅ Spinner test complete');
            }, 3000);
        },

        checkSystem: function () {
            console.log('🔍 Checking login system...');
            console.log('Client form:', document.querySelector('#client-login form'));
            console.log('Admin form:', document.querySelector('#admin-login form'));
            console.log('Global showSpinner:', typeof window.showSpinner);
            console.log('Global hideSpinner:', typeof window.hideSpinner);
        },

        reset: function () {
            console.log('🔄 Resetting...');
            hideSimpleSpinner();
            resetAllForms();
            console.log('✅ Reset complete');
        },

        testLogin: function (type = 'client') {
            console.log(`🧪 Testing ${type} login process...`);
            const form = document.querySelector(`#${type}-login form`);
            const button = form.querySelector('button[type="submit"]');

            setButtonLoadingState(button, true, type);
            showSimpleSpinner(`Testing ${type} login...`);

            setTimeout(() => {
                setButtonLoadingState(button, false, type);
                hideSimpleSpinner();
                console.log('✅ Test complete');
            }, 3000);
        }
    };

    console.log('🐛 Debug functions: window.loginDebug.testSpinner(), .checkSystem(), .reset(), .testLogin()');
    console.log('✅ Simplified login system ready');
});

// Add basic styling for quick errors
const basicCSS = `
<style>
.quick-error {
    font-size: 0.9rem;
    border-radius: 0.375rem;
    margin-bottom: 1rem;
}

.btn-loading {
    position: relative;
}

#simpleLoginSpinner {
    backdrop-filter: blur(2px);
}
</style>
`;

if (document.head && !document.getElementById('simple-login-styles')) {
    const styleEl = document.createElement('div');
    styleEl.id = 'simple-login-styles';
    styleEl.innerHTML = basicCSS;
    document.head.appendChild(styleEl);
}