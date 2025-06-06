/**
 * Enhanced Login System with Remember Me Functionality
 * Handles login form interactions and remember me features
 */

document.addEventListener('DOMContentLoaded', function () {
    console.log('🔐 Enhanced Login System Initializing...');

    initializeRememberMeFeatures();
    initializeLoginFormHandlers();
    checkRememberMeStatus();

    console.log('✅ Enhanced Login System Ready');
});

function initializeRememberMeFeatures() {
    const rememberMeCheckboxes = document.querySelectorAll('input[name="rememberMe"]');

    rememberMeCheckboxes.forEach(checkbox => {
        // Add visual feedback
        checkbox.addEventListener('change', function () {
            const label = document.querySelector(`label[for="${this.id}"]`);
            if (label) {
                if (this.checked) {
                    label.style.fontWeight = 'bold';
                    label.style.color = '#342580';
                    showRememberMeInfo(this);
                } else {
                    label.style.fontWeight = 'normal';
                    label.style.color = '';
                    hideRememberMeInfo(this);
                }
            }
        });

        // Add tooltip
        checkbox.setAttribute('title', 'Keep me signed in for 30 days on this device');
    });
}

function showRememberMeInfo(checkbox) {
    // Remove any existing info
    hideRememberMeInfo(checkbox);

    const infoDiv = document.createElement('div');
    infoDiv.className = 'remember-me-info text-muted small mt-1';
    infoDiv.innerHTML = '<i class="bi bi-info-circle me-1"></i>You\'ll stay logged in for 30 days on this device';

    const parent = checkbox.closest('.form-check');
    if (parent) {
        parent.appendChild(infoDiv);
    }
}

function hideRememberMeInfo(checkbox) {
    const parent = checkbox.closest('.form-check');
    if (parent) {
        const existingInfo = parent.querySelector('.remember-me-info');
        if (existingInfo) {
            existingInfo.remove();
        }
    }
}

function initializeLoginFormHandlers() {
    const loginForms = document.querySelectorAll('#loginModal form');

    loginForms.forEach(form => {
        form.addEventListener('submit', function (e) {
            const rememberMeCheckbox = form.querySelector('input[name="rememberMe"]');
            const submitButton = form.querySelector('button[type="submit"]');

            if (rememberMeCheckbox && rememberMeCheckbox.checked) {
                console.log('🔐 Remember me enabled for this login');

                // Visual feedback for remember me
                if (submitButton) {
                    const originalText = submitButton.textContent;
                    submitButton.innerHTML = `
                        <span class="spinner-border spinner-border-sm me-2" role="status"></span>
                        Signing in & remembering...
                    `;
                }
            }
        });
    });
}

function checkRememberMeStatus() {
    // Check if user has persistent cookies
    const rememberMeCookie = getCookie('UH_RememberMe');
    const userDataCookie = getCookie('UH_UserData');

    if (rememberMeCookie && userDataCookie) {
        console.log('🔐 Remember me cookies found');

        // Add visual indicator that user is remembered
        addRememberedUserIndicator();
    }
}

function addRememberedUserIndicator() {
    const loginButton = document.querySelector('button[data-bs-target="#loginModal"]');
    if (loginButton && !document.querySelector('.remembered-user-badge')) {
        const badge = document.createElement('span');
        badge.className = 'remembered-user-badge badge bg-success ms-1';
        badge.innerHTML = '<i class="bi bi-check-circle"></i>';
        badge.title = 'You have an active remember me session';
        badge.style.fontSize = '0.7rem';

        loginButton.appendChild(badge);
    }
}

// Utility function to get cookie value
function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
    return null;
}

// Enhanced logout functionality
function enhancedLogout() {
    // Clear remember me status
    const rememberedUserBadge = document.querySelector('.remembered-user-badge');
    if (rememberedUserBadge) {
        rememberedUserBadge.remove();
    }

    console.log('🔐 Enhanced logout completed');
}

// Export functions for global use
window.LoginSystem = {
    showRememberMeInfo,
    hideRememberMeInfo,
    checkRememberMeStatus,
    enhancedLogout
};

// Auto-check remember me for returning users (optional)
document.addEventListener('DOMContentLoaded', function () {
    const rememberMeCookie = getCookie('UH_RememberMe');
    if (rememberMeCookie) {
        // Pre-check remember me boxes for convenience
        const rememberMeCheckboxes = document.querySelectorAll('input[name="rememberMe"]');
        rememberMeCheckboxes.forEach(checkbox => {
            checkbox.checked = true;
            checkbox.dispatchEvent(new Event('change'));
        });
    }
});

console.log('🔐 Enhanced Login with Remember Me loaded');