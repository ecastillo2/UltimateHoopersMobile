/**
 * Login/Signup Modal Handler
 * Manages switching between login and signup forms in the modal
 */

document.addEventListener('DOMContentLoaded', function () {
    initializeLoginSignupModal();
});

function initializeLoginSignupModal() {
    const loginContainer = document.getElementById('loginFormContainer');
    const signupContainer = document.getElementById('signupFormContainer');
    const loginFooter = document.getElementById('loginFooter');
    const signupFooter = document.getElementById('signupFooter');
    const modalTitle = document.getElementById('loginModalLabel');
    const showSignupBtn = document.getElementById('showSignupForm');
    const showLoginBtn = document.getElementById('showLoginForm');

    // Check if modal elements exist
    if (!loginContainer || !signupContainer) {
        console.log('Login/Signup modal elements not found');
        return;
    }

    // Function to show signup form
    function showSignupForm() {
        loginContainer.style.display = 'none';
        signupContainer.style.display = 'block';
        loginFooter.style.display = 'none';
        signupFooter.style.display = 'block';
        modalTitle.textContent = 'Create Your Account';

        // Reset signup form tabs to client
        resetTabsToDefault('signup');
    }

    // Function to show login form
    function showLoginForm() {
        loginContainer.style.display = 'block';
        signupContainer.style.display = 'none';
        loginFooter.style.display = 'block';
        signupFooter.style.display = 'none';
        modalTitle.textContent = 'Log in to Ultimate Hoopers';

        // Reset login form tabs to client
        resetTabsToDefault('login');
    }

    // Function to reset tabs to default (client tab active)
    function resetTabsToDefault(formType) {
        if (formType === 'signup') {
            const clientTab = document.getElementById('signup-client-tab');
            const clientPane = document.getElementById('client-signup');
            const adminTab = document.getElementById('signup-admin-tab');
            const adminPane = document.getElementById('admin-signup');

            if (clientTab && adminTab && clientPane && adminPane) {
                clientTab.classList.add('active');
                clientTab.setAttribute('aria-selected', 'true');
                adminTab.classList.remove('active');
                adminTab.setAttribute('aria-selected', 'false');

                clientPane.classList.add('show', 'active');
                adminPane.classList.remove('show', 'active');
            }
        } else if (formType === 'login') {
            const clientTab = document.getElementById('client-tab');
            const clientPane = document.getElementById('client-login');
            const adminTab = document.getElementById('admin-tab');
            const adminPane = document.getElementById('admin-login');

            if (clientTab && adminTab && clientPane && adminPane) {
                clientTab.classList.add('active');
                clientTab.setAttribute('aria-selected', 'true');
                adminTab.classList.remove('active');
                adminTab.setAttribute('aria-selected', 'false');

                clientPane.classList.add('show', 'active');
                adminPane.classList.remove('show', 'active');
            }
        }
    }

    // Event listeners for form switching
    if (showSignupBtn) {
        showSignupBtn.addEventListener('click', function (e) {
            e.preventDefault();
            showSignupForm();
        });
    }

    if (showLoginBtn) {
        showLoginBtn.addEventListener('click', function (e) {
            e.preventDefault();
            showLoginForm();
        });
    }

    // Password confirmation validation
    setupPasswordValidation('clientSignupPassword', 'clientSignupConfirmPassword');
    setupPasswordValidation('adminSignupPassword', 'adminSignupConfirmPassword');

    // Email domain validation for admin signup
    setupAdminEmailValidation();

    // Reset forms and show login when modal is closed
    const modal = document.getElementById('loginModal');
    if (modal) {
        modal.addEventListener('hidden.bs.modal', function () {
            showLoginForm();
            resetAllForms();
        });
    }

    console.log('✅ Login/Signup modal initialized successfully');
}

