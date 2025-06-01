// Initialize DataTable if the table exists
const clientsTable = $('#clientsTable');
if (clientsTable.length > 0) {
    initializeClientsTable();
}

// Initialize modals and event handlers
initializeModalHandlers();
initializeFormHandlers();

console.log('? Client Management initialized successfully');

// ========== TABLE INITIALIZATION ==========
function initializeClientsTable() {
    const table = clientsTable.DataTable({
        responsive: true,
        lengthMenu: [[10, 25, 50, -1], [10, 25, 50, "All"]],
        dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>rtip',
        language: {
            search: "_INPUT_",
            searchPlaceholder: "Search clients...",
            lengthMenu: "Show _MENU_ clients per page",
            info: "Showing _START_ to _END_ of _TOTAL_ clients",
            infoEmpty: "Showing 0 to 0 of 0 clients",
            infoFiltered: "(filtered from _MAX_ total clients)"
        },
        columnDefs: [
            { className: "align-middle", targets: "_all" },
            { orderable: false, targets: [4] }
        ],
        order: [[1, 'desc']]
    });

    // Initialize filters
    initializeTableFilters(table);
}

// ========== TABLE FILTERS ==========
function initializeTableFilters(table) {
    const statusFilter = $('#statusFilter');
    const joinDateFilter = $('#joinDateFilter');
    const resetFiltersBtn = $('#resetFilters');
    const activeFiltersContainer = $('#activeFilters');

    if (!statusFilter.length || !joinDateFilter.length) {
        console.warn('?? Filter elements not found');
        return;
    }

    function applyFilters() {
        // Remove existing custom filters
        $.fn.dataTable.ext.search = $.fn.dataTable.ext.search.filter(fn =>
            !fn.name || fn.name !== 'clientTableFilter'
        );

        // Add new filter
        const customFilter = function (settings, data, dataIndex) {
            if (settings.nTable.id !== 'clientsTable') return true;

            // Skip if all filters are 'all'
            if (statusFilter.val() === 'all' && joinDateFilter.val() === 'all') {
                return true;
            }

            const row = $(table.row(dataIndex).node());

            try {
                // Status filtering
                if (statusFilter.val() !== 'all') {
                    const statusValue = statusFilter.val().toLowerCase();
                    const rowStatus = (row.attr('data-status') || 'active').toLowerCase();
                    if (rowStatus !== statusValue) {
                        return false;
                    }
                }

                // Date filtering
                if (joinDateFilter.val() !== 'all') {
                    if (!filterByDate(row, data[1], joinDateFilter.val())) {
                        return false;
                    }
                }

                return true;
            } catch (error) {
                console.error('? Error in filter function:', error);
                return true;
            }
        };

        customFilter.name = 'clientTableFilter';
        $.fn.dataTable.ext.search.push(customFilter);
        table.draw();
        updateActiveFilters();
    }

    function filterByDate(row, dateText, filterValue) {
        try {
            const dateString = row.attr('data-date') || dateText;
            let date = new Date(dateString);

            if (isNaN(date.getTime())) {
                const parts = dateText.split('/');
                if (parts.length === 3) {
                    date = new Date(parts[2], parts[0] - 1, parts[1]);
                } else {
                    return false;
                }
            }

            const today = new Date();
            today.setHours(0, 0, 0, 0);

            switch (filterValue) {
                case 'this-month':
                    const startOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);
                    return date >= startOfMonth;
                case 'last-3-months':
                    const threeMonthsAgo = new Date(today);
                    threeMonthsAgo.setMonth(today.getMonth() - 3);
                    return date >= threeMonthsAgo;
                case 'last-6-months':
                    const sixMonthsAgo = new Date(today);
                    sixMonthsAgo.setMonth(today.getMonth() - 6);
                    return date >= sixMonthsAgo;
                case 'this-year':
                    const startOfYear = new Date(today.getFullYear(), 0, 1);
                    return date >= startOfYear;
                default:
                    return true;
            }
        } catch (e) {
            console.error("? Date filtering error:", e);
            return false;
        }
    }

    function updateActiveFilters() {
        if (!activeFiltersContainer.length) return;

        activeFiltersContainer.find('.filter-badge, .filter-none').remove();

        const hasActiveFilters = statusFilter.val() !== 'all' || joinDateFilter.val() !== 'all';

        if (!hasActiveFilters) {
            activeFiltersContainer.append($('<span>').addClass('text-muted filter-none').text('None'));
            return;
        }

        if (statusFilter.val() !== 'all') {
            addFilterBadge('Status', formatFilterValue(statusFilter.val()), () => {
                statusFilter.val('all');
                applyFilters();
            });
        }

        if (joinDateFilter.val() !== 'all') {
            addFilterBadge('Join Date', formatFilterValue(joinDateFilter.val()), () => {
                joinDateFilter.val('all');
                applyFilters();
            });
        }
    }

    function addFilterBadge(label, value, removeCallback) {
        const badge = $('<span>').addClass('badge bg-primary me-2 filter-badge').text(label + ': ' + value);
        const removeBtn = $('<button>').addClass('btn-close btn-close-white ms-1').css('font-size', '0.5rem').on('click', removeCallback);
        badge.append(removeBtn);
        activeFiltersContainer.append(badge);
    }

    function formatFilterValue(value) {
        return value.split('-').map(word => word.charAt(0).toUpperCase() + word.slice(1)).join(' ');
    }

    // Event listeners
    statusFilter.on('change', applyFilters);
    joinDateFilter.on('change', applyFilters);

    if (resetFiltersBtn.length) {
        resetFiltersBtn.on('click', function () {
            statusFilter.val('all');
            joinDateFilter.val('all');
            applyFilters();
        });
    }

    applyFilters();
}

