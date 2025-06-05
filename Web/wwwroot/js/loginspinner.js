/**
 * Login Form Enhancement with Bootstrap Spinners
 * Complete solution for Ultimate Hoopers login system
 */

document.addEventListener('DOMContentLoaded', function () {
    console.log('🔐 Initializing Login System with Bootstrap Spinners');

    // Initialize login handlers
    initializeLoginHandlers();
    initializeModalHandlers();
    initializeFormValidation();

    console.log('✅ Login system initialized successfully');

    // ========== LOGIN FORM HANDLERS ==========
    function initializeLoginHandlers() {
        // Client Login Form
        const clientLoginForm = document.querySelector('#client-login form');
        if (clientLoginForm) {
            clientLoginForm.addEventListener('submit', function (e) {
                handleLoginSubmit(e, 'client');
            });
            console.log('✅ Client login handler attached');
        }

        // Admin Login Form
        const adminLoginForm = document.querySelector('#admin-login form');
        if (adminLoginForm) {
            adminLoginForm.addEventListener('submit', function (e) {
                handleLoginSubmit(e, 'admin');
            });
            console.log('✅ Admin login handler attached');
        }

        // Demo login buttons (if needed)
        initializeDemoLoginButtons();
    }

    function handleLoginSubmit(e, loginType) {
        e.preventDefault();

        const form = e.target;
        const submitBtn = form.querySelector('button[type="submit"]');
        const emailInput = form.querySelector('input[type="email"]');
        const passwordInput = form.querySelector('input[type="password"]');

        console.log(`🔐 ${loginType} login attempted`);

        // Validate form
        if (!validateLoginForm(form, loginType)) {
            return;
        }

        // Show loading state
        setLoginButtonLoading(submitBtn, true, loginType);

        // Disable form inputs
        setFormInputsDisabled(form, true);

        // Clear any existing error messages
        clearLoginErrors(form);

        // Prepare form data
        const formData = new FormData(form);

        // Get the correct endpoint
        const endpoint = loginType === 'admin' ? '/Account/AdminLogin' : '/Account/ClientLogin';

        // Submit form with fetch
        fetch(endpoint, {
            method: 'POST',
            body: formData,
            headers: {
                'RequestVerificationToken': getAntiForgeryToken()
            }
        })
            .then(response => {
                console.log(`📡 ${loginType} login response:`, response.status);

                if (response.ok) {
                    return response.text().then(text => {
                        // Check if response is JSON or HTML redirect
                        try {
                            return JSON.parse(text);
                        } catch (e) {
                            // If not JSON, likely a redirect (success)
                            if (text.includes('<!DOCTYPE html>') || response.url !== window.location.href) {
                                return { success: true, redirect: true };
                            }
                            throw new Error('Unexpected response format');
                        }
                    });
                } else {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
            })
            .then(result => {
                console.log(`📦 ${loginType} login result:`, result);

                if (result.success || result.redirect) {
                    // Success - show success state briefly then redirect
                    setLoginButtonSuccess(submitBtn, loginType);

                    setTimeout(() => {
                        if (result.redirectUrl) {
                            window.location.href = result.redirectUrl;
                        } else {
                            // Default redirect for successful login
                            window.location.href = '/Dashboard';
                        }
                    }, 1000);
                } else {
                    // Handle login failure
                    handleLoginError(form, submitBtn, result.message || 'Login failed', loginType);
                }
            })
            .catch(error => {
                console.error(`❌ ${loginType} login error:`, error);
                handleLoginError(form, submitBtn, error.message || 'Network error occurred', loginType);
            });
    }

    function handleLoginError(form, submitBtn, errorMessage, loginType) {
        // Reset button state
        setLoginButtonLoading(submitBtn, false, loginType);

        // Re-enable form inputs
        setFormInputsDisabled(form, false);

        // Show error message
        showLoginError(form, errorMessage);

        // Shake animation for visual feedback
        addShakeAnimation(form);

        // Focus on email field for retry
        const emailInput = form.querySelector('input[type="email"]');
        if (emailInput) {
            setTimeout(() => emailInput.focus(), 300);
        }
    }

    // ========== BUTTON STATE MANAGEMENT ==========
    function setLoginButtonLoading(button, loading, loginType) {
        if (!button) return;

        const loginText = loginType === 'admin' ? 'Login as Admin' : 'Login as Client';

        if (loading) {
            // Store original text
            button.setAttribute('data-original-text', button.innerHTML);

            // Set loading state
            button.disabled = true;
            button.innerHTML = `
                <span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                <span>Signing in...</span>
            `;
            button.classList.add('btn-loading');

            console.log(`⏳ ${loginType} button set to loading state`);
        } else {
            // Reset to original state
            button.disabled = false;
            const originalText = button.getAttribute('data-original-text') || loginText;
            button.innerHTML = originalText;
            button.classList.remove('btn-loading', 'btn-success');

            console.log(`✅ ${loginType} button reset to normal state`);
        }
    }

    function setLoginButtonSuccess(button, loginType) {
        if (!button) return;

        button.innerHTML = `
            <i class="bi bi-check-circle me-2"></i>
            <span>Success! Redirecting...</span>
        `;
        button.classList.remove('btn-loading');
        button.classList.add('btn-success');

        console.log(`✅ ${loginType} button set to success state`);
    }

    // ========== FORM STATE MANAGEMENT ==========
    function setFormInputsDisabled(form, disabled) {
        const inputs = form.querySelectorAll('input, button');
        inputs.forEach(input => {
            input.disabled = disabled;
        });
    }

    function clearLoginErrors(form) {
        // Remove existing error messages
        const existingErrors = form.querySelectorAll('.login-error-message');
        existingErrors.forEach(error => error.remove());

        // Remove error classes from inputs
        const inputs = form.querySelectorAll('input');
        inputs.forEach(input => {
            input.classList.remove('is-invalid');
        });
    }

    function showLoginError(form, message) {
        // Remove any existing error messages
        clearLoginErrors(form);

        // Create error alert
        const errorDiv = document.createElement('div');
        errorDiv.className = 'alert alert-danger alert-dismissible fade show login-error-message mt-3';
        errorDiv.innerHTML = `
            <div class="d-flex align-items-center">
                <i class="bi bi-exclamation-triangle me-2"></i>
                <div class="flex-grow-1">${message}</div>
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;

        // Insert error message before the submit button
        const submitButton = form.querySelector('button[type="submit"]');
        if (submitButton) {
            submitButton.parentNode.insertBefore(errorDiv, submitButton);
        } else {
            form.appendChild(errorDiv);
        }

        console.log('❌ Login error displayed:', message);
    }

    function addShakeAnimation(element) {
        element.classList.add('shake-animation');
        setTimeout(() => {
            element.classList.remove('shake-animation');
        }, 600);
    }

    // ========== FORM VALIDATION ==========
    function initializeFormValidation() {
        // Real-time validation for email fields
        const emailInputs = document.querySelectorAll('#loginModal input[type="email"]');
        emailInputs.forEach(input => {
            input.addEventListener('blur', function () {
                validateEmailField(this);
            });

            input.addEventListener('input', function () {
                if (this.classList.contains('is-invalid')) {
                    validateEmailField(this);
                }
            });
        });

        // Real-time validation for password fields
        const passwordInputs = document.querySelectorAll('#loginModal input[type="password"]');
        passwordInputs.forEach(input => {
            input.addEventListener('input', function () {
                if (this.classList.contains('is-invalid')) {
                    validatePasswordField(this);
                }
            });
        });
    }

    function validateLoginForm(form, loginType) {
        const emailInput = form.querySelector('input[type="email"]');
        const passwordInput = form.querySelector('input[type="password"]');

        let isValid = true;

        // Clear previous validation states
        clearLoginErrors(form);

        // Validate email
        if (!validateEmailField(emailInput)) {
            isValid = false;
        }

        // Validate password
        if (!validatePasswordField(passwordInput)) {
            isValid = false;
        }

        if (!isValid) {
            showLoginError(form, 'Please check your email and password.');
        }

        return isValid;
    }

    function validateEmailField(input) {
        if (!input) return false;

        const email = input.value.trim();
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

        if (!email) {
            input.classList.add('is-invalid');
            return false;
        } else if (!emailRegex.test(email)) {
            input.classList.add('is-invalid');
            return false;
        } else {
            input.classList.remove('is-invalid');
            input.classList.add('is-valid');
            return true;
        }
    }

    function validatePasswordField(input) {
        if (!input) return false;

        const password = input.value;

        if (!password || password.length < 3) {
            input.classList.add('is-invalid');
            return false;
        } else {
            input.classList.remove('is-invalid');
            input.classList.add('is-valid');
            return true;
        }
    }

    // ========== MODAL HANDLERS ==========
    function initializeModalHandlers() {
        const loginModal = document.getElementById('loginModal');
        if (!loginModal) return;

        // Reset forms when modal is shown
        loginModal.addEventListener('show.bs.modal', function () {
            console.log('🔐 Login modal opening');
            resetAllLoginForms();
        });

        // Reset forms when modal is hidden
        loginModal.addEventListener('hidden.bs.modal', function () {
            console.log('🔐 Login modal closing');
            resetAllLoginForms();
        });

        // Tab switching handlers
        const tabButtons = loginModal.querySelectorAll('button[data-bs-toggle="tab"]');
        tabButtons.forEach(button => {
            button.addEventListener('shown.bs.tab', function () {
                const targetTab = this.getAttribute('data-bs-target');
                console.log('🔄 Switched to tab:', targetTab);

                // Focus on email field of active tab
                setTimeout(() => {
                    const activeForm = document.querySelector(targetTab);
                    const emailInput = activeForm?.querySelector('input[type="email"]');
                    if (emailInput) {
                        emailInput.focus();
                    }
                }, 100);
            });
        });
    }

    function resetAllLoginForms() {
        const forms = document.querySelectorAll('#loginModal form');
        forms.forEach(form => {
            form.reset();
            clearLoginErrors(form);
            setFormInputsDisabled(form, false);

            const submitBtn = form.querySelector('button[type="submit"]');
            if (submitBtn) {
                const loginType = form.closest('#client-login') ? 'client' : 'admin';
                setLoginButtonLoading(submitBtn, false, loginType);
            }

            // Remove validation classes
            const inputs = form.querySelectorAll('input');
            inputs.forEach(input => {
                input.classList.remove('is-valid', 'is-invalid');
            });
        });
    }

    // ========== DEMO LOGIN BUTTONS ==========
    function initializeDemoLoginButtons() {
        // Add quick demo login buttons (optional)
        const clientTab = document.getElementById('client-login');
        const adminTab = document.getElementById('admin-login');

        if (clientTab) {
            addDemoLoginButton(clientTab, 'client', 'user@client.com', 'client123');
        }

        if (adminTab) {
            addDemoLoginButton(adminTab, 'admin', 'admin@company.com', 'admin123');
        }
    }

    function addDemoLoginButton(container, type, email, password) {
        const form = container.querySelector('form');
        if (!form) return;

        const demoBtn = document.createElement('button');
        demoBtn.type = 'button';
        demoBtn.className = 'btn btn-outline-secondary btn-sm w-100 mt-2';
        demoBtn.innerHTML = `<i class="bi bi-lightning me-1"></i>Quick Demo Login`;

        demoBtn.addEventListener('click', function () {
            const emailInput = form.querySelector('input[type="email"]');
            const passwordInput = form.querySelector('input[type="password"]');

            if (emailInput && passwordInput) {
                emailInput.value = email;
                passwordInput.value = password;

                // Trigger validation
                validateEmailField(emailInput);
                validatePasswordField(passwordInput);

                // Add visual feedback
                this.innerHTML = '<i class="bi bi-check me-1"></i>Demo credentials filled';
                this.classList.add('btn-success');
                this.classList.remove('btn-outline-secondary');

                setTimeout(() => {
                    this.innerHTML = '<i class="bi bi-lightning me-1"></i>Quick Demo Login';
                    this.classList.remove('btn-success');
                    this.classList.add('btn-outline-secondary');
                }, 2000);
            }
        });

        // Insert before submit button
        const submitButton = form.querySelector('button[type="submit"]');
        if (submitButton) {
            submitButton.parentNode.insertBefore(demoBtn, submitButton);
        }
    }

    // ========== UTILITY FUNCTIONS ==========
    function getAntiForgeryToken() {
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        return token ? token.value : '';
    }

    // ========== GLOBAL API (for debugging) ==========
    window.loginDebug = {
        validateEmailField,
        validatePasswordField,
        setLoginButtonLoading,
        setLoginButtonSuccess,
        showLoginError,
        clearLoginErrors,
        resetAllLoginForms,

        // Test functions
        testClientLogin: function () {
            const form = document.querySelector('#client-login form');
            const email = form.querySelector('input[type="email"]');
            const password = form.querySelector('input[type="password"]');

            email.value = 'test@client.com';
            password.value = 'client123';

            return validateLoginForm(form, 'client');
        },

        testAdminLogin: function () {
            const form = document.querySelector('#admin-login form');
            const email = form.querySelector('input[type="email"]');
            const password = form.querySelector('input[type="password"]');

            email.value = 'admin@company.com';
            password.value = 'admin123';

            return validateLoginForm(form, 'admin');
        },

        simulateLoading: function (type = 'client') {
            const form = document.querySelector(`#${type}-login form`);
            const btn = form.querySelector('button[type="submit"]');

            setLoginButtonLoading(btn, true, type);

            setTimeout(() => {
                setLoginButtonSuccess(btn, type);
                setTimeout(() => {
                    setLoginButtonLoading(btn, false, type);
                }, 2000);
            }, 3000);
        }
    };

    console.log('🐛 Login debug functions available: window.loginDebug');
    console.log('🐛 Try: window.loginDebug.simulateLoading() to test spinner');
    console.log('🐛 Try: window.loginDebug.testClientLogin() to test validation');
});

