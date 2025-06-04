/**
 * COMPLETE RUN MANAGEMENT FIX
 * Fixes toggle checkbox, address population, and all related functionality
 */

document.addEventListener('DOMContentLoaded', function () {
    console.log('🔧 Starting Complete Run Management Fix');

    // Global state management
    window.runManagementState = {
        originalClientAddress: null,
        customRunAddress: null,
        isInitialized: false,
        currentRunId: null
    };

    // Initialize everything
    initializeCompleteRunManagement();

    console.log('✅ Complete Run Management Fix loaded successfully');

    // ========== MAIN INITIALIZATION ==========
    function initializeCompleteRunManagement() {
        // Wait for DOM to be fully ready
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', initializeCompleteRunManagement);
            return;
        }

        console.log('🚀 Initializing complete run management...');

        // Initialize core functionality
        setupModalEventHandlers();
        setupFormEventHandlers();
        setupCustomAddressToggle();

        // Mark as initialized
        window.runManagementState.isInitialized = true;

        console.log('✅ Complete run management initialized');
    }

    // ========== MODAL EVENT HANDLERS ==========
    function setupModalEventHandlers() {
        const editRunModal = document.getElementById('editRunModal');
        if (!editRunModal) {
            console.warn('⚠️ Edit run modal not found');
            return;
        }

        // Remove existing listeners to prevent duplicates
        editRunModal.removeEventListener('show.bs.modal', handleModalShow);
        editRunModal.removeEventListener('hidden.bs.modal', handleModalHide);

        // Add fresh listeners
        editRunModal.addEventListener('show.bs.modal', handleModalShow);
        editRunModal.addEventListener('hidden.bs.modal', handleModalHide);

        // Tab switching
        const tabButtons = editRunModal.querySelectorAll('button[data-bs-toggle="tab"]');
        tabButtons.forEach(button => {
            button.removeEventListener('shown.bs.tab', handleTabSwitch);
            button.addEventListener('shown.bs.tab', handleTabSwitch);
        });

        console.log('📝 Modal event handlers setup complete');
    }

    function handleModalShow(event) {
        console.log('📂 Modal opening...');

        const button = event.relatedTarget;
        let runId = button?.getAttribute('data-run-id');

        // Try multiple ways to get run ID
        if (!runId) {
            runId = button?.getAttribute('data-runid') ||
                button?.dataset?.runId ||
                button?.closest('tr')?.getAttribute('data-run-id');
        }

        if (!runId) {
            console.error('❌ No run ID found');
            showToast('Run ID is missing. Please refresh the page.', 'error');
            return;
        }

        console.log('📂 Opening modal for run ID:', runId);

        // Store current run ID
        window.runManagementState.currentRunId = runId;

        // Set form values
        safeSetValue('editRunId', runId);
        safeSetValue('deleteRunId', runId);

        // Clear and reset everything
        resetModalState();

        // Load run data
        loadRunDataComplete(runId);
    }

    function handleModalHide() {
        console.log('🚪 Modal closing...');
        resetModalState();
        window.runManagementState.currentRunId = null;
    }

    function handleTabSwitch(event) {
        const targetTab = event.target.getAttribute('data-bs-target');
        console.log('🔄 Tab switch to:', targetTab);

        // If switching to details tab, ensure custom address is working
        if (targetTab === '#details-tab-pane') {
            setTimeout(() => {
                setupCustomAddressToggle();
                updateAddressDisplay();
            }, 100);
        }
    }

    // ========== CUSTOM ADDRESS TOGGLE SETUP ==========
    function setupCustomAddressToggle() {
        console.log('🏠 Setting up custom address toggle...');

        const checkbox = document.getElementById('useCustomAddress');
        if (!checkbox) {
            console.warn('⚠️ Custom address checkbox not found');
            return;
        }

        // Remove existing listener to prevent duplicates
        checkbox.removeEventListener('change', handleAddressToggleChange);

        // Add fresh listener
        checkbox.addEventListener('change', handleAddressToggleChange);

        // Set initial state
        const isChecked = checkbox.checked;
        toggleAddressFieldsState(isChecked);

        console.log(`✅ Custom address toggle setup complete (state: ${isChecked})`);
    }

    function handleAddressToggleChange(event) {
        const isCustom = event.target.checked;
        console.log('🔄 Address toggle changed to:', isCustom);

        // Save current values if switching to custom
        if (isCustom) {
            window.runManagementState.customRunAddress = {
                address: safeGetValue('editAddress'),
                city: safeGetValue('editCity'),
                state: safeGetValue('editState'),
                zip: safeGetValue('editZip')
            };
        }

        // Toggle field states
        toggleAddressFieldsState(isCustom);

        // Update address display
        updateAddressDisplay();

        // Show feedback
        const message = isCustom ?
            'Custom address enabled - you can now edit the address fields' :
            'Using client address - address fields are now read-only';
        showToast(message, 'info');
    }

    function toggleAddressFieldsState(useCustom) {
        console.log(`🔄 Toggling address fields to ${useCustom ? 'custom' : 'client'} mode`);

        const addressFields = ['editAddress', 'editCity', 'editState', 'editZip'];

        addressFields.forEach(fieldId => {
            const field = document.getElementById(fieldId);
            if (field) {
                field.disabled = !useCustom;
                field.readOnly = !useCustom;

                // Update styling
                if (useCustom) {
                    field.classList.remove('bg-light');
                    field.classList.add('editable-field');
                    field.style.backgroundColor = '';
                    field.style.cursor = '';
                } else {
                    field.classList.add('bg-light');
                    field.classList.remove('editable-field');
                    field.style.backgroundColor = '#f8f9fa';
                    field.style.cursor = 'not-allowed';
                }
            }
        });

        // Update indicators
        updateAddressIndicators(useCustom);

        // If switching to custom and no custom data, focus on address field
        if (useCustom) {
            const addressField = document.getElementById('editAddress');
            if (addressField && !addressField.value.trim()) {
                setTimeout(() => addressField.focus(), 100);
            }
        }

        console.log(`✅ Address fields toggled to ${useCustom ? 'custom' : 'client'} mode`);
    }

    function updateAddressIndicators(isCustom) {
        const customIndicator = document.getElementById('customAddressIndicator');
        const clientIndicator = document.getElementById('clientAddressIndicator');
        const sourceText = document.getElementById('addressSourceText');

        if (customIndicator) {
            customIndicator.style.display = isCustom ? 'block' : 'none';
        }

        if (clientIndicator) {
            clientIndicator.style.display = isCustom ? 'none' : 'block';
        }

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

    // ========== DATA LOADING ==========
    function loadRunDataComplete(runId) {
        console.log('📥 Loading complete run data for ID:', runId);

        if (!runId) {
            console.error('❌ No run ID provided');
            return;
        }

        showLoadingState();

        const url = `/Run/GetRunData?id=${encodeURIComponent(runId)}`;

        fetch(url)
            .then(response => {
                console.log('📡 Response status:', response.status);

                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                return response.json();
            })
            .then(data => {
                hideLoadingState();
                console.log('📦 Received run data:', data);

                if (data.success === false) {
                    throw new Error(data.message || 'API returned error');
                }

                // Populate all data
                populateRunDataComplete(data);

                console.log('✅ Run data loaded and populated successfully');
                showToast('Run data loaded successfully', 'success');
            })
            .catch(error => {
                hideLoadingState();
                console.error('❌ Error loading run data:', error);
                showToast(`Error loading run data: ${error.message}`, 'error');

                // Try to extract data from table as fallback
                tryExtractFromTable(runId);
            });
    }

    function populateRunDataComplete(data) {
        console.log('📝 Populating complete run data:', data);

        try {
            // Basic run information
            safeSetValue('editRunTitle', data.name || data.runName || '');
            safeSetValue('editRunDescription', data.description || '');
            safeSetValue('editMaxParticipants', data.playerLimit || 10);

            // Date and time
            if (data.runDate) {
                const date = new Date(data.runDate);
                safeSetValue('editRunDate', date.toISOString().split('T')[0]);
            }

            if (data.startTime) {
                safeSetValue('editRunTime', formatTimeForInput(data.startTime));
            }

            if (data.endTime) {
                safeSetValue('editEndTime', formatTimeForInput(data.endTime));
            }

            // Dropdowns
            safeSetSelect('editSkillLevel', data.skillLevel || 'Intermediate');
            safeSetSelect('editRunType', data.type || 'Pickup');
            safeSetSelect('editStatus', data.status || 'Active');
            safeSetSelect('editIsPublic', data.isPublic !== false ? 'true' : 'false');

            // Address handling - this is the key fix
            populateAddressDataComplete(data);

            console.log('✅ Run data population complete');

        } catch (error) {
            console.error('❌ Error populating run data:', error);
            showToast('Error populating run data', 'error');
        }
    }

    function populateAddressDataComplete(data) {
        console.log('🏠 Populating address data:', data);

        // Store original client address
        window.runManagementState.originalClientAddress = {
            address: data.clientAddress || data.address || '',
            city: data.clientCity || data.city || '',
            state: data.clientState || data.state || '',
            zip: data.clientZip || data.zip || ''
        };

        // Store custom address if it exists
        window.runManagementState.customRunAddress = {
            address: data.customAddress || '',
            city: data.customCity || '',
            state: data.customState || '',
            zip: data.customZip || ''
        };

        // Determine if custom address should be used
        const useCustomAddress = data.useCustomAddress === true ||
            data.useCustomAddress === 'true' ||
            (data.customAddress && data.customAddress.trim() !== '');

        console.log('🏠 Address data:', {
            useCustomAddress,
            originalClient: window.runManagementState.originalClientAddress,
            custom: window.runManagementState.customRunAddress
        });

        // Set checkbox state
        const checkbox = document.getElementById('useCustomAddress');
        if (checkbox) {
            checkbox.checked = useCustomAddress;
        }

        // Populate address fields based on mode
        if (useCustomAddress && window.runManagementState.customRunAddress.address) {
            populateAddressFields(window.runManagementState.customRunAddress);
        } else {
            populateAddressFields(window.runManagementState.originalClientAddress);
        }

        // Set field states
        toggleAddressFieldsState(useCustomAddress);

        // Update display
        updateAddressDisplay();

        console.log('✅ Address data populated successfully');
    }

    function populateAddressFields(addressData) {
        if (!addressData) {
            console.warn('⚠️ No address data to populate');
            return;
        }

        safeSetValue('editAddress', addressData.address || '');
        safeSetValue('editCity', addressData.city || '');
        safeSetValue('editState', addressData.state || '');
        safeSetValue('editZip', addressData.zip || '');

        console.log('📝 Address fields populated:', addressData);
    }

    function tryExtractFromTable(runId) {
        console.log('📋 Trying to extract data from table for run:', runId);

        // Try to find the table row
        const row = document.querySelector(`tr[data-run-id="${runId}"]`) ||
            document.querySelector(`button[data-run-id="${runId}"]`)?.closest('tr');

        if (!row) {
            console.warn('⚠️ Could not find table row for run');
            return;
        }

        // Extract basic data from table cells
        const cells = row.querySelectorAll('td');
        if (cells.length >= 3) {
            // Extract name from first cell
            const nameEl = cells[0].querySelector('.fw-semibold');
            if (nameEl) {
                safeSetValue('editRunTitle', nameEl.textContent.trim());
            }

            // Extract date/time from second cell  
            const dateEl = cells[1].querySelector('.fw-semibold');
            if (dateEl) {
                const dateText = dateEl.textContent.trim();
                try {
                    const date = new Date(dateText);
                    safeSetValue('editRunDate', date.toISOString().split('T')[0]);
                } catch (e) {
                    console.warn('Could not parse date:', dateText);
                }
            }

            // Extract location from third cell
            const locationEl = cells[2].querySelector('.fw-semibold');
            if (locationEl) {
                const location = locationEl.textContent.trim();
                // Try to parse address
                const parts = location.split(',').map(p => p.trim());
                if (parts.length >= 1) {
                    safeSetValue('editAddress', parts[0]);
                }
                if (parts.length >= 2) {
                    safeSetValue('editCity', parts[1]);
                }
                if (parts.length >= 3) {
                    safeSetValue('editState', parts[2]);
                }
            }
        }

        console.log('📋 Extracted what data we could from table');
    }

    // ========== FORM HANDLERS ==========
    function setupFormEventHandlers() {
        const editForm = document.getElementById('editRunForm');
        if (editForm) {
            editForm.removeEventListener('submit', handleFormSubmit);
            editForm.addEventListener('submit', handleFormSubmit);
        }

        // Address field change handlers
        const addressFields = ['editAddress', 'editCity', 'editState', 'editZip'];
        addressFields.forEach(fieldId => {
            const field = document.getElementById(fieldId);
            if (field) {
                field.removeEventListener('input', handleAddressFieldChange);
                field.addEventListener('input', handleAddressFieldChange);
            }
        });
    }

    function handleFormSubmit(e) {
        e.preventDefault();
        console.log('📤 Form submission started');

        // Validate form
        if (!validateForm()) {
            return;
        }

        // Get form data
        const formData = getCompleteFormData();

        console.log('📤 Submitting form data:', formData);

        const submitBtn = e.target.querySelector('button[type="submit"]');
        if (submitBtn) {
            setButtonLoading(submitBtn, true);
        }

        // Submit
        fetch('/Run/Edit', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify(formData)
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                return response.json();
            })
            .then(result => {
                if (submitBtn) {
                    setButtonLoading(submitBtn, false);
                }

                if (result.success) {
                    showToast('Run updated successfully!', 'success');
                    setTimeout(() => {
                        const modal = bootstrap.Modal.getInstance(document.getElementById('editRunModal'));
                        if (modal) modal.hide();
                        location.reload();
                    }, 1000);
                } else {
                    showToast(`Error: ${result.message || 'Unknown error'}`, 'error');
                }
            })
            .catch(error => {
                console.error('❌ Form submission error:', error);
                if (submitBtn) {
                    setButtonLoading(submitBtn, false);
                }
                showToast(`Error: ${error.message}`, 'error');
            });
    }

    function handleAddressFieldChange() {
        const useCustom = document.getElementById('useCustomAddress')?.checked;
        if (useCustom) {
            // Update custom address data
            window.runManagementState.customRunAddress = {
                address: safeGetValue('editAddress'),
                city: safeGetValue('editCity'),
                state: safeGetValue('editState'),
                zip: safeGetValue('editZip')
            };

            // Update display
            updateAddressDisplay();
        }
    }

    function getCompleteFormData() {
        const formData = {
            RunId: safeGetValue('editRunId'),
            Name: safeGetValue('editRunTitle'),
            Description: safeGetValue('editRunDescription'),
            RunDate: safeGetValue('editRunDate'),
            StartTime: safeGetValue('editRunTime'),
            EndTime: safeGetValue('editEndTime'),
            PlayerLimit: parseInt(safeGetValue('editMaxParticipants')) || 10,
            SkillLevel: safeGetValue('editSkillLevel'),
            Type: safeGetValue('editRunType'),
            Status: safeGetValue('editStatus'),
            IsPublic: safeGetValue('editIsPublic') === 'true'
        };

        // Add address data
        const useCustom = document.getElementById('useCustomAddress')?.checked;
        formData.UseCustomAddress = useCustom;

        if (useCustom) {
            formData.CustomAddress = safeGetValue('editAddress');
            formData.CustomCity = safeGetValue('editCity');
            formData.CustomState = safeGetValue('editState');
            formData.CustomZip = safeGetValue('editZip');
        }

        formData.Address = safeGetValue('editAddress');
        formData.City = safeGetValue('editCity');
        formData.State = safeGetValue('editState');
        formData.Zip = safeGetValue('editZip');

        return formData;
    }

    function validateForm() {
        const errors = [];

        if (!safeGetValue('editRunTitle').trim()) {
            errors.push('Run title is required');
        }

        if (!safeGetValue('editRunDate')) {
            errors.push('Run date is required');
        }

        if (!safeGetValue('editRunTime')) {
            errors.push('Start time is required');
        }

        const playerLimit = parseInt(safeGetValue('editMaxParticipants'));
        if (!playerLimit || playerLimit < 1) {
            errors.push('Player limit must be at least 1');
        }

        // Validate custom address if enabled
        const useCustom = document.getElementById('useCustomAddress')?.checked;
        if (useCustom) {
            if (!safeGetValue('editAddress').trim()) {
                errors.push('Address is required when using custom address');
            }
            if (!safeGetValue('editCity').trim()) {
                errors.push('City is required when using custom address');
            }
            if (!safeGetValue('editState').trim()) {
                errors.push('State is required when using custom address');
            }
        }

        if (errors.length > 0) {
            showToast(`Please fix the following errors:\n• ${errors.join('\n• ')}`, 'error');
            return false;
        }

        return true;
    }

    // ========== DISPLAY UPDATES ==========
    function updateAddressDisplay() {
        const previewContainer = document.getElementById('addressPreview');
        if (!previewContainer) return;

        const useCustom = document.getElementById('useCustomAddress')?.checked;
        const currentAddress = {
            address: safeGetValue('editAddress'),
            city: safeGetValue('editCity'),
            state: safeGetValue('editState'),
            zip: safeGetValue('editZip')
        };

        let previewHtml = '';

        if (useCustom) {
            previewHtml = `
                <div class="address-preview custom-address border border-primary bg-light-primary p-3 rounded">
                    <div class="d-flex align-items-center mb-2">
                        <i class="bi bi-pencil-square text-primary me-2"></i>
                        <strong class="text-primary">Custom Run Address</strong>
                    </div>
                    <div class="address-text">
                        ${currentAddress.address || 'No address entered'}<br>
                        ${currentAddress.city || ''}, ${currentAddress.state || ''} ${currentAddress.zip || ''}
                    </div>
                </div>
            `;
        } else {
            const clientAddr = window.runManagementState.originalClientAddress || {};
            previewHtml = `
                <div class="address-preview client-address border border-secondary bg-light p-3 rounded">
                    <div class="d-flex align-items-center mb-2">
                        <i class="bi bi-building text-muted me-2"></i>
                        <strong class="text-muted">Client Address</strong>
                    </div>
                    <div class="address-text">
                        ${clientAddr.address || 'N/A'}<br>
                        ${clientAddr.city || ''}, ${clientAddr.state || ''} ${clientAddr.zip || ''}
                    </div>
                </div>
            `;
        }

        previewContainer.innerHTML = previewHtml;
    }

    // ========== UTILITY FUNCTIONS ==========
    function resetModalState() {
        console.log('🧹 Resetting modal state');

        // Clear form fields
        const fields = [
            'editRunTitle', 'editRunDescription', 'editRunDate',
            'editRunTime', 'editEndTime', 'editMaxParticipants',
            'editAddress', 'editCity', 'editState', 'editZip'
        ];

        fields.forEach(field => safeSetValue(field, ''));

        // Reset selects
        const selects = ['editSkillLevel', 'editRunType', 'editStatus', 'editIsPublic'];
        selects.forEach(select => {
            const element = document.getElementById(select);
            if (element) element.selectedIndex = 0;
        });

        // Reset checkbox
        const checkbox = document.getElementById('useCustomAddress');
        if (checkbox) {
            checkbox.checked = false;
        }

        // Reset address state
        window.runManagementState.originalClientAddress = null;
        window.runManagementState.customRunAddress = null;

        // Disable address fields
        toggleAddressFieldsState(false);

        // Clear preview
        const previewContainer = document.getElementById('addressPreview');
        if (previewContainer) {
            previewContainer.innerHTML = '';
        }
    }

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

    function formatTimeForInput(timeString) {
        if (!timeString) return '';

        try {
            // Handle various time formats
            if (timeString.includes(':')) {
                const parts = timeString.split(':');
                if (parts.length >= 2) {
                    const hours = parts[0].padStart(2, '0');
                    const minutes = parts[1].padStart(2, '0');
                    return `${hours}:${minutes}`;
                }
            }
            return timeString;
        } catch (e) {
            console.warn('Error formatting time:', timeString, e);
            return '';
        }
    }

    function getAntiForgeryToken() {
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        return token ? token.value : '';
    }

    function showToast(message, type = 'info') {
        if (window.UIUtils && window.UIUtils.showToast) {
            window.UIUtils.showToast(message, type);
        } else {
            console.log(`${type}: ${message}`);
            // Fallback to alert if no toast system
            if (type === 'error') {
                alert(`Error: ${message}`);
            }
        }
    }

    function showLoadingState() {
        if (window.UIUtils && window.UIUtils.showLoading) {
            window.UIUtils.showLoading();
        }
    }

    function hideLoadingState() {
        if (window.UIUtils && window.UIUtils.hideLoading) {
            window.UIUtils.hideLoading();
        }
    }

    function setButtonLoading(button, loading) {
        if (window.UIUtils && window.UIUtils.setButtonLoading) {
            window.UIUtils.setButtonLoading(button, loading);
        } else {
            // Fallback
            if (loading) {
                button.disabled = true;
                button.textContent = 'Loading...';
            } else {
                button.disabled = false;
                button.textContent = 'Save Changes';
            }
        }
    }

    // ========== GLOBAL API ==========
    // Override any existing functions to ensure our implementation is used
    window.loadRunDataEnhanced = loadRunDataComplete;
    window.populateRunDetailsEnhanced = populateRunDataComplete;
    window.toggleCustomAddressFields = toggleAddressFieldsState;
    window.setupCustomAddressToggle = setupCustomAddressToggle;
    window.updateAddressDisplay = updateAddressDisplay;

    // Debug functions
    window.runDebugComplete = {
        state: () => window.runManagementState,
        setupToggle: setupCustomAddressToggle,
        toggleFields: toggleAddressFieldsState,
        loadData: loadRunDataComplete,
        populateData: populateRunDataComplete,
        resetState: resetModalState,
        updateDisplay: updateAddressDisplay
    };

    console.log('🐛 Debug functions available: window.runDebugComplete');
    console.log('🐛 Current state:', window.runManagementState);
});