// ========== MODAL HANDLERS ==========
function initializeModalHandlers() {
    const editClientModal = document.getElementById('editClientModal');
    if (editClientModal) {
        editClientModal.addEventListener('show.bs.modal', handleEditModalShow);
        editClientModal.addEventListener('hidden.bs.modal', handleEditModalHide);

        // Tab switching handlers
        const tabButtons = editClientModal.querySelectorAll('button[data-bs-toggle="tab"]');
        tabButtons.forEach(button => {
            button.addEventListener('shown.bs.tab', handleTabSwitch);
        });
    }

    const deleteClientBtn = document.getElementById('deleteClientBtn');
    if (deleteClientBtn) {
        deleteClientBtn.addEventListener('click', handleDeleteClient);
    }
}

function handleEditModalShow(event) {
    const button = event.relatedTarget;
    const clientId = button.getAttribute('data-client-id');

    console.log('?? Opening edit modal for client ID:', clientId);

    if (!clientId) {
        console.error('? No client ID found on button');
        showToast('Client ID is missing', 'error');
        return;
    }

    // Set client IDs in forms
    safeSetValue('editClientId', clientId);
    safeSetValue('deleteClientId', clientId);

    // Clear previous data
    clearAllForms();

    // Load client data
    loadClientData(clientId);
}

function handleEditModalHide() {
    console.log('?? Edit modal closed, clearing forms');
    clearAllForms();
}

function handleTabSwitch(event) {
    const targetTab = event.target.getAttribute('data-bs-target');
    const clientId = document.getElementById('editClientId')?.value;

    console.log('?? Switching to tab:', targetTab, 'for client:', clientId);

    if (!clientId) return;

    switch (targetTab) {
        case '#courts-tab-pane':
            loadClientCourts(clientId);
            break;
        case '#users-tab-pane':
            loadClientUsers(clientId);
            break;
        case '#business-tab-pane':
            loadClientBusinessData(clientId);
            break;
    }
}

function handleDeleteClient() {
    const clientId = safeGetValue('editClientId');
    if (!clientId) return;

    safeSetValue('deleteClientId', clientId);

    const editModal = bootstrap.Modal.getInstance(document.getElementById('editClientModal'));
    if (editModal) editModal.hide();

    const deleteModalEl = document.getElementById('deleteClientModal');
    if (deleteModalEl) {
        const deleteModal = new bootstrap.Modal(deleteModalEl);
        deleteModal.show();
    }
}

// ========== FORM HANDLERS ==========
function initializeFormHandlers() {
    // AJAX form submission for client edit
    const editClientForm = document.getElementById('editClientForm');
    if (editClientForm) {
        editClientForm.addEventListener('submit', handleClientFormSubmit);
    }

    // Court management buttons
    document.addEventListener('click', function (e) {
        if (e.target.closest('#addCourtBtn')) {
            e.preventDefault();
            const clientId = safeGetValue('editClientId');
            if (clientId) addClientCourt(clientId);
        }
    });

    // User management buttons  
    document.addEventListener('click', function (e) {
        if (e.target.closest('#addUserBtn')) {
            e.preventDefault();
            const clientId = safeGetValue('editClientId');
            if (clientId) addClientUser(clientId);
        }
    });
}