// ========== CSS ANIMATIONS ==========
// Add these styles to your CSS file or include them in a <style> tag

const styles = `
<style>
/* Login Button Loading State */
.btn-loading {
    position: relative;
    pointer-events: none;
}

/* Shake Animation for Login Errors */
@keyframes shake {
    0%, 100% { transform: translateX(0); }
    10%, 30%, 50%, 70%, 90% { transform: translateX(-5px); }
    20%, 40%, 60%, 80% { transform: translateX(5px); }
}

.shake-animation {
    animation: shake 0.6s ease-in-out;
}

/* Loading Spinner Enhancement */
.spinner-border-sm {
    width: 1rem;
    height: 1rem;
    border-width: 0.125em;
}

/* Login Form Enhancements */
#loginModal .form-control {
    transition: border-color 0.15s ease-in-out, box-shadow 0.15s ease-in-out;
}

#loginModal .form-control:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 0.2rem rgba(255, 98, 56, 0.25);
}

#loginModal .form-control.is-valid {
    border-color: #28a745;
}

#loginModal .form-control.is-invalid {
    border-color: #dc3545;
}

/* Tab Enhancement */
#loginModal .nav-tabs .nav-link.active {
    color: var(--primary-color);
    border-bottom-color: var(--primary-color);
}

/* Button State Transitions */
#loginModal .btn {
    transition: all 0.15s ease-in-out;
}

#loginModal .btn:disabled {
    opacity: 0.8;
}

/* Success State */
.btn-success {
    background-color: #28a745 !important;
    border-color: #28a745 !important;
}

/* Demo Button Styling */
.btn-outline-secondary:hover {
    background-color: #6c757d;
    border-color: #6c757d;
}

/* Modal Backdrop Enhancement */
.modal-backdrop {
    backdrop-filter: blur(2px);
}

/* Login Error Message */
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

/* Responsive Improvements */
@media (max-width: 576px) {
    #loginModal .modal-dialog {
        margin: 1rem 0.5rem;
    }
    
    #loginModal .btn {
        padding: 0.5rem 1rem;
        font-size: 0.875rem;
    }
}
</style>
`;

// Inject styles into document head
if (document.head) {
    document.head.insertAdjacentHTML('beforeend', styles);
}