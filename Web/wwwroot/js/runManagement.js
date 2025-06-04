/**
 * FIXED Run Management JavaScript - Custom Address Toggle
 * This fixes the checkbox and toggle functionality issues
 */

document.addEventListener('DOMContentLoaded', function () {
    console.log('🔧 Initializing Fixed Run Management System');

    // Global storage for address data
    window.originalClientAddress = null;
    window.customRunAddress = null;

    // Initialize all functionality
    initializeRunManagement();
    initializeCustomAddressFunctionality();
    initializeModalHandlers();
    initializeFormHandlers();

    console.log('✅ Fixed Run Management System loaded successfully');

    // ========== MAIN INITIALIZATION ==========
    function initializeRunManagement() {
        // Fix any existing edit buttons that might not have proper data attributes
        fixEditButtons();

        // Initialize enhanced run data loading
        enhanceExistingFunctions();
    }

    function fixEditButtons() {
        const editButtons = document.querySelectorAll('[data-bs-target="#editRunModal"]');
        console.log(`🔧 Found ${editButtons.length} edit buttons to fix`);

        editButtons.forEach((button, index) => {
            let runId = button.getAttribute('data-run-id');

            if (!runId) {
                const row = button.closest('tr');
                if (row) {
                    runId = row.getAttribute('data-run-id') ||
                        row.getAttribute('data-runid') ||
                        row.dataset.runId;

                    if (runId) {
                        button.setAttribute('data-run-id', runId);
                        console.log(`✅ Fixed button ${index + 1}: added data-run-id="${runId}"`);
                    }
                }
            }
        });
    }

    // ========== CUSTOM ADDRESS FUNCTIONALITY ==========
    function initializeCustomAddressFunctionality() {
        console.log('🏠 Initializing custom address functionality');

        // Get the custom address checkbox
        const customAddressCheckbox = document.getElementById('useCustomAddress');

        if (!customAddressCheckbox) {
            console.warn('⚠️ Custom address checkbox not found');
            return;
        }

        console.log('✅ Found custom address checkbox, attaching events');

        // Remove any existing event listeners to prevent duplicates
        customAddressCheckbox.removeEventListener('change', handleCustomAddressToggle);

        // Add the change event listener
        customAddressCheckbox.addEventListener('change', handleCustomAddressToggle);

        // Initialize address field event listeners
        initializeAddressFieldEvents();

        // Set initial state
        const initialState = customAddressCheckbox.checked;
        toggleCustomAddressFields(initialState);

        console.log(`🏠 Custom address functionality initialized (initial state: ${initialState})`);
    }

    function handleCustomAddressToggle(event) {
        const useCustom = event.target.checked;
        console.log('🏠 Custom address toggle changed to:', useCustom);

        // Store current values if switching to custom
        if (useCustom) {
            window.customRunAddress = {
                address: safeGetValue('editAddress'),
                city: safeGetValue('editCity'),
                state: safeGetValue('editState'),
                zip: safeGetValue('editZip')
            };
            console.log('💾 Stored custom address data:', window.customRunAddress);
        }

        // Toggle the fields
        toggleCustomAddressFields(useCustom);

        // Update the address preview
        displayAddressPreview();

        // Show feedback to user
        if (useCustom) {
            showToast('You can now enter a custom address for this run', 'info');
        } else {
            showToast('Using client address for this run', 'info');
        }
    }

    function toggleCustomAddressFields(useCustom) {
        console.log('🔄 Toggling custom address fields:', useCustom);

        const addressFieldsContainer = document.getElementById('addressFieldsContainer');
        const customAddressIndicator = document.getElementById('customAddressIndicator');
        const clientAddressIndicator = document.getElementById('clientAddressIndicator');

        if (useCustom) {
            // Enable custom address entry
            enableAddressFields(true);

            // Show custom address indicator
            if (customAddressIndicator) {
                customAddressIndicator.style.display = 'block';
            }
            if (clientAddressIndicator) {
                clientAddressIndicator.style.display = 'none';
            }

            // Populate with custom address or clear for new entry
            if (window.customRunAddress && window.customRunAddress.address) {
                populateAddressFields(window.customRunAddress);
            } else {
                // Clear fields for new custom address entry
                populateAddressFields({ address: '', city: '', state: '', zip: '' });

                // Focus on address field after a short delay
                setTimeout(() => {
                    const addressField = document.getElementById('editAddress');
                    if (addressField) {
                        addressField.focus();
                    }
                }, 100);
            }

            // Add custom styling
            if (addressFieldsContainer) {
                addressFieldsContainer.classList.add('custom-address-active');
            }

        } else {
            // Use client address
            enableAddressFields(false);

            // Show client address indicator
            if (clientAddressIndicator) {
                clientAddressIndicator.style.display = 'block';
            }
            if (customAddressIndicator) {
                customAddressIndicator.style.display = 'none';
            }

            // Populate with client address
            if (window.originalClientAddress) {
                populateAddressFields(window.originalClientAddress);
            }

            // Remove custom styling
            if (addressFieldsContainer) {
                addressFieldsContainer.classList.remove('custom-address-active');
            }
        }

        // Update address source indicator
        updateAddressSourceIndicator(useCustom);

        console.log(`✅ Address fields toggled to ${useCustom ? 'custom' : 'client'} mode`);
    }

    function enableAddressFields(enabled) {
        const addressFields = ['editAddress', 'editCity', 'editState', 'editZip'];

        addressFields.forEach(fieldId => {
            const field = document.getElementById(fieldId);
            if (field) {
                field.disabled = !enabled;

                if (enabled) {
                    field.classList.remove('bg-light');
                    field.classList.add('editable-address-field');
                    field.style.backgroundColor = '';
                } else {
                    field.classList.add('bg-light');
                    field.classList.remove('editable-address-field');
                    field.style.backgroundColor = '#f8f9fa';
                }
            }
        });
    }

    function populateAddressFields(addressData) {
        if (!addressData) return;

        safeSetValue('editAddress', addressData.address);
        safeSetValue('editCity', addressData.city);
        safeSetValue('editState', addressData.state);
        safeSetValue('editZip', addressData.zip);

        console.log('📝 Address fields populated:', addressData);
    }

    function updateAddressSourceIndicator(isCustom) {
        const sourceText = document.getElementById('addressSourceText');
        if (sourceText) {
            if (isCustom) {
                sourceText.innerHTML = '<i class="bi bi-pencil-square text-primary me-1"></i>Custom Address';
                sourceText.className = 'text-primary small';
            } else {
                sourceText.innerHTML = '<i class="bi bi-building text-muted me-1"></i>Client Address';
                sourceText.className = 'text-muted small';
            }
        }
    }

    function initializeAddressFieldEvents() {
        const addressFields = ['editAddress', 'editCity', 'editState', 'editZip'];

        addressFields.forEach(fieldId => {
            const field = document.getElementById(fieldId);
            if (field) {
                field.addEventListener('input', function () {
                    const useCustom = document.getElementById('useCustomAddress')?.checked;
                    if (useCustom) {
                        // Update custom address data as user types
                        if (!window.customRunAddress) {
                            window.customRunAddress = {};
                        }

                        const fieldName = fieldId.replace('edit', '').toLowerCase();
                        window.customRunAddress[fieldName] = this.value;

                        // Real-time validation feedback
                        validateCustomAddressField(this);

                        // Update preview
                        displayAddressPreview();
                    }
                });
            }
        });
    }

    function validateCustomAddressField(field) {
        const value = field.value.trim();
        const fieldName = field.id.replace('edit', '').toLowerCase();
        const isRequired = ['address', 'city', 'state'].includes(fieldName);

        if (isRequired && !value) {
            field.classList.add('is-invalid');
            field.classList.remove('is-valid');
        } else if (value) {
            field.classList.remove('is-invalid');
            field.classList.add('is-valid');
        } else {
            field.classList.remove('is-invalid', 'is-valid');
        }
    }

    // ========== MODAL HANDLERS ==========
    function initializeModalHandlers() {
        const editRunModal = document.getElementById('editRunModal');
        if (editRunModal) {
            editRunModal.addEventListener('show.bs.modal', handleEditModalShow);
            editRunModal.addEventListener('hidden.bs.modal', handleEditModalHide);

            // Tab switching handlers
            const tabButtons = editRunModal.querySelectorAll('button[data-bs-toggle="tab"]');
            tabButtons.forEach(button => {
                button.addEventListener('shown.bs.tab', handleTabSwitch);
            });
        }
    }

    function handleEditModalShow(event) {
        const button = event.relatedTarget;
        let runId = button.getAttribute('data-run-id');

        console.log('📂 Opening edit modal for run ID:', runId);

        if (!runId) {
            // Try to recover run ID
            const row = button.closest('tr');
            if (row) {
                runId = row.getAttribute('data-run-id') ||
                    row.getAttribute('data-runid') ||
                    row.dataset.runId;
            }
        }

        if (!runId) {
            console.error('🚨 No run ID found');
            showToast('Run ID is missing. Please refresh the page.', 'error');
            return;
        }

        // Set run IDs in forms
        safeSetValue('editRunId', runId);
        safeSetValue('deleteRunId', runId);

        // Clear previous data
        clearAllForms();
        clearAddressData();

        // Load run data with enhanced address functionality
        loadRunDataEnhanced(runId);
    }

    function handleEditModalHide() {
        console.log('🚪 Edit modal closed, clearing forms');
        clearAllForms();
        clearAddressData();
    }

    function handleTabSwitch(event) {
        const targetTab = event.target.getAttribute('data-bs-target');
        const runId = document.getElementById('editRunId')?.value;

        console.log('🔄 Switching to tab:', targetTab, 'for run:', runId);

        if (!runId) return;

        switch (targetTab) {
            case '#details-tab-pane':
                // Ensure custom address functionality is working
                initializeCustomAddressFunctionality();
                displayAddressPreview();
                break;
            case '#participants-tab-pane':
                loadParticipants(runId);
                break;
        }
    }

    // ========== FORM HANDLERS ==========
    function initializeFormHandlers() {
        const editRunForm = document.getElementById('editRunForm');
        if (editRunForm) {
            editRunForm.addEventListener('submit', handleEditFormSubmit);
        }
    }

    function handleEditFormSubmit(e) {
        e.preventDefault();

        // Validate address fields
        const addressValidation = validateAddressFields();
        if (!addressValidation.isValid) {
            showToast(`Address validation failed:\n• ${addressValidation.errors.join('\n• ')}`, 'error');
            return;
        }

        // Get enhanced run data including address information
        const runData = getEnhancedRunDataForSubmission();

        console.log('🚀 Submitting enhanced run data:', runData);

        const submitBtn = e.target.querySelector('button[type="submit"]');
        if (submitBtn && window.UIUtils) {
            window.UIUtils.setButtonLoading(submitBtn, true, 'Saving...');
        }

        const token = getAntiForgeryToken();

        fetch('/Run/Edit', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(runData)
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                return response.json();
            })
            .then(result => {
                if (submitBtn && window.UIUtils) {
                    window.UIUtils.setButtonLoading(submitBtn, false);
                }

                if (result.success) {
                    showToast('Run updated successfully!', 'success');
                    setTimeout(() => {
                        const modal = bootstrap.Modal.getInstance(document.getElementById('editRunModal'));
                        if (modal) modal.hide();
                        location.reload();
                    }, 1000);
                } else {
                    showToast(`Error updating run: ${result.message || 'Unknown error'}`, 'error');
                }
            })
            .catch(error => {
                console.error('🚨 Error updating run:', error);
                if (submitBtn && window.UIUtils) {
                    window.UIUtils.setButtonLoading(submitBtn, false);
                }
                showToast(`Error updating run: ${error.message}`, 'error');
            });
    }

    // ========== DATA LOADING FUNCTIONS ==========
    function loadRunDataEnhanced(runId) {
        console.log('📥 Loading enhanced run data for ID:', runId);

        if (!runId) {
            console.error('❌ No run ID provided');
            return;
        }

        showLoading();

        fetch(`/Run/GetRunData?id=${encodeURIComponent(runId)}`)
            .then(response => {
                console.log('📡 API Response status:', response.status);

                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                return response.json();
            })
            .then(data => {
                hideLoading();
                console.log('📦 Received run data:', data);

                if (data.success === false) {
                    console.error('❌ API returned error:', data.message);
                    showToast(`Error loading run data: ${data.message || 'Unknown error'}`, 'error');
                    return;
                }

                // Use enhanced population function
                populateRunDetailsEnhanced(data);

                console.log('✅ Enhanced run data loaded successfully');
                showToast('Run data loaded successfully', 'success');

            })
            .catch(error => {
                hideLoading();
                console.error('❌ Error loading run data:', error);
                showToast(`Error loading run data: ${error.message}`, 'error');
            });
    }

    function populateRunDetailsEnhanced(data) {
        console.log('📝 Populating enhanced run details with data:', data);

        if (!data) {
            console.error('❌ No data provided to populateRunDetailsEnhanced');
            return;
        }

        // Populate basic run information
        populateBasicRunFields(data);

        // Store original client address data
        window.originalClientAddress = {
            address: data.clientAddress || data.address || '',
            city: data.clientCity || data.city || '',
            state: data.clientState || data.state || '',
            zip: data.clientZip || data.zip || ''
        };

        // Store custom address data if it exists
        window.customRunAddress = {
            address: data.customAddress || '',
            city: data.customCity || '',
            state: data.customState || '',
            zip: data.customZip || ''
        };

        // Check if run has custom address
        const hasCustomAddress = data.useCustomAddress ||
            (data.customAddress && data.customAddress !== window.originalClientAddress.address);

        // Set the custom address checkbox
        const customAddressCheckbox = document.getElementById('useCustomAddress');
        if (customAddressCheckbox) {
            customAddressCheckbox.checked = hasCustomAddress;

            // Trigger the toggle functionality
            toggleCustomAddressFields(hasCustomAddress);
        }

        // Populate address fields based on whether custom address is used
        if (hasCustomAddress && window.customRunAddress.address) {
            // Use custom address
            populateAddressFields(window.customRunAddress);
        } else {
            // Use client address
            populateAddressFields(window.originalClientAddress);
        }

        // Update address source indicator
        updateAddressSourceIndicator(hasCustomAddress);

        // Display address preview
        displayAddressPreview();

        console.log('✅ Enhanced run details populated successfully');
    }

    function populateBasicRunFields(data) {
        // Populate basic run fields
        safeSetValue('editRunTitle', data.name || data.runName);
        safeSetValue('editRunDate', formatDateForInput(data.runDate));
        safeSetValue('editRunTime', formatTimeForInput(data.startTime));
        safeSetValue('editEndTime', formatTimeForInput(data.endTime));
        safeSetValue('editMaxParticipants', data.playerLimit);
        safeSetValue('editRunDescription', data.description);

        safeSetSelect('editSkillLevel', data.skillLevel || 'Intermediate');
        safeSetSelect('editRunType', data.type || 'Pickup');
        safeSetSelect('editStatus', data.status || 'Active');
        safeSetSelect('editIsPublic', data.isPublic !== false ? 'true' : 'false');
    }

    // ========== ADDRESS VALIDATION ==========
    function validateAddressFields() {
        const useCustom = document.getElementById('useCustomAddress')?.checked;

        if (!useCustom) {
            // Using client address - always valid
            return { isValid: true };
        }

        // Validate custom address
        const address = safeGetValue('editAddress').trim();
        const city = safeGetValue('editCity').trim();
        const state = safeGetValue('editState').trim();

        const errors = [];

        if (!address) {
            errors.push('Address is required when using custom address');
        }

        if (!city) {
            errors.push('City is required when using custom address');
        }

        if (!state) {
            errors.push('State is required when using custom address');
        }

        return {
            isValid: errors.length === 0,
            errors: errors
        };
    }

    function getCurrentAddressData() {
        const useCustom = document.getElementById('useCustomAddress')?.checked;

        return {
            useCustomAddress: useCustom,
            address: safeGetValue('editAddress'),
            city: safeGetValue('editCity'),
            state: safeGetValue('editState'),
            zip: safeGetValue('editZip'),
            customAddress: useCustom ? safeGetValue('editAddress') : '',
            customCity: useCustom ? safeGetValue('editCity') : '',
            customState: useCustom ? safeGetValue('editState') : '',
            customZip: useCustom ? safeGetValue('editZip') : ''
        };
    }

    function getEnhancedRunDataForSubmission() {
        const formData = new FormData(document.getElementById('editRunForm'));
        const runData = {};

        // Convert FormData to object
        for (const [key, value] of formData.entries()) {
            runData[key] = value;
        }

        // Add address data
        const addressData = getCurrentAddressData();
        Object.assign(runData, addressData);

        // Add run ID if not present
        if (!runData.RunId) {
            const runIdField = document.getElementById('editRunId');
            if (runIdField && runIdField.value) {
                runData.RunId = runIdField.value;
            }
        }

        return runData;
    }

    function displayAddressPreview() {
        const previewContainer = document.getElementById('addressPreview');
        if (!previewContainer) return;

        const addressData = getCurrentAddressData();
        const useCustom = addressData.useCustomAddress;

        let previewHtml = '';

        if (useCustom) {
            previewHtml = `
                <div class="address-preview custom-address">
                    <div class="d-flex align-items-center mb-2">
                        <i class="bi bi-pencil-square text-primary me-2"></i>
                        <strong class="text-primary">Custom Run Address</strong>
                    </div>
                    <div class="address-text">
                        ${addressData.address || 'No address entered'}<br>
                        ${addressData.city || ''}, ${addressData.state || ''} ${addressData.zip || ''}
                    </div>
                </div>
            `;
        } else {
            previewHtml = `
                <div class="address-preview client-address">
                    <div class="d-flex align-items-center mb-2">
                        <i class="bi bi-building text-muted me-2"></i>
                        <strong class="text-muted">Client Address</strong>
                    </div>
                    <div class="address-text">
                        ${window.originalClientAddress?.address || 'N/A'}<br>
                        ${window.originalClientAddress?.city || ''}, ${window.originalClientAddress?.state || ''} ${window.originalClientAddress?.zip || ''}
                    </div>
                </div>
            `;
        }

        previewContainer.innerHTML = previewHtml;
    }

    // ========== CLEANUP FUNCTIONS ==========
    function clearAllForms() {
        // Clear basic form fields
        const fields = [
            'editRunTitle', 'editRunDate', 'editRunTime', 'editEndTime',
            'editMaxParticipants', 'editRunDescription'
        ];

        fields.forEach(field => safeSetValue(field, ''));

        // Reset select fields
        const selects = ['editSkillLevel', 'editRunType', 'editStatus', 'editIsPublic'];
        selects.forEach(select => {
            const element = document.getElementById(select);
            if (element) element.selectedIndex = 0;
        });
    }

    function clearAddressData() {
        window.originalClientAddress = null;
        window.customRunAddress = null;

        const customAddressCheckbox = document.getElementById('useCustomAddress');
        if (customAddressCheckbox) {
            customAddressCheckbox.checked = false;
        }

        enableAddressFields(false);
        updateAddressSourceIndicator(false);
    }

    // ========== ENHANCE EXISTING FUNCTIONS ==========
    function enhanceExistingFunctions() {
        // Override the global loadRunDataEnhanced function if it exists
        if (window.loadRunDataEnhanced) {
            const originalFunction = window.loadRunDataEnhanced;
            window.loadRunDataEnhanced = function (runId) {
                console.log('🔄 Using enhanced loadRunDataEnhanced function');
                loadRunDataEnhanced(runId);
            };
        } else {
            // Create the global function
            window.loadRunDataEnhanced = loadRunDataEnhanced;
        }

        // Export other functions for global access
        window.populateRunDetailsEnhanced = populateRunDetailsEnhanced;
        window.toggleCustomAddressFields = toggleCustomAddressFields;
        window.getCurrentAddressData = getCurrentAddressData;
        window.validateAddressFields = validateAddressFields;
        window.displayAddressPreview = displayAddressPreview;
    }

    // ========== UTILITY FUNCTIONS ==========
    function safeSetValue(elementId, value) {
        const element = document.getElementById(elementId);
        if (element) {
            element.value = value || '';
        }
    }

    function safeGetValue(elementId) {
        const element = document.getElementById(elementId);
        return element ? element.value : '';
    }

    function safeSetSelect(elementId, value) {
        const select = document.getElementById(elementId);
        if (select && value) {
            for (let i = 0; i < select.options.length; i++) {
                if (select.options[i].value === value) {
                    select.selectedIndex = i;
                    break;
                }
            }
        }
    }

    function formatDateForInput(dateString) {
        if (!dateString) return '';
        try {
            const date = new Date(dateString);
            return date.toISOString().split('T')[0];
        } catch (e) {
            return '';
        }
    }

    function formatTimeForInput(timeString) {
        if (!timeString) return '';
        try {
            // Handle different time formats
            if (timeString.includes(':')) {
                return timeString.substring(0, 5); // HH:MM format
            }
            return timeString;
        } catch (e) {
            return '';
        }
    }

    function getAntiForgeryToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : '';
    }

    function showToast(message, type = 'success') {
        if (window.UIUtils) {
            window.UIUtils.showToast(message, type);
        } else {
            console.log(`${type}: ${message}`);
        }
    }

    function showLoading() {
        if (window.UIUtils) {
            window.UIUtils.showLoading();
        }
    }

    function hideLoading() {
        if (window.UIUtils) {
            window.UIUtils.hideLoading();
        }
    }

    // ========== DEBUG FUNCTIONS ==========
    window.runDebugFixed = {
        initializeCustomAddressFunctionality,
        toggleCustomAddressFields,
        handleCustomAddressToggle,
        validateAddressFields,
        getCurrentAddressData,
        displayAddressPreview,
        populateRunDetailsEnhanced,
        loadRunDataEnhanced,
        clearAddressData,
        originalClientAddress: () => window.originalClientAddress,
        customRunAddress: () => window.customRunAddress
    };

    console.log('🐛 Debug functions available: window.runDebugFixed');
});