function handleClientFormSubmit(e) {
    e.preventDefault();

    const formData = new FormData(e.target);
    const clientData = {};

    // Convert FormData to object
    for (const [key, value] of formData.entries()) {
        clientData[key] = value;
    }

    console.log('?? Submitting client data:', clientData);

    const submitBtn = e.target.querySelector('button[type="submit"]');
    const originalText = submitBtn ? submitBtn.innerHTML : '';

    if (submitBtn) {
        setButtonLoading(submitBtn, true, 'Saving...');
    }

    const token = getAntiForgeryToken();

    fetch('/Client/Edit', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        body: JSON.stringify(clientData)
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
                showToast('? ' + result.message, 'success');

                // Update the table row if visible
                updateTableRow(clientData);

                // Close modal after short delay
                setTimeout(() => {
                    const modal = bootstrap.Modal.getInstance(document.getElementById('editClientModal'));
                    if (modal) modal.hide();
                }, 1000);
            } else {
                showToast('? ' + (result.message || 'Failed to save client'), 'error');
            }
        })
        .catch(error => {
            console.error('? Error saving client:', error);
            if (submitBtn) {
                setButtonLoading(submitBtn, false);
            }
            showToast('?? Error saving client: ' + error.message, 'error');
        });
}

// ========== DATA LOADING FUNCTIONS ==========
function loadClientData(clientId) {
    console.log('?? Loading client data for ID:', clientId);

    if (!clientId) {
        console.error('? No client ID provided');
        return;
    }

    // Try to populate from table data first
    const row = findClientRowById(clientId);
    if (row) {
        console.log('?? Found table row, extracting data...');
        const tableData = extractTableData(row);
        populateFromTableData(tableData);
    }

    // Always call API for complete data
    fetch(`/Client/GetClientData?id=${encodeURIComponent(clientId)}`)
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }
            return response.json();
        })
        .then(data => {
            console.log('?? Received client data:', data);

            if (data.success !== false) {
                populateFromAPIData(data);
                showToast('Client data loaded successfully', 'success');
            } else {
                showToast('Failed to load complete client data: ' + (data.message || 'Unknown error'), 'warning');
            }
        })
        .catch(error => {
            console.error('? Error loading client data:', error);
            showToast('Error loading client data: ' + error.message, 'error');
        });
}

