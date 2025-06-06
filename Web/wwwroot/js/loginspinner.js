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
        console.log('🟣 Showing simple spinner over login form:', message);

        // FORCE modal spinner - ignore global spinner completely for login
        hideSimpleSpinner(); // Clear any existing spinners first
        createModalSpinner(message);
    }

    function createModalSpinner(message) {
        // Remove any existing spinner first
        hideSimpleSpinner();

        const loginModal = document.getElementById('loginModal');
        const modalContent = loginModal?.querySelector('.modal-content');

        console.log('🔍 Modal found:', !!loginModal);
        console.log('🔍 Modal content found:', !!modalContent);

        if (!modalContent) {
            console.log('❌ Modal content not found, falling back to full screen');
            createFullScreenSpinner(message);
            return;
        }

        const spinner = document.createElement('div');
        spinner.id = 'simpleLoginSpinner';
        // Target the entire modal content, not just body
        spinner.style.cssText = `
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(255, 255, 255, 0.95);
            display: flex;
            align-items: center;
            justify-content: center;
            z-index: 99999;
            flex-direction: column;
            border-radius: 0.375rem;
            backdrop-filter: blur(2px);
        `;

        spinner.innerHTML = `
            <div class="spinner-border text-primary" style="width: 3rem; height: 3rem;" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
            <div class="text-dark mt-3 fw-bold" style="font-size: 1.1rem;">
                ${message}
            </div>
        `;

        // Force the modal content to be relative positioned
        modalContent.style.position = 'relative';
        modalContent.appendChild(spinner);

        console.log('✅ MODAL CONTENT SPINNER created - should cover entire modal');
        console.log('📍 Spinner parent:', modalContent);
        console.log('📍 Modal content position:', window.getComputedStyle(modalContent).position);
    }

    function createFullScreenSpinner(message) {
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
        console.log('✅ Fallback full screen spinner created');
    }

    function hideSimpleSpinner() {
        console.log('🟣 Hiding all spinners...');

        // Hide ANY global spinner that might exist
        if (window.hideSpinner && typeof window.hideSpinner === 'function') {
            try {
                window.hideSpinner();
                console.log('✅ Global spinner hidden');
            } catch (e) {
                console.log('⚠️ Error hiding global spinner:', e);
            }
        }

        // Hide our modal spinner (check multiple locations)
        const modalSpinners = document.querySelectorAll('#simpleLoginSpinner');
        modalSpinners.forEach((spinner, index) => {
            spinner.remove();
            console.log(`✅ Modal spinner ${index + 1} removed`);
        });

        // Check modal content specifically
        const loginModal = document.getElementById('loginModal');
        const modalContent = loginModal?.querySelector('.modal-content');
        if (modalContent) {
            const spinnersInModal = modalContent.querySelectorAll('#simpleLoginSpinner');
            spinnersInModal.forEach((spinner, index) => {
                spinner.remove();
                console.log(`✅ Modal content spinner ${index + 1} removed`);
            });
        }

        // Also check modal body for any leftover spinners
        const modalBody = loginModal?.querySelector('.modal-body');
        if (modalBody) {
            const spinnersInBody = modalBody.querySelectorAll('#simpleLoginSpinner');
            spinnersInBody.forEach((spinner, index) => {
                spinner.remove();
                console.log(`✅ Modal body spinner ${index + 1} removed`);
            });
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
        forceModalSpinner: function () {
            console.log('🚨 FORCING MODAL CONTENT SPINNER TEST');

            // Open modal if it's not open
            const loginModal = document.getElementById('loginModal');
            const modalInstance = bootstrap.Modal.getInstance(loginModal) || new bootstrap.Modal(loginModal);
            modalInstance.show();

            // Force modal spinner immediately
            setTimeout(() => {
                console.log('🔴 Creating spinner over ENTIRE MODAL CONTENT');
                createModalSpinner('FORCED TEST - Modal Content Spinner');

                setTimeout(() => {
                    hideSimpleSpinner();
                    console.log('✅ Forced test complete');
                }, 4000);
            }, 500);
        },

        testSpinner: function () {
            console.log('🧪 Testing modal spinner...');

            // Open modal if it's not open
            const loginModal = document.getElementById('loginModal');
            const modalInstance = bootstrap.Modal.getInstance(loginModal) || new bootstrap.Modal(loginModal);
            modalInstance.show();

            // Show spinner after modal is visible
            setTimeout(() => {
                showSimpleSpinner('Test modal spinner');
                setTimeout(() => {
                    hideSimpleSpinner();
                    console.log('✅ Modal spinner test complete');
                }, 3000);
            }, 500);
        },

        testFullScreenSpinner: function () {
            console.log('🧪 Testing full-screen spinner...');
            createFullScreenSpinner('Test full-screen spinner');
            setTimeout(() => {
                hideSimpleSpinner();
                console.log('✅ Full-screen spinner test complete');
            }, 3000);
        },

        checkSystem: function () {
            console.log('🔍 Checking login system...');
            console.log('Client form:', document.querySelector('#client-login form'));
            console.log('Admin form:', document.querySelector('#admin-login form'));
            console.log('Modal body:', document.querySelector('#loginModal .modal-body'));
            console.log('Modal body position:', window.getComputedStyle(document.querySelector('#loginModal .modal-body')).position);
            console.log('Global showSpinner:', typeof window.showSpinner);
            console.log('Global hideSpinner:', typeof window.hideSpinner);
            console.log('Any existing spinners:', document.querySelectorAll('[id*="pinner"]'));
        },

        reset: function () {
            console.log('🔄 Resetting...');
            hideSimpleSpinner();
            resetAllForms();
            console.log('✅ Reset complete');
        },

        testLogin: function (type = 'client') {
            console.log(`🧪 Testing ${type} login process...`);

            // Open modal if needed
            const loginModal = document.getElementById('loginModal');
            const modalInstance = bootstrap.Modal.getInstance(loginModal) || new bootstrap.Modal(loginModal);
            modalInstance.show();

            setTimeout(() => {
                const form = document.querySelector(`#${type}-login form`);
                const button = form.querySelector('button[type="submit"]');

                setButtonLoadingState(button, true, type);
                showSimpleSpinner(`Testing ${type} login...`);

                setTimeout(() => {
                    setButtonLoadingState(button, false, type);
                    hideSimpleSpinner();
                    console.log('✅ Test complete');
                }, 3000);
            }, 500);
        }
    };

    console.log('🐛 Debug functions:');
    console.log('  🚨 window.loginDebug.forceModalSpinner() - FORCE OBVIOUS RED SPINNER TEST');
    console.log('  window.loginDebug.testSpinner() - Test modal spinner');
    console.log('  window.loginDebug.testFullScreenSpinner() - Test full-screen spinner');
    console.log('  window.loginDebug.checkSystem() - Check system status');
    console.log('  window.loginDebug.testLogin("client") - Test login flow');
    console.log('  window.loginDebug.reset() - Reset everything');
    console.log('✅ Modal-focused login system ready');
});

// Add basic styling for modal spinner and quick errors
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
    backdrop-filter: blur(1px);
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
}

/* Ensure modal content can contain positioned elements */
#loginModal .modal-content {
    position: relative !important;
}

/* Prevent interaction with modal elements when spinner is active */
#loginModal .modal-content:has(#simpleLoginSpinner) .modal-header,
#loginModal .modal-content:has(#simpleLoginSpinner) .modal-body,
#loginModal .modal-content:has(#simpleLoginSpinner) .modal-footer {
    pointer-events: none;
    user-select: none;
}

/* Smooth spinner appearance */
#simpleLoginSpinner {
    animation: fadeIn 0.3s ease-out;
}

@keyframes fadeIn {
    from {
        opacity: 0;
        transform: scale(0.95);
    }
    to {
        opacity: 1;
        transform: scale(1);
    }
}

/* Make sure spinner is always on top */
#simpleLoginSpinner {
    z-index: 99999 !important;
}
</style>
`;

if (document.head && !document.getElementById('simple-login-styles')) {
    const styleEl = document.createElement('div');
    styleEl.id = 'simple-login-styles';
    styleEl.innerHTML = basicCSS;
    document.head.appendChild(styleEl);
}