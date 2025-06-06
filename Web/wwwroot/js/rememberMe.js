/**
 * Remember Me Client-Side Handler
 * Manages remember me functionality on the frontend
 */

document.addEventListener('DOMContentLoaded', function () {
    initializeRememberMeHandlers();
});

function initializeRememberMeHandlers() {
    console.log('🔐 Initializing Remember Me handlers');

    // Check if user has remember me cookies and show appropriate UI feedback
    checkRememberMeStatus();

    // Add event listeners to remember me checkboxes
    attachRememberMeListeners();

    // Add form submission handlers
    attachFormSubmissionHandlers();

    console.log('✅ Remember Me handlers initialized');
}

function checkRememberMeStatus() {
    // Check if user has active remember me cookies
    const hasRememberMe = document.cookie.includes('RememberMe_Active=true');

    if (hasRememberMe) {
        console.log('🍪 Remember Me cookies detected');

        // Show a subtle indicator that the user was remembered
        showRememberMeIndicator();

        // Pre-check the remember me boxes if user opens login modal
        preCheckRememberMeBoxes();
    }
}

function showRememberMeIndicator() {
    // Add a small indicator to the login button if user is remembered
    const loginButton = document.querySelector('button[data-bs-target="#loginModal"]');
    if (loginButton && !loginButton.querySelector('.remember-indicator')) {
        const indicator = document.createElement('small');
        indicator.className = 'remember-indicator text-muted ms-1';
        indicator.textContent = '(Remembered)';
        indicator.style.fontSize = '0.75em';
        loginButton.appendChild(indicator);
    }
}

function preCheckRememberMeBoxes() {
    // Pre-check remember me checkboxes when modal opens
    const loginModal = document.getElementById('loginModal');
    if (loginModal) {
        loginModal.addEventListener('shown.bs.modal', function () {
            const rememberMeBoxes = loginModal.querySelectorAll('input[name="rememberMe"]');
            rememberMeBoxes.forEach(checkbox => {
                checkbox.checked = true;
            });
        });
    }
}

function attachRememberMeListeners() {
    // Add change listeners to remember me checkboxes
    const rememberMeCheckboxes = document.querySelectorAll('input[name="rememberMe"]');

    rememberMeCheckboxes.forEach(checkbox => {
        checkbox.addEventListener('change', function () {
            const isChecked = this.checked;
            const loginType = this.id.includes('Client') ? 'Client' : 'Admin';

            console.log(`${loginType} Remember Me: ${isChecked ? 'Checked' : 'Unchecked'}`);

            // Update the label text to reflect the duration
            const label = document.querySelector(`label[for="${this.id}"]`);
            if (label) {
                if (isChecked) {
                    label.innerHTML = 'Remember me for 30 days <small class="text-muted">(Keep me logged in)</small>';
                } else {
                    label.textContent = 'Remember me for 30 days';
                }
            }

            // Show/hide additional info about remember me
            showRememberMeInfo(this, isChecked);
        });
    });
}

function showRememberMeInfo(checkbox, isChecked) {
    // Remove existing info
    const existingInfo = checkbox.closest('.form-check').querySelector('.remember-me-info');
    if (existingInfo) {
        existingInfo.remove();
    }

    if (isChecked) {
        // Add information about what remember me does
        const infoDiv = document.createElement('div');
        infoDiv.className = 'remember-me-info mt-2 p-2 bg-light rounded';
        infoDiv.innerHTML = `
            <small class="text-muted">
                <i class="bi bi-info-circle me-1"></i>
                Your login will be saved securely for 30 days. You can disable this anytime from your account settings.
            </small>
        `;

        checkbox.closest('.form-check').appendChild(infoDiv);

        // Auto-hide after 5 seconds
        setTimeout(() => {
            if (infoDiv.parentNode) {
                infoDiv.style.transition = 'opacity 0.5s ease';
                infoDiv.style.opacity = '0';
                setTimeout(() => {
                    if (infoDiv.parentNode) {
                        infoDiv.remove();
                    }
                }, 500);
            }
        }, 5000);
    }
}

function attachFormSubmissionHandlers() {
    // Enhance form submission to show better feedback for remember me
    const loginForms = document.querySelectorAll('#loginModal form');

    loginForms.forEach(form => {
        form.addEventListener('submit', function (e) {
            const rememberMeCheckbox = form.querySelector('input[name="rememberMe"]');
            const submitButton = form.querySelector('button[type="submit"]');

            if (rememberMeCheckbox && rememberMeCheckbox.checked && submitButton) {
                // Update button text to reflect remember me action
                const originalText = submitButton.textContent;
                const loginType = originalText.includes('Client') ? 'Client' : 'Admin';

                // Store original text for potential reset
                submitButton.dataset.originalText = originalText;

                // Show loading state with remember me message
                setTimeout(() => {
                    if (submitButton.querySelector('.spinner-border')) {
                        const loadingText = submitButton.querySelector('.spinner-border').nextSibling;
                        if (loadingText) {
                            loadingText.textContent = ` Signing in and remembering...`;
                        }
                    }
                }, 100);
            }
        });
    });
}

