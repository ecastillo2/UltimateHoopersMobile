/**
 * Simplified Login System with Working Spinner
 * Fixes conflicts and ensures proper dashboard redirect
 */

document.addEventListener('DOMContentLoaded', function () {
    console.log('🔐 Initializing Fixed Login System');

    // Initialize the fixed login system
    initializeFixedLoginHandlers();
    initializeModalHandlers();
    initializeFormValidation();

    console.log('✅ Fixed login system initialized');

    // ========== FIXED LOGIN FORM HANDLERS ==========
    function initializeFixedLoginHandlers() {
        // Find and attach handlers to both login forms
        const clientForm = document.querySelector('#client-login form');
        const adminForm = document.querySelector('#admin-login form');

        if (clientForm) {
            // Remove any existing onsubmit handlers
            clientForm.removeAttribute('onsubmit');
            clientForm.addEventListener('submit', function (e) {
                handleFormSubmit(e, 'client');
            });
            console.log('✅ Client form handler attached');
        }

        if (adminForm) {
            // Remove any existing onsubmit handlers
            adminForm.removeAttribute('onsubmit');
            adminForm.addEventListener('submit', function (e) {
                handleFormSubmit(e, 'admin');
            });
            console.log('✅ Admin form handler attached');
        }

        // Add immediate click feedback to submit buttons
        const submitButtons = document.querySelectorAll('#loginModal button[type="submit"]');
        submitButtons.forEach(button => {
            button.addEventListener('click', function () {
                // Immediate visual feedback on button click
                this.style.transform = 'scale(0.95)';
                setTimeout(() => {
                    this.style.transform = '';
                }, 150);
                console.log('👆 Login button clicked - immediate feedback given');
            });
        });
    }

    function handleFormSubmit(e, loginType) {
        const form = e.target;
        const submitBtn = form.querySelector('button[type="submit"]');

        console.log(`🔐 ${loginType} login form submitted`);

        // Validate form first
        if (!validateLoginForm(form)) {
            e.preventDefault();
            return false;
        }

        // IMMEDIATELY show spinner and loading state before any delays
        console.log('🟣 Showing spinner IMMEDIATELY for login');

        // Show spinner with multiple methods to ensure it displays
        showLoginSpinnerImmediately(`Logging in as ${loginType}...`);
        setButtonLoadingState(submitBtn, true, loginType);

        // Disable form to prevent double submission
        disableForm(form, true);

        // Force a small delay to ensure spinner is visible before form submission
        e.preventDefault(); // Prevent immediate submission

        setTimeout(() => {
            console.log('🔐 Submitting form after spinner is shown');
            // Now submit the form programmatically
            form.submit();
        }, 100); // Small delay to ensure spinner renders

        // Set a longer timeout to hide spinner if something goes wrong
        setTimeout(() => {
            // Only hide if we're still on the same page (login failed)
            if (window.location.pathname === '/' || window.location.pathname === '') {
                console.log('⚠️ Login timeout - hiding spinner');
                hideLoginSpinner();
                setButtonLoadingState(submitBtn, false, loginType);
                disableForm(form, false);
                showLoginError(form, 'Login took too long. Please try again.');
            }
        }, 15000); // 15 second timeout

        return false; // Prevent default, we'll submit manually after spinner shows
    }

    // ========== SPINNER MANAGEMENT ==========
    function showLoginSpinner(message = 'Logging in...') {
        console.log('🟣 Showing login spinner:', message);

        // Use the existing spinner system from _Layout.cshtml
        if (window.showSpinner) {
            window.showSpinner(message);
        } else {
            // Fallback spinner
            showFallbackSpinner(message);
        }
    }

    function hideLoginSpinner() {
        console.log('🟣 Hiding login spinner');

        if (window.hideSpinner) {
            window.hideSpinner();
        } else {
            hideFallbackSpinner();
        }
    }

    function showFallbackSpinner(message) {
        let spinner = document.getElementById('fallbackSpinner');
        if (!spinner) {
            spinner = document.createElement('div');
            spinner.id = 'fallbackSpinner';
            spinner.innerHTML = `
                <div style="position: fixed; top: 0; left: 0; width: 100%; height: 100%; 
                           background: rgba(0,0,0,0.7); display: flex; align-items: center; 
                           justify-content: center; z-index: 9999; flex-direction: column;">
                    <div class="spinner-border text-light" style="width: 3rem; height: 3rem;" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    <div class="text-light mt-3" id="fallbackSpinnerMessage">${message}</div>
                </div>
            `;
            document.body.appendChild(spinner);
        } else {
            spinner.style.display = 'flex';
            const messageEl = spinner.querySelector('#fallbackSpinnerMessage');
            if (messageEl) messageEl.textContent = message;
        }
        document.body.style.overflow = 'hidden';
    }

    function hideFallbackSpinner() {
        const spinner = document.getElementById('fallbackSpinner');
        if (spinner) {
            spinner.style.display = 'none';
            document.body.style.overflow = '';
        }
    }

    // ========== BUTTON STATE MANAGEMENT ==========
    function setButtonLoadingState(button, loading, loginType) {
        if (!button) return;

        if (loading) {
            button.disabled = true;
            button.setAttribute('data-original-html', button.innerHTML);
            button.innerHTML = `
                <span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                Signing in...
            `;
            button.classList.add('btn-loading');
        } else {
            button.disabled = false;
            const originalHtml = button.getAttribute('data-original-html') ||
                `Login as ${loginType.charAt(0).toUpperCase() + loginType.slice(1)}`;
            button.innerHTML = originalHtml;
            button.classList.remove('btn-loading');
        }
    }

    function disableForm(form, disabled) {
        const inputs = form.querySelectorAll('input, button');
        inputs.forEach(input => {
            input.disabled = disabled;
        });

        // Add visual feedback to the entire modal during login
        const modal = document.getElementById('loginModal');
        if (disabled) {
            modal?.classList.add('logging-in');
            console.log('🔒 Modal locked during login');
        } else {
            modal?.classList.remove('logging-in');
            console.log('🔓 Modal unlocked');
        }
    }

    // ========== FORM VALIDATION ==========
    function validateLoginForm(form) {
        const emailInput = form.querySelector('input[type="email"]');
        const passwordInput = form.querySelector('input[type="password"]');

        let isValid = true;

        // Clear previous errors
        clearFormErrors(form);

        // Validate email
        if (!emailInput.value.trim()) {
            markFieldInvalid(emailInput);
            isValid = false;
        } else if (!isValidEmail(emailInput.value)) {
            markFieldInvalid(emailInput);
            isValid = false;
        } else {
            markFieldValid(emailInput);
        }

        // Validate password
        if (!passwordInput.value || passwordInput.value.length < 3) {
            markFieldInvalid(passwordInput);
            isValid = false;
        } else {
            markFieldValid(passwordInput);
        }

        if (!isValid) {
            showLoginError(form, 'Please check your email and password.');
            shakeForm(form);
        }

        return isValid;
    }

    function isValidEmail(email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    }

    function markFieldValid(field) {
        field.classList.remove('is-invalid');
        field.classList.add('is-valid');
    }

    function markFieldInvalid(field) {
        field.classList.remove('is-valid');
        field.classList.add('is-invalid');
    }

    function clearFormErrors(form) {
        // Remove error messages
        const errors = form.querySelectorAll('.login-error-message');
        errors.forEach(error => error.remove());

        // Remove validation classes
        const inputs = form.querySelectorAll('input');
        inputs.forEach(input => {
            input.classList.remove('is-valid', 'is-invalid');
        });
    }

    function showLoginError(form, message) {
        clearFormErrors(form);

        const errorDiv = document.createElement('div');
        errorDiv.className = 'alert alert-danger alert-dismissible fade show login-error-message mt-3';
        errorDiv.innerHTML = `
            <i class="bi bi-exclamation-triangle me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        const submitButton = form.querySelector('button[type="submit"]');
        submitButton.parentNode.insertBefore(errorDiv, submitButton);
    }

    function shakeForm(form) {
        form.style.animation = 'shake 0.6s ease-in-out';
        setTimeout(() => {
            form.style.animation = '';
        }, 600);
    }

    // ========== MODAL HANDLERS ==========
    function initializeModalHandlers() {
        const loginModal = document.getElementById('loginModal');
        if (!loginModal) return;

        loginModal.addEventListener('show.bs.modal', function () {
            resetAllForms();
        });

        loginModal.addEventListener('hidden.bs.modal', function () {
            resetAllForms();
            hideLoginSpinner(); // Make sure spinner is hidden when modal closes
        });
    }

    function resetAllForms() {
        const forms = document.querySelectorAll('#loginModal form');
        forms.forEach(form => {
            form.reset();
            clearFormErrors(form);
            disableForm(form, false);

            const submitBtn = form.querySelector('button[type="submit"]');
            if (submitBtn) {
                const loginType = form.closest('#client-login') ? 'client' : 'admin';
                setButtonLoadingState(submitBtn, false, loginType);
            }
        });
    }

    // ========== REAL-TIME VALIDATION ==========
    function initializeFormValidation() {
        const emailInputs = document.querySelectorAll('#loginModal input[type="email"]');
        const passwordInputs = document.querySelectorAll('#loginModal input[type="password"]');

        emailInputs.forEach(input => {
            input.addEventListener('blur', function () {
                if (this.value.trim()) {
                    if (isValidEmail(this.value)) {
                        markFieldValid(this);
                    } else {
                        markFieldInvalid(this);
                    }
                }
            });

            input.addEventListener('input', function () {
                if (this.classList.contains('is-invalid') && isValidEmail(this.value)) {
                    markFieldValid(this);
                }
            });
        });

        passwordInputs.forEach(input => {
            input.addEventListener('input', function () {
                if (this.classList.contains('is-invalid') && this.value.length >= 3) {
                    markFieldValid(this);
                }
            });
        });
    }

    // ========== GLOBAL CLEANUP ==========

    // Hide spinner on page unload/reload
    window.addEventListener('beforeunload', hideLoginSpinner);
    window.addEventListener('pagehide', hideLoginSpinner);

    // Hide spinner if user navigates away
    window.addEventListener('popstate', hideLoginSpinner);

    // ========== DEMO LOGIN BUTTONS (Optional) ==========
    function addDemoLoginButtons() {
        const clientForm = document.querySelector('#client-login form');
        const adminForm = document.querySelector('#admin-login form');

        if (clientForm) {
            addDemoButton(clientForm, 'user@client.com', 'client123');
        }

        if (adminForm) {
            addDemoButton(adminForm, 'admin@company.com', 'admin123');
        }
    }

    function addDemoButton(form, email, password) {
        const demoBtn = document.createElement('button');
        demoBtn.type = 'button';
        demoBtn.className = 'btn btn-outline-secondary btn-sm w-100 mt-2';
        demoBtn.innerHTML = '<i class="bi bi-lightning me-1"></i>Quick Demo';

        demoBtn.addEventListener('click', function () {
            const emailInput = form.querySelector('input[type="email"]');
            const passwordInput = form.querySelector('input[type="password"]');

            emailInput.value = email;
            passwordInput.value = password;

            markFieldValid(emailInput);
            markFieldValid(passwordInput);
        });

        const submitButton = form.querySelector('button[type="submit"]');
        submitButton.parentNode.insertBefore(demoBtn, submitButton);
    }

    // Uncomment to add demo buttons
    // addDemoLoginButtons();

    console.log('✅ Fixed login system ready');

    // ========== DEBUG FUNCTIONS ==========
    window.loginDebug = {
        // Test spinner display
        testSpinner: function () {
            console.log('🧪 Testing spinner display...');
            showLoginSpinnerImmediately('Testing spinner...');
            setTimeout(() => {
                hideLoginSpinner();
                console.log('✅ Spinner test completed');
            }, 3000);
        },

        // Test login flow without actual submission
        testLoginFlow: function (type = 'client') {
            console.log(`🧪 Testing ${type} login flow...`);
            const form = document.querySelector(`#${type}-login form`);
            const button = form.querySelector('button[type="submit"]');

            showLoginSpinnerImmediately(`Testing ${type} login...`);
            setButtonLoadingState(button, true, type);
            disableForm(form, true);

            setTimeout(() => {
                hideLoginSpinner();
                setButtonLoadingState(button, false, type);
                disableForm(form, false);
                console.log('✅ Login flow test completed');
            }, 3000);
        },

        // Force show emergency spinner
        forceEmergencySpinner: function () {
            console.log('🚨 Forcing emergency spinner...');
            createAndShowEmergencySpinner('Emergency spinner test');
        },

        // Check if spinners are working
        checkSpinners: function () {
            console.log('🔍 Checking spinner availability...');
            console.log('Global showSpinner:', typeof window.showSpinner);
            console.log('Global hideSpinner:', typeof window.hideSpinner);
            console.log('Emergency spinner element:', document.getElementById('emergencyLoginSpinner'));
            console.log('Global spinner element:', document.getElementById('globalSpinner'));
        },

        // Reset everything
        reset: function () {
            console.log('🔄 Resetting login system...');
            hideLoginSpinner();
            resetAllForms();
            console.log('✅ Reset completed');
        }
    };

    console.log('🐛 Enhanced login debug functions available: window.loginDebug');
    console.log('🐛 Try: window.loginDebug.testSpinner() to test spinner');
    console.log('🐛 Try: window.loginDebug.testLoginFlow("client") to test login flow');
    console.log('🐛 Try: window.loginDebug.checkSpinners() to check spinner status');
});

