/**
 * Fixed Run Management JavaScript
 * Resolves Edit Run Details tab population issues
 */

document.addEventListener('DOMContentLoaded', function () {
    console.log('🚀 Initializing Fixed Run Management');

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

        let container = document.querySelector('.toast-container');
        if (!container) {
            container = document.createElement('div');
            container.className = 'toast-container position-fixed bottom-0 end-0 p-3';
            container.style.zIndex = '9999';
            document.body.appendChild(container);
        }

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
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="modal"></button>
                </div>
            </div>
        `;

        container.insertAdjacentHTML('beforeend', toastHtml);

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

    // FIXED: Helper function to safely set field values
    function safeSetValue(fieldId, value, defaultValue = '') {
        const field = document.getElementById(fieldId);
        if (field) {
            field.value = value || defaultValue;
            console.log(`✅ Set ${fieldId} = "${value || defaultValue}"`);
            return true;
        } else {
            console.warn(`⚠️ Field ${fieldId} not found`);
            return false;
        }
    }

    // FIXED: Helper function to safely set select values
    function safeSetSelect(fieldId, value, defaultValue = '') {
        const select = document.getElementById(fieldId);
        if (!select) {
            console.warn(`⚠️ Select ${fieldId} not found`);
            return false;
        }

        const valueToSet = value || defaultValue;
        let optionFound = false;

        // Try exact match first
        for (let i = 0; i < select.options.length; i++) {
            if (select.options[i].value === valueToSet) {
                select.selectedIndex = i;
                console.log(`✅ Set ${fieldId} = "${valueToSet}" (exact match)`);
                optionFound = true;
                break;
            }
        }

        // Try case-insensitive match if exact match fails
        if (!optionFound && valueToSet) {
            for (let i = 0; i < select.options.length; i++) {
                if (select.options[i].value.toLowerCase() === valueToSet.toLowerCase()) {
                    select.selectedIndex = i;
                    console.log(`✅ Set ${fieldId} = "${select.options[i].value}" (case-insensitive match)`);
                    optionFound = true;
                    break;
                }
            }
        }

        if (!optionFound && valueToSet) {
            console.warn(`⚠️ Option "${valueToSet}" not found for ${fieldId}`);
        }

        return optionFound;
    }

    // FIXED: Function to populate basic run fields with better error handling
    function populateBasicRunFields(data) {
        console.log('📝 Populating basic run fields with data:', data);

        if (!data) {
            console.error('❌ No data provided to populateBasicRunFields');
            return;
        }

        // FIXED: Populate basic run information with proper fallbacks
        safeSetValue('editRunId', data.runId);
        safeSetValue('editRunTitle', data.name);
        safeSetValue('editRunDescription', data.description);

        // FIXED: Date handling with validation
        if (data.runDate) {
            let dateValue = data.runDate;
            // If date includes time, extract just the date part
            if (dateValue.includes('T')) {
                dateValue = dateValue.split('T')[0];
            }
            // Validate date format (YYYY-MM-DD)
            if (/^\d{4}-\d{2}-\d{2}$/.test(dateValue)) {
                safeSetValue('editRunDate', dateValue);
            } else {
                console.warn('⚠️ Invalid date format:', data.runDate);
            }
        }

        // FIXED: Time handling with proper format conversion
        if (data.startTime) {
            let timeValue = data.startTime;
            // Handle different time formats
            if (timeValue.includes(':')) {
                const timeParts = timeValue.split(':');
                if (timeParts.length >= 2) {
                    // Ensure HH:MM format for HTML time input
                    const hours = timeParts[0].padStart(2, '0');
                    const minutes = timeParts[1].padStart(2, '0');
                    timeValue = `${hours}:${minutes}`;
                }
            }
            safeSetValue('editRunTime', timeValue);
        }

        if (data.endTime) {
            let timeValue = data.endTime;
            if (timeValue.includes(':')) {
                const timeParts = timeValue.split(':');
                if (timeParts.length >= 2) {
                    const hours = timeParts[0].padStart(2, '0');
                    const minutes = timeParts[1].padStart(2, '0');
                    timeValue = `${hours}:${minutes}`;
                }
            }
            safeSetValue('editEndTime', timeValue);
        }

        // FIXED: Address fields with proper handling
        safeSetValue('editAddress', data.address);
        safeSetValue('editCity', data.city);
        safeSetValue('editState', data.state);
        safeSetValue('editZip', data.zip);

        // FIXED: Numeric fields with validation
        const playerLimit = parseInt(data.playerLimit) || 10;
        safeSetValue('editMaxParticipants', playerLimit);

        // FIXED: Client ID field
        safeSetValue('editClientId', data.clientId);

        // FIXED: Select fields with proper defaults and validation
        safeSetSelect('editSkillLevel', data.skillLevel, 'Intermediate');
        safeSetSelect('editStatus', data.status, 'Active');
        safeSetSelect('editRunType', data.type, 'Pickup');
        safeSetSelect('editTeamType', data.teamType, 'Individual');

        // FIXED: Boolean fields handling
        const isPublicValue = data.isPublic !== undefined ? (data.isPublic ? 'true' : 'false') : 'true';
        safeSetSelect('editIsPublic', isPublicValue, 'true');

        const isOutdoorValue = data.isOutdoor !== undefined ? (data.isOutdoor ? 'true' : 'false') : 'false';
        safeSetSelect('editCourtType', isOutdoorValue, 'false');

        // Store profile ID for form submission
        safeSetValue('editProfileId', data.profileId);

        console.log('✅ Basic run fields populated successfully');
    }

    // FIXED: Enhanced court dropdown population
    function populateCourtDropdown(courtList, selectedCourtId = null) {
        const courtSelect = document.getElementById('editCourtList');

        if (!courtSelect) {
            console.warn('⚠️ Court dropdown element not found');
            return;
        }

        console.log('🏀 Populating court dropdown with:', courtList);
        console.log('🏀 Selected court ID:', selectedCourtId);

        // Show loading state
        courtSelect.classList.add('court-dropdown-loading');
        courtSelect.disabled = true;
        courtSelect.innerHTML = '<option value="">Loading courts...</option>';

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
                        const courtId = court.courtId || court.CourtId || court.id || `court-${index}`;
                        const courtName = court.name || court.Name || court.courtName || court.CourtName || `Court ${index + 1}`;

                        const option = document.createElement('option');
                        option.value = courtId;
                        option.textContent = courtName;

                        // Select this option if it matches
                        if (selectedCourtId && courtId.toString() === selectedCourtId.toString()) {
                            option.selected = true;
                            console.log(`✅ Selected court: ${courtName} (ID: ${courtId})`);
                        }

                        courtSelect.appendChild(option);
                    } catch (error) {
                        console.error('❌ Error processing court:', court, error);
                    }
                });
            } else {
                console.warn('⚠️ No courts available');
                const noOption = document.createElement('option');
                noOption.value = '';
                noOption.textContent = 'No courts available';
                noOption.disabled = true;
                courtSelect.appendChild(noOption);
            }

            // Remove loading state
            courtSelect.classList.remove('court-dropdown-loading');
            courtSelect.disabled = false;

            console.log('✅ Court dropdown populated');
        }, 300);
    }

    // FIXED: Function to handle court data
    function handleCourtData(data) {
        console.log('🏀 Handling court data...');

        // First, set the client ID in the form
        if (data.clientId) {
            safeSetValue('editClientId', data.clientId);
        }

        // Handle court dropdown population
        if (data.courtList && Array.isArray(data.courtList)) {
            console.log('🏀 Using court list from run data');
            populateCourtDropdown(data.courtList, data.courtId);
        } else if (data.clientId) {
            console.log('🏀 Fetching courts for client:', data.clientId);
            fetchCourtsForClient(data.clientId, data.courtId);
        } else {
            console.warn('⚠️ No court list or client ID available');
            populateCourtDropdown([], null);
        }
    }

    // Function to fetch courts for a client
    function fetchCourtsForClient(clientId, selectedCourtId = null) {
        if (!clientId) {
            populateCourtDropdown([], null);
            return;
        }

        fetch(`/Run/GetRunCourts?clientId=${encodeURIComponent(clientId)}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                return response.json();
            })
            .then(data => {
                if (data.success && data.courts) {
                    populateCourtDropdown(data.courts, selectedCourtId);
                } else {
                    populateCourtDropdown([], selectedCourtId);
                }
            })
            .catch(error => {
                console.error('❌ Error fetching courts:', error);
                populateCourtDropdown([], selectedCourtId);
                showToast('Warning: Could not load courts for this client', 'warning');
            });
    }

    // FIXED: Function to update participant counts
    function updateParticipantCounts(data) {
        const updateCount = (elementId, count, defaultValue = 0) => {
            const element = document.getElementById(elementId);
            if (element) {
                const numCount = parseInt(count) || defaultValue;
                element.textContent = numCount;
                console.log(`✅ Updated ${elementId} to: ${numCount}`);
            } else {
                console.warn(`⚠️ Element ${elementId} not found`);
            }
        };

        updateCount('currentParticipantsCount', data.playerCount, 0);
        updateCount('maxParticipantsCount', data.playerLimit, 0);
    }

    // FIXED: Main function to load run data
    function loadRunDataEnhanced(runId) {
        console.log('🔄 Loading run data for ID:', runId);

        if (!runId) {
            console.error('❌ No run ID provided');
            hideLoading();
            showToast('Error: No run ID provided', 'error');
            return;
        }

        showLoading();

        // Clear form first to avoid showing stale data
        clearRunForm();

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

                // FIXED: Populate form fields step by step with validation
                try {
                    // 1. Populate basic form fields
                    populateBasicRunFields(data);

                    // 2. Handle court data
                    handleCourtData(data);

                    // 3. Update participant counts
                    updateParticipantCounts(data);

                    // 4. Set delete button run ID
                    const deleteIdField = document.getElementById('deleteRunId');
                    if (deleteIdField) {
                        deleteIdField.value = data.runId || runId;
                    }

                    // 5. Load participants
                    loadParticipants(data.runId || runId);

                    console.log('✅ Run data loaded successfully');
                    showToast('Run data loaded successfully', 'success');

                } catch (error) {
                    console.error('❌ Error processing run data:', error);
                    showToast('Error processing run data: ' + error.message, 'error');
                }
            })
            .catch(error => {
                hideLoading();
                console.error('❌ Error loading run data:', error);
                showToast(`Error loading run data: ${error.message}`, 'error');
            });
    }

    // FIXED: Function to clear run form
    function clearRunForm() {
        console.log('🧹 Clearing run form...');

        const textFields = [
            'editRunId', 'editRunTitle', 'editRunDescription', 'editRunDate',
            'editRunTime', 'editEndTime', 'editAddress', 'editCity', 'editState',
            'editZip', 'editMaxParticipants', 'editClientId', 'editProfileId'
        ];

        textFields.forEach(fieldId => {
            const field = document.getElementById(fieldId);
            if (field) {
                field.value = '';
            }
        });

        const selectFields = [
            'editSkillLevel', 'editStatus', 'editRunType', 'editTeamType',
            'editIsPublic', 'editCourtType', 'editCourtList'
        ];

        selectFields.forEach(fieldId => {
            const field = document.getElementById(fieldId);
            if (field) {
                field.selectedIndex = 0;
            }
        });

        console.log('✅ Run form cleared');
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
                                <button class="btn btn-sm btn-light" type="button" data-bs-toggle="dropdown">
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

    // Initialize DataTable
    function initializeDataTable() {
        const tableElement = document.getElementById('runsTable');
        if (tableElement && tableElement.querySelector('tbody tr')) {
            console.log('📊 Initializing DataTable...');
            const runsTable = $('#runsTable').DataTable({
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
        }
    }

    // Initialize everything
    initializeDataTable();

    // Set minimum date for date pickers
    const today = new Date().toISOString().split('T')[0];
    const dateInputs = document.querySelectorAll('input[type="date"]');
    dateInputs.forEach(input => {
        input.min = today;
    });

    // FIXED: Handle edit run modal show event
    const editRunModal = document.getElementById('editRunModal');
    if (editRunModal) {
        editRunModal.addEventListener('show.bs.modal', function (event) {
            const button = event.relatedTarget;
            const runId = button.getAttribute('data-run-id');

            console.log('🎯 Opening edit modal for run ID:', runId);

            if (!runId) {
                console.error('❌ No run ID found on button');
                showToast('Error: No run ID found', 'error');
                return;
            }

            // Store run ID in the form
            const editRunIdField = document.getElementById('editRunId');
            if (editRunIdField) {
                editRunIdField.value = runId;
                console.log('✅ Set editRunId field to:', runId);
            }

            // Load run data
            loadRunDataEnhanced(runId);
        });

        // FIXED: Handle modal hide event to clear form
        editRunModal.addEventListener('hidden.bs.modal', function () {
            console.log('🚪 Edit modal closed, clearing form');
            clearRunForm();
        });
    }

    // Handle delete run button
    const deleteRunBtn = document.getElementById('deleteRunBtn');
    if (deleteRunBtn) {
        deleteRunBtn.addEventListener('click', function () {
            const runId = document.getElementById('editRunId')?.value;
            if (!runId) {
                showToast('Error: No run ID found', 'error');
                return;
            }

            const deleteIdField = document.getElementById('deleteRunId');
            if (deleteIdField) {
                deleteIdField.value = runId;
            }

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
            const runId = document.getElementById('editRunId')?.value;
            const currentCountEl = document.getElementById('currentParticipantsCount');
            const maxCountEl = document.getElementById('maxParticipantsCount');

            if (!runId) {
                showToast('Error: No run ID found', 'error');
                return;
            }

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

            fetch('/Run/AddParticipant', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: JSON.stringify({ RunId: runId, ProfileId: profileId })
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

    // Client change handler to reload courts
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

    // Form submission handler
    const editRunForm = document.getElementById('editRunForm');
    if (editRunForm) {
        editRunForm.addEventListener('submit', function (e) {
            e.preventDefault();

            if (!validateRunForm()) {
                return;
            }

            const formData = new FormData(this);
            const runData = {};

            for (const [key, value] of formData.entries()) {
                if (!value && value !== '0' && value !== 0) {
                    continue;
                }

                if (key === 'IsPublic' || key === 'IsOutdoor') {
                    runData[key] = value === 'true';
                } else if (key === 'PlayerLimit' || key === 'PlayerCount') {
                    const numValue = parseInt(value);
                    if (!isNaN(numValue)) {
                        runData[key] = numValue;
                    }
                } else if (key === 'RunDate' && value) {
                    runData[key] = value;
                } else if (key === 'StartTime' || key === 'EndTime') {
                    if (value) {
                        runData[key] = value.includes(':') ? value + ':00' : value;
                    }
                } else {
                    runData[key] = value;
                }
            }

            if (!runData.RunId) {
                const runIdField = document.getElementById('editRunId');
                if (runIdField && runIdField.value) {
                    runData.RunId = runIdField.value;
                }
            }

            console.log('🚀 Submitting run data:', runData);
            showLoading();

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
                    if (!response.ok) {
                        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                    }
                    return response.json();
                })
                .then(data => {
                    hideLoading();
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalBtnText;

                    if (data.success) {
                        showToast('✅ ' + data.message, 'success');

                        setTimeout(() => {
                            const modal = bootstrap.Modal.getInstance(document.getElementById('editRunModal'));
                            if (modal) {
                                modal.hide();
                            }
                            location.reload(); // Refresh page to show updated data
                        }, 1000);
                    } else {
                        if (data.requiresLogin) {
                            showToast('⚠️ Session expired. Please log in again.', 'warning');
                            setTimeout(() => {
                                window.location.href = '/Home/Index#login';
                            }, 2000);
                        } else {
                            showToast('❌ ' + data.message, 'error');
                        }
                    }
                })
                .catch(error => {
                    hideLoading();
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalBtnText;
                    showToast('💥 Error updating run: ' + error.message, 'error');
                });
        });
    }

    // Client-side form validation
    function validateRunForm() {
        const errors = [];

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
            }
        });

        if (errors.length > 0) {
            showToast('Please fix the following errors:\n• ' + errors.join('\n• '), 'error');
            return false;
        }

        return true;
    }

    // Debug functions for testing
    window.debugRuns = {
        loadRunDataEnhanced,
        populateBasicRunFields,
        clearRunForm,
        testRunData: function (runId = 'test-run-123') {
            console.log('🧪 Testing with run ID:', runId);
            loadRunDataEnhanced(runId);
        }
    };

    console.log('✅ Fixed Run Management loaded successfully');
    console.log('🧪 Debug functions available: window.debugRuns');
});