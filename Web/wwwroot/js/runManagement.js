/**
 * Enhanced Run Details Population with Custom Address Option
 * Extends the existing runManagement.js functionality
 */

// Enhanced function to populate run details with custom address option
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
        address: data.client?.address || data.address || '',
        city: data.client?.city || data.city || '',
        state: data.client?.state || data.state || '',
        zip: data.client?.zip || data.zip || ''
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

    console.log('✅ Enhanced run details populated successfully');
}

// Function to populate address fields
function populateAddressFields(addressData) {
    safeSetValue('editAddress', addressData.address);
    safeSetValue('editCity', addressData.city);
    safeSetValue('editState', addressData.state);
    safeSetValue('editZip', addressData.zip);
}

// Function to toggle custom address fields
function toggleCustomAddressFields(useCustom) {
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
            // Focus on address field
            const addressField = document.getElementById('editAddress');
            if (addressField) {
                setTimeout(() => addressField.focus(), 100);
            }
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
}

// Function to enable/disable address fields
function enableAddressFields(enabled) {
    const addressFields = ['editAddress', 'editCity', 'editState', 'editZip'];

    addressFields.forEach(fieldId => {
        const field = document.getElementById(fieldId);
        if (field) {
            field.disabled = !enabled;
            if (enabled) {
                field.classList.remove('bg-light');
                field.classList.add('editable-address-field');
            } else {
                field.classList.add('bg-light');
                field.classList.remove('editable-address-field');
            }
        }
    });
}

// Function to update address source indicator
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

// Enhanced address validation
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

// Function to get current address data for submission
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

// Initialize custom address functionality
function initializeCustomAddressFunctionality() {
    const customAddressCheckbox = document.getElementById('useCustomAddress');

    if (customAddressCheckbox) {
        customAddressCheckbox.addEventListener('change', function () {
            const useCustom = this.checked;
            console.log('🏠 Custom address toggle:', useCustom);

            if (useCustom) {
                // Save current values as custom address
                window.customRunAddress = {
                    address: safeGetValue('editAddress'),
                    city: safeGetValue('editCity'),
                    state: safeGetValue('editState'),
                    zip: safeGetValue('editZip')
                };
            }

            toggleCustomAddressFields(useCustom);

            // Show helpful message
            if (useCustom) {
                showToast('You can now enter a custom address for this run', 'info');
            } else {
                showToast('Using client address for this run', 'info');
            }
        });
    }

    // Add real-time validation for custom address fields
    const addressFields = ['editAddress', 'editCity', 'editState', 'editZip'];
    addressFields.forEach(fieldId => {
        const field = document.getElementById(fieldId);
        if (field) {
            field.addEventListener('input', function () {
                const useCustom = document.getElementById('useCustomAddress')?.checked;
                if (useCustom) {
                    // Update custom address data as user types
                    window.customRunAddress = window.customRunAddress || {};
                    const fieldName = fieldId.replace('edit', '').toLowerCase();
                    window.customRunAddress[fieldName] = this.value;

                    // Real-time validation feedback
                    validateCustomAddressField(this);
                }
            });
        }
    });
}

// Real-time validation for individual address fields
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

// Enhanced form submission with address data
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

// Function to display address preview
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
                    ${addressData.address}<br>
                    ${addressData.city}, ${addressData.state} ${addressData.zip}
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

// Clear address data when modal closes
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

// Initialize everything when DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
    initializeCustomAddressFunctionality();

    // Override the existing loadRunDataEnhanced function
    const originalLoadRunDataEnhanced = window.loadRunDataEnhanced;
    if (originalLoadRunDataEnhanced) {
        window.loadRunDataEnhanced = function (runId) {
            console.log('🔄 Loading run data with enhanced address functionality for ID:', runId);

            if (!runId) {
                console.error('❌ No run ID provided');
                return;
            }

            showLoading();
            clearRunForm();
            clearAddressData();

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

                    // Update participant counts
                    updateParticipantCounts(data);

                    // Set delete button run ID
                    const deleteIdField = document.getElementById('deleteRunId');
                    if (deleteIdField) {
                        deleteIdField.value = data.runId || runId;
                    }

                    // Load participants
                    loadParticipants(data.runId || runId);

                    // Display address preview
                    displayAddressPreview();

                    console.log('✅ Enhanced run data loaded successfully');
                    showToast('Run data loaded successfully', 'success');

                })
                .catch(error => {
                    hideLoading();
                    console.error('❌ Error loading run data:', error);
                    showToast(`Error loading run data: ${error.message}`, 'error');
                });
        };
    }

    // Update form submission to include address validation
    const editRunForm = document.getElementById('editRunForm');
    if (editRunForm) {
        editRunForm.addEventListener('submit', function (e) {
            e.preventDefault();

            // Validate address fields
            const addressValidation = validateAddressFields();
            if (!addressValidation.isValid) {
                showToast(`Address validation failed:\n• ${addressValidation.errors.join('\n• ')}`, 'error');
                return;
            }

            // Continue with existing validation
            if (!validateRunForm()) {
                return;
            }

            // Get enhanced run data including address information
            const runData = getEnhancedRunDataForSubmission();

            console.log('🚀 Submitting enhanced run data:', runData);
            // ... rest of the submission logic remains the same
        });
    }
});

// Export functions for global access
window.populateRunDetailsEnhanced = populateRunDetailsEnhanced;
window.toggleCustomAddressFields = toggleCustomAddressFields;
window.getCurrentAddressData = getCurrentAddressData;
window.validateAddressFields = validateAddressFields;
window.displayAddressPreview = displayAddressPreview;