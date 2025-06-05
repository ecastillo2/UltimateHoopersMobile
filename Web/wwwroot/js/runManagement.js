/**
 * FIXED RUN MANAGEMENT - Compatible with runCalendar.js
 * Handles both Edit Run and Create Run forms with identical functionality
 * 
 * NEW FEATURES:
 * - Schedule conflict validation prevents overlapping runs on same court
 * - Real-time validation as user types/selects fields
 * - Automatic court auto-population from existing run data
 * - Enhanced court selection with multiple fallback methods
 * - Create Run form matches Edit Run layout and functionality
 * - Client selection and dynamic court loading for Create Run
 * - Custom address toggle functionality for both Edit and Create
 * 
 * BACKEND REQUIREMENTS:
 * - Recommended: Add /Run/CheckScheduleConflicts endpoint for optimal performance
 * - Fallback: Uses existing /Run/GetRunsForCalendar endpoint if conflict API unavailable
 * - Court loading: Uses /Run/GetRunCourts endpoint for dynamic court loading
 * - Client loading: Uses /Client/GetAllClients endpoint for create run client selection
 * - Client data: Uses /Run/GetClientData endpoint for client address information
 */

document.addEventListener('DOMContentLoaded', function () {
    console.log('🔧 Starting Fixed Run Management (Calendar Compatible)');

    // Check if already initialized to prevent conflicts
    if (window.runManagementState && window.runManagementState.isInitialized) {
        console.log('⚠️ Run Management already initialized, skipping...');
        return;
    }

    // Global state management with namespace protection
    window.runManagementState = window.runManagementState || {
        originalClientAddress: null,
        customRunAddress: null,
        isInitialized: false,
        currentRunId: null,
        isLoading: false,
        calendarCompatible: true
    };

    // Initialize everything
    initializeFixedRunManagement();

    console.log('✅ Fixed Run Management loaded successfully (Calendar Compatible)');

    // ========== MAIN INITIALIZATION ==========
    function initializeFixedRunManagement() {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', initializeFixedRunManagement);
            return;
        }

        console.log('🚀 Initializing fixed run management...');

        // Detect potential conflicts with calendar
        detectAndReportConflicts();

        // Initialize core functionality
        setupModalEventHandlers();
        setupFormEventHandlers();
        setupCustomAddressToggle();
        setupLoadingManagement();

        // NEW: Initialize Create Run functionality
        setupCreateRunHandlers();

        // Mark as initialized
        window.runManagementState.isInitialized = true;

        console.log('✅ Fixed run management initialized (Calendar Compatible)');
    }

    // ========== CONFLICT DETECTION ==========
    function detectAndReportConflicts() {
        console.log('🔍 Detecting potential conflicts...');

        const detectedItems = [];

        // Check for calendar
        if (window.fixedRunCalendar) {
            detectedItems.push('✅ Calendar detected: window.fixedRunCalendar');
            window.runManagementState.calendarPresent = true;
        }

        // Check for existing UIUtils
        if (window.UIUtils) {
            detectedItems.push('✅ UIUtils detected (likely from calendar)');
        }

        // Check for function conflicts
        const potentialConflicts = [
            'loadRunDataEnhanced',
            'populateRunDetailsEnhanced'
        ];

        potentialConflicts.forEach(funcName => {
            if (window[funcName]) {
                detectedItems.push(`⚠️ Function exists: ${funcName}`);
            }
        });

        // Check modal presence
        const editModal = document.getElementById('editRunModal');
        if (editModal) {
            detectedItems.push('✅ Edit modal found');
        } else {
            detectedItems.push('❌ Edit modal NOT found');
        }

        // Check calendar modal
        const calendarModal = document.getElementById('runCalendarModal');
        if (calendarModal) {
            detectedItems.push('✅ Calendar modal found');
        }

        if (detectedItems.length > 0) {
            console.log('🔍 Environment Detection Results:');
            detectedItems.forEach(item => console.log(`  ${item}`));

            // Check for utilities.js toast system
            if (window.UIUtils && window.UIUtils.showToast) {
                const toastStr = window.UIUtils.showToast.toString();
                if (toastStr.includes('RunMgmt Fallback')) {
                    console.log('  ⚠️ Using fallback toast system - utilities.js may not be loaded');
                } else {
                    console.log('  ✅ utilities.js toast system detected and will be used');
                }
            } else {
                console.log('  ❌ No toast system detected - will use fallback alerts');
            }
        }

        // Set compatibility mode
        if (window.runManagementState.calendarPresent) {
            console.log('📅 Calendar compatibility mode enabled');
        }
    }

    // ========== ENHANCED LOADING MANAGEMENT (CALENDAR COMPATIBLE) ==========
    function setupLoadingManagement() {
        // Don't override existing UIUtils if calendar already created it
        if (!window.UIUtils) {
            createRunManagementUIUtils();
        } else {
            console.log('📊 Using existing UIUtils (likely from calendar)');
            // Enhance existing UIUtils if needed
            enhanceExistingUIUtils();
        }
    }

    function createRunManagementUIUtils() {
        console.log('📊 Creating Run Management UIUtils');

        // Create loading overlay element with unique ID
        let loadingOverlay = document.getElementById('runManagementLoading');
        if (!loadingOverlay) {
            loadingOverlay = document.createElement('div');
            loadingOverlay.id = 'runManagementLoading';
            loadingOverlay.style.cssText = `
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background: rgba(0, 0, 0, 0.5);
                display: none;
                align-items: center;
                justify-content: center;
                z-index: 9999;
                backdrop-filter: blur(2px);
            `;
            loadingOverlay.innerHTML = `
                <div style="background: white; padding: 2rem; border-radius: 8px; text-align: center;">
                    <div class="spinner-border text-primary mb-3" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    <div>Loading run details...</div>
                </div>
            `;
            document.body.appendChild(loadingOverlay);
        }

        // Create UIUtils with run management namespace
        window.UIUtils = {
            showLoading: function () {
                console.log('📊 [RunMgmt] Showing loading overlay');
                window.runManagementState.isLoading = true;
                const overlay = document.getElementById('runManagementLoading');
                if (overlay) {
                    overlay.style.display = 'flex';
                }
            },

            hideLoading: function () {
                console.log('📊 [RunMgmt] Hiding loading overlay');
                window.runManagementState.isLoading = false;
                const overlay = document.getElementById('runManagementLoading');
                if (overlay) {
                    overlay.style.display = 'none';
                }
            },

            // Toast functionality will be provided by utilities.js
            // This is just a fallback if utilities.js isn't loaded yet
            showToast: function (message, type = 'info') {
                console.log(`Toast [RunMgmt Fallback][${type}]: ${message}`);
                // Simple fallback - utilities.js should override this
                if (type === 'error') {
                    console.error(`Run Management Error: ${message}`);
                } else {
                    console.log(`Run Management ${type}: ${message}`);
                }
            }
        };
    }

    function enhanceExistingUIUtils() {
        console.log('📊 Enhancing existing UIUtils for run management');

        // Create run management specific loading overlay
        let rmLoadingOverlay = document.getElementById('runManagementLoading');
        if (!rmLoadingOverlay) {
            rmLoadingOverlay = document.createElement('div');
            rmLoadingOverlay.id = 'runManagementLoading';
            rmLoadingOverlay.style.cssText = `
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background: rgba(0, 0, 0, 0.5);
                display: none;
                align-items: center;
                justify-content: center;
                z-index: 9999;
                backdrop-filter: blur(2px);
            `;
            rmLoadingOverlay.innerHTML = `
                <div style="background: white; padding: 2rem; border-radius: 8px; text-align: center;">
                    <div class="spinner-border text-primary mb-3" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    <div>Loading run details...</div>
                </div>
            `;
            document.body.appendChild(rmLoadingOverlay);
        }

        // Enhance UIUtils with run management specific loading methods
        window.UIUtils.showRunManagementLoading = function () {
            console.log('📊 [RunMgmt] Showing run management loading');
            window.runManagementState.isLoading = true;
            const overlay = document.getElementById('runManagementLoading');
            if (overlay) {
                overlay.style.display = 'flex';
            }
        };

        window.UIUtils.hideRunManagementLoading = function () {
            console.log('📊 [RunMgmt] Hiding run management loading');
            window.runManagementState.isLoading = false;
            const overlay = document.getElementById('runManagementLoading');
            if (overlay) {
                overlay.style.display = 'none';
            }
        };

        // Note: Toast functionality will use existing utilities.js implementation
    }

    function showRunManagementToast(message, type = 'info') {
        // Create a toast specific to run management
        const toast = document.createElement('div');
        toast.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            padding: 1rem;
            border-radius: 4px;
            color: white;
            z-index: 10001;
            max-width: 300px;
            background: ${type === 'error' ? '#dc3545' : type === 'success' ? '#28a745' : '#17a2b8'};
            box-shadow: 0 4px 12px rgba(0,0,0,0.3);
        `;
        toast.innerHTML = `
            <div style="display: flex; align-items: center; gap: 0.5rem;">
                <span style="font-weight: bold;">[Run Management]</span>
                <span>${message}</span>
            </div>
        `;
        document.body.appendChild(toast);

        setTimeout(() => {
            if (toast.parentNode) {
                toast.remove();
            }
        }, 5000);
    }

    function showLoadingState() {
        console.log('📊 [RunMgmt] Showing loading state');
        try {
            window.runManagementState.isLoading = true;

            // Use run management specific loading if available
            if (window.UIUtils && window.UIUtils.showRunManagementLoading) {
                window.UIUtils.showRunManagementLoading();
            } else if (window.UIUtils && window.UIUtils.showLoading) {
                window.UIUtils.showLoading();
            } else {
                // Fallback to modal loading state
                showModalLoadingState();
            }
        } catch (error) {
            console.error('❌ Error showing loading state:', error);
            showModalLoadingState();
        }
    }

    function hideLoadingState() {
        console.log('📊 [RunMgmt] Hiding loading state');
        try {
            window.runManagementState.isLoading = false;

            // Use run management specific loading if available
            if (window.UIUtils && window.UIUtils.hideRunManagementLoading) {
                window.UIUtils.hideRunManagementLoading();
            } else if (window.UIUtils && window.UIUtils.hideLoading) {
                window.UIUtils.hideLoading();
            }

            // Always hide modal loading state as well
            hideModalLoadingState();
        } catch (error) {
            console.error('❌ Error hiding loading state:', error);
            hideModalLoadingState();
        }
    }

    function showModalLoadingState() {
        const modal = document.getElementById('editRunModal');
        if (!modal) return;

        const modalBody = modal.querySelector('.modal-body');
        if (!modalBody) return;

        // Add loading overlay to modal
        let loadingDiv = modal.querySelector('.modal-loading-overlay');
        if (!loadingDiv) {
            loadingDiv = document.createElement('div');
            loadingDiv.className = 'modal-loading-overlay';
            loadingDiv.style.cssText = `
                position: absolute;
                top: 0;
                left: 0;
                right: 0;
                bottom: 0;
                background: rgba(255, 255, 255, 0.8);
                display: flex;
                align-items: center;
                justify-content: center;
                z-index: 1000;
            `;
            loadingDiv.innerHTML = `
                <div class="text-center">
                    <div class="spinner-border text-primary mb-2" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    <div class="text-muted">Loading run details...</div>
                </div>
            `;
            modalBody.style.position = 'relative';
            modalBody.appendChild(loadingDiv);
        }
        loadingDiv.style.display = 'flex';
    }

    function hideModalLoadingState() {
        const modal = document.getElementById('editRunModal');
        if (!modal) return;

        const loadingDiv = modal.querySelector('.modal-loading-overlay');
        if (loadingDiv) {
            loadingDiv.style.display = 'none';
        }
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
        console.log('📂 [RunMgmt] Modal opening...');

        // Check if this was triggered by calendar
        const isFromCalendar = event.detail?.source === 'calendar' ||
            event.relatedTarget?.closest('.calendar-container') ||
            window.fixedRunCalendar?.getRuns;

        if (isFromCalendar) {
            console.log('📅 Modal opened from calendar, using calendar-compatible mode');
        }

        // Always hide any existing loading state first
        hideLoadingState();

        const button = event.relatedTarget;
        let runId = button?.getAttribute('data-run-id');

        // Try multiple ways to get run ID
        if (!runId) {
            runId = button?.getAttribute('data-runid') ||
                button?.dataset?.runId ||
                button?.closest('tr')?.getAttribute('data-run-id');
        }

        // If calendar integration, check for run ID in calendar context
        if (!runId && isFromCalendar && window.fixedRunCalendar) {
            // Calendar might have stored the run ID differently
            runId = button?.getAttribute('data-calendar-run-id') ||
                event.detail?.runId ||
                window.fixedRunCalendar.selectedRunId;
        }

        if (!runId) {
            console.error('❌ No run ID found');
            showToast('Run ID is missing. Please refresh the page.', 'error');
            return;
        }

        console.log('📂 Opening modal for run ID:', runId, isFromCalendar ? '(from calendar)' : '(from table)');

        // Store current run ID and source
        window.runManagementState.currentRunId = runId;
        window.runManagementState.openedFromCalendar = isFromCalendar;

        // Set form values
        safeSetValue('editRunId', runId);
        safeSetValue('deleteRunId', runId);

        // Clear and reset everything
        resetModalState();

        // Load run data with timeout protection
        loadRunDataWithTimeout(runId);
    }

    function handleModalHide() {
        console.log('🚪 Modal closing...');

        // Ensure loading state is hidden
        hideLoadingState();

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

    // ========== ENHANCED DATA LOADING WITH TIMEOUT ==========
    function loadRunDataWithTimeout(runId) {
        console.log('📥 [RunMgmt] Loading run data with timeout protection for ID:', runId);

        if (!runId) {
            console.error('❌ No run ID provided');
            showToast('No run ID provided', 'error');
            return Promise.reject(new Error('No run ID provided'));
        }

        // Check if we're already loading to prevent conflicts
        if (window.runManagementState.isLoading) {
            console.warn('⚠️ Already loading run data, preventing duplicate request');
            return Promise.reject(new Error('Already loading'));
        }

        // Return a promise for calendar compatibility
        return new Promise((resolve, reject) => {
            // Set a timeout to ensure loading state is always hidden
            const timeoutId = setTimeout(() => {
                console.warn('⏰ Loading timeout reached, forcing hide loading state');
                hideLoadingState();
                const error = new Error('Loading took too long. Please try again.');
                showToast(error.message, 'error');
                reject(error);
            }, 10000); // 10 second timeout

            // Show loading state
            showLoadingState();

            const url = `/Run/GetRunData?id=${encodeURIComponent(runId)}`;

            fetch(url, {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                credentials: 'same-origin'
            })
                .then(response => {
                    console.log('📡 Response status:', response.status);

                    if (!response.ok) {
                        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                    }
                    return response.json();
                })
                .then(data => {
                    // Clear timeout and hide loading
                    clearTimeout(timeoutId);
                    hideLoadingState();

                    console.log('📦 Received run data:', data);

                    if (data.success === false) {
                        throw new Error(data.message || 'API returned error');
                    }

                    // Populate all data
                    populateRunDataComplete(data);

                    console.log('✅ Run data loaded and populated successfully');
                    showToast('Run data loaded successfully', 'success');

                    resolve(data);
                })
                .catch(error => {
                    // Clear timeout and hide loading
                    clearTimeout(timeoutId);
                    hideLoadingState();

                    console.error('❌ Error loading run data:', error);
                    showToast(`Error loading run data: ${error.message}`, 'error');

                    // Try to extract data from table as fallback
                    setTimeout(() => {
                        tryExtractFromTable(runId);
                    }, 500);

                    reject(error);
                });
        });
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

    // ========== DATA POPULATION ==========
    function populateRunDataComplete(data) {
        console.log('📝 Populating complete run data:', data);

        try {
            // ENHANCED: Handle run name with client name priority
            const effectiveName = data.name || data.clientName || data.runName || 'Basketball Run';
            console.log('🏷️ Using effective name:', effectiveName);

            // Basic run information - use client name as primary name
            safeSetValue('editRunTitle', effectiveName);
            safeSetValue('editRunDescription', data.description || '');
            safeSetValue('editMaxParticipants', data.playerLimit || 10);

            // Client ID population
            const clientId = data.clientId || data.ClientId || '';
            console.log('🏢 Setting client ID:', clientId);
            safeSetValue('editClientId', clientId);
            safeSetValue('clientId', clientId);
            safeSetValue('editClientName', data.clientName || data.ClientName || '');

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

            // ENHANCED: Court selection with multiple fallback properties and auto-population
            const selectedCourtId = extractCourtId(data);
            console.log('🏀 Extracted court ID for auto-selection:', selectedCourtId);

            if (data.courtList && data.courtList.length > 0) {
                populateCourtDropdown(data.courtList, selectedCourtId);
            } else {
                console.warn('⚠️ No court list provided, attempting to load courts...');
                // If no court list but we have a client ID, try to load courts
                if (clientId) {
                    loadCourtsForClient(clientId, selectedCourtId);
                } else {
                    handleNoCourtList(selectedCourtId);
                }
            }

            // Address handling
            populateAddressDataComplete(data);

            // Store complete data for later use
            window.runManagementState.currentRunData = {
                ...data,
                effectiveName: effectiveName,
                clientId: clientId,
                selectedCourtId: selectedCourtId
            };

            console.log('✅ Run data population complete');

        } catch (error) {
            console.error('❌ Error populating run data:', error);
            showToast('Error populating run data', 'error');
        }
    }

    // NEW: Enhanced court ID extraction with multiple fallbacks
    function extractCourtId(data) {
        // Try multiple possible property names for court ID
        const possibleCourtIds = [
            data.courtId,
            data.CourtId,
            data.court_id,
            data.selectedCourtId,
            data.runCourtId,
            data.Court?.CourtId,
            data.Court?.courtId,
            data.venue?.id,
            data.venueId,
            data.VenueId
        ];

        for (const courtId of possibleCourtIds) {
            if (courtId !== null && courtId !== undefined && courtId !== '') {
                const normalizedId = String(courtId).trim();
                if (normalizedId !== '') {
                    console.log('🏀 Found court ID:', normalizedId, 'from property');
                    return normalizedId;
                }
            }
        }

        console.log('🏀 No court ID found in data');
        return null;
    }

    // NEW: Load courts for client when not provided in initial data
    async function loadCourtsForClient(clientId, selectedCourtId) {
        console.log('🏀 Loading courts for client:', clientId);

        try {
            const response = await fetch(`/Run/GetRunCourts?clientId=${encodeURIComponent(clientId)}`, {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                credentials: 'same-origin'
            });

            if (response.ok) {
                const courtData = await response.json();
                if (courtData.success && courtData.courts) {
                    console.log('🏀 Loaded courts for client:', courtData.courts.length);
                    populateCourtDropdown(courtData.courts, selectedCourtId);
                } else {
                    console.warn('⚠️ Failed to load courts:', courtData.message);
                    handleNoCourtList(selectedCourtId);
                }
            } else {
                console.warn('⚠️ Court loading API not available');
                handleNoCourtList(selectedCourtId);
            }
        } catch (error) {
            console.warn('⚠️ Error loading courts:', error);
            handleNoCourtList(selectedCourtId);
        }
    }

    // NEW: Handle case when no court list is available
    function handleNoCourtList(selectedCourtId) {
        const courtSelect = document.getElementById('editCourtList');
        if (!courtSelect) return;

        if (selectedCourtId) {
            // Create option for the selected court even if we don't have the full list
            courtSelect.innerHTML = `
                <option value="">Select a court...</option>
                <option value="${selectedCourtId}" selected>Current Court (ID: ${selectedCourtId})</option>
            `;
            console.log('🏀 Created option for selected court:', selectedCourtId);
        } else {
            courtSelect.innerHTML = '<option value="">No courts available</option>';
        }
    }

    function populateCourtDropdown(courts, selectedCourtId) {
        console.log('🏀 Populating court dropdown with:', courts.length, 'courts');
        console.log('🏀 Auto-selecting court ID:', selectedCourtId);

        const courtSelect = document.getElementById('editCourtList');
        if (!courtSelect) {
            console.warn('⚠️ Court select element not found');
            return;
        }

        // Clear existing options
        courtSelect.innerHTML = '<option value="">Select a court...</option>';

        // Normalize the selected court ID for comparison
        const normalizedSelectedId = selectedCourtId ? String(selectedCourtId).trim() : null;

        // Add courts from the list
        let selectedFound = false;
        courts.forEach((court, index) => {
            const option = document.createElement('option');

            // Handle different court ID property names
            const courtId = court.courtId || court.CourtId || court.id || court.Id || '';
            const courtName = court.name || court.Name || court.courtName || `Court ${index + 1}`;

            option.value = String(courtId);
            option.textContent = courtName;

            // Enhanced court matching - try multiple comparison methods
            const shouldSelect = normalizedSelectedId && (
                String(courtId).trim() === normalizedSelectedId ||
                String(courtId) === String(selectedCourtId) ||
                courtId === selectedCourtId
            );

            if (shouldSelect) {
                option.selected = true;
                selectedFound = true;
                console.log('🏀 ✅ Auto-selected court:', courtName, '(ID:', courtId, ')');

                // Show user feedback
                setTimeout(() => {
                    showToast(`Court auto-selected: ${courtName}`, 'info');
                }, 500);
            }

            courtSelect.appendChild(option);
        });

        // Handle case where selected court ID doesn't match any available courts
        if (normalizedSelectedId && !selectedFound) {
            console.warn('⚠️ Selected court ID not found in available courts:', normalizedSelectedId);

            // Add the missing court as an option
            const missingOption = document.createElement('option');
            missingOption.value = normalizedSelectedId;
            missingOption.textContent = `Current Court (ID: ${normalizedSelectedId})`;
            missingOption.selected = true;
            courtSelect.appendChild(missingOption);

            console.log('🏀 ⚠️ Added missing court option for ID:', normalizedSelectedId);
            showToast(`Court selection preserved (ID: ${normalizedSelectedId})`, 'warning');
        } else if (!normalizedSelectedId) {
            console.log('🏀 No court pre-selected - user can choose from available courts');
        }

        // Log final state
        const finalSelectedValue = courtSelect.value;
        const finalSelectedText = courtSelect.options[courtSelect.selectedIndex]?.textContent;

        console.log(`✅ Court dropdown populated:`, {
            totalCourts: courts.length,
            selectedValue: finalSelectedValue,
            selectedText: finalSelectedText,
            autoSelected: !!finalSelectedValue
        });

        return selectedFound || !!finalSelectedValue;
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

        console.log('🏠 Address mode:', useCustomAddress ? 'custom' : 'client');

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

        // Hide loading state when using fallback
        hideLoadingState();

        const row = document.querySelector(`tr[data-run-id="${runId}"]`) ||
            document.querySelector(`button[data-run-id="${runId}"]`)?.closest('tr');

        if (!row) {
            console.warn('⚠️ Could not find table row for run');
            showToast('Could not load run data from any source', 'error');
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

        showToast('Basic run data extracted from table', 'info');
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

        // Court selection change handler
        const courtSelect = document.getElementById('editCourtList');
        if (courtSelect) {
            courtSelect.removeEventListener('change', handleCourtSelectionChange);
            courtSelect.addEventListener('change', handleCourtSelectionChange);
        }

        // Schedule conflict validation handlers
        const scheduleFields = ['editRunDate', 'editRunTime', 'editEndTime', 'editCourtList'];
        scheduleFields.forEach(fieldId => {
            const field = document.getElementById(fieldId);
            if (field) {
                field.removeEventListener('change', handleScheduleFieldChange);
                field.addEventListener('change', handleScheduleFieldChange);
            }
        });

        console.log('✅ Form event handlers setup complete with conflict validation');
    }

    function handleScheduleFieldChange() {
        // Debounce the validation to avoid too many API calls
        if (window.runManagementState.scheduleValidationTimeout) {
            clearTimeout(window.runManagementState.scheduleValidationTimeout);
        }

        window.runManagementState.scheduleValidationTimeout = setTimeout(() => {
            validateScheduleConflicts(false); // false = don't block form, just warn
        }, 1000); // Wait 1 second after user stops typing
    }

    // ========== CREATE RUN FORM HANDLERS ==========
    function setupCreateRunHandlers() {
        console.log('📝 Setting up Create Run handlers...');

        const addRunModal = document.getElementById('addRunModal');
        if (!addRunModal) {
            console.warn('⚠️ Add run modal not found');
            return;
        }

        // Modal event handlers
        addRunModal.addEventListener('show.bs.modal', handleCreateRunModalShow);
        addRunModal.addEventListener('hidden.bs.modal', handleCreateRunModalHide);

        // Form submission
        const addRunForm = document.getElementById('addRunForm');
        if (addRunForm) {
            addRunForm.removeEventListener('submit', handleCreateRunSubmit);
            addRunForm.addEventListener('submit', handleCreateRunSubmit);
        }

        // Client selection handler
        const clientSelect = document.getElementById('addClientSelect');
        if (clientSelect) {
            clientSelect.removeEventListener('change', handleCreateClientChange);
            clientSelect.addEventListener('change', handleCreateClientChange);
        }

        // Custom address toggle
        const customAddressToggle = document.getElementById('addUseCustomAddress');
        if (customAddressToggle) {
            customAddressToggle.removeEventListener('change', handleCreateAddressToggle);
            customAddressToggle.addEventListener('change', handleCreateAddressToggle);
        }

        // Schedule field handlers for conflict validation
        const scheduleFields = ['addRunDate', 'addRunTime', 'addEndTime', 'addCourtList'];
        scheduleFields.forEach(fieldId => {
            const field = document.getElementById(fieldId);
            if (field) {
                field.removeEventListener('change', handleCreateScheduleChange);
                field.addEventListener('change', handleCreateScheduleChange);
            }
        });

        // Address field handlers
        const addressFields = ['addAddress', 'addCity', 'addState', 'addZip'];
        addressFields.forEach(fieldId => {
            const field = document.getElementById(fieldId);
            if (field) {
                field.removeEventListener('input', handleCreateAddressFieldChange);
                field.addEventListener('input', handleCreateAddressFieldChange);
            }
        });

        console.log('✅ Create Run handlers setup complete');
    }

    function handleCreateRunModalShow() {
        console.log('📝 Create Run modal opening...');

        // Reset the form
        resetCreateRunForm();

        // Load clients
        loadClientsForCreateRun();

        // Set default date to today
        const today = new Date().toISOString().split('T')[0];
        safeSetValue('addRunDate', today);

        // Set anti-forgery token
        updateAntiForgeryToken('addRunForm');
    }

    function handleCreateRunModalHide() {
        console.log('📝 Create Run modal closing...');
        resetCreateRunForm();
    }

    async function handleCreateRunSubmit(e) {
        e.preventDefault();
        console.log('📝 Create Run form submission started');

        // Validate form
        const isValid = await validateCreateRunForm();
        if (!isValid) {
            return;
        }

        // Get form data
        const formData = getCreateRunFormData();
        console.log('📝 Submitting create run data:', formData);

        const submitBtn = e.target.querySelector('button[type="submit"]');
        if (submitBtn) {
            setButtonLoading(submitBtn, true);
        }

        try {
            const response = await fetch('/Run/Create', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: JSON.stringify(formData)
            });

            const result = await response.json();

            if (submitBtn) {
                setButtonLoading(submitBtn, false);
            }

            if (result.success || response.ok) {
                showToast('Run created successfully!', 'success');
                clearCreateConflictWarnings();

                setTimeout(() => {
                    const modal = bootstrap.Modal.getInstance(document.getElementById('addRunModal'));
                    if (modal) modal.hide();
                    location.reload();
                }, 1000);
            } else {
                showToast(`Error: ${result.message || 'Failed to create run'}`, 'error');
            }
        } catch (error) {
            console.error('❌ Create run submission error:', error);
            if (submitBtn) {
                setButtonLoading(submitBtn, false);
            }
            showToast(`Error: ${error.message}`, 'error');
        }
    }

    async function loadClientsForCreateRun() {
        console.log('🏢 Loading clients for create run...');

        const clientSelect = document.getElementById('addClientSelect');
        if (!clientSelect) return;

        try {
            clientSelect.innerHTML = '<option value="">Loading clients...</option>';

            const response = await fetch('/Client/GetAllClients', {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                credentials: 'same-origin'
            });

            if (response.ok) {
                const data = await response.json();
                const clients = data.clients || data || [];

                // Populate client dropdown
                clientSelect.innerHTML = '<option value="">Select a client...</option>';
                clients.forEach(client => {
                    const option = document.createElement('option');
                    option.value = client.clientId || client.id;
                    option.textContent = client.name || 'Unnamed Client';
                    clientSelect.appendChild(option);
                });

                console.log(`✅ Loaded ${clients.length} clients for create run`);
            } else {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }
        } catch (error) {
            console.error('❌ Error loading clients:', error);
            clientSelect.innerHTML = '<option value="">Error loading clients</option>';
            showToast('Unable to load clients. Please refresh and try again.', 'error');
        }
    }

    async function handleCreateClientChange(e) {
        const clientId = e.target.value;
        const clientName = e.target.options[e.target.selectedIndex]?.textContent;

        console.log('🏢 Client selected for create run:', clientName, '(ID:', clientId, ')');

        if (clientId) {
            // Load courts for selected client
            await loadCourtsForCreateRun(clientId);

            // Load client address information
            await loadClientAddressForCreateRun(clientId);

            showToast(`Client selected: ${clientName}`, 'info');
        } else {
            // Clear courts if no client selected
            const courtSelect = document.getElementById('addCourtList');
            if (courtSelect) {
                courtSelect.innerHTML = '<option value="">First select a client...</option>';
            }

            // Clear address fields
            clearCreateRunAddress();
        }

        // Clear any existing conflict warnings
        clearCreateConflictWarnings();
    }

    async function loadCourtsForCreateRun(clientId) {
        console.log('🏀 Loading courts for create run client:', clientId);

        const courtSelect = document.getElementById('addCourtList');
        if (!courtSelect) return;

        try {
            courtSelect.innerHTML = '<option value="">Loading courts...</option>';

            const response = await fetch(`/Run/GetRunCourts?clientId=${encodeURIComponent(clientId)}`, {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                credentials: 'same-origin'
            });

            if (response.ok) {
                const data = await response.json();
                if (data.success && data.courts) {
                    // Populate court dropdown
                    courtSelect.innerHTML = '<option value="">Select a court...</option>';
                    data.courts.forEach(court => {
                        const option = document.createElement('option');
                        option.value = court.courtId || court.id;
                        option.textContent = court.name || 'Unnamed Court';
                        courtSelect.appendChild(option);
                    });

                    console.log(`✅ Loaded ${data.courts.length} courts for create run`);
                } else {
                    courtSelect.innerHTML = '<option value="">No courts available</option>';
                    console.warn('⚠️ No courts found for client:', clientId);
                }
            } else {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }
        } catch (error) {
            console.error('❌ Error loading courts for create run:', error);
            courtSelect.innerHTML = '<option value="">Error loading courts</option>';
        }
    }

    async function loadClientAddressForCreateRun(clientId) {
        console.log('🏠 Loading client address for create run:', clientId);

        try {
            const response = await fetch(`/Run/GetClientData?clientId=${encodeURIComponent(clientId)}`, {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                credentials: 'same-origin'
            });

            if (response.ok) {
                const data = await response.json();
                if (data.success && data.client) {
                    // Store client address
                    window.runManagementState.createRunClientAddress = {
                        address: data.client.address || '',
                        city: data.client.city || '',
                        state: data.client.state || '',
                        zip: data.client.zip || ''
                    };

                    // Update address fields if not using custom address
                    const useCustom = document.getElementById('addUseCustomAddress')?.checked;
                    if (!useCustom) {
                        populateCreateRunAddress(window.runManagementState.createRunClientAddress);
                    }

                    // Update address preview
                    updateCreateAddressDisplay();

                    console.log('✅ Loaded client address for create run');
                } else {
                    console.warn('⚠️ No client address data found');
                    window.runManagementState.createRunClientAddress = null;
                }
            } else {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }
        } catch (error) {
            console.error('❌ Error loading client address:', error);
            window.runManagementState.createRunClientAddress = null;
        }
    }

    function handleCreateAddressToggle(e) {
        const isCustom = e.target.checked;
        console.log('🏠 Create run address toggle changed to:', isCustom);

        toggleCreateAddressFields(isCustom);
        updateCreateAddressIndicators(isCustom);
        updateCreateAddressDisplay();

        const message = isCustom ?
            'Custom address enabled - you can now edit the address fields' :
            'Using client address - address fields are now read-only';
        showToast(message, 'info');
    }

    function toggleCreateAddressFields(useCustom) {
        const addressFields = ['addAddress', 'addCity', 'addState', 'addZip'];

        addressFields.forEach(fieldId => {
            const field = document.getElementById(fieldId);
            if (field) {
                field.disabled = !useCustom;
                field.readOnly = !useCustom;

                if (useCustom) {
                    field.classList.remove('bg-light');
                    field.style.backgroundColor = '';
                    field.style.cursor = '';
                } else {
                    field.classList.add('bg-light');
                    field.style.backgroundColor = '#f8f9fa';
                    field.style.cursor = 'not-allowed';
                }
            }
        });

        // Update container styling
        const container = document.getElementById('addAddressFieldsContainer');
        if (container) {
            if (useCustom) {
                container.classList.add('custom-address-active');
            } else {
                container.classList.remove('custom-address-active');
            }
        }

        // If switching back to client address, repopulate with client data
        if (!useCustom && window.runManagementState.createRunClientAddress) {
            populateCreateRunAddress(window.runManagementState.createRunClientAddress);
        }
    }

    function updateCreateAddressIndicators(isCustom) {
        const customIndicator = document.getElementById('addCustomAddressIndicator');
        const clientIndicator = document.getElementById('addClientAddressIndicator');
        const sourceText = document.getElementById('addAddressSourceText');

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

    function populateCreateRunAddress(addressData) {
        if (!addressData) return;

        safeSetValue('addAddress', addressData.address || '');
        safeSetValue('addCity', addressData.city || '');
        safeSetValue('addState', addressData.state || '');
        safeSetValue('addZip', addressData.zip || '');
    }

    function clearCreateRunAddress() {
        const addressFields = ['addAddress', 'addCity', 'addState', 'addZip'];
        addressFields.forEach(field => safeSetValue(field, ''));

        window.runManagementState.createRunClientAddress = null;
        updateCreateAddressDisplay();
    }

    function updateCreateAddressDisplay() {
        const previewContainer = document.getElementById('addAddressPreview');
        if (!previewContainer) return;

        const useCustom = document.getElementById('addUseCustomAddress')?.checked;
        const currentAddress = {
            address: safeGetValue('addAddress'),
            city: safeGetValue('addCity'),
            state: safeGetValue('addState'),
            zip: safeGetValue('addZip')
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
            const clientAddr = window.runManagementState.createRunClientAddress || {};
            previewHtml = `
                <div class="address-preview client-address border border-secondary bg-light p-3 rounded">
                    <div class="d-flex align-items-center mb-2">
                        <i class="bi bi-building text-muted me-2"></i>
                        <strong class="text-muted">Client Address</strong>
                    </div>
                    <div class="address-text">
                        ${clientAddr.address || 'Select a client to see address'}<br>
                        ${clientAddr.city || ''}, ${clientAddr.state || ''} ${clientAddr.zip || ''}
                    </div>
                </div>
            `;
        }

        previewContainer.innerHTML = previewHtml;
    }

    function handleCreateScheduleChange() {
        // Debounce the validation to avoid too many API calls
        if (window.runManagementState.createScheduleValidationTimeout) {
            clearTimeout(window.runManagementState.createScheduleValidationTimeout);
        }

        window.runManagementState.createScheduleValidationTimeout = setTimeout(() => {
            validateCreateScheduleConflicts(false); // false = don't block form, just warn
        }, 1000);
    }

    function handleCreateAddressFieldChange() {
        const useCustom = document.getElementById('addUseCustomAddress')?.checked;
        if (useCustom) {
            updateCreateAddressDisplay();
        }
    }

    async function validateCreateScheduleConflicts(blockingValidation = true) {
        console.log('⏰ Validating create run schedule conflicts...');

        const runDate = safeGetValue('addRunDate');
        const startTime = safeGetValue('addRunTime');
        const endTime = safeGetValue('addEndTime');
        const courtId = safeGetValue('addCourtList');

        // Check if we have the minimum required fields
        if (!runDate || !startTime || !courtId) {
            console.log('⏰ Skipping create run conflict validation - missing required fields');
            clearCreateConflictWarnings();
            return { valid: true, message: 'Incomplete data for validation' };
        }

        try {
            showCreateConflictValidationLoading(true);

            const conflicts = await checkForConflictingRuns({
                runDate,
                startTime,
                endTime,
                courtId
                // No excludeRunId for create run
            });

            showCreateConflictValidationLoading(false);

            if (conflicts.length > 0) {
                const conflictMessage = formatConflictMessage(conflicts);
                console.warn('⚠️ Create run schedule conflicts found:', conflicts);

                if (blockingValidation) {
                    showToast(`Schedule Conflict: ${conflictMessage}`, 'error');
                    showCreateConflictWarnings(conflicts);
                    return { valid: false, message: conflictMessage, conflicts: conflicts };
                } else {
                    showCreateConflictWarnings(conflicts, false);
                    showToast(`Warning: ${conflictMessage}`, 'warning');
                    return { valid: true, message: conflictMessage, conflicts: conflicts, warning: true };
                }
            } else {
                console.log('✅ No create run schedule conflicts found');
                clearCreateConflictWarnings();

                if (!blockingValidation) {
                    showToast('Schedule looks good - no conflicts found', 'success');
                }

                return { valid: true, message: 'No conflicts found' };
            }

        } catch (error) {
            console.error('❌ Error validating create run schedule conflicts:', error);
            showCreateConflictValidationLoading(false);

            if (blockingValidation) {
                showToast('Unable to validate schedule conflicts. Please try again.', 'error');
                return { valid: false, message: 'Validation error: ' + error.message };
            } else {
                console.warn('⚠️ Non-blocking create run conflict validation failed, allowing form to proceed');
                return { valid: true, message: 'Validation error (non-blocking)' };
            }
        }
    }

    function showCreateConflictWarnings(conflicts, isError = true) {
        clearCreateConflictWarnings();

        const warningContainer = getOrCreateCreateConflictWarningContainer();

        const warningClass = isError ? 'alert-danger' : 'alert-warning';
        const warningIcon = isError ? 'bi-exclamation-triangle-fill' : 'bi-exclamation-triangle';
        const warningTitle = isError ? 'Schedule Conflict' : 'Schedule Warning';

        let warningHtml = `
            <div class="alert ${warningClass} d-flex align-items-start" role="alert" id="createScheduleConflictWarning">
                <i class="bi ${warningIcon} flex-shrink-0 me-2"></i>
                <div>
                    <h6 class="alert-heading mb-2">${warningTitle}</h6>
                    <p class="mb-2">${formatConflictMessage(conflicts)}</p>
        `;

        if (conflicts.length > 0 && conflicts.length <= 3) {
            warningHtml += '<div class="mb-0"><strong>Conflicting runs:</strong><ul class="mb-0 mt-1">';
            conflicts.forEach(conflict => {
                const name = conflict.name || conflict.runName || 'Unnamed run';
                const time = conflict.startTime || 'Unknown time';
                const duration = conflict.endTime ? ` - ${conflict.endTime}` : '';
                warningHtml += `<li>${name} (${time}${duration})</li>`;
            });
            warningHtml += '</ul></div>';
        }

        warningHtml += `
                </div>
            </div>
        `;

        warningContainer.innerHTML = warningHtml;

        setTimeout(() => {
            warningContainer.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }, 100);
    }

    function clearCreateConflictWarnings() {
        const warningContainer = document.getElementById('addScheduleConflictContainer');
        if (warningContainer) {
            warningContainer.innerHTML = '';
        }

        const warningElement = document.getElementById('createScheduleConflictWarning');
        if (warningElement) {
            warningElement.remove();
        }
    }

    function getOrCreateCreateConflictWarningContainer() {
        let container = document.getElementById('addScheduleConflictContainer');
        if (!container) {
            container = document.createElement('div');
            container.id = 'addScheduleConflictContainer';
            container.className = 'mb-3';

            const modalFooter = document.querySelector('#addRunModal .modal-footer');
            if (modalFooter) {
                modalFooter.parentNode.insertBefore(container, modalFooter);
            }
        }
        return container;
    }

    function showCreateConflictValidationLoading(show) {
        let loadingElement = document.getElementById('createConflictValidationLoading');

        if (show) {
            if (!loadingElement) {
                loadingElement = document.createElement('div');
                loadingElement.id = 'createConflictValidationLoading';
                loadingElement.className = 'text-center text-muted small mt-2';
                loadingElement.innerHTML = `
                    <div class="spinner-border spinner-border-sm me-2" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    Checking for schedule conflicts...
                `;

                const courtSelect = document.getElementById('addCourtList');
                if (courtSelect && courtSelect.parentNode) {
                    courtSelect.parentNode.appendChild(loadingElement);
                }
            }
            loadingElement.style.display = 'block';
        } else {
            if (loadingElement) {
                loadingElement.style.display = 'none';
            }
        }
    }

    async function validateCreateRunForm() {
        console.log('📝 Validating create run form...');

        const errors = [];

        if (!safeGetValue('addRunTitle').trim()) {
            errors.push('Run title is required');
        }

        if (!safeGetValue('addClientSelect')) {
            errors.push('Client selection is required');
        }

        if (!safeGetValue('addRunDate')) {
            errors.push('Run date is required');
        }

        if (!safeGetValue('addRunTime')) {
            errors.push('Start time is required');
        }

        const playerLimit = parseInt(safeGetValue('addMaxParticipants'));
        if (!playerLimit || playerLimit < 1) {
            errors.push('Player limit must be greater than 0');
        }

        if (!safeGetValue('addRunDescription').trim()) {
            errors.push('Run description is required');
        }

        // Validate custom address if enabled
        const useCustom = document.getElementById('addUseCustomAddress')?.checked;
        if (useCustom) {
            if (!safeGetValue('addAddress').trim()) {
                errors.push('Address is required when using custom address');
            }
            if (!safeGetValue('addCity').trim()) {
                errors.push('City is required when using custom address');
            }
            if (!safeGetValue('addState').trim()) {
                errors.push('State is required when using custom address');
            }
        }

        // Show basic validation errors first
        if (errors.length > 0) {
            showToast(`Please fix the following errors:\n• ${errors.join('\n• ')}`, 'error');
            return false;
        }

        // Check for schedule conflicts if court is selected
        const courtId = safeGetValue('addCourtList');
        if (courtId) {
            try {
                console.log('⏰ Performing create run conflict validation...');
                const conflictResult = await validateCreateScheduleConflicts(true);

                if (!conflictResult.valid) {
                    console.warn('❌ Create run validation failed due to schedule conflicts');
                    return false;
                }

                console.log('✅ No conflicts found, create run validation passed');
            } catch (error) {
                console.error('❌ Error during create run conflict validation:', error);
                showToast('Unable to validate schedule. Please try again.', 'error');
                return false;
            }
        }

        return true;
    }

    function getCreateRunFormData() {
        const useCustom = document.getElementById('addUseCustomAddress')?.checked;

        const formData = {
            Name: safeGetValue('addRunTitle'),
            Description: safeGetValue('addRunDescription'),
            RunDate: safeGetValue('addRunDate'),
            StartTime: safeGetValue('addRunTime'),
            EndTime: safeGetValue('addEndTime') || null,
            PlayerLimit: parseInt(safeGetValue('addMaxParticipants')) || 10,
            SkillLevel: safeGetValue('addSkillLevel'),
            Type: safeGetValue('addRunType'),
            Status: safeGetValue('addStatus'),
            IsPublic: safeGetValue('addIsPublic') === 'true',
            TeamType: safeGetValue('addTeamType'),
            IsOutdoor: safeGetValue('addCourtType') === 'true',
            ClientId: safeGetValue('addClientSelect'),
            CourtId: safeGetValue('addCourtList') || null,
            UseCustomAddress: useCustom
        };

        // Add address data
        if (useCustom) {
            formData.CustomAddress = safeGetValue('addAddress');
            formData.CustomCity = safeGetValue('addCity');
            formData.CustomState = safeGetValue('addState');
            formData.CustomZip = safeGetValue('addZip');
        }

        formData.Address = safeGetValue('addAddress');
        formData.City = safeGetValue('addCity');
        formData.State = safeGetValue('addState');
        formData.Zip = safeGetValue('addZip');

        console.log('📝 Create run form data:', formData);
        return formData;
    }

    function resetCreateRunForm() {
        console.log('🧹 Resetting create run form...');

        // Clear all form fields
        const fields = [
            'addRunTitle', 'addRunDescription', 'addRunDate', 'addRunTime', 'addEndTime',
            'addMaxParticipants', 'addAddress', 'addCity', 'addState', 'addZip'
        ];
        fields.forEach(field => safeSetValue(field, ''));

        // Reset selects to default values
        const selects = [
            { id: 'addClientSelect', defaultIndex: 0 },
            { id: 'addCourtList', defaultIndex: 0 },
            { id: 'addSkillLevel', defaultValue: 'Intermediate' },
            { id: 'addRunType', defaultValue: 'Pickup' },
            { id: 'addIsPublic', defaultValue: 'true' },
            { id: 'addTeamType', defaultValue: 'Individual' },
            { id: 'addStatus', defaultValue: 'Active' },
            { id: 'addCourtType', defaultValue: 'false' }
        ];

        selects.forEach(selectInfo => {
            const select = document.getElementById(selectInfo.id);
            if (select) {
                if (selectInfo.defaultValue) {
                    select.value = selectInfo.defaultValue;
                } else {
                    select.selectedIndex = selectInfo.defaultIndex || 0;
                }
            }
        });

        // Reset checkboxes
        const customAddressToggle = document.getElementById('addUseCustomAddress');
        if (customAddressToggle) {
            customAddressToggle.checked = false;
        }

        // Reset address fields state
        toggleCreateAddressFields(false);
        updateCreateAddressIndicators(false);

        // Clear stored state
        window.runManagementState.createRunClientAddress = null;

        // Clear conflict warnings
        clearCreateConflictWarnings();

        // Clear validation timers
        if (window.runManagementState.createScheduleValidationTimeout) {
            clearTimeout(window.runManagementState.createScheduleValidationTimeout);
            window.runManagementState.createScheduleValidationTimeout = null;
        }

        // Clear address preview
        updateCreateAddressDisplay();

        console.log('✅ Create run form reset complete');
    }

    function updateAntiForgeryToken(formId) {
        const form = document.getElementById(formId);
        if (!form) return;

        const existingToken = form.querySelector('input[name="__RequestVerificationToken"]');
        const globalToken = document.querySelector('input[name="__RequestVerificationToken"]');

        if (existingToken && globalToken) {
            existingToken.value = globalToken.value;
        }
    }
    async function validateScheduleConflicts(blockingValidation = true) {
        console.log('⏰ Validating schedule conflicts...');

        const runDate = safeGetValue('editRunDate');
        const startTime = safeGetValue('editRunTime');
        const endTime = safeGetValue('editEndTime');
        const courtId = safeGetValue('editCourtList');
        const currentRunId = safeGetValue('editRunId');

        // Check if we have the minimum required fields
        if (!runDate || !startTime || !courtId) {
            console.log('⏰ Skipping conflict validation - missing required fields');
            clearConflictWarnings();
            return { valid: true, message: 'Incomplete data for validation' };
        }

        try {
            // Show loading indicator for validation
            showConflictValidationLoading(true);

            const conflicts = await checkForConflictingRuns({
                runDate,
                startTime,
                endTime,
                courtId,
                excludeRunId: currentRunId // Exclude current run when editing
            });

            showConflictValidationLoading(false);

            if (conflicts.length > 0) {
                const conflictMessage = formatConflictMessage(conflicts);
                console.warn('⚠️ Schedule conflicts found:', conflicts);

                if (blockingValidation) {
                    showToast(`Schedule Conflict: ${conflictMessage}`, 'error');
                    showConflictWarnings(conflicts);
                    return { valid: false, message: conflictMessage, conflicts: conflicts };
                } else {
                    // Non-blocking validation - just show warning
                    showConflictWarnings(conflicts, false);
                    showToast(`Warning: ${conflictMessage}`, 'warning');
                    return { valid: true, message: conflictMessage, conflicts: conflicts, warning: true };
                }
            } else {
                console.log('✅ No schedule conflicts found');
                clearConflictWarnings();

                if (!blockingValidation) {
                    showToast('Schedule looks good - no conflicts found', 'success');
                }

                return { valid: true, message: 'No conflicts found' };
            }

        } catch (error) {
            console.error('❌ Error validating schedule conflicts:', error);
            showConflictValidationLoading(false);

            if (blockingValidation) {
                showToast('Unable to validate schedule conflicts. Please try again.', 'error');
                return { valid: false, message: 'Validation error: ' + error.message };
            } else {
                // For non-blocking validation, don't show errors to user
                console.warn('⚠️ Non-blocking conflict validation failed, allowing form to proceed');
                return { valid: true, message: 'Validation error (non-blocking)' };
            }
        }
    }

    async function checkForConflictingRuns(scheduleData) {
        const { runDate, startTime, endTime, courtId, excludeRunId } = scheduleData;

        // Build query parameters
        const params = new URLSearchParams({
            date: runDate,
            courtId: courtId,
            startTime: startTime
        });

        if (endTime) {
            params.append('endTime', endTime);
        }

        if (excludeRunId) {
            params.append('excludeRunId', excludeRunId);
        }

        const url = `/Run/CheckScheduleConflicts?${params.toString()}`;
        console.log('📡 Checking conflicts at:', url);

        const response = await fetch(url, {
            method: 'GET',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            credentials: 'same-origin'
        });

        if (!response.ok) {
            // If endpoint doesn't exist, try alternative method
            if (response.status === 404) {
                console.warn('⚠️ Schedule conflict API not available, using fallback validation');
                return await checkConflictsUsingRunsData(scheduleData);
            } else {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }
        }

        const data = await response.json();

        if (data.success === false) {
            throw new Error(data.message || 'Conflict check API returned error');
        }

        return data.conflicts || data.conflictingRuns || [];
    }

    async function checkConflictsUsingRunsData(scheduleData) {
        console.log('⏰ Using fallback conflict checking with existing runs data');

        const { runDate, startTime, endTime, courtId, excludeRunId } = scheduleData;

        try {
            // Try to get runs for the specific date
            const targetDate = new Date(runDate);
            const startOfDay = new Date(targetDate);
            startOfDay.setHours(0, 0, 0, 0);
            const endOfDay = new Date(targetDate);
            endOfDay.setHours(23, 59, 59, 999);

            const response = await fetch(`/Run/GetRunsForCalendar?startDate=${startOfDay.toISOString()}&endDate=${endOfDay.toISOString()}`, {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                credentials: 'same-origin'
            });

            if (!response.ok) {
                console.warn('⚠️ Fallback conflict check also failed, skipping validation');
                return [];
            }

            const data = await response.json();
            const runs = data.runs || [];

            // Filter and check for conflicts manually
            const conflicts = runs.filter(run => {
                // Skip if same run (when editing)
                if (excludeRunId && run.runId === excludeRunId) {
                    return false;
                }

                // Check if same court
                if (run.courtId !== courtId) {
                    return false;
                }

                // Check if same date
                const runDate_obj = new Date(run.runDate || run.date);
                const targetDate_obj = new Date(runDate);
                if (runDate_obj.toDateString() !== targetDate_obj.toDateString()) {
                    return false;
                }

                // Check time overlap
                return checkTimeOverlap(
                    { start: startTime, end: endTime },
                    { start: run.startTime, end: run.endTime }
                );
            });

            console.log(`⏰ Fallback found ${conflicts.length} conflicts`);
            return conflicts;

        } catch (error) {
            console.warn('⚠️ Fallback conflict check failed:', error);
            return []; // Return empty array to allow form submission
        }
    }

    function checkTimeOverlap(time1, time2) {
        const start1 = parseTimeToMinutes(time1.start);
        const end1 = time1.end ? parseTimeToMinutes(time1.end) : start1 + 120; // Default 2 hours if no end time
        const start2 = parseTimeToMinutes(time2.start);
        const end2 = time2.end ? parseTimeToMinutes(time2.end) : start2 + 120;

        // Check for overlap (start of one is before end of other and vice versa)
        return start1 < end2 && start2 < end1;
    }

    function parseTimeToMinutes(timeString) {
        if (!timeString) return 0;

        try {
            // Handle different time formats
            let time = timeString;

            // If it's in HH:MM format
            if (time.includes(':')) {
                const [hours, minutes] = time.split(':').map(Number);
                return hours * 60 + minutes;
            }

            // If it's a time with AM/PM
            if (time.includes('AM') || time.includes('PM')) {
                const isPM = time.includes('PM');
                const cleanTime = time.replace(/[^\d:]/g, '');
                const [hours, minutes = 0] = cleanTime.split(':').map(Number);
                let totalHours = hours;

                if (isPM && hours !== 12) totalHours += 12;
                if (!isPM && hours === 12) totalHours = 0;

                return totalHours * 60 + minutes;
            }

            return 0;
        } catch (error) {
            console.warn('Error parsing time:', timeString, error);
            return 0;
        }
    }

    function formatConflictMessage(conflicts) {
        if (conflicts.length === 0) return '';

        if (conflicts.length === 1) {
            const conflict = conflicts[0];
            const conflictName = conflict.name || conflict.runName || 'Another run';
            const conflictTime = conflict.startTime || 'unknown time';
            return `${conflictName} is already scheduled at ${conflictTime} on the same court.`;
        } else {
            return `${conflicts.length} other runs are already scheduled at overlapping times on the same court.`;
        }
    }

    function showConflictWarnings(conflicts, isError = true) {
        // Remove existing warnings
        clearConflictWarnings();

        // Show warning in the form
        const warningContainer = getOrCreateConflictWarningContainer();

        const warningClass = isError ? 'alert-danger' : 'alert-warning';
        const warningIcon = isError ? 'bi-exclamation-triangle-fill' : 'bi-exclamation-triangle';
        const warningTitle = isError ? 'Schedule Conflict' : 'Schedule Warning';

        let warningHtml = `
            <div class="alert ${warningClass} d-flex align-items-start" role="alert" id="scheduleConflictWarning">
                <i class="bi ${warningIcon} flex-shrink-0 me-2"></i>
                <div>
                    <h6 class="alert-heading mb-2">${warningTitle}</h6>
                    <p class="mb-2">${formatConflictMessage(conflicts)}</p>
        `;

        if (conflicts.length > 0 && conflicts.length <= 3) {
            warningHtml += '<div class="mb-0"><strong>Conflicting runs:</strong><ul class="mb-0 mt-1">';
            conflicts.forEach(conflict => {
                const name = conflict.name || conflict.runName || 'Unnamed run';
                const time = conflict.startTime || 'Unknown time';
                const duration = conflict.endTime ? ` - ${conflict.endTime}` : '';
                warningHtml += `<li>${name} (${time}${duration})</li>`;
            });
            warningHtml += '</ul></div>';
        }

        warningHtml += `
                </div>
            </div>
        `;

        warningContainer.innerHTML = warningHtml;

        // Scroll to warning
        setTimeout(() => {
            warningContainer.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }, 100);
    }

    function clearConflictWarnings() {
        const warningContainer = document.getElementById('scheduleConflictContainer');
        if (warningContainer) {
            warningContainer.innerHTML = '';
        }

        const warningElement = document.getElementById('scheduleConflictWarning');
        if (warningElement) {
            warningElement.remove();
        }
    }

    function getOrCreateConflictWarningContainer() {
        let container = document.getElementById('scheduleConflictContainer');
        if (!container) {
            container = document.createElement('div');
            container.id = 'scheduleConflictContainer';
            container.className = 'mb-3';

            // Insert before the form buttons
            const modalFooter = document.querySelector('#editRunModal .modal-footer');
            if (modalFooter) {
                modalFooter.parentNode.insertBefore(container, modalFooter);
            } else {
                // Fallback: add at end of modal body
                const modalBody = document.querySelector('#editRunModal .modal-body');
                if (modalBody) {
                    modalBody.appendChild(container);
                }
            }
        }
        return container;
    }

    function showConflictValidationLoading(show) {
        let loadingElement = document.getElementById('conflictValidationLoading');

        if (show) {
            if (!loadingElement) {
                loadingElement = document.createElement('div');
                loadingElement.id = 'conflictValidationLoading';
                loadingElement.className = 'text-center text-muted small mt-2';
                loadingElement.innerHTML = `
                    <div class="spinner-border spinner-border-sm me-2" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    Checking for schedule conflicts...
                `;

                // Add after the court selection
                const courtSelect = document.getElementById('editCourtList');
                if (courtSelect && courtSelect.parentNode) {
                    courtSelect.parentNode.appendChild(loadingElement);
                }
            }
            loadingElement.style.display = 'block';
        } else {
            if (loadingElement) {
                loadingElement.style.display = 'none';
            }
        }
    }

    async function handleFormSubmit(e) {
        e.preventDefault();
        console.log('📤 Form submission started');

        // Validate form (now includes conflict checking)
        const isValid = await validateForm();
        if (!isValid) {
            return;
        }

        // Get form data
        const formData = getCompleteFormData();

        // Additional validation for debugging
        console.log('📤 Form submission details:', {
            runId: formData.RunId,
            runName: formData.Name,
            courtId: formData.CourtId,
            clientId: formData.ClientId,
            useCustomAddress: formData.UseCustomAddress
        });

        console.log('📤 Submitting complete form data:', formData);

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
                    clearConflictWarnings(); // Clear any warnings
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
            IsPublic: safeGetValue('editIsPublic') === 'true',

            // Add court selection
            CourtId: safeGetValue('editCourtList') || null,

            // Add client ID (important for court association)
            ClientId: safeGetValue('editClientId') || null
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

        // Log the form data for debugging
        console.log('📤 Complete form data being sent:', formData);

        // Specifically log court selection
        if (formData.CourtId) {
            console.log('🏀 Selected Court ID:', formData.CourtId);
        } else {
            console.log('⚠️ No court selected');
        }

        return formData;
    }

    async function validateForm() {
        console.log('📋 Validating form with conflict checking...');

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

        // Optional: Validate court selection if required by business rules
        const courtId = safeGetValue('editCourtList');
        if (!courtId) {
            console.log('⚠️ No court selected - this may be intentional');
            // Uncomment the line below if court selection should be required:
            // errors.push('Please select a court for this run');
        }

        // Show basic validation errors first
        if (errors.length > 0) {
            showToast(`Please fix the following errors:\n• ${errors.join('\n• ')}`, 'error');
            return false;
        }

        // If basic validation passes and we have a court selected, check for conflicts
        if (courtId) {
            try {
                console.log('⏰ Performing conflict validation before form submission...');
                const conflictResult = await validateScheduleConflicts(true); // true = blocking validation

                if (!conflictResult.valid) {
                    console.warn('❌ Form validation failed due to schedule conflicts');
                    return false;
                }

                console.log('✅ No conflicts found, form validation passed');
            } catch (error) {
                console.error('❌ Error during conflict validation:', error);
                showToast('Unable to validate schedule. Please try again.', 'error');
                return false;
            }
        } else {
            console.log('⚠️ Skipping conflict validation - no court selected');
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

        // Ensure loading is hidden
        hideLoadingState();

        // Clear form fields
        const fields = [
            'editRunTitle', 'editRunDescription', 'editRunDate',
            'editRunTime', 'editEndTime', 'editMaxParticipants',
            'editAddress', 'editCity', 'editState', 'editZip'
        ];

        fields.forEach(field => safeSetValue(field, ''));

        // Reset selects
        const selects = ['editSkillLevel', 'editRunType', 'editStatus', 'editIsPublic', 'editCourtList'];
        selects.forEach(select => {
            const element = document.getElementById(select);
            if (element) {
                element.selectedIndex = 0;
                console.log(`🔄 Reset select: ${select}`);
            }
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

        // Reset court selection
        const courtSelect = document.getElementById('editCourtList');
        if (courtSelect) {
            courtSelect.innerHTML = '<option value="">Loading courts...</option>';
            console.log('🏀 Court dropdown reset');
        }

        // Clear conflict warnings
        clearConflictWarnings();

        // Clear any validation timers
        if (window.runManagementState.scheduleValidationTimeout) {
            clearTimeout(window.runManagementState.scheduleValidationTimeout);
            window.runManagementState.scheduleValidationTimeout = null;
        }

        console.log('✅ Modal state reset complete');
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
        // Use run management specific toast to avoid conflicts
        if (window.UIUtils && window.UIUtils.showRunManagementToast) {
            window.UIUtils.showRunManagementToast(message, type);
        } else if (window.UIUtils && window.UIUtils.showToast) {
            window.UIUtils.showToast(`[Run Mgmt] ${message}`, type);
        } else {
            console.log(`[RunMgmt] ${type}: ${message}`);
            showRunManagementToast(message, type);
        }
    }

    function setButtonLoading(button, loading) {
        if (window.UIUtils && window.UIUtils.setButtonLoading) {
            window.UIUtils.setButtonLoading(button, loading);
        } else {
            if (loading) {
                button.disabled = true;
                button.textContent = 'Loading...';
            } else {
                button.disabled = false;
                button.textContent = 'Save Changes';
            }
        }
    }

    // ========== GLOBAL API (CALENDAR COMPATIBLE) ==========
    // Use different function names to avoid conflicts with calendar
    window.loadRunDataFixed = loadRunDataWithTimeout;
    window.populateRunDetailsFixed = populateRunDataComplete;
    window.toggleCustomAddressFieldsFixed = toggleAddressFieldsState;
    window.setupCustomAddressToggleFixed = setupCustomAddressToggle;
    window.updateAddressDisplayFixed = updateAddressDisplay;
    window.hideRunLoadingStateFixed = hideLoadingState;
    window.showRunLoadingStateFixed = showLoadingState;
    window.extractCourtIdFixed = extractCourtId;
    window.validateScheduleConflictsFixed = validateScheduleConflicts;

    // NEW: Create Run API functions
    window.loadClientsForCreateRunFixed = loadClientsForCreateRun;
    window.validateCreateRunFormFixed = validateCreateRunForm;
    window.resetCreateRunFormFixed = resetCreateRunForm;
    window.validateCreateScheduleConflictsFixed = validateCreateScheduleConflicts;

    // Keep original names for backwards compatibility, but check for conflicts
    if (!window.loadRunDataEnhanced || window.runManagementState.calendarCompatible) {
        window.loadRunDataEnhanced = loadRunDataWithTimeout;
    }
    if (!window.populateRunDetailsEnhanced || window.runManagementState.calendarCompatible) {
        window.populateRunDetailsEnhanced = populateRunDataComplete;
    }

    // Debug functions with unique namespace
    window.runDebugFixed = {
        state: () => window.runManagementState,
        setupToggle: setupCustomAddressToggle,
        toggleFields: toggleAddressFieldsState,
        loadData: loadRunDataWithTimeout,
        populateData: populateRunDataComplete,
        resetState: resetModalState,
        updateDisplay: updateAddressDisplay,
        hideLoading: hideLoadingState,
        showLoading: showLoadingState,

        // Court-related debug functions
        extractCourtId: extractCourtId,
        populateCourtDropdown: populateCourtDropdown,
        loadCourtsForClient: loadCourtsForClient,

        // Conflict validation functions
        validateScheduleConflicts: validateScheduleConflicts,
        checkForConflictingRuns: checkForConflictingRuns,
        checkTimeOverlap: checkTimeOverlap,
        parseTimeToMinutes: parseTimeToMinutes,
        clearConflictWarnings: clearConflictWarnings,

        // Create run functions
        setupCreateRunHandlers: setupCreateRunHandlers,
        loadClientsForCreateRun: loadClientsForCreateRun,
        validateCreateRunForm: validateCreateRunForm,
        resetCreateRunForm: resetCreateRunForm,
        getCreateRunFormData: getCreateRunFormData,
        validateCreateScheduleConflicts: validateCreateScheduleConflicts,
        clearCreateConflictWarnings: clearCreateConflictWarnings,

        forceHideLoading: () => {
            console.log('🚨 Force hiding all loading states');
            hideLoadingState();
            hideModalLoadingState();

            // Force hide all possible loading overlays
            const overlays = [
                'runManagementLoading',
                'loadingSpinner',
                'globalLoadingOverlay'
            ];

            overlays.forEach(id => {
                const overlay = document.getElementById(id);
                if (overlay) {
                    overlay.style.display = 'none';
                    console.log(`🚨 Force hid: ${id}`);
                }
            });

            // Force hide any modal loading overlays
            const modalLoadingOverlays = document.querySelectorAll('.modal-loading-overlay');
            modalLoadingOverlays.forEach(overlay => {
                overlay.style.display = 'none';
                console.log('🚨 Force hid modal loading overlay');
            });

            // Reset loading state
            window.runManagementState.isLoading = false;

            console.log('🚨 All loading states forcefully hidden');
        },
        checkConflicts: () => {
            const conflicts = [];

            // Check for calendar presence
            if (window.fixedRunCalendar) {
                conflicts.push('Calendar detected: window.fixedRunCalendar exists');
            }

            // Check for function conflicts
            const conflictingFunctions = [
                'loadRunDataEnhanced',
                'populateRunDetailsEnhanced'
            ];

            conflictingFunctions.forEach(funcName => {
                if (window[funcName] && window[funcName].toString().includes('calendar')) {
                    conflicts.push(`Function conflict: ${funcName} may be from calendar`);
                }
            });

            // Check for UIUtils conflicts
            if (window.UIUtils) {
                if (window.UIUtils.showRunManagementLoading) {
                    conflicts.push('UIUtils enhanced for run management');
                } else {
                    conflicts.push('UIUtils exists but not enhanced - may be from calendar');
                }

                // Check toast system
                if (window.UIUtils.showToast) {
                    const toastStr = window.UIUtils.showToast.toString();
                    if (toastStr.includes('RunMgmt Fallback')) {
                        conflicts.push('Using fallback toast - utilities.js not detected');
                    } else {
                        conflicts.push('utilities.js toast system detected and active');
                    }
                }
            }

            console.log('🔍 Conflict Analysis:', conflicts.length === 0 ? 'No conflicts detected' : conflicts);
            return conflicts;
        },

        testToast: (message = 'Test toast from run management', type = 'info') => {
            console.log('🧪 Testing toast system...');
            showToast(message, type);
        },

        getFormData: () => {
            console.log('📋 Current form data:');
            const formData = getCompleteFormData();
            console.table(formData);
            return formData;
        },

        getCourtInfo: () => {
            const courtSelect = document.getElementById('editCourtList');
            const selectedCourtId = safeGetValue('editCourtList');
            const currentRunData = window.runManagementState.currentRunData;

            const courtInfo = {
                selectedCourtId: selectedCourtId,
                availableOptions: [],
                selectElement: !!courtSelect,
                selectValue: courtSelect?.value,
                selectedOptionText: courtSelect?.options[courtSelect?.selectedIndex]?.textContent,
                autoPopulated: false,
                currentRunCourtId: currentRunData?.selectedCourtId || null,
                rawCourtData: {
                    courtId: currentRunData?.courtId,
                    CourtId: currentRunData?.CourtId,
                    extractedCourtId: currentRunData?.selectedCourtId
                }
            };

            if (courtSelect) {
                for (let i = 0; i < courtSelect.options.length; i++) {
                    const option = courtSelect.options[i];
                    courtInfo.availableOptions.push({
                        value: option.value,
                        text: option.textContent,
                        selected: option.selected
                    });
                }

                // Check if auto-populated
                courtInfo.autoPopulated = courtSelect.selectedIndex > 0; // Greater than 0 means not the default "Select a court..." option
            }

            console.log('🏀 Court Selection Info:', courtInfo);
            return courtInfo;
        },

        testCourtSelection: (courtId) => {
            console.log('🧪 Testing court selection with ID:', courtId);
            const courtSelect = document.getElementById('editCourtList');
            if (courtSelect) {
                courtSelect.value = courtId;
                courtSelect.dispatchEvent(new Event('change'));
                console.log('🏀 Court selection updated');

                // Verify selection
                const selected = courtSelect.options[courtSelect.selectedIndex];
                console.log('🏀 Verified selection:', {
                    value: selected.value,
                    text: selected.textContent,
                    selected: selected.selected
                });
            } else {
                console.error('❌ Court select element not found');
            }
        },

        simulateCourtAutoPopulation: (mockRunData) => {
            console.log('🧪 Simulating court auto-population with mock data...');
            const testData = mockRunData || {
                courtId: 'test-court-123',
                courtList: [
                    { courtId: 'court-1', name: 'Court 1' },
                    { courtId: 'test-court-123', name: 'Test Court' },
                    { courtId: 'court-3', name: 'Court 3' }
                ]
            };

            const extractedCourtId = extractCourtId(testData);
            console.log('🏀 Extracted court ID:', extractedCourtId);

            if (testData.courtList) {
                const success = populateCourtDropdown(testData.courtList, extractedCourtId);
                console.log('🏀 Auto-population success:', success);
                return success;
            } else {
                console.log('🏀 No court list in test data');
                return false;
            }
        },

        verifyAutoPopulation: () => {
            const courtInfo = window.runDebugFixed.getCourtInfo();
            const isAutoPopulated = courtInfo.autoPopulated && courtInfo.selectedCourtId;

            console.log('🔍 Auto-population verification:', {
                isAutoPopulated: isAutoPopulated,
                selectedCourtId: courtInfo.selectedCourtId,
                selectedText: courtInfo.selectedOptionText,
                hasCurrentRunData: !!window.runManagementState.currentRunData,
                expectedCourtId: courtInfo.currentRunCourtId
            });

            if (isAutoPopulated) {
                console.log('✅ Court auto-population working correctly');
            } else {
                console.log('❌ Court auto-population may not be working');
            }

            return isAutoPopulated;
        },

        // ========== CONFLICT VALIDATION DEBUG FUNCTIONS ==========
        testConflictValidation: async (testData) => {
            console.log('🧪 Testing conflict validation...');

            const defaultTestData = {
                runDate: '2024-12-15',
                startTime: '18:00',
                endTime: '20:00',
                courtId: 'test-court-123'
            };

            const scheduleData = testData || defaultTestData;
            console.log('🧪 Using test data:', scheduleData);

            try {
                const result = await validateScheduleConflicts(false); // Non-blocking test
                console.log('🧪 Conflict validation result:', result);
                return result;
            } catch (error) {
                console.error('🧪 Conflict validation test failed:', error);
                return { valid: false, error: error.message };
            }
        },

        checkTimeOverlap: (time1, time2) => {
            console.log('🧪 Testing time overlap:', time1, 'vs', time2);
            const result = checkTimeOverlap(time1, time2);
            console.log('🧪 Overlap result:', result);
            return result;
        },

        parseTimeToMinutes: (timeString) => {
            console.log('🧪 Parsing time:', timeString);
            const minutes = parseTimeToMinutes(timeString);
            console.log('🧪 Parsed to minutes:', minutes);
            return minutes;
        },

        simulateConflict: () => {
            console.log('🧪 Simulating conflict warning...');
            const mockConflicts = [
                {
                    name: 'Evening Practice',
                    startTime: '18:00',
                    endTime: '20:00',
                    runId: 'mock-conflict-1'
                }
            ];
            showConflictWarnings(mockConflicts, true);
            return mockConflicts;
        },

        clearConflicts: () => {
            console.log('🧪 Clearing conflict warnings...');
            clearConflictWarnings();
        },

        getCurrentScheduleData: () => {
            const scheduleData = {
                runDate: safeGetValue('editRunDate'),
                startTime: safeGetValue('editRunTime'),
                endTime: safeGetValue('editEndTime'),
                courtId: safeGetValue('editCourtList'),
                currentRunId: safeGetValue('editRunId')
            };
            console.log('📅 Current schedule data:', scheduleData);
            return scheduleData;
        },

        function handleCourtSelectionChange(event) {
        const selectedCourtId = event.target.value;
        const selectedOption = event.target.options[event.target.selectedIndex];
        const courtName = selectedOption.textContent;

        if (selectedCourtId) {
            console.log('🏀 Court selected:', courtName, '(ID:', selectedCourtId, ')');
            showToast(`Court selected: ${courtName}`, 'info');
        } else {
            console.log('🏀 No court selected');
        }

        // Clear any existing conflict warnings when court changes
        clearConflictWarnings();
    }
};

console.log('🐛 Debug functions available: window.runDebugFixed');
console.log('🐛 EDIT RUN FUNCTIONS:');
console.log('🐛 Try: window.runDebugFixed.forceHideLoading() to force hide loading');
console.log('🐛 Try: window.runDebugFixed.checkConflicts() to check for conflicts');
console.log('🐛 Try: window.runDebugFixed.testToast() to test utilities.js toast system');
console.log('🐛 Try: window.runDebugFixed.getFormData() to see current form data');
console.log('🐛 Try: window.runDebugFixed.getCourtInfo() to check court selection');
console.log('🐛 Try: window.runDebugFixed.testCourtSelection("courtId") to test court selection');
console.log('🐛 Try: window.runDebugFixed.simulateCourtAutoPopulation() to test auto-population');
console.log('🐛 Try: window.runDebugFixed.verifyAutoPopulation() to verify court auto-selection');
console.log('🐛 CONFLICT VALIDATION:');
console.log('🐛 Try: window.runDebugFixed.testConflictValidation() to test conflict checking');
console.log('🐛 Try: window.runDebugFixed.validateCurrentSchedule() to check current form');
console.log('🐛 Try: window.runDebugFixed.simulateConflict() to show conflict warning');
console.log('🐛 Try: window.runDebugFixed.clearConflicts() to clear conflict warnings');
console.log('🐛 Try: window.runDebugFixed.getCurrentScheduleData() to see schedule data');
console.log('🐛 CREATE RUN FUNCTIONS:');
console.log('🐛 Try: window.runDebugFixed.testCreateRunForm() to open create run modal');
console.log('🐛 Try: window.runDebugFixed.getCreateFormData() to see create form data');
console.log('🐛 Try: window.runDebugFixed.loadTestClients() to load clients');
console.log('🐛 Try: window.runDebugFixed.testCreateConflictValidation() to test create conflicts');
console.log('🐛 Try: window.runDebugFixed.simulateCreateConflict() to show create conflict warning');
console.log('🐛 Try: window.runDebugFixed.clearCreateConflicts() to clear create warnings');
console.log('🐛 Try: window.runDebugFixed.testCreateClientSelection("clientId") to test client selection');
console.log('🐛 Try: window.runDebugFixed.validateCreateForm() to validate create form');
console.log('🐛 Try: window.runDebugFixed.resetCreateForm() to reset create form');
console.log('🐛 Try: window.runDebugFixed.getCreateRunState() to see create run state');
});