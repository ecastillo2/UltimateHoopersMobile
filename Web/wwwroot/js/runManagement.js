// Enhanced Run Management JavaScript with AJAX Save - Prevent DataTable Reinitialization
(function () {
    'use strict';

    // Prevent multiple executions
    if (window.runManagementLoaded) {
        console.warn('Run Management script already loaded, preventing duplicate execution');
        return;
    }
    window.runManagementLoaded = true;

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

        function showToast(title, message, type = 'success') {
            console.log(`${type}: ${title} - ${message}`);

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

        // Function to populate court dropdown
        function populateCourtDropdown(courtList, selectedCourtId = null) {
            const courtSelect = document.getElementById('editCourtList');

            if (!courtSelect) {
                console.warn('Court dropdown element not found');
                return;
            }

            // Show loading state
            courtSelect.classList.add('court-dropdown-loading');
            courtSelect.disabled = true;

            // Clear existing options except the first one
            courtSelect.innerHTML = '<option value="">Loading courts...</option>';

            // Add courts from the data
            setTimeout(() => { // Small delay to show loading state
                courtSelect.innerHTML = '<option value="">Select a court...</option>';

                if (courtList && courtList.length > 0) {
                    courtList.forEach(court => {
                        const option = document.createElement('option');
                        option.value = court.courtId;
                        option.textContent = court.name;

                        // Select this option if it matches the current court
                        if (selectedCourtId && court.courtId === selectedCourtId) {
                            option.selected = true;
                        }

                        courtSelect.appendChild(option);
                    });

                    console.log(`Added ${courtList.length} courts to dropdown`);
                } else {
                    // Add a "No courts available" option
                    const noOption = document.createElement('option');
                    noOption.value = '';
                    noOption.textContent = 'No courts available for this client';
                    noOption.disabled = true;
                    courtSelect.appendChild(noOption);

                    console.log('No courts available for this client');
                }

                // Remove loading state
                courtSelect.classList.remove('court-dropdown-loading');
                courtSelect.disabled = false;
            }, 300);
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
                                    <li><a class="dropdown-item" href="#"><i class="bi bi-envelope me-2"></i>Message</a></li>
                                    <li><a class="dropdown-item" href="#"><i class="bi bi-person me-2"></i>View Profile</a></li>
                                    <li><hr class="dropdown-divider"></li>
                                    <li><a class="dropdown-item text-danger remove-participant" data-participant-id="${participant.profileId}" href="#"><i class="bi bi-person-x me-2"></i>Remove</a></li>
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
                        showToast('Success', 'Participant removed successfully');
                    } else {
                        showToast('Error', 'Error removing participant: ' + (data.message || 'Unknown error'), 'error');
                    }
                })
                .catch(error => {
                    hideLoading();
                    console.error('Error removing participant:', error);
                    showToast('Error', 'Error removing participant. Please try again.', 'error');
                });
        }

        // Enhanced run data loading function
        function loadRunDataEnhanced(runId) {
            console.log('?? Loading run data for ID:', runId);

            if (!runId) {
                console.error('? No run ID provided');
                hideLoading();
                return;
            }

            showLoading();

            fetch(`/Run/GetRunData?id=${runId}`)
                .then(response => {
                    console.log('?? API Response status:', response.status);

                    if (!response.ok) {
                        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                    }
                    return response.json();
                })
                .then(data => {
                    hideLoading();
                    console.log('?? Received data:', data);

                    if (data.success === false) {
                        console.error('? API returned error:', data.message);
                        showToast('Error', 'Error loading run data: ' + (data.message || 'Unknown error'), 'error');
                        return;
                    }

                    // Populate form fields with safe value assignment
                    const setFieldValue = (fieldId, value) => {
                        const field = document.getElementById(fieldId);
                        if (field && value !== null && value !== undefined) {
                            field.value = value;
                            console.log(`? Set ${fieldId} to: ${value}`);
                        } else if (!field) {
                            console.warn(`?? Field ${fieldId} not found`);
                        }
                    };

                    // Basic run information
                    setFieldValue('editRunTitle', data.name || '');
                    setFieldValue('editRunDescription', data.description || '');

                    // Date handling with validation
                    if (data.runDate) {
                        try {
                            const date = new Date(data.runDate);
                            if (!isNaN(date.getTime())) {
                                setFieldValue('editRunDate', data.runDate);
                            }
                        } catch (e) {
                            console.warn('?? Invalid run date format:', data.runDate);
                        }
                    }

                    // Time handling with validation
                    if (data.startTime) {
                        try {
                            // Handle different time formats
                            let timeValue = data.startTime;

                            // If it's in format "hh:mm:ss", convert to "hh:mm"
                            if (timeValue.includes(':')) {
                                const timeParts = timeValue.split(':');
                                if (timeParts.length >= 2) {
                                    timeValue = `${timeParts[0]}:${timeParts[1]}`;
                                }
                            }

                            setFieldValue('editRunTime', timeValue);
                        } catch (e) {
                            console.warn('?? Invalid start time format:', data.startTime);
                        }
                    }

                    if (data.endTime) {
                        try {
                            let timeValue = data.endTime;

                            // If it's in format "hh:mm:ss", convert to "hh:mm"
                            if (timeValue.includes(':')) {
                                const timeParts = timeValue.split(':');
                                if (timeParts.length >= 2) {
                                    timeValue = `${timeParts[0]}:${timeParts[1]}`;
                                }
                            }

                            setFieldValue('editEndTime', timeValue);
                        } catch (e) {
                            console.warn('?? Invalid end time format:', data.endTime);
                        }
                    }

                    // Numeric fields with validation
                    const setNumericField = (fieldId, value, defaultValue = 0) => {
                        const field = document.getElementById(fieldId);
                        if (field) {
                            const numValue = parseInt(value) || defaultValue;
                            field.value = numValue;
                            console.log(`? Set ${fieldId} to: ${numValue}`);
                        }
                    };

                    setNumericField('editMaxParticipants', data.playerLimit, 10);

                    // Select fields with validation
                    const setSelectField = (fieldId, value, defaultValue = '') => {
                        const select = document.getElementById(fieldId);
                        if (select && value) {
                            // Try to find matching option
                            for (let i = 0; i < select.options.length; i++) {
                                if (select.options[i].value.toLowerCase() === value.toLowerCase()) {
                                    select.selectedIndex = i;
                                    console.log(`? Set ${fieldId} to: ${value}`);
                                    return;
                                }
                            }
                            console.warn(`?? Option '${value}' not found for ${fieldId}`);
                        } else if (!select) {
                            console.warn(`?? Select field ${fieldId} not found`);
                        }
                    };

                    setSelectField('editSkillLevel', data.skillLevel, 'Intermediate');
                    setSelectField('editStatus', data.status, 'Active');
                    setSelectField('editRunType', data.type, 'Pickup');
                    setSelectField('editTeamType', data.teamType, 'Individual');
                    setSelectField('editIsPublic', data.isPublic ? 'true' : 'false', 'true');

                    // Court dropdown population with error handling
                    console.log('?? Court data received:', data.courtList);
                    console.log('?? Current court ID:', data.courtId);

                    try {
                        populateCourtDropdown(data.courtList || [], data.courtId);
                    } catch (e) {
                        console.error('? Error populating court dropdown:', e);
                        const courtSelect = document.getElementById('editCourtList');
                        if (courtSelect) {
                            courtSelect.innerHTML = '<option value="">Error loading courts</option>';
                        }
                    }

                    // Update participant counts with validation
                    const updateParticipantCount = (elementId, count, defaultValue = 0) => {
                        const element = document.getElementById(elementId);
                        if (element) {
                            const numCount = parseInt(count) || defaultValue;
                            element.textContent = numCount;
                            console.log(`? Updated ${elementId} to: ${numCount}`);
                        }
                    };

                    updateParticipantCount('currentParticipantsCount', data.playerCount, 0);
                    updateParticipantCount('maxParticipantsCount', data.playerLimit, 0);

                    // Set delete button run ID
                    const deleteIdField = document.getElementById('deleteRunId');
                    if (deleteIdField) {
                        deleteIdField.value = data.runId || runId;
                    }

                    // Load participants
                    try {
                        loadParticipants(data.runId || runId);
                    } catch (e) {
                        console.error('? Error loading participants:', e);
                    }

                    console.log('? Run data loaded successfully');
                })
                .catch(error => {
                    hideLoading();
                    console.error('? Detailed error loading run data:', {
                        error: error,
                        message: error.message,
                        stack: error.stack,
                        runId: runId
                    });
                    showToast('Error', 'Error loading run data: ' + error.message, 'error');
                });
        }

        // AJAX Save Function for Run Form
        function saveRunWithAjax(formData) {
            console.log('?? Saving run data via AJAX');

            // Show loading state
            showLoading();

            // Disable save button
            const saveButton = document.querySelector('#editRunForm button[type="submit"]');
            const originalButtonText = saveButton ? saveButton.innerHTML : '';
            if (saveButton) {
                saveButton.disabled = true;
                saveButton.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status"></span>Saving...';
            }

            // Prepare the data for AJAX submission
            const runData = {
                RunId: formData.get('RunId'),
                Name: formData.get('Name'),
                Description: formData.get('Description'),
                RunDate: formData.get('RunDate'),
                StartTime: formData.get('StartTime'),
                EndTime: formData.get('EndTime'),
                PlayerLimit: parseInt(formData.get('PlayerLimit')),
                SkillLevel: formData.get('SkillLevel'),
                Status: formData.get('Status'),
                Type: formData.get('Type'),
                TeamType: formData.get('TeamType'),
                IsPublic: formData.get('IsPublic') === 'true',
                Address: formData.get('Address'),
                City: formData.get('City'),
                State: formData.get('State'),
                Zip: formData.get('Zip'),
                CourtId: formData.get('CourtId'),
                ClientId: formData.get('ClientId')
            };

            console.log('?? Sending run data:', runData);

            // Make AJAX request to save run
            fetch('/Run/Edit', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: JSON.stringify(runData)
            })
                .then(response => {
                    console.log('?? Save response status:', response.status);

                    if (!response.ok) {
                        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                    }
                    return response.json();
                })
                .then(data => {
                    console.log('?? Save response data:', data);

                    hideLoading();

                    // Restore save button
                    if (saveButton) {
                        saveButton.disabled = false;
                        saveButton.innerHTML = originalButtonText;
                    }

                    if (data.success) {
                        showToast('Success', data.message || 'Run updated successfully!');

                        // Optionally close the modal after successful save
                        const editModal = bootstrap.Modal.getInstance(document.getElementById('editRunModal'));
                        if (editModal) {
                            setTimeout(() => {
                                editModal.hide();

                                // Refresh the DataTable instead of full page reload
                                if (window.runsDataTable && $.fn.dataTable.isDataTable('#runsTable')) {
                                    console.log('?? Refreshing DataTable...');
                                    try {
                                        window.runsDataTable.ajax.reload(null, false); // false = don't reset paging
                                    } catch (e) {
                                        console.log('?? Ajax reload failed, doing full page reload...');
                                        window.location.reload();
                                    }
                                } else {
                                    // Fallback to page reload if DataTable not available
                                    console.log('?? Refreshing page...');
                                    window.location.reload();
                                }
                            }, 1500);
                        }
                    } else {
                        showToast('Error', data.message || 'Failed to save run', 'error');
                    }
                })
                .catch(error => {
                    console.error('? Error saving run:', error);

                    hideLoading();

                    // Restore save button
                    if (saveButton) {
                        saveButton.disabled = false;
                        saveButton.innerHTML = originalButtonText;
                    }

                    showToast('Error', 'Error saving run: ' + error.message, 'error');
                });
        }

        // Initialize DataTable with comprehensive safety checks
        function initializeDataTable() {
            console.log('?? DataTable initialization attempt...');

            // Check if jQuery and DataTables are available
            if (typeof $ === 'undefined' || typeof $.fn.dataTable === 'undefined') {
                console.warn('DataTable: jQuery or DataTables not available');
                return null;
            }

            const tableElement = $('#runsTable');

            if (tableElement.length === 0) {
                console.log('DataTable: Table element #runsTable not found');
                return null;
            }

            // Multiple checks for existing DataTable
            let isDataTableInitialized = false;

            try {
                // Method 1: jQuery DataTables check
                isDataTableInitialized = $.fn.dataTable.isDataTable('#runsTable');
            } catch (e) {
                console.log('DataTable: Error checking isDataTable, trying alternative method');
            }

            // Method 2: Check if DataTable API exists on element
            if (!isDataTableInitialized) {
                try {
                    isDataTableInitialized = tableElement.hasClass('dataTable') ||
                        tableElement.data('datatable') !== undefined ||
                        tableElement[0].classList.contains('dataTable');
                } catch (e) {
                    console.log('DataTable: Error with alternative check');
                }
            }

            // If already initialized, destroy it completely
            if (isDataTableInitialized) {
                console.log('DataTable: Found existing instance, destroying...');
                try {
                    const existingTable = tableElement.DataTable();
                    existingTable.destroy(true); // true = remove from DOM completely
                    console.log('DataTable: Existing instance destroyed successfully');
                } catch (e) {
                    console.error('DataTable: Error destroying existing instance:', e);
                    // Force cleanup
                    tableElement.removeClass('dataTable');
                    tableElement.removeData();
                    tableElement.find('thead, tbody, tfoot').off();
                }
            }

            // Only initialize if table has data rows
            const dataRows = tableElement.find('tbody tr');
            if (dataRows.length === 0 || (dataRows.length === 1 && dataRows.find('td').length === 1)) {
                console.log('DataTable: No data rows found, skipping initialization');
                return null;
            }

            console.log('DataTable: Initializing new instance...');

            try {
                // Small delay to ensure DOM is ready
                setTimeout(() => {
                    const dataTable = tableElement.DataTable({
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
                        order: [[1, 'asc']],
                        // Prevent reinitialization
                        retrieve: true,
                        destroy: false,
                        // Add error handling
                        initComplete: function () {
                            console.log('? DataTable: Initialization complete');
                        },
                        drawCallback: function () {
                            console.log('DataTable: Draw callback executed');
                        }
                    });

                    // Store reference globally for cleanup
                    window.runsDataTable = dataTable;
                    console.log('? DataTable: Successfully initialized and stored globally');

                }, 100);

                return true; // Return success indicator
            } catch (error) {
                console.error('? DataTable: Initialization failed:', error);
                return null;
            }
        }

        // Initialize DataTable with safety checks
        let runsTable = null;

        // Only initialize if not already done
        if (!window.runsDataTable) {
            runsTable = initializeDataTable();
        } else {
            console.log('DataTable: Using existing global instance');
            runsTable = window.runsDataTable;
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

                // Load run data with improved error handling
                loadRunDataEnhanced(runId);
            });
        }

        // Handle edit run form submission with AJAX
        const editRunForm = document.getElementById('editRunForm');
        if (editRunForm) {
            editRunForm.addEventListener('submit', function (e) {
                e.preventDefault(); // Prevent normal form submission

                console.log('?? Edit run form submitted');

                // Validate form before submission
                if (!this.checkValidity()) {
                    e.stopPropagation();
                    this.classList.add('was-validated');
                    showToast('Error', 'Please fill in all required fields correctly.', 'error');
                    return;
                }

                // Get form data
                const formData = new FormData(this);

                // Validate required fields
                const requiredFields = ['RunId', 'Name', 'Description', 'RunDate', 'StartTime', 'PlayerLimit'];
                const missingFields = [];

                requiredFields.forEach(field => {
                    if (!formData.get(field)) {
                        missingFields.push(field);
                    }
                });

                if (missingFields.length > 0) {
                    showToast('Error', `Missing required fields: ${missingFields.join(', ')}`, 'error');
                    return;
                }

                // Save with AJAX
                saveRunWithAjax(formData);
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
                    showToast('Error', 'Unable to determine current capacity. Please refresh and try again.', 'error');
                    return;
                }

                const currentCount = parseInt(currentCountEl.textContent);
                const maxCount = parseInt(maxCountEl.textContent);

                if (currentCount >= maxCount) {
                    showToast('Error', 'This run is already at maximum capacity. Cannot add more participants.', 'error');
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
                            showToast('Success', 'Participant added successfully');
                        } else {
                            showToast('Error', 'Error adding participant: ' + (data.message || 'Unknown error'), 'error');
                        }
                    })
                    .catch(error => {
                        hideLoading();
                        console.error('Error adding participant:', error);
                        showToast('Error', 'Error adding participant. Please try again.', 'error');
                    });
            });
        }

        // Comprehensive cleanup function
        function cleanupDataTable() {
            if (window.runsDataTable) {
                try {
                    console.log('DataTable: Performing cleanup...');
                    window.runsDataTable.destroy(true);
                    window.runsDataTable = null;
                    console.log('DataTable: Cleanup completed');
                } catch (e) {
                    console.error('DataTable: Error during cleanup:', e);
                }
            }
        }

        // Cleanup on page unload
        window.addEventListener('beforeunload', cleanupDataTable);

        // Cleanup on page hide (for mobile/tablet)
        window.addEventListener('pagehide', cleanupDataTable);

        console.log('? Enhanced Run Management JavaScript with AJAX save fully loaded');

    }); // End DOMContentLoaded

})(); // End IIFE
});