// Password validation function
function setupPasswordValidation(passwordId, confirmPasswordId) {
    const password = document.getElementById(passwordId);
    const confirmPassword = document.getElementById(confirmPasswordId);

    if (!password || !confirmPassword) return;

    function validatePassword() {
        if (password.value !== confirmPassword.value) {
            confirmPassword.setCustomValidity("Passwords don't match");
            confirmPassword.classList.add('is-invalid');

            // Add/update error message
            let errorMsg = confirmPassword.parentNode.querySelector('.password-error');
            if (!errorMsg) {
                errorMsg = document.createElement('div');
                errorMsg.className = 'password-error text-danger small mt-1';
                confirmPassword.parentNode.appendChild(errorMsg);
            }
            errorMsg.textContent = "Passwords don't match";
        } else {
            confirmPassword.setCustomValidity('');
            confirmPassword.classList.remove('is-invalid');

            // Remove error message
            const errorMsg = confirmPassword.parentNode.querySelector('.password-error');
            if (errorMsg) {
                errorMsg.remove();
            }
        }
    }

    password.addEventListener('input', validatePassword);
    confirmPassword.addEventListener('input', validatePassword);
}

// Admin email validation function
function setupAdminEmailValidation() {
    const adminEmailInput = document.getElementById('adminSignupEmail');
    if (!adminEmailInput) return;

    adminEmailInput.addEventListener('blur', function () {
        const email = this.value.toLowerCase();
        const commonPersonalDomains = ['gmail.com', 'yahoo.com', 'hotmail.com', 'outlook.com', 'aol.com', 'icloud.com'];
        const domain = email.split('@')[1];

        if (domain && commonPersonalDomains.includes(domain)) {
            this.setCustomValidity('Please use your business email address');
            this.classList.add('is-invalid');

            // Add/update error message
            let errorMsg = this.parentNode.querySelector('.email-error');
            if (!errorMsg) {
                errorMsg = document.createElement('div');
                errorMsg.className = 'email-error text-danger small mt-1';
                this.parentNode.appendChild(errorMsg);
            }
            errorMsg.textContent = 'Please use your business email address';
        } else {
            this.setCustomValidity('');
            this.classList.remove('is-invalid');

            // Remove error message
            const errorMsg = this.parentNode.querySelector('.email-error');
            if (errorMsg) {
                errorMsg.remove();
            }
        }
    });
}

// Reset all forms function
function resetAllForms() {
    const modal = document.getElementById('loginModal');
    if (!modal) return;

    // Reset all forms
    const forms = modal.querySelectorAll('form');
    forms.forEach(form => {
        form.reset();

        // Remove validation classes and error messages
        const invalidInputs = form.querySelectorAll('.is-invalid');
        invalidInputs.forEach(input => {
            input.classList.remove('is-invalid');
            input.setCustomValidity('');
        });

        // Remove custom error messages
        const errorMessages = form.querySelectorAll('.password-error, .email-error');
        errorMessages.forEach(msg => msg.remove());
    });
}

// Utility function to show specific form programmatically
window.showLoginModal = function (formType = 'login', userType = 'client') {
    const modal = document.getElementById('loginModal');
    if (!modal) return;

    const modalInstance = bootstrap.Modal.getInstance(modal) || new bootstrap.Modal(modal);
    modalInstance.show();

    // Wait for modal to be shown, then switch to desired form
    modal.addEventListener('shown.bs.modal', function () {
        if (formType === 'signup') {
            document.getElementById('showSignupForm')?.click();

            // Switch to correct user type tab
            setTimeout(() => {
                if (userType === 'admin') {
                    document.getElementById('signup-admin-tab')?.click();
                }
            }, 100);
        } else {
            document.getElementById('showLoginForm')?.click();

            // Switch to correct user type tab
            setTimeout(() => {
                if (userType === 'admin') {
                    document.getElementById('admin-tab')?.click();
                }
            }, 100);
        }
    }, { once: true });
};

// Enhanced form submission handling with better loading states
document.addEventListener('DOMContentLoaded', function () {
    const modal = document.getElementById('loginModal');
    if (!modal) return;

    // Add loading states to all form submissions
    const forms = modal.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function (e) {
            const submitBtn = form.querySelector('button[type="submit"]');
            if (submitBtn) {
                const originalText = submitBtn.textContent;
                const isSignup = submitBtn.textContent.includes('Create');

                submitBtn.disabled = true;
                submitBtn.innerHTML = `
                    <span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                    ${isSignup ? 'Creating account...' : 'Signing in...'}
                `;

                // Store original text for reset
                submitBtn.dataset.originalText = originalText;
            }
        });
    });
});

console.log('🔐 Login/Signup modal handler loaded');