// Enhanced animations and visual feedback CSS
const enhancedCSS = `
<style>
@keyframes shake {
    0%, 100% { transform: translateX(0); }
    10%, 30%, 50%, 70%, 90% { transform: translateX(-5px); }
    20%, 40%, 60%, 80% { transform: translateX(5px); }
}

@keyframes pulse {
    0% { opacity: 1; }
    50% { opacity: 0.8; }
    100% { opacity: 1; }
}

@keyframes bounceIn {
    from, 20%, 40%, 60%, 80%, to {
        animation-timing-function: cubic-bezier(0.215, 0.610, 0.355, 1.000);
    }
    0% {
        opacity: 0;
        transform: scale3d(.3, .3, .3);
    }
    20% {
        transform: scale3d(1.1, 1.1, 1.1);
    }
    40% {
        transform: scale3d(.9, .9, .9);
    }
    60% {
        opacity: 1;
        transform: scale3d(1.03, 1.03, 1.03);
    }
    80% {
        transform: scale3d(.97, .97, .97);
    }
    to {
        opacity: 1;
        transform: scale3d(1, 1, 1);
    }
}

.login-error-message {
    animation: slideInDown 0.3s ease-out;
}

@keyframes slideInDown {
    from {
        transform: translateY(-20px);
        opacity: 0;
    }
    to {
        transform: translateY(0);
        opacity: 1;
    }
}

/* Enhanced button loading state */
.btn-loading {
    position: relative;
    pointer-events: none;
    overflow: hidden;
}

.btn-loading::before {
    content: '';
    position: absolute;
    top: 0;
    left: -100%;
    width: 100%;
    height: 100%;
    background: linear-gradient(90deg, transparent, rgba(255,255,255,0.2), transparent);
    animation: shimmer 2s infinite;
}

@keyframes shimmer {
    0% { left: -100%; }
    100% { left: 100%; }
}

/* Emergency spinner enhancements */
#emergencyLoginSpinner {
    backdrop-filter: blur(3px);
}

#emergencyLoginSpinner .spinner-border {
    filter: drop-shadow(0 0 10px rgba(255,255,255,0.5));
}

/* Modal enhancements during login */
#loginModal.logging-in {
    pointer-events: none;
}

#loginModal.logging-in .modal-content {
    opacity: 0.7;
    transform: scale(0.98);
    transition: all 0.3s ease;
}

/* Form field validation enhancements */
.form-control.is-valid {
    border-color: #28a745;
    box-shadow: 0 0 0 0.2rem rgba(40, 167, 69, 0.25);
}

.form-control.is-invalid {
    border-color: #dc3545;
    box-shadow: 0 0 0 0.2rem rgba(220, 53, 69, 0.25);
}

/* Spinner overlay improvements */
.spinner-overlay {
    backdrop-filter: blur(2px);
    animation: bounceIn 0.3s ease-out;
}
</style>
`;

if (document.head && !document.getElementById('enhanced-login-styles')) {
    const styleEl = document.createElement('div');
    styleEl.id = 'enhanced-login-styles';
    styleEl.innerHTML = enhancedCSS;
    document.head.appendChild(styleEl);
}