// Utility functions for managing remember me cookies
window.RememberMeUtils = {

    // Check if remember me is active
    isActive: function () {
        return document.cookie.includes('RememberMe_Active=true');
    },

    // Get remember me expiration date
    getExpirationDate: function () {
        const cookies = document.cookie.split(';');
        for (let cookie of cookies) {
            if (cookie.trim().startsWith('RememberMe_Active=')) {
                // Note: We can't actually read the expiration date from JavaScript
                // This would need to be stored separately if needed
                return new Date(Date.now() + (30 * 24 * 60 * 60 * 1000)); // 30 days from now
            }
        }
        return null;
    },

    // Clear remember me (for manual logout)
    clear: function () {
        // This would be called when user manually logs out
        // The server-side logout will handle clearing the cookies
        console.log('Clearing remember me status');

        // Remove any UI indicators
        const indicator = document.querySelector('.remember-indicator');
        if (indicator) {
            indicator.remove();
        }
    },

    // Show remember me status in UI
    showStatus: function () {
        if (this.isActive()) {
            const expirationDate = this.getExpirationDate();
            if (expirationDate) {
                const daysLeft = Math.ceil((expirationDate - new Date()) / (1000 * 60 * 60 * 24));
                console.log(`Remember Me active: ${daysLeft} days remaining`);

                // You could show this in the UI if desired
                return `Active (${daysLeft} days remaining)`;
            }
        }
        return 'Not active';
    }
};

// Enhanced login modal behavior for remember me
document.addEventListener('DOMContentLoaded', function () {
    const loginModal = document.getElementById('loginModal');
    if (!loginModal) return;

    // When modal is shown, check for remember me status
    loginModal.addEventListener('show.bs.modal', function () {
        const hasRememberMe = RememberMeUtils.isActive();

        if (hasRememberMe) {
            // Pre-fill remember me checkboxes
            const checkboxes = loginModal.querySelectorAll('input[name="rememberMe"]');
            checkboxes.forEach(checkbox => {
                checkbox.checked = true;

                // Trigger change event to show info
                const event = new Event('change', { bubbles: true });
                checkbox.dispatchEvent(event);
            });
        }
    });

    // When modal is hidden, clean up any temporary UI elements
    loginModal.addEventListener('hidden.bs.modal', function () {
        const rememberMeInfos = loginModal.querySelectorAll('.remember-me-info');
        rememberMeInfos.forEach(info => info.remove());

        // Reset any modified label text
        const labels = loginModal.querySelectorAll('label[for*="remember"]');
        labels.forEach(label => {
            if (label.innerHTML.includes('<small')) {
                label.textContent = 'Remember me for 30 days';
            }
        });
    });
});

// Add CSS for remember me styling
const rememberMeCSS = `
<style>
.remember-me-info {
    animation: slideDown 0.3s ease-out;
    border-left: 3px solid var(--primary-color, #ff6238);
}

@keyframes slideDown {
    from {
        opacity: 0;
        transform: translateY(-10px);
    }
    to {
        opacity: 1;
        transform: translateY(0);
    }
}

.remember-indicator {
    font-weight: normal !important;
    opacity: 0.8;
}

.form-check-input:checked + .form-check-label {
    font-weight: 500;
}

/* Enhance remember me checkbox styling */
input[name="rememberMe"]:checked {
    background-color: var(--primary-color, #ff6238);
    border-color: var(--primary-color, #ff6238);
}

input[name="rememberMe"]:focus {
    border-color: var(--primary-color, #ff6238);
    outline: 0;
    box-shadow: 0 0 0 0.25rem rgba(255, 98, 56, 0.25);
}

/* Loading state for remember me forms */
.btn-loading-remember {
    position: relative;
}

.btn-loading-remember::after {
    content: " (Remembering...)";
    font-weight: normal;
    opacity: 0.8;
}
</style>
`;

// Inject CSS if not already present
if (!document.getElementById('remember-me-styles')) {
    const styleDiv = document.createElement('div');
    styleDiv.id = 'remember-me-styles';
    styleDiv.innerHTML = rememberMeCSS;
    document.head.appendChild(styleDiv);
}

// Debug functions for testing
window.RememberMeDebug = {

    testRememberMe: function () {
        console.log('🧪 Testing Remember Me functionality');
        console.log('Is Active:', RememberMeUtils.isActive());
        console.log('Status:', RememberMeUtils.showStatus());
        console.log('Cookies:', document.cookie);
    },

    simulateRememberMe: function () {
        console.log('🎭 Simulating Remember Me state');
        // This would normally be set by the server
        document.cookie = 'RememberMe_Active=true; path=/; max-age=' + (30 * 24 * 60 * 60);
        console.log('Remember Me simulated - reload page to see effect');
    },

    clearRememberMe: function () {
        console.log('🧹 Clearing Remember Me cookies');
        document.cookie = 'RememberMe_Active=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT';
        RememberMeUtils.clear();
        console.log('Remember Me cleared');
    }
};

console.log('🔐 Remember Me client-side handler loaded');
console.log('🧪 Test functions: window.RememberMeDebug.testRememberMe()');
console.log('🎭 Simulate: window.RememberMeDebug.simulateRememberMe()');
console.log('🧹 Clear: window.RememberMeDebug.clearRememberMe()');