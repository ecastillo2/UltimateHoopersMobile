/**
 * Fixed Run Management JavaScript - Court Dropdown Issue Resolution
 * This fixes the court list not displaying in the dropdown
 */

document.addEventListener('DOMContentLoaded', function () {
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
        // You can implement a proper toast notification here
        if (type === 'error') {
            alert(`Error: ${message}`);
        }
    }

    // FIXED: Enhanced court dropdown population function
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

    // FIXED: Enhanced run data loading with better court handling
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

    // NEW: Separate function to handle court data
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

    // NEW: Function to fetch courts for a specific client
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

    // NEW: Function to populate basic run fields
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

    // NEW: Function to update participant counts
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

    // Function to load participants (keeping existing implementation)
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

    // Function to remove participant (keeping existing implementation)
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

    // Initialize DataTable only if table exists
    let runsTable = null;
    const tableElement = document.getElementById('runsTable');

    if (tableElement && tableElement.querySelector('tbody tr')) {
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
    }

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

    // NEW: Add client change handler to reload courts
    const clientIdField = document.getElementById('editClientId');
    if (clientIdField) {
        // Add change event listener to reload courts when client changes
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

    console.log('✅ Enhanced Run Management with Court Fix loaded successfully');
    console.log('🧪 Debug functions available: window.debugCourts');
});