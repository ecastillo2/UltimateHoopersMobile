/**
 * Enhanced AJAX Save Implementation for Run Edit
 * Includes spinner, toast notifications, validation, and error handling
 */

document.addEventListener('DOMContentLoaded', function () {
    // Helper functions
    function getAntiForgeryToken() {
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (!token) {
            console.error('Anti-forgery token not found');
            throw new Error('Security token is missing');
        }
        return token;
    }

    function showLoading() {
        const spinner = document.querySelector('.loading-spinner');
        if (spinner) {
            spinner.classList.add('active');
        }
    }

    function hideLoading() {
        const spinner = document.querySelector('.loading-spinner');
        if (spinner) {
            spinner.classList.remove('active');
        }
    }

    function showToast(title, message, type = 'success') {
        console.log(`${type.toUpperCase()}: ${title} - ${message}`);

        // Create toast container if it doesn't exist
        let container = document.querySelector('.toast-container');
        if (!container) {
            container = document.createElement('div');
            container.className = 'toast-container position-fixed bottom-0 end-0 p-3';
            container.style.zIndex = '9999';
            document.body.appendChild(container);
        }

        // Create toast
        const toastId = `toast-${Date.now()}`;
        const iconClass = {
            'success': 'bi-check-circle',
            'error': 'bi-exclamation-triangle',
            'warning': 'bi-exclamation-triangle',
            'info': 'bi-info-circle'
        }[type] || 'bi-info-circle';

        const bgClass = {
            'success': 'bg-success',
            'error': 'bg-danger',
            'warning': 'bg-warning',
            'info': 'bg-info'
        }[type] || 'bg-success';

        const toastHtml = `
            <div id="${toastId}" class="toast align-items-center text-white ${bgClass} border-0" role="alert">
                <div class="d-flex">
                    <div class="toast-body">
                        <i class="bi ${iconClass} me-2"></i>
                        <strong>${title}:</strong> ${message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                </div>
            </div>
        `;

        container.insertAdjacentHTML('beforeend', toastHtml);

        // Initialize and show toast
        const toastElement = document.getElementById(toastId);
        if (toastElement && typeof bootstrap !== 'undefined' && bootstrap.Toast) {
            const toast = new bootstrap.Toast(toastElement, {
                autohide: type !== 'error',
                delay: type === 'error' ? 8000 : 5000
            });
            toast.show();

            toastElement.addEventListener('hidden.bs.toast', () => {
                toastElement.remove();
            });
        }
    }

    function showButtonSpinner(button, loadingText = 'Saving...') {
        if (!button) return null;

        const originalContent = button.innerHTML;
        button.disabled = true;
        button.innerHTML = `
            <span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
            ${loadingText}
        `;

        return originalContent;
    }

    function hideButtonSpinner(button, originalContent) {
        if (!button || !originalContent) return;

        button.disabled = false;
        button.innerHTML = originalContent;
    }

    function validateRunForm(formData) {
        const errors = [];

        // Required field validation
        const requiredFields = [
            { key: 'RunId', name: 'Run ID' },
            { key: 'Name', name: 'Run Name' },
            { key: 'Description', name: 'Description' },
            { key: 'RunDate', name: 'Run Date' },
            { key: 'StartTime', name: 'Start Time' },
            { key: 'PlayerLimit', name: 'Player Limit' }
        ];

        requiredFields.forEach(field => {
            const value = formData.get(field.key);
            if (!value || value.trim() === '') {
                errors.push(`${field.name} is required`);
            }
        });

        // Date validation
        const runDate = formData.get('RunDate');
        if (runDate) {
            const selectedDate = new Date(runDate);
            const today = new Date();
            today.setHours(0, 0, 0, 0);

            if (selectedDate < today) {
                errors.push('Run date cannot be in the past');
            }
        }

        // Player limit validation
        const playerLimit = parseInt(formData.get('PlayerLimit'));
        if (isNaN(playerLimit) || playerLimit < 2 || playerLimit > 50) {
            errors.push('Player limit must be between 2 and 50');
        }

        // Time validation
        const startTime = formData.get('StartTime');
        const endTime = formData.get('EndTime');
        if (startTime && endTime) {
            const start = new Date(`2000-01-01T${startTime}`);
            const end = new Date(`2000-01-01T${endTime}`);

            if (end <= start) {
                errors.push('End time must be after start time');
            }
        }

        return errors;
    }

    function convertFormDataToRunObject(formData) {
        // Convert FormData to Run object matching the server model
        const runData = {
            RunId: formData.get('RunId'),
            Name: formData.get('Name')?.trim(),
            Description: formData.get('Description')?.trim(),
            RunDate: formData.get('RunDate'),
            StartTime: formData.get('StartTime'),
            EndTime: formData.get('EndTime') || null,
            PlayerLimit: parseInt(formData.get('PlayerLimit')) || 10,
            SkillLevel: formData.get('SkillLevel') || 'Intermediate',
            Status: formData.get('Status') || 'Active',
            Type: formData.get('Type') || 'Pickup',
            TeamType: formData.get('TeamType') || 'Individual',
            IsPublic: formData.get('IsPublic') === 'true',
            Address: formData.get('Address')?.trim() || null,
            City: formData.get('City')?.trim() || null,
            State: formData.get('State')?.trim() || null,
            Zip: formData.get('Zip')?.trim() || null,
            CourtId: formData.get('CourtId') || null,
            ClientId: formData.get('ClientId') || null,
            IsOutdoor: formData.get('IsOutdoor') === 'true'
        };

        // Log the data being sent for debugging
        console.log('🚀 Sending run data:', runData);

        return runData;
    }

    async function saveRunWithAjax(formData, saveButton) {
        console.log('💾 Starting AJAX save process...');

        let originalButtonContent = null;

        try {
            // Show loading states
            showLoading();
            originalButtonContent = showButtonSpinner(saveButton, 'Saving Changes...');

            // Validate form data
            const validationErrors = validateRunForm(formData);
            if (validationErrors.length > 0) {
                throw new Error(`Validation failed: ${validationErrors.join(', ')}`);
            }

            // Convert form data to run object
            const runData = convertFormDataToRunObject(formData);

            // Get anti-forgery token
            const token = getAntiForgeryToken();

            // Make AJAX request
            console.log('📡 Making AJAX request to /Run/Edit...');

            const response = await fetch('/Run/Edit', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token,
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify(runData)
            });

            console.log('📨 Response status:', response.status);

            // Check if response is ok
            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`HTTP ${response.status}: ${response.statusText}. ${errorText}`);
            }

            // Parse JSON response
            const result = await response.json();
            console.log('📦 Response data:', result);

            // Handle response
            if (result.success) {
                // Success case
                showToast('Success', result.message || 'Run updated successfully!', 'success');

                // Close modal after a short delay
                setTimeout(() => {
                    const editModal = bootstrap.Modal.getInstance(document.getElementById('editRunModal'));
                    if (editModal) {
                        editModal.hide();

                        // Refresh the page or DataTable after modal closes
                        setTimeout(() => {
                            refreshRunsDisplay();
                        }, 500);
                    }
                }, 1500);

            } else {
                // Server returned error
                throw new Error(result.message || 'Server returned an error');
            }

        } catch (error) {
            console.error('❌ Error saving run:', error);

            // Show error toast
            let errorMessage = 'An unexpected error occurred while saving the run.';

            if (error.message.includes('Validation failed')) {
                errorMessage = error.message.replace('Validation failed: ', '');
            } else if (error.message.includes('Security token')) {
                errorMessage = 'Security validation failed. Please refresh the page and try again.';
            } else if (error.message.includes('HTTP')) {
                errorMessage = 'Server error occurred. Please try again later.';
            } else if (error.message) {
                errorMessage = error.message;
            }

            showToast('Error', errorMessage, 'error');

        } finally {
            // Always hide loading states
            hideLoading();
            hideButtonSpinner(saveButton, originalButtonContent);
        }
    }

    function refreshRunsDisplay() {
        console.log('🔄 Refreshing runs display...');

        // Try to refresh DataTable if it exists
        if (window.runsDataTable && $.fn.dataTable.isDataTable('#runsTable')) {
            try {
                console.log('📊 Refreshing DataTable...');
                window.runsDataTable.ajax.reload(null, false); // false = don't reset paging
                showToast('Success', 'Runs list updated successfully!', 'info');
                return;
            } catch (e) {
                console.warn('⚠️ DataTable refresh failed:', e);
            }
        }

        // Fallback to page reload
        console.log('🔄 Performing page reload...');
        window.location.reload();
    }

    function initializeFormValidation() {
        // Add real-time validation to form fields
        const form = document.getElementById('editRunForm');
        if (!form) return;

        // Add Bootstrap validation classes
        const inputs = form.querySelectorAll('input[required], select[required], textarea[required]');
        inputs.forEach(input => {
            input.addEventListener('blur', function () {
                if (this.checkValidity()) {
                    this.classList.remove('is-invalid');
                    this.classList.add('is-valid');
                } else {
                    this.classList.remove('is-valid');
                    this.classList.add('is-invalid');
                }
            });

            input.addEventListener('input', function () {
                if (this.classList.contains('is-invalid') && this.checkValidity()) {
                    this.classList.remove('is-invalid');
                    this.classList.add('is-valid');
                }
            });
        });

        // Date validation
        const dateInput = document.getElementById('editRunDate');
        if (dateInput) {
            const today = new Date().toISOString().split('T')[0];
            dateInput.min = today;
        }

        // Player limit validation
        const playerLimitInput = document.getElementById('editMaxParticipants');
        if (playerLimitInput) {
            playerLimitInput.addEventListener('input', function () {
                const value = parseInt(this.value);
                if (value < 2 || value > 50) {
                    this.setCustomValidity('Player limit must be between 2 and 50');
                } else {
                    this.setCustomValidity('');
                }
            });
        }

        // Time validation
        const startTimeInput = document.getElementById('editRunTime');
        const endTimeInput = document.getElementById('editEndTime');

        if (startTimeInput && endTimeInput) {
            function validateTimes() {
                if (startTimeInput.value && endTimeInput.value) {
                    const start = new Date(`2000-01-01T${startTimeInput.value}`);
                    const end = new Date(`2000-01-01T${endTimeInput.value}`);

                    if (end <= start) {
                        endTimeInput.setCustomValidity('End time must be after start time');
                    } else {
                        endTimeInput.setCustomValidity('');
                    }
                }
            }

            startTimeInput.addEventListener('change', validateTimes);
            endTimeInput.addEventListener('change', validateTimes);
        }
    }

    // Main form submission handler
    function initializeFormSubmission() {
        const editRunForm = document.getElementById('editRunForm');
        if (!editRunForm) {
            console.warn('Edit run form not found');
            return;
        }

        editRunForm.addEventListener('submit', async function (e) {
            e.preventDefault(); // Prevent default form submission
            e.stopPropagation();

            console.log('📝 Form submitted, starting validation and save process...');

            // Find save button
            const saveButton = this.querySelector('button[type="submit"]');

            // Client-side validation
            if (!this.checkValidity()) {
                this.classList.add('was-validated');
                showToast('Validation Error', 'Please fill in all required fields correctly.', 'error');
                return;
            }

            // Get form data
            const formData = new FormData(this);

            // Log form data for debugging
            console.log('📋 Form data entries:');
            for (const [key, value] of formData.entries()) {
                console.log(`  ${key}: ${value}`);
            }

            // Save with AJAX
            await saveRunWithAjax(formData, saveButton);
        });

        console.log('✅ Form submission handler initialized');
    }

    // Initialize everything
    function initialize() {
        console.log('🚀 Initializing enhanced AJAX save functionality...');

        try {
            initializeFormValidation();
            initializeFormSubmission();
            console.log('✅ Enhanced AJAX save functionality initialized successfully');
        } catch (error) {
            console.error('❌ Error initializing AJAX save functionality:', error);
            showToast('Initialization Error', 'Some features may not work correctly.', 'warning');
        }
    }

    // Start initialization
    initialize();

    // Expose functions for debugging/testing
    window.runEditAjax = {
        saveRunWithAjax,
        showToast,
        validateRunForm,
        convertFormDataToRunObject,
        refreshRunsDisplay
    };

    console.log('🎯 Enhanced AJAX save module loaded successfully');
});