function extractTableData(row) {
    if (!row) return {};

    console.log('?? Extracting data from table row');

    // Get data attributes
    const dataFromAttributes = {
        clientId: row.getAttribute('data-client-id'),
        clientNumber: row.getAttribute('data-client-number'),
        name: row.getAttribute('data-name'),
        address: row.getAttribute('data-address'),
        city: row.getAttribute('data-city'),
        state: row.getAttribute('data-state'),
        zip: row.getAttribute('data-zip'),
        phoneNumber: row.getAttribute('data-phone-number'),
        createdDate: row.getAttribute('data-created-date')
    };

    // Extract from cell content as fallback
    const cells = row.querySelectorAll('td');
    if (cells.length >= 4) {
        // Client name and number from first column
        const clientCell = cells[0];
        const nameEl = clientCell.querySelector('.fw-semibold');
        const numberEl = clientCell.querySelector('.text-muted.small');

        if (nameEl && !dataFromAttributes.name) {
            dataFromAttributes.name = nameEl.textContent.trim();
        }
        if (numberEl && !dataFromAttributes.clientNumber) {
            const match = numberEl.textContent.match(/ID: #(\S+)/);
            if (match) dataFromAttributes.clientNumber = match[1];
        }

        // Date from second column
        if (cells[1] && !dataFromAttributes.createdDate) {
            dataFromAttributes.createdDate = cells[1].textContent.trim();
        }

        // Address from third column
        if (cells[2] && !dataFromAttributes.address) {
            const addressText = cells[2].textContent.trim();
            const addressParts = addressText.split(',').map(part => part.trim());
            if (addressParts.length >= 4) {
                dataFromAttributes.address = addressParts[0];
                dataFromAttributes.city = addressParts[1];
                dataFromAttributes.state = addressParts[2];
                dataFromAttributes.zip = addressParts[3];
            }
        }

        // Phone from fourth column
        if (cells[3] && !dataFromAttributes.phoneNumber) {
            dataFromAttributes.phoneNumber = cells[3].textContent.trim();
        }
    }

    return dataFromAttributes;
}

function populateFromTableData(data) {
    console.log('?? Populating form from table data:', data);

    try {
        safeSetValue('editClientNumber', data.clientNumber);
        safeSetValue('editName', data.name);
        safeSetValue('editAddress', data.address);
        safeSetValue('editCity', data.city);
        safeSetValue('editState', data.state);
        safeSetValue('editZip', data.zip);
        safeSetValue('editPhoneNumber', data.phoneNumber);
        safeSetValue('editCreatedDate', data.createdDate);

        console.log('? Table data populated successfully');
    } catch (error) {
        console.error('? Error populating from table data:', error);
    }
}

function populateFromAPIData(data) {
    console.log('?? Populating form from API data:', data);

    try {
        const client = data.client || data;

        safeSetValue('editClientNumber', client.clientNumber);
        safeSetValue('editName', client.name);
        safeSetValue('editAddress', client.address);
        safeSetValue('editCity', client.city);
        safeSetValue('editState', client.state);
        safeSetValue('editZip', client.zip);
        safeSetValue('editPhoneNumber', client.phoneNumber);
        safeSetValue('editNotes', client.notes);
        safeSetValue('editCreatedDate', client.createdDate);

        // Set status if available
        if (client.status) {
            safeSetSelect('editStatus', client.status);
        }

        // Load courts data
        if (data.courtList) {
            displayClientCourts(data.courtList);
        }

        console.log('? API data populated successfully');
    } catch (error) {
        console.error('? Error populating from API data:', error);
    }
}

function findClientRowById(clientId) {
    if (!clientId) return null;

    console.log('?? Looking for row with client ID:', clientId);

    // Try different strategies to find the row
    let row = document.querySelector(`tr[data-client-id="${clientId}"]`);
    if (row) return row;

    const button = document.querySelector(`button[data-client-id="${clientId}"]`);
    if (button) {
        row = button.closest('tr');
        if (row) return row;
    }

    // Search within table body
    const tableBody = document.querySelector('#clientsTable tbody');
    if (tableBody) {
        const allRows = tableBody.querySelectorAll('tr');
        for (const tr of allRows) {
            const editBtn = tr.querySelector(`[data-client-id="${clientId}"]`);
            if (editBtn) return tr;
        }
    }

    console.warn('?? Row not found for client ID:', clientId);
    return null;
}

function loadClientCourts(clientId) {
    console.log('?? Loading courts for client:', clientId);

    const courtsTableBody = document.getElementById('courtsTableBody');
    if (!courtsTableBody) return;

    courtsTableBody.innerHTML = `
        <tr>
            <td colspan="3" class="text-center py-4 text-muted">
                <div class="spinner-border spinner-border-sm text-secondary me-2" role="status"></div>
                Loading courts...
            </td>
        </tr>`;

    fetch(`/Client/GetClientData?id=${encodeURIComponent(clientId)}`)
        .then(response => response.json())
        .then(data => {
            if (data.success !== false && data.courtList) {
                displayClientCourts(data.courtList);
            } else {
                displayClientCourts([]);
            }
        })
        .catch(error => {
            console.error('? Error loading courts:', error);
            courtsTableBody.innerHTML = `
                <tr>
                    <td colspan="3" class="text-center py-4 text-danger">
                        Error loading courts. Please try again.
                    </td>
                </tr>`;
        });
}

function loadClientUsers(clientId) {
    console.log('?? Loading users for client:', clientId);

    const usersTableBody = document.getElementById('usersTableBody');
    if (!usersTableBody) return;

    usersTableBody.innerHTML = `
        <tr>
            <td colspan="5" class="text-center py-4 text-muted">
                <div class="spinner-border spinner-border-sm text-secondary me-2" role="status"></div>
                Loading users...
            </td>
        </tr>`;

    // Mock data for now - replace with actual API call
    setTimeout(() => {
        const mockUsers = [
            {
                userId: 'user-1',
                firstName: 'John',
                lastName: 'Smith',
                email: 'john.smith@example.com',
                role: 'Manager',
                status: 'Active'
            },
            {
                userId: 'user-2',
                firstName: 'Jane',
                lastName: 'Doe',
                email: 'jane.doe@example.com',
                role: 'Staff',
                status: 'Active'
            }
        ];
        displayClientUsers(mockUsers);
    }, 500);
}

function loadClientBusinessData(clientId) {
    console.log('?? Loading business data for client:', clientId);

    fetch(`/Client/GetClientBusinessData?id=${encodeURIComponent(clientId)}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                updateClientBusinessDisplay(data.business);
            } else {
                showToast('Failed to load business data', 'warning');
            }
        })
        .catch(error => {
            console.error('? Error loading business data:', error);
            showToast('Error loading business data', 'error');
        });
}

// ========== DISPLAY FUNCTIONS ==========
function displayClientCourts(courts) {
    console.log('?? Displaying courts:', courts);
    const courtsTableBody = document.getElementById('courtsTableBody');
    if (!courtsTableBody) return;

    if (!courts || courts.length === 0) {
        courtsTableBody.innerHTML = `
            <tr>
                <td colspan="3" class="text-center py-4 text-muted">
                    No courts associated with this client.
                </td>
            </tr>`;
        return;
    }

    let html = '';
    courts.forEach(court => {
        html += `
            <tr>
                <td>${court.name || 'N/A'}</td>
                <td>Standard</td>
                <td>
                    <div class="btn-group btn-group-sm">
                        <button type="button" class="btn btn-outline-primary edit-court-btn"
                                data-court-id="${court.courtId}"
                                title="Edit Court">
                            <i class="bi bi-pencil"></i>
                        </button>
                        <button type="button" class="btn btn-outline-danger remove-court-btn"
                                data-court-id="${court.courtId}"
                                title="Remove Court">
                            <i class="bi bi-trash"></i>
                        </button>
                    </div>
                </td>
            </tr>`;
    });

    courtsTableBody.innerHTML = html;

    // Add event listeners
    courtsTableBody.querySelectorAll('.edit-court-btn').forEach(button => {
        button.addEventListener('click', function () {
            const courtId = this.getAttribute('data-court-id');
            const clientId = safeGetValue('editClientId');
            editClientCourt(clientId, courtId);
        });
    });

    courtsTableBody.querySelectorAll('.remove-court-btn').forEach(button => {
        button.addEventListener('click', function () {
            const courtId = this.getAttribute('data-court-id');
            const clientId = safeGetValue('editClientId');
            removeClientCourt(clientId, courtId);
        });
    });
}

function displayClientUsers(users) {
    console.log('?? Displaying users:', users);
    const usersTableBody = document.getElementById('usersTableBody');
    if (!usersTableBody) return;

    if (!users || users.length === 0) {
        usersTableBody.innerHTML = `
            <tr>
                <td colspan="5" class="text-center py-4 text-muted">
                    No users associated with this client.
                </td>
            </tr>`;
        return;
    }

    let html = '';
    users.forEach(user => {
        const userInitials = getUserInitials(user.firstName, user.lastName, user.email);
        const statusClass = getStatusClass(user.status);

        html += `
            <tr>
                <td>
                    <div class="d-flex align-items-center">
                        <div class="flex-shrink-0 me-3" style="width: 36px; height: 36px; background-color: var(--secondary-color); color: white; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: bold;">
                            ${userInitials}
                        </div>
                        <div>
                            <div class="fw-semibold">${user.firstName || ''} ${user.lastName || ''}</div>
                        </div>
                    </div>
                </td>
                <td>${user.email || 'N/A'}</td>
                <td>${user.role || 'User'}</td>
                <td><span class="badge ${statusClass} badge-pill">${user.status || 'Active'}</span></td>
                <td>
                    <div class="btn-group btn-group-sm">
                        <button type="button" class="btn btn-outline-primary edit-user-btn"
                                data-user-id="${user.userId}"
                                title="Edit User">
                            <i class="bi bi-pencil"></i>
                        </button>
                        <button type="button" class="btn btn-outline-danger remove-user-btn"
                                data-user-id="${user.userId}"
                                title="Remove User">
                            <i class="bi bi-trash"></i>
                        </button>
                    </div>
                </td>
            </tr>`;
    });

    usersTableBody.innerHTML = html;

    // Add event listeners
    usersTableBody.querySelectorAll('.edit-user-btn').forEach(button => {
        button.addEventListener('click', function () {
            const userId = this.getAttribute('data-user-id');
            const clientId = safeGetValue('editClientId');
            editClientUser(clientId, userId);
        });
    });

    usersTableBody.querySelectorAll('.remove-user-btn').forEach(button => {
        button.addEventListener('click', function () {
            const userId = this.getAttribute('data-user-id');
            const clientId = safeGetValue('editClientId');
            removeClientUser(clientId, userId);
        });
    });
}

function updateClientBusinessDisplay(business) {
    console.log('?? Updating business display:', business);

    // Update business information
    safeUpdateElement('businessName', business.businessName);
    safeUpdateElement('businessType', business.businessType);
    safeUpdateElement('establishedDate', formatDate(business.establishedDate));
    safeUpdateElement('employeeCount', business.employeeCount);
    safeUpdateElement('annualRevenue', business.annualRevenue);
    safeUpdateElement('operatingHours', business.operatingHours);
    safeUpdateElement('website', business.website);

    // Update statistics
    if (business.stats) {
        safeUpdateElement('totalBookings', business.stats.totalBookings);
        safeUpdateElement('monthlyUsers', business.stats.monthlyUsers);
        safeUpdateElement('averageRating', business.stats.averageRating);
        safeUpdateElement('repeatCustomers', business.stats.repeatCustomers + '%');
    }

    // Update services list
    if (business.primaryServices) {
        const servicesList = document.getElementById('primaryServices');
        if (servicesList) {
            servicesList.innerHTML = business.primaryServices.map(service =>
                `<span class="badge bg-primary me-1 mb-1">${service}</span>`
            ).join('');
        }
    }
}

// ========== COURT MANAGEMENT ==========
function addClientCourt(clientId) {
    console.log('? Adding court for client:', clientId);

    const courtName = prompt('Enter court name:');
    if (!courtName) return;

    const courtData = {
        clientId: clientId,
        name: courtName
    };

    const token = getAntiForgeryToken();

    fetch('/Client/AddClientCourt', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        body: JSON.stringify(courtData)
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showToast('Court added successfully', 'success');
                loadClientCourts(clientId);
            } else {
                showToast('Error adding court: ' + (data.message || 'Unknown error'), 'error');
            }
        })
        .catch(error => {
            console.error('? Error adding court:', error);
            showToast('Error adding court. Please try again.', 'error');
        });
}

function editClientCourt(clientId, courtId) {
    console.log('?? Editing court:', courtId, 'for client:', clientId);

    const courtName = prompt('Enter new court name:');
    if (courtName === null) return;

    const courtData = {
        courtId: courtId,
        clientId: clientId,
        name: courtName
    };

    const token = getAntiForgeryToken();

    fetch('/Client/UpdateClientCourt', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        body: JSON.stringify(courtData)
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showToast('Court updated successfully', 'success');
                loadClientCourts(clientId);
            } else {
                showToast('Error updating court: ' + (data.message || 'Unknown error'), 'error');
            }
        })
        .catch(error => {
            console.error('? Error updating court:', error);
            showToast('Error updating court. Please try again.', 'error');
        });
}

function removeClientCourt(clientId, courtId) {
    console.log('??? Removing court:', courtId, 'for client:', clientId);

    if (!confirm('Are you sure you want to remove this court?')) return;

    const token = getAntiForgeryToken();

    fetch('/Client/RemoveClientCourt', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        body: JSON.stringify({ clientId, courtId })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showToast('Court removed successfully', 'success');
                loadClientCourts(clientId);
            } else {
                showToast('Error removing court: ' + (data.message || 'Unknown error'), 'error');
            }
        })
        .catch(error => {
            console.error('? Error removing court:', error);
            showToast('Error removing court. Please try again.', 'error');
        });
}

// ========== USER MANAGEMENT ==========
function addClientUser(clientId) {
    console.log('? Adding user for client:', clientId);

    const email = prompt('Enter user email address:');
    if (!email) return;

    const userData = {
        clientId: clientId,
        email: email
    };

    showToast('User management functionality coming soon', 'info');
}

function editClientUser(clientId, userId) {
    console.log('?? Editing user:', userId, 'for client:', clientId);
    showToast('User management functionality coming soon', 'info');
}

function removeClientUser(clientId, userId) {
    console.log('??? Removing user:', userId, 'for client:', clientId);

    if (!confirm('Are you sure you want to remove this user from the client?')) return;

    showToast('User management functionality coming soon', 'info');
}

// ========== UI STATE MANAGEMENT ==========
function clearAllForms() {
    clearClientDetailsForm();
    clearCourtsDisplay();
    clearUsersDisplay();
    clearBusinessDisplay();
}

function clearClientDetailsForm() {
    const fields = [
        'editClientNumber', 'editName', 'editAddress', 'editCity',
        'editState', 'editZip', 'editPhoneNumber', 'editNotes', 'editCreatedDate'
    ];

    fields.forEach(field => safeSetValue(field, ''));
}

function clearCourtsDisplay() {
    const courtsTableBody = document.getElementById('courtsTableBody');
    if (courtsTableBody) {
        courtsTableBody.innerHTML = `
            <tr>
                <td colspan="3" class="text-center py-4 text-muted">
                    Loading courts...
                </td>
            </tr>`;
    }
}

function clearUsersDisplay() {
    const usersTableBody = document.getElementById('usersTableBody');
    if (usersTableBody) {
        usersTableBody.innerHTML = `
            <tr>
                <td colspan="5" class="text-center py-4 text-muted">
                    Loading users...
                </td>
            </tr>`;
    }
}

function clearBusinessDisplay() {
    const businessElements = [
        'businessName', 'businessType', 'establishedDate', 'employeeCount',
        'annualRevenue', 'operatingHours', 'website', 'totalBookings',
        'monthlyUsers', 'averageRating', 'repeatCustomers'
    ];

    businessElements.forEach(elementId => safeUpdateElement(elementId, '--'));

    const servicesList = document.getElementById('primaryServices');
    if (servicesList) {
        servicesList.innerHTML = '';
    }
}

function updateTableRow(clientData) {
    // Update the table row with new data if the table is visible
    const row = findClientRowById(clientData.ClientId);
    if (row) {
        // Update name
        const nameEl = row.querySelector('.fw-semibold');
        if (nameEl) nameEl.textContent = clientData.Name;

        // Update address
        const addressCell = row.cells[2];
        if (addressCell) {
            addressCell.textContent = `${clientData.Address}, ${clientData.City}, ${clientData.State}, ${clientData.Zip}`;
        }

        // Update phone
        const phoneCell = row.cells[3];
        if (phoneCell) {
            phoneCell.textContent = clientData.PhoneNumber;
        }
    }
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

function safeUpdateElement(elementId, value) {
    const element = document.getElementById(elementId);
    if (element) {
        element.textContent = value || '--';
    }
}

function getAntiForgeryToken() {
    return document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
}

function getUserInitials(firstName, lastName, email) {
    if (firstName && lastName) {
        return `${firstName[0]}${lastName[0]}`.toUpperCase();
    } else if (firstName) {
        return firstName[0].toUpperCase();
    } else if (email) {
        return email[0].toUpperCase();
    }
    return 'NA';
}

function getStatusClass(status) {
    switch (status?.toLowerCase()) {
        case 'inactive': return 'bg-secondary';
        case 'pending': return 'bg-warning';
        default: return 'bg-success';
    }
}

function setButtonLoading(button, loading = true, loadingText = 'Loading...') {
    if (loading) {
        button.dataset.originalText = button.innerHTML;
        button.disabled = true;
        button.innerHTML = `<span class="spinner-border spinner-border-sm me-2" role="status"></span>${loadingText}`;
    } else {
        button.disabled = false;
        button.innerHTML = button.dataset.originalText || button.innerHTML;
        delete button.dataset.originalText;
    }
}

function formatDate(dateString) {
    if (!dateString) return '--';
    try {
        const date = new Date(dateString);
        return date.toLocaleDateString();
    } catch (e) {
        return '--';
    }
}

function showToast(message, type = 'success', title = null) {
    if (window.UIUtils) {
        window.UIUtils.showToast(message, type, title);
    } else {
        console.log(`${type.toUpperCase()}: ${message}`);
        alert(`${type}: ${message}`);
    }
}

// Debug functions
window.clientDebug = {
    loadClientData,
    findClientRowById,
    extractTableData,
    populateFromTableData,
    populateFromAPIData,
    loadClientCourts,
    loadClientUsers,
    loadClientBusinessData
};

console.log('?? Debug functions available: window.clientDebug');