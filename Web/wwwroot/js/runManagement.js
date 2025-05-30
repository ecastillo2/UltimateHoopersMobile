/**
 * Complete Fixed Run Management JavaScript
 * Consolidated version to prevent DataTable reinitialization errors
 */

document.addEventListener('DOMContentLoaded', function () {
    console.log('🚀 Initializing Enhanced Run Management');

    // Helper functions
    function getAntiForgeryToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
    }

    function showLoading() {
        const spinner = document.querySelector('.loading-spinner');
        if (spinner) spinner.classList.add('active');
    }

    function hideLoading() {
        const spinner = document.querySelector('.loading-spinner');
        if (spinner) spinner.classList.remove('active');
    }

    function showToast(message, type = 'success') {
        console.log(`${type}: ${message}`);
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

        const toastHtml = `
            <div id="${toastId}" class="toast align-items-center text-white bg-${type === 'error' ? 'danger' : type} border-0" role="alert">
                <div class="d-flex">
                    <div class="toast-body">
                        <i class="bi ${iconClass} me-2"></i>
                        ${message}
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
                delay: 5000
            });
            toast.show();

            toastElement.addEventListener('hidden.bs.toast', () => {
                toastElement.remove();
            });
        }
    }

    // Enhanced court dropdown population function
    function populateCourtDropdown(courtList, selectedCourtId = null) {
        const courtSelect = document.getElementById('editCourtList');

        if (!courtSelect) {
            console.warn('Court dropdown element not found');
            return;
        }

        console.log('🏀 Populating court dropdown with data:', courtList);
        console.log('🏀 Selected court ID:', selectedCourtId);

        // Show loading state
        courtSelect.classList.add('court-dropdown-loading');
        courtSelect.disabled = true;
        courtSelect.innerHTML = '<option value="">Loading courts...</option>';

        // Small delay to show loading state
        setTimeout(() => {
            // Clear existing options
            courtSelect.innerHTML = '';

            // Add default option
            const defaultOption = document.createElement('option');
            defaultOption.value = '';
            defaultOption.textContent = 'Select a court...';
            courtSelect.appendChild(defaultOption);

            // Check if we have court data
            if (courtList && Array.isArray(courtList) && courtList.length > 0) {
                console.log(`✅ Adding ${courtList.length} courts to dropdown`);

                courtList.forEach((court, index) => {
                    try {
                        // Handle different possible court object structures
                        const courtId = court.courtId || court.CourtId || court.id || '';
                        const courtName = court.name || court.Name || court.courtName || court.CourtName || `Court ${index + 1}`;

                        console.log(`🏀 Processing court: ID=${courtId}, Name=${courtName}`);

                        const option = document.createElement('option');
                        option.value = courtId;
                        option.textContent = courtName;

                        // Select this option if it matches the current court
                        if (selectedCourtId && (courtId === selectedCourtId || courtId.toString() === selectedCourtId.toString())) {
                            option.selected = true;
                            console.log(`✅ Selected court: ${courtName} (ID: ${courtId})`);
                        }

                        courtSelect.appendChild(option);
                    } catch (error) {
                        console.error('❌ Error processing court:', court, error);
                    }
                });

                console.log(`✅ Successfully added ${courtList.length} courts to dropdown`);
            } else {
                console.warn('⚠️ No courts available or invalid court data:', courtList);

                // Add a "No courts available" option
                const noOption = document.createElement('option');
                noOption.value = '';
                noOption.textContent = 'No courts available for this client';
                noOption.disabled = true;
                courtSelect.appendChild(noOption);
            }

            // Remove loading state
            courtSelect.classList.remove('court-dropdown-loading');
            courtSelect.disabled = false;

            console.log('🏀 Court dropdown population completed');
        }, 300);
    }

    // Function to fetch courts for a specific client
    function fetchCourtsForClient(clientId, selectedCourtId = null) {
        console.log('🔄 Fetching courts for client:', clientId);

        if (!clientId) {
            console.warn('⚠️ No client ID provided for court fetch');
            populateCourtDropdown([], null);
            return;
        }

        fetch(`/Run/GetRunCourts?clientId=${encodeURIComponent(clientId)}`)
            .then(response => {
                console.log('📡 Courts API Response status:', response.status);

                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                return response.json();
            })
            .then(data => {
                console.log('📦 Courts API response:', data);

                if (data.success && data.courts) {
                    console.log('✅ Successfully fetched courts:', data.courts);
                    populateCourtDropdown(data.courts, selectedCourtId);
                } else {
                    console.warn('⚠️ No courts returned from API:', data);
                    populateCourtDropdown([], selectedCourtId);
                }
            })
            .catch(error => {
                console.error('❌ Error fetching courts:', error);
                populateCourtDropdown([], selectedCourtId);
                showToast('Warning: Could not load courts for this client', 'warning');
            });
    }

    // Function to handle court data
    function handleCourtData(data) {
        console.log('🏀 Handling court data...');
        console.log('🏀 Court list from API:', data.courtList);
        console.log('🏀 Selected court ID:', data.courtId);
        console.log('🏀 Client ID:', data.clientId);

        // First, set the client ID in the form
        const clientIdField = document.getElementById('editClientId');
        if (clientIdField && data.clientId) {
            clientIdField.value = data.clientId;
            console.log('✅ Set client ID:', data.clientId);
        }

        // Handle court dropdown population
        try {
            if (data.courtList && Array.isArray(data.courtList)) {
                console.log('🏀 Using court list from run data');
                populateCourtDropdown(data.courtList, data.courtId);
            } else if (data.clientId) {
                console.log('🏀 No court list in run data, fetching courts for client:', data.clientId);
                fetchCourtsForClient(data.clientId, data.courtId);
            } else {
                console.warn('⚠️ No court list or client ID available');
                populateCourtDropdown([], null);
            }
        } catch (e) {
            console.error('❌ Error handling court data:', e);
            populateCourtDropdown([], null);
        }
    }

    // Function to populate basic run fields
    function populateBasicRunFields(data) {
        console.log('📝 Populating basic run fields...');

        // Helper function to safely set field values
        const setFieldValue = (fieldId, value) => {
            const field = document.getElementById(fieldId);
            if (field && value !== null && value !== undefined) {
                field.value = value;
                console.log(`✅ Set ${fieldId} to: ${value}`);
            } else if (!field) {
                console.warn(`⚠️ Field ${fieldId} not found`);
            }
        };

        // Basic run information
        setFieldValue('editRunId', data.runId || '');
        setFieldValue('editRunTitle', data.name || '');
        setFieldValue('editRunDescription', data.description || '');

        // Date handling
        if (data.runDate) {
            try {
                setFieldValue('editRunDate', data.runDate);
            } catch (e) {
                console.warn('⚠️ Invalid run date format:', data.runDate);
            }
        }

        // Time handling
        if (data.startTime) {
            try {
                let timeValue = data.startTime;
                if (timeValue.includes(':')) {
                    const timeParts = timeValue.split(':');
                    if (timeParts.length >= 2) {
                        timeValue = `${timeParts[0]}:${timeParts[1]}`;
                    }
                }
                setFieldValue('editRunTime', timeValue);
            } catch (e) {
                console.warn('⚠️ Invalid start time format:', data.startTime);
            }
        }

        if (data.endTime) {
            try {
                let timeValue = data.endTime;
                if (timeValue.includes(':')) {
                    const timeParts = timeValue.split(':');
                    if (timeParts.length >= 2) {
                        timeValue = `${timeParts[0]}:${timeParts[1]}`;
                    }
                }
                setFieldValue('editEndTime', timeValue);
            } catch (e) {
                console.warn('⚠️ Invalid end time format:', data.endTime);
            }
        }

        // Address fields
        setFieldValue('editAddress', data.address || '');
        setFieldValue('editCity', data.city || '');
        setFieldValue('editState', data.state || '');
        setFieldValue('editZip', data.zip || '');

        // Numeric fields
        setFieldValue('editMaxParticipants', data.playerLimit || 10);

        // Select fields
        const setSelectField = (fieldId, value, defaultValue = '') => {
            const select = document.getElementById(fieldId);
            if (select && value) {
                for (let i = 0; i < select.options.length; i++) {
                    if (select.options[i].value.toLowerCase() === value.toLowerCase()) {
                        select.selectedIndex = i;
                        console.log(`✅ Set ${fieldId} to: ${value}`);
                        return;
                    }
                }
                console.warn(`⚠️ Option '${value}' not found for ${fieldId}`);
            }
        };

        setSelectField('editSkillLevel', data.skillLevel, 'Intermediate');
        setSelectField('editStatus', data.status, 'Active');
        setSelectField('editRunType', data.type, 'Pickup');
        setSelectField('editTeamType', data.teamType, 'Individual');
        setSelectField('editIsPublic', data.isPublic ? 'true' : 'false', 'true');
        setSelectField('editCourtType', data.isOutdoor ? 'true' : 'false', 'false');
    }

    // Function to update participant counts
    function updateParticipantCounts(data) {
        const updateCount = (elementId, count, defaultValue = 0) => {
            const element = document.getElementById(elementId);
            if (element) {
                const numCount = parseInt(count) || defaultValue;
                element.textContent = numCount;
                console.log(`✅ Updated ${elementId} to: ${numCount}`);
            }
        };

        updateCount('currentParticipantsCount', data.playerCount, 0);
        updateCount('maxParticipantsCount', data.playerLimit, 0);
    }

    // Enhanced run data loading function
    function loadRunDataEnhanced(runId) {
        console.log('🔄 Loading run data for ID:', runId);

        if (!runId) {
            console.error('❌ No run ID provided');
            hideLoading();
            return;
        }

        showLoading();

        fetch(`/Run/GetRunData?id=${runId}`)
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
                    showToast('Error loading run data: ' + (data.message || 'Unknown error'), 'error');
                    return;
                }

                // Populate basic form fields
                populateBasicRunFields(data);

                // Handle court data with enhanced logging
                handleCourtData(data);

                // Update participant counts
                updateParticipantCounts(data);

                // Set delete button run ID
                const deleteIdField = document.getElementById('deleteRunId');
                if (deleteIdField) {
                    deleteIdField.value = data.runId || runId;
                }

                // Load participants
                try {
                    loadParticipants(data.runId || runId);
                } catch (e) {
                    console.error('❌ Error loading participants:', e);
                }

                console.log('✅ Run data loaded successfully');
            })
            .catch(error => {
                hideLoading();
                console.error('❌ Error loading run data:', error);
                showToast('Error loading run data: ' + error.message, 'error');
            });
    }

    // Function to load participants
    function loadParticipants(runId) {
        const participantsList = document.getElementById('participantsList');
        if (!participantsList) return;

        participantsList.innerHTML = '<div class="text-center py-4 text-muted"><i class="bi bi-people"></i> Loading participants...</div>';

        fetch(`/Run/GetRunParticipants?runId=${runId}`)
            .then(response => {
                if (!response.ok) throw new Error('Network response was not ok');
                return response.json();
            })
            .then(data => {
                if (!Array.isArray(data) || data.length === 0) {
                    participantsList.innerHTML = `
                        <div class="text-center py-4 text-muted">
                            <p>No participants have joined this run yet.</p>
                        </div>`;
                    return;
                }

                let html = '';
                data.forEach(participant => {
                    const userName = participant.userName || 'Unknown Player';
                    const initials = userName
                        .split(' ')
                        .map(n => n[0] || '')
                        .join('')
                        .substring(0, 2)
                        .toUpperCase() || 'UP';

                    html += `
                        <div class="d-flex align-items-center p-2 border-bottom">
                            <div class="participant-avatar flex-shrink-0 me-3">
                                ${initials}
                            </div>
                            <div class="flex-grow-1">
                                <h6 class="mb-0">${userName}</h6>
                                <span class="text-muted small">Status: ${participant.status || 'Active'}</span>
                            </div>
                            <div class="dropdown">
                                <button class="btn btn-sm btn-light" type="button" data-bs-toggle="dropdown" aria-expanded="false">
                                    <i class="bi bi-three-dots-vertical"></i>
                                </button>
                                <ul class="dropdown-menu dropdown-menu-end">
                                    <li><a class="dropdown-item remove-participant" data-participant-id="${participant.profileId}" href="#"><i class="bi bi-person-x me-2"></i>Remove</a></li>
                                </ul>
                            </div>
                        </div>`;
                });

                participantsList.innerHTML = html;

                // Add event listeners to remove buttons
                document.querySelectorAll('.remove-participant').forEach(button => {
                    button.addEventListener('click', function (e) {
                        e.preventDefault();
                        const participantId = this.getAttribute('data-participant-id');
                        if (confirm('Are you sure you want to remove this participant?')) {
                            removeParticipant(runId, participantId);
                        }
                    });
                });
            })
            .catch(error => {
                console.error('Error loading participants:', error);
                participantsList.innerHTML = `
                    <div class="text-center py-4 text-danger">
                        <p>Error loading participants. Please try again.</p>
                    </div>`;
            });
    }

    // Function to remove participant
    function removeParticipant(runId, participantId) {
        showLoading();

        fetch('/Run/RemoveParticipant', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify({ RunId: runId, ProfileId: participantId })
        })
            .then(response => {
                if (!response.ok) throw new Error('Network response was not ok');
                return response.json();
            })
            .then(data => {
                hideLoading();
                if (data.success) {
                    loadParticipants(runId);
                    const currentCount = document.getElementById('currentParticipantsCount');
                    if (currentCount) {
                        const count = parseInt(currentCount.textContent);
                        currentCount.textContent = Math.max(0, count - 1);
                    }
                    showToast('Participant removed successfully');
                } else {
                    showToast('Error removing participant: ' + (data.message || 'Unknown error'), 'error');
                }
            })
            .catch(error => {
                hideLoading();
                console.error('Error removing participant:', error);
                showToast('Error removing participant. Please try again.', 'error');
            });
    }

    // Initialize DataTable only if table exists AND hasn't been initialized yet
    let runsTable = null;

    // Call the initialization function
    initializeDataTable();

    // Set minimum date for date pickers
    const today = new Date().toISOString().split('T')[0];
    const dateInputs = document.querySelectorAll('input[type="date"]');
    dateInputs.forEach(input => {
        input.min = today;
    });

    // Handle edit run modal
    const editRunModal = document.getElementById('editRunModal');
    if (editRunModal) {
        editRunModal.addEventListener('show.bs.modal', function (event) {
            const button = event.relatedTarget;
            const runId = button.getAttribute('data-run-id');

            if (!runId) {
                console.error('No run ID found');
                return;
            }

            const editRunIdField = document.getElementById('editRunId');
            if (editRunIdField) editRunIdField.value = runId;

            // Load run data with enhanced court handling
            loadRunDataEnhanced(runId);
        });
    }

    // Handle delete run button
    const deleteRunBtn = document.getElementById('deleteRunBtn');
    if (deleteRunBtn) {
        deleteRunBtn.addEventListener('click', function () {
            const runId = document.getElementById('editRunId').value;
            const deleteIdField = document.getElementById('deleteRunId');
            if (deleteIdField) deleteIdField.value = runId;

            const editModal = bootstrap.Modal.getInstance(document.getElementById('editRunModal'));
            if (editModal) editModal.hide();

            const deleteModal = new bootstrap.Modal(document.getElementById('deleteRunModal'));
            deleteModal.show();
        });
    }

    // Add participant button
    const addParticipantBtn = document.getElementById('addParticipantBtn');
    if (addParticipantBtn) {
        addParticipantBtn.addEventListener('click', function () {
            const runId = document.getElementById('editRunId').value;
            const currentCountEl = document.getElementById('currentParticipantsCount');
            const maxCountEl = document.getElementById('maxParticipantsCount');

            if (!currentCountEl || !maxCountEl) {
                showToast('Unable to determine current capacity. Please refresh and try again.', 'error');
                return;
            }

            const currentCount = parseInt(currentCountEl.textContent);
            const maxCount = parseInt(maxCountEl.textContent);

            if (currentCount >= maxCount) {
                showToast('This run is already at maximum capacity. Cannot add more participants.', 'error');
                return;
            }

            const profileId = prompt('Enter participant profile ID:');
            if (!profileId) return;

            showLoading();

            const requestData = {
                RunId: runId,
                ProfileId: profileId
            };

            fetch('/Run/AddParticipant', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: JSON.stringify(requestData)
            })
                .then(response => {
                    if (!response.ok) throw new Error('Network response was not ok');
                    return response.json();
                })
                .then(data => {
                    hideLoading();
                    if (data.success) {
                        loadParticipants(runId);
                        const currentCount = parseInt(currentCountEl.textContent);
                        currentCountEl.textContent = currentCount + 1;
                        showToast('Participant added successfully');
                    } else {
                        showToast('Error adding participant: ' + (data.message || 'Unknown error'), 'error');
                    }
                })
                .catch(error => {
                    hideLoading();
                    console.error('Error adding participant:', error);
                    showToast('Error adding participant. Please try again.', 'error');
                });
        });
    }

    // Add client change handler to reload courts
    const clientIdField = document.getElementById('editClientId');
    if (clientIdField) {
        clientIdField.addEventListener('change', function () {
            const clientId = this.value;
            if (clientId) {
                console.log('🔄 Client changed, fetching courts for:', clientId);
                fetchCourtsForClient(clientId);
            } else {
                console.log('🏀 Client cleared, clearing courts');
                populateCourtDropdown([], null);
            }
        });
    }

    // Enhanced form submission handler with proper JSON formatting
    const editRunForm = document.getElementById('editRunForm');
    if (editRunForm) {
        editRunForm.addEventListener('submit', function (e) {
            e.preventDefault();

            // Client-side validation first
            if (!validateRunForm()) {
                return;
            }

            // Get form data and convert to proper JSON structure
            const formData = new FormData(this);
            const runData = {};

            // Convert FormData to object with proper type conversion
            for (const [key, value] of formData.entries()) {
                if (!value && value !== '0' && value !== 0) {
                    // Skip empty values except for '0'
                    continue;
                }

                // Handle boolean fields
                if (key === 'IsPublic' || key === 'IsOutdoor') {
                    runData[key] = value === 'true';
                }
                // Handle numeric fields
                else if (key === 'PlayerLimit' || key === 'PlayerCount') {
                    const numValue = parseInt(value);
                    if (!isNaN(numValue)) {
                        runData[key] = numValue;
                    }
                }
                // Handle date fields - ensure proper format
                else if (key === 'RunDate' && value) {
                    runData[key] = value; // Keep as string in YYYY-MM-DD format
                }
                // Handle time fields - convert to TimeSpan format
                else if (key === 'StartTime' || key === 'EndTime') {
                    if (value) {
                        // Convert HH:MM to HH:MM:SS for TimeSpan
                        runData[key] = value.includes(':') ? value + ':00' : value;
                    }
                }
                // Handle regular string fields
                else {
                    runData[key] = value;
                }
            }

            // Ensure required fields are present
            if (!runData.RunId) {
                const runIdField = document.getElementById('editRunId');
                if (runIdField && runIdField.value) {
                    runData.RunId = runIdField.value;
                }
            }

            console.log('🚀 Submitting run data:', runData);
            showLoading();

            // Disable submit button to prevent double submission
            const submitBtn = this.querySelector('button[type="submit"]');
            const originalBtnText = submitBtn.innerHTML;
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Saving...';

            fetch('/Run/Edit', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: JSON.stringify(runData)
            })
                .then(response => {
                    console.log('📡 Response status:', response.status);
                    if (!response.ok) {
                        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                    }
                    return response.json();
                })
                .then(data => {
                    hideLoading();
                    console.log('📦 Response data:', data);

                    // Re-enable submit button
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalBtnText;

                    if (data.success) {
                        showToast('✅ ' + data.message, 'success');

                        // Close modal after a short delay
                        setTimeout(() => {
                            const modal = bootstrap.Modal.getInstance(document.getElementById('editRunModal'));
                            if (modal) {
                                modal.hide();
                            }
                        }, 1000);

                        // Refresh the runs table
                        refreshRunsTable();
                    } else {
                        // Handle different error types
                        if (data.requiresLogin) {
                            showToast('⚠️ Session expired. Please log in again.', 'warning');
                            setTimeout(() => {
                                window.location.href = '/Home/Index#login';
                            }, 2000);
                        } else if (data.field) {
                            // Field-specific validation error
                            showToast('❌ ' + data.message, 'error');
                            highlightErrorField(data.field);
                        } else {
                            showToast('❌ ' + data.message, 'error');
                        }
                    }
                })
                .catch(error => {
                    hideLoading();
                    console.error('❌ Error updating run:', error);

                    // Re-enable submit button
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalBtnText;

                    if (error.message.includes('NetworkError') || error.message.includes('Failed to fetch')) {
                        showToast('🌐 Network error. Please check your connection and try again.', 'error');
                    } else {
                        showToast('💥 Unexpected error occurred. Please try again.', 'error');
                    }
                });
        });
    }

    // Client-side form validation
    function validateRunForm() {
        const errors = [];

        // Required fields validation
        const requiredFields = [
            { id: 'editRunTitle', name: 'Run name' },
            { id: 'editRunDescription', name: 'Description' },
            { id: 'editRunDate', name: 'Run date' },
            { id: 'editRunTime', name: 'Start time' },
            { id: 'editMaxParticipants', name: 'Player limit' }
        ];

        requiredFields.forEach(field => {
            const element = document.getElementById(field.id);
            if (element && !element.value.trim()) {
                errors.push(`${field.name} is required`);
                highlightErrorField(field.id);
            } else if (element) {
                clearErrorField(field.id);
            }
        });

        // Date validation
        const dateField = document.getElementById('editRunDate');
        if (dateField && dateField.value) {
            const selectedDate = new Date(dateField.value);
            const today = new Date();
            today.setHours(0, 0, 0, 0);

            if (selectedDate < today) {
                errors.push('Run date cannot be in the past');
                highlightErrorField('editRunDate');
            }
        }

        // Time validation
        const startTime = document.getElementById('editRunTime');
        const endTime = document.getElementById('editEndTime');
        if (startTime && endTime && startTime.value && endTime.value) {
            if (endTime.value <= startTime.value) {
                errors.push('End time must be after start time');
                highlightErrorField('editEndTime');
            }
        }

        // Player limit validation
        const playerLimit = document.getElementById('editMaxParticipants');
        if (playerLimit && playerLimit.value) {
            const limit = parseInt(playerLimit.value);
            if (limit <= 0 || limit > 50) {
                errors.push('Player limit must be between 1 and 50');
                highlightErrorField('editMaxParticipants');
            }
        }

        if (errors.length > 0) {
            showToast('Please fix the following errors:\n• ' + errors.join('\n• '), 'error');
            return false;
        }

        return true;
    }

    // Highlight error field
    function highlightErrorField(fieldId) {
        const field = document.getElementById(fieldId);
        if (field) {
            field.classList.add('is-invalid');
            field.addEventListener('input', () => clearErrorField(fieldId), { once: true });
        }
    }

    // Clear error field highlighting
    function clearErrorField(fieldId) {
        const field = document.getElementById(fieldId);
        if (field) {
            field.classList.remove('is-invalid');
        }
    }

    // Function to refresh the runs table
    function refreshRunsTable() {
        console.log('🔄 Refreshing runs table...');

        if (!runsTable) {
            console.log('📊 No DataTable instance found, reloading page...');
            setTimeout(() => location.reload(), 1500);
            return;
        }

        showLoading();

        fetch('/Run/Run', {
            method: 'GET',
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'Content-Type': 'text/html'
            }
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                return response.text();
            })
            .then(html => {
                hideLoading();

                // Parse the HTML response to extract table data
                const parser = new DOMParser();
                const doc = parser.parseFromString(html, 'text/html');
                const newTableBody = doc.querySelector('#runsTable tbody');

                if (newTableBody) {
                    // Destroy existing DataTable
                    runsTable.destroy();

                    // Update table body content
                    const currentTableBody = document.querySelector('#runsTable tbody');
                    if (currentTableBody) {
                        currentTableBody.innerHTML = newTableBody.innerHTML;
                    }

                    // Reinitialize DataTable
                    initializeDataTable();

                    console.log('✅ Runs table refreshed successfully');
                    showToast('📊 Table updated with latest data', 'info');
                } else {
                    console.warn('⚠️ Could not find table data in response');
                    // Fallback to page reload
                    setTimeout(() => location.reload(), 1000);
                }
            })
            .catch(error => {
                hideLoading();
                console.error('❌ Error refreshing table:', error);
                showToast('⚠️ Could not refresh table. Reloading page...', 'warning');
                // Fallback to page reload
                setTimeout(() => location.reload(), 2000);
            });
    }

    // Enhanced DataTable initialization function
    function initializeDataTable() {
        const tableElement = document.getElementById('runsTable');

        if (tableElement && tableElement.querySelector('tbody tr')) {
            console.log('📊 Initializing DataTable...');
            runsTable = $('#runsTable').DataTable({
                responsive: true,
                lengthMenu: [[10, 25, 50, -1], [10, 25, 50, "All"]],
                dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>rtip',
                language: {
                    search: "_INPUT_",
                    searchPlaceholder: "Search runs...",
                    lengthMenu: "Show _MENU_ runs per page",
                    info: "Showing _START_ to _END_ of _TOTAL_ runs",
                    infoEmpty: "Showing 0 to 0 of 0 runs",
                    infoFiltered: "(filtered from _MAX_ total runs)"
                },
                columnDefs: [
                    { className: "align-middle", targets: "_all" },
                    { orderable: false, targets: [5] }
                ],
                order: [[1, 'asc']]
            });
            console.log('📊 DataTable initialized successfully');
        } else {
            console.log('📊 No table data found for DataTable initialization');
        }
    }

    // Debug functions for testing
    window.debugCourts = {
        populateCourtDropdown,
        fetchCourtsForClient,
        loadRunDataEnhanced,
        handleCourtData,
        testCourtData: function () {
            const testData = [
                { courtId: '1', name: 'Main Court' },
                { courtId: '2', name: 'Practice Court' },
                { courtId: '3', name: 'Youth Court' }
            ];
            console.log('🧪 Testing court dropdown with sample data');
            populateCourtDropdown(testData, '2');
        }
    };

    console.log('✅ Enhanced Run Management loaded successfully');
    console.log('🧪 Debug functions available: window.debugCourts');
});