// CSS for loading spinner (add to your CSS file or style section)
const spinnerCSS = `
.loading-spinner {
    display: none;
    position: fixed;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    z-index: 9999;
    background: rgba(255, 255, 255, 0.95);
    padding: 30px;
    border-radius: 15px;
    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
    text-align: center;
    min-width: 200px;
}

.loading-spinner.active {
    display: block;
}

.loading-spinner .spinner-border {
    width: 3rem;
    height: 3rem;
}

.toast-container {
    z-index: 10000;
}

.form-control.is-valid {
    border-color: #198754;
    background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 8 8'%3e%3cpath fill='%23198754' d='m2.3 6.73.6-.6L4 7.5l1.1-1.37.6.6L4 8.5z'/%3e%3c/svg%3e");
}

.form-control.is-invalid {
    border-color: #dc3545;
    background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 12 12' width='12' height='12' fill='none' stroke='%23dc3545'%3e%3ccircle cx='6' cy='6' r='4.5'/%3e%3cpath d='m5.8 4.6 1.4 1.4m0-1.4-1.4 1.4'/%3e%3c/svg%3e");
}
`;

// Inject CSS if not already present
if (!document.querySelector('#run-edit-ajax-styles')) {
    const styleEl = document.createElement('style');
    styleEl.id = 'run-edit-ajax-styles';
    styleEl.textContent = spinnerCSS;
    document.head.appendChild(styleEl);
}