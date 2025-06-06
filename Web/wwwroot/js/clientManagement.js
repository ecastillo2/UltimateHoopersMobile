/**
 * Enhanced Client Management JavaScript with Image Upload Functionality
 * Enhanced with proper DataTable refresh, button state management, fixed toast notifications, and image handling
 */

// ========== TOAST NOTIFICATION SYSTEM (FIXED) ==========
function createToastNotification(message, type = 'info', title = '', duration = 5000) {
    console.log(`${type.toUpperCase()}: ${title} - ${message}`);

    // Create toast element
    const toast = document.createElement('div');
    toast.className = `alert alert-${getBootstrapAlertClass(type)} alert-dismissible fade show position-fixed`;
    toast.style.cssText = `
        top: 20px;
        right: 20px;
        z-index: 9999;
        min-width: 300px;
        max-width: 500px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
    `;

    const icon = getToastIcon(type);
    toast.innerHTML = `
        <div class="d-flex align-items-center">
            <i class="bi bi-${icon} me-2"></i>
            <div class="flex-grow-1">
                ${title ? `<strong>${title}:</strong> ` : ''}${message}
            </div>
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;

    document.body.appendChild(toast);

    // Auto remove after duration
    setTimeout(() => {
        if (toast.parentElement) {
            toast.remove();
        }
    }, duration);

    return toast;
}

function getBootstrapAlertClass(type) {
    const classMap = {
        'success': 'success',
        'error': 'danger',
        'warning': 'warning',
        'info': 'info'
    };
    return classMap[type] || 'info';
}

function getToastIcon(type) {
    const iconMap = {
        'success': 'check-circle',
        'error': 'x-circle',
        'warning': 'exclamation-triangle',
        'info': 'info-circle'
    };
    return iconMap[type] || 'info-circle';
}

// ========== UIUTILS SYSTEM (FIXED) ==========
if (typeof UIUtils === 'undefined') {
    window.UIUtils = {};
}

// Fixed UIUtils functions without recursion
UIUtils.showSuccess = function (message, title = 'Success') {
    console.log(`✅ ${title}: ${message}`);
    createToastNotification(message, 'success', title);
};

UIUtils.showError = function (message, title = 'Error') {
    console.error(`❌ ${title}: ${message}`);
    createToastNotification(message, 'error', title);
};

UIUtils.showWarning = function (message, title = 'Warning') {
    console.warn(`⚠️ ${title}: ${message}`);
    createToastNotification(message, 'warning', title);
};

UIUtils.showInfo = function (message, title = 'Info') {
    console.info(`ℹ️ ${title}: ${message}`);
    createToastNotification(message, 'info', title);
};

UIUtils.setButtonLoading = function (button, isLoading, loadingText = 'Loading...') {
    if (!button) return;

    if (isLoading) {
        button.disabled = true;
        if (!button.dataset.originalText) {
            button.dataset.originalText = button.innerHTML;
        }
        button.innerHTML = `<span class="spinner-border spinner-border-sm me-2" role="status"></span>${loadingText}`;
    } else {
        button.disabled = false;
        if (button.dataset.originalText) {
            button.innerHTML = button.dataset.originalText;
            delete button.dataset.originalText;
        }
    }
};

UIUtils.showElementLoading = function (selector, message = 'Loading...') {
    const element = document.querySelector(selector);
    if (element) {
        if (!element.dataset.originalContent) {
            element.dataset.originalContent = element.innerHTML;
        }
        element.innerHTML = `
            <div class="d-flex justify-content-center align-items-center py-4">
                <div class="spinner-border spinner-border-sm text-primary me-2" role="status"></div>
                <span class="text-muted">${message}</span>
            </div>`;
    }
};

UIUtils.hideElementLoading = function (selector) {
    const element = document.querySelector(selector);
    if (element && element.dataset.originalContent) {
        element.innerHTML = element.dataset.originalContent;
        delete element.dataset.originalContent;
    }
};

UIUtils.showLoading = function (message = 'Loading...') {
    let loader = document.getElementById('globalLoader');
    if (!loader) {
        loader = document.createElement('div');
        loader.id = 'globalLoader';
        loader.className = 'position-fixed top-0 start-0 w-100 h-100 d-flex align-items-center justify-content-center';
        loader.style.cssText = `
            background: rgba(255, 255, 255, 0.9);
            z-index: 9999;
            backdrop-filter: blur(2px);
        `;
        loader.innerHTML = `
            <div class="text-center">
                <div class="spinner-border text-primary mb-3" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
                <div class="text-muted">${message}</div>
            </div>
        `;
        document.body.appendChild(loader);
    }
    loader.style.display = 'flex';
};

UIUtils.hideLoading = function () {
    const loader = document.getElementById('globalLoader');
    if (loader) {
        loader.style.display = 'none';
    }
};

// ========== ENHANCED DATATABLE REFRESH SYSTEM ==========

/**
 * Main refresh function - determines best strategy based on context
 */
function refreshClientDataTable(options = {}) {
    console.log('🔄 Starting DataTable refresh with options:', options);

    const { clientData, action = 'update', forceReload = false } = options;

    try {
        if (forceReload) {
            return reloadDataTableFromServer();
        }

        switch (action) {
            case 'create':
                return handleClientCreate(clientData);
            case 'update':
                return handleClientUpdate(clientData);
            case 'delete':
                return handleClientDelete(clientData);
            default:
                return refreshDataTableDisplay();
        }
    } catch (error) {
        console.error('🚨 Error in DataTable refresh:', error);
        fallbackRefresh();
    }
}

function reloadDataTableFromServer() {
    console.log('🔄 Performing full DataTable reload from server');

    try {
        showTableLoading('Refreshing client data...');

        // Option 1: Try dedicated endpoint
        fetch('/Client/GetClientsTableData')
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                if (data.success && data.clients) {
                    rebuildDataTable(data.clients);
                    hideTableLoading();
                    console.log('✅ DataTable reloaded from server successfully');
                } else {
                    throw new Error(data.message || 'Invalid server response');
                }
            })
            .catch(error => {
                console.error('🚨 Server reload failed:', error);
                hideTableLoading();
                reloadCurrentPage();
            });
    } catch (error) {
        console.error('🚨 Error in server reload:', error);
        reloadCurrentPage();
    }
}

function rebuildDataTable(clientsData) {
    console.log('🔧 Rebuilding DataTable with new data');

    try {
        if (window.clientsTable && $.fn.dataTable.isDataTable('#clientsTable')) {
            window.clientsTable.destroy();
        }

        const tableBody = document.querySelector('#clientsTable tbody');
        if (tableBody) {
            tableBody.innerHTML = '';

            clientsData.forEach(item => {
                const row = createClientTableRow(item);
                tableBody.appendChild(row);
            });
        }

        initializeClientsTable();
        console.log('✅ DataTable rebuilt successfully');
        return true;
    } catch (error) {
        console.error('🚨 Error rebuilding DataTable:', error);
        return false;
    }
}

function handleClientUpdate(clientData) {
    console.log('📝 Handling client update in DataTable');

    if (!clientData || !clientData.ClientId) {
        console.warn('⚠️ No client data provided for update');
        return refreshDataTableDisplay();
    }

    try {
        const success = updateTableRowSmart(clientData);

        if (success) {
            refreshDataTableDisplay();
            console.log('✅ Client row updated successfully');
        } else {
            console.warn('⚠️ Row update failed, falling back to full refresh');
            reloadDataTableFromServer();
        }
    } catch (error) {
        console.error('🚨 Error updating client row:', error);
        reloadDataTableFromServer();
    }
}

function handleClientCreate(clientData) {
    console.log('➕ Handling new client creation in DataTable');
    reloadDataTableFromServer();
}

function handleClientDelete(clientData) {
    console.log('🗑️ Handling client deletion in DataTable');

    if (!clientData || !clientData.ClientId) {
        return refreshDataTableDisplay();
    }

    try {
        const row = findClientRowById(clientData.ClientId);
        if (row && window.clientsTable) {
            const dataTableRow = window.clientsTable.row(row);
            dataTableRow.remove();
            window.clientsTable.draw();
            console.log('✅ Client row removed successfully');
        } else {
            refreshDataTableDisplay();
        }
    } catch (error) {
        console.error('🚨 Error removing client row:', error);
        refreshDataTableDisplay();
    }
}

function updateTableRowSmart(clientData) {
    console.log('🎯 Smart row update for client:', clientData.ClientId);

    try {
        const row = findClientRowById(clientData.ClientId);
        if (!row) {
            console.warn('⚠️ Row not found for client:', clientData.ClientId);
            return false;
        }

        const cells = row.querySelectorAll('td');
        if (cells.length >= 4) {
            updateClientNameCell(cells[0], clientData);
            updateAddressCell(cells[2], clientData);
            updatePhoneCell(cells[3], clientData);

            row.setAttribute('data-client-name', clientData.Name || '');

            console.log('✅ Row updated successfully');
            return true;
        }
        return false;
    } catch (error) {
        console.error('🚨 Error in smart row update:', error);
        return false;
    }
}

function updateClientNameCell(cell, clientData) {
    if (!cell || !clientData.Name) return;

    try {
        const nameEl = cell.querySelector('.fw-semibold');
        const avatar = cell.querySelector('.client-avatar');
        const image = cell.querySelector('.client-image');

        if (nameEl) {
            nameEl.textContent = clientData.Name;
        }

        if (avatar) {
            const initials = getClientInitials(clientData.Name);
            avatar.textContent = initials;
        }

        // Update image if provided
        if (clientData.imageUrl && image) {
            image.src = clientData.imageUrl;
            image.style.display = 'block';
            if (avatar) avatar.style.display = 'none';
        } else if (!clientData.imageUrl && image && avatar) {
            image.style.display = 'none';
            avatar.style.display = 'flex';
        }
    } catch (error) {
        console.error('🚨 Error updating name cell:', error);
    }
}

function updateAddressCell(cell, clientData) {
    if (!cell) return;

    try {
        const address = clientData.Address || '';
        const city = clientData.City || '';
        const state = clientData.State || '';
        const zip = clientData.Zip || '';

        const fullAddress = [address, city, state, zip]
            .filter(part => part.trim())
            .join(', ');

        cell.textContent = fullAddress;
    } catch (error) {
        console.error('🚨 Error updating address cell:', error);
    }
}

function updatePhoneCell(cell, clientData) {
    if (!cell) return;

    try {
        cell.textContent = clientData.PhoneNumber || '';
    } catch (error) {
        console.error('🚨 Error updating phone cell:', error);
    }
}

function refreshDataTableDisplay() {
    console.log('🔄 Refreshing DataTable display');

    try {
        if (window.clientsTable && $.fn.dataTable.isDataTable('#clientsTable')) {
            window.clientsTable.draw(false);
            console.log('✅ DataTable display refreshed');
            return true;
        } else {
            console.warn('⚠️ DataTable not found');
            return false;
        }
    } catch (error) {
        console.error('🚨 Error refreshing DataTable display:', error);
        return false;
    }
}

function showTableLoading(message = 'Loading...') {
    const tableContainer = document.querySelector('#clientsTable_wrapper');
    if (tableContainer) {
        let overlay = tableContainer.querySelector('.table-loading-overlay');
        if (!overlay) {
            overlay = document.createElement('div');
            overlay.className = 'table-loading-overlay position-absolute top-0 start-0 w-100 h-100 d-flex align-items-center justify-content-center';
            overlay.style.cssText = `
                background: rgba(255, 255, 255, 0.9);
                z-index: 1000;
                backdrop-filter: blur(1px);
            `;
            tableContainer.style.position = 'relative';
            tableContainer.appendChild(overlay);
        }

        overlay.innerHTML = `
            <div class="text-center">
                <div class="spinner-border text-primary mb-2" role="status"></div>
                <div class="text-muted">${message}</div>
            </div>
        `;
        overlay.style.display = 'flex';
    }
}

function hideTableLoading() {
    const overlay = document.querySelector('.table-loading-overlay');
    if (overlay) {
        overlay.style.display = 'none';
    }
}

function fallbackRefresh() {
    console.log('⚠️ Using fallback refresh strategy');
    UIUtils.showInfo('Refreshing client data...', 'Info');
    setTimeout(() => {
        refreshDataTableDisplay() || reloadCurrentPage();
    }, 1000);
}

function reloadCurrentPage() {
    console.log('🔄 Reloading current page as last resort');
    UIUtils.showInfo('Refreshing page to update client data...', 'Info');
    setTimeout(() => {
        window.location.reload();
    }, 1500);
}

function createClientTableRow(clientItem) {
    const client = clientItem.Client || clientItem;
    const row = document.createElement('tr');
    row.setAttribute('data-status', 'active');
    row.setAttribute('data-client-name', client.Name || '');
    row.setAttribute('data-client-id', client.ClientId || '');

    const initials = getClientInitials(client.Name);

    row.innerHTML = `
        <td>
            <div class="client-container d-flex align-items-center">
                <div class="product-image-cell me-3">
                    ${client.ImageUrl ?
            `<img src="${client.ImageUrl}" alt="${client.Name || 'Client'}" class="client-image" 
                             onerror="this.style.display='none'; this.nextElementSibling.style.display='flex';" loading="lazy">
                         <div class="client-avatar" style="display: none;">${initials}</div>` :
            `<div class="client-avatar">${initials}</div>`
        }
                </div>
                <div>
                    <div class="fw-semibold">${client.Name || 'N/A'}</div>
                    <div class="text-muted small">ID: #${client.ClientNumber || 'N/A'}</div>
                </div>
            </div>
        </td>
        <td>${client.CreatedDate ? new Date(client.CreatedDate).toLocaleDateString() : 'N/A'}</td>
        <td>${[client.Address, client.City, client.State, client.Zip].filter(p => p).join(', ')}</td>
        <td>${client.PhoneNumber || ''}</td>
        <td>
            <div class="table-actions">
                <button type="button" class="btn btn-sm btn-outline-primary action-icon" 
                        data-bs-toggle="modal" data-bs-target="#editClientModal" 
                        data-client-id="${client.ClientId}" title="Edit">
                    <i class="bi bi-pencil"></i>
                </button>
                <a href="/Client/Details/${client.ClientId}" class="btn btn-sm btn-primary action-icon" title="View">
                    <i class="bi bi-eye"></i>
                </a>
            </div>
        </td>
    `;

    return row;
}

// Public API for external use
window.ClientTableRefresh = {
    refresh: refreshClientDataTable,
    afterUpdate: (clientData) => refreshClientDataTable({ clientData, action: 'update' }),
    afterCreate: (clientData) => refreshClientDataTable({ clientData, action: 'create' }),
    afterDelete: (clientData) => refreshClientDataTable({ clientData, action: 'delete' }),
    forceReload: () => refreshClientDataTable({ forceReload: true }),
    redraw: refreshDataTableDisplay,
    showLoading: showTableLoading,
    hideLoading: hideTableLoading
};

// ========== ORIGINAL CLIENT MANAGEMENT FUNCTIONALITY ==========

// Initialize DataTable if the table exists
const clientsTable = $('#clientsTable');
if (clientsTable.length > 0) {
    initializeClientsTable();
}

// Initialize modals and event handlers
initializeModalHandlers();
initializeFormHandlers();
initializeImageHandlers();

console.log('🎯 Enhanced Client Management with Image Upload initialized successfully');

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

    window.clientsTable = table;
    initializeTableFilters(table);
    console.log('✅ DataTable initialized and stored globally');
    return table;
}

// ========== TABLE FILTERS ==========
function initializeTableFilters(table) {
    const statusFilter = $('#statusFilter');
    const joinDateFilter = $('#joinDateFilter');
    const resetFiltersBtn = $('#resetFilters');
    const activeFiltersContainer = $('#activeFilters');

    if (!statusFilter.length || !joinDateFilter.length) {
        console.warn('⚠️ Filter elements not found');
        return;
    }

    function applyFilters() {
        $.fn.dataTable.ext.search = $.fn.dataTable.ext.search.filter(fn =>
            !fn.name || fn.name !== 'clientTableFilter'
        );

        const customFilter = function (settings, data, dataIndex) {
            if (settings.nTable.id !== 'clientsTable') return true;

            if (statusFilter.val() === 'all' && joinDateFilter.val() === 'all') {
                return true;
            }

            const row = $(table.row(dataIndex).node());

            try {
                if (statusFilter.val() !== 'all') {
                    const statusValue = statusFilter.val().toLowerCase();
                    const rowStatus = (row.attr('data-status') || 'active').toLowerCase();
                    if (rowStatus !== statusValue) {
                        return false;
                    }
                }

                if (joinDateFilter.val() !== 'all') {
                    if (!filterByDate(row, data[1], joinDateFilter.val())) {
                        return false;
                    }
                }

                return true;
            } catch (error) {
                console.error('🚨 Error in filter function:', error);
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
            console.error("🚨 Date filtering error:", e);
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

    console.log('📂 Opening edit modal for client ID:', clientId);

    if (!clientId) {
        console.error('🚨 No client ID found on button');
        UIUtils.showError('Client ID is missing', 'Error');
        return;
    }

    safeSetValue('editClientId', clientId);
    safeSetValue('deleteClientId', clientId);
    clearAllForms();
    loadClientData(clientId);
}

function handleEditModalHide() {
    console.log('🚪 Edit modal closed, clearing forms and resetting buttons');

    clearAllForms();

    const modal = document.getElementById('editClientModal');
    if (modal) {
        const buttons = modal.querySelectorAll('button[type="submit"]');
        buttons.forEach(button => {
            resetSaveButton(button);
        });
    }
}

function handleTabSwitch(event) {
    const targetTab = event.target.getAttribute('data-bs-target');
    const clientId = document.getElementById('editClientId')?.value;

    console.log('🔄 Switching to tab:', targetTab, 'for client:', clientId);

    if (!clientId) return;

    switch (targetTab) {
        case '#details-tab-pane':
            break;
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
    // Add Client Form
    const addClientForm = document.getElementById('addClientForm');
    if (addClientForm) {
        addClientForm.addEventListener('submit', handleAddClientFormSubmit);
        console.log('✅ Add client form handler attached');
    }

    // Edit Client Form
    const editClientForm = document.getElementById('editClientForm');
    if (editClientForm) {
        editClientForm.addEventListener('submit', handleEditClientFormSubmit);
        console.log('✅ Edit client form handler attached');
    }

    document.addEventListener('click', function (e) {
        if (e.target.closest('#addCourtBtn')) {
            e.preventDefault();
            const clientId = safeGetValue('editClientId');
            if (clientId) addClientCourt(clientId);
        }
    });

    document.addEventListener('click', function (e) {
        if (e.target.closest('#addUserBtn')) {
            e.preventDefault();
            const clientId = safeGetValue('editClientId');
            if (clientId) addClientUser(clientId);
        }
    });
}

function handleAddClientFormSubmit(e) {
    e.preventDefault();
    console.log('📤 Add client form submitted');

    const form = e.target;
    const submitBtn = form.querySelector('button[type="submit"]');

    // Enhanced validation
    const validationErrors = validateClientForm(form);
    if (validationErrors.length > 0) {
        UIUtils.showError(`Please fix the following errors: ${validationErrors.join(', ')}`, 'Validation Error');
        return;
    }

    if (submitBtn) {
        UIUtils.setButtonLoading(submitBtn, true, 'Adding Client...');
    }

    const formData = new FormData(form);

    fetch(form.action, {
        method: 'POST',
        body: formData
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }
            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                return response.json();
            } else {
                // Handle redirect for successful creation
                UIUtils.showSuccess('Client created successfully!', 'Success');
                setTimeout(() => {
                    window.location.reload();
                }, 1000);
                return;
            }
        })
        .then(result => {
            if (result && result.success !== undefined) {
                if (result.success) {
                    UIUtils.showSuccess('Client created successfully!', 'Success');
                    const modal = bootstrap.Modal.getInstance(document.getElementById('addClientModal'));
                    if (modal) modal.hide();
                    setTimeout(() => location.reload(), 1000);
                } else {
                    UIUtils.showError(`Error creating client: ${result.message || 'Unknown error'}`, 'Error');
                }
            }
        })
        .catch(error => {
            console.error('🚨 Error creating client:', error);
            UIUtils.showError(`Error creating client: ${error.message}`, 'Error');
        })
        .finally(() => {
            if (submitBtn) {
                UIUtils.setButtonLoading(submitBtn, false);
            }
        });
}

function handleEditClientFormSubmit(e) {
    e.preventDefault();
    console.log('📤 Edit client form submitted');

    const form = e.target;
    const submitBtn = form.querySelector('button[type="submit"]');

    // Pre-submission validation
    const clientIdField = form.querySelector('#editClientId');
    const clientIdValue = clientIdField?.value;

    console.log('🔍 PRE-SUBMISSION VALIDATION:');
    console.log('  ClientId value:', `"${clientIdValue}"`);

    if (!clientIdValue || clientIdValue.trim() === '') {
        console.error('❌ CRITICAL: ClientId is empty at submission!');
        UIUtils.showError('Client ID is missing. Please close and reopen the edit dialog.', 'Error');
        return;
    }

    // Enhanced validation
    const validationErrors = validateClientForm(form);
    if (validationErrors.length > 0) {
        UIUtils.showError(`Please fix the following errors: ${validationErrors.join(', ')}`, 'Validation Error');
        return;
    }

    if (submitBtn) {
        UIUtils.setButtonLoading(submitBtn, true, 'Saving...');
    }

    const formData = new FormData(form);

    // Debug: Log form data
    console.log('📦 FormData contents:');
    for (let [key, value] of formData.entries()) {
        if (value instanceof File) {
            console.log(`  ${key}: File(${value.name}, ${value.size} bytes)`);
        } else {
            console.log(`  ${key}: "${value}"`);
        }
    }

    fetch(form.action, {
        method: 'POST',
        body: formData,
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(response => {
            console.log('📡 Response received:', response.status, response.statusText);

            if (!response.ok) {
                return response.text().then(text => {
                    console.error('❌ Server error response:', text);
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                });
            }
            return response.json();
        })
        .then(result => {
            console.log('📦 Parsed result:', result);

            if (result.success) {
                UIUtils.showSuccess('Client updated successfully!', 'Success');

                // Use enhanced refresh system
                ClientTableRefresh.afterUpdate(result.client);

                setTimeout(() => {
                    const modal = bootstrap.Modal.getInstance(document.getElementById('editClientModal'));
                    if (modal) modal.hide();
                }, 1500);
            } else {
                console.error('❌ Server error:', result.message);
                UIUtils.showError(`Error: ${result.message}`, 'Error');

                if (result.field) {
                    const errorField = form.querySelector(`#edit${result.field}`);
                    if (errorField) {
                        errorField.classList.add('is-invalid');
                        errorField.focus();
                    }
                }
            }
        })
        .catch(error => {
            console.error('❌ Error updating client:', error);
            UIUtils.showError(`Error updating client: ${error.message}`, 'Error');
        })
        .finally(() => {
            if (submitBtn) {
                UIUtils.setButtonLoading(submitBtn, false);
            }
        });
}

// ========== IMAGE HANDLERS ==========
function initializeImageHandlers() {
    console.log('🖼️ Initializing image handlers...');

    // Initialize image file inputs
    const imageFileInputs = document.querySelectorAll('input[type="file"][accept*="image"]');
    imageFileInputs.forEach(input => {
        input.addEventListener('change', function (e) {
            const file = e.target.files[0];
            if (file) {
                handleImagePreview(file, input);
            }
        });
        console.log('✅ Image file input handler attached:', input.id);
    });

    // Initialize image URL inputs
    const imageUrlInputs = document.querySelectorAll('input[name="ImageUrl"]');
    imageUrlInputs.forEach(input => {
        input.addEventListener('blur', function () {
            if (this.value) {
                handleImageUrlChange(this);
            }
        });
        console.log('✅ Image URL input handler attached:', input.id);
    });

    // Initialize clear image buttons
    const clearAddImageBtn = document.getElementById('clearAddClientImage');
    if (clearAddImageBtn) {
        clearAddImageBtn.addEventListener('click', function () {
            clearAddImagePreview();
        });
        console.log('✅ Clear add image button handler attached');
    }

    const clearEditImageBtn = document.getElementById('clearEditClientImage');
    if (clearEditImageBtn) {
        clearEditImageBtn.addEventListener('click', function () {
            clearEditImagePreview();
        });
        console.log('✅ Clear edit image button handler attached');
    }

    const removeImageBtn = document.getElementById('removeClientImage');
    if (removeImageBtn) {
        removeImageBtn.addEventListener('click', function () {
            removeClientImage();
        });
        console.log('✅ Remove image button handler attached');
    }

    console.log('✅ Image handlers initialized successfully');
}

function handleImagePreview(file, input) {
    console.log('🖼️ Handling image preview for file:', file.name);

    // Validate file
    const validation = validateImageFile(file);
    if (!validation.isValid) {
        UIUtils.showError(validation.errorMessage, 'Invalid Image');
        input.value = '';
        return;
    }

    const reader = new FileReader();
    reader.onload = function (e) {
        const imageUrl = e.target.result;
        updateImagePreview(imageUrl, input);
    };
    reader.readAsDataURL(file);
}

function handleImageUrlChange(input) {
    console.log('🔗 Handling image URL change:', input.value);

    if (!input.value.trim()) {
        updateImagePreview('', input);
        return;
    }

    // Basic URL validation
    if (!input.value.match(/^https?:\/\/.+\.(jpg|jpeg|png|gif|webp|bmp)(\?.*)?$/i)) {
        UIUtils.showWarning('Please enter a valid image URL ending with .jpg, .png, .gif, etc.', 'Invalid URL');
        return;
    }

    // If API is available, validate the URL
    if (window.clientUrls?.validateImageUrl) {
        validateImageUrlServer(input.value, input);
    } else {
        updateImagePreview(input.value, input);
    }
}

function validateImageUrlServer(imageUrl, input) {
    console.log('🔍 Validating image URL on server:', imageUrl);

    fetch(window.clientUrls.validateImageUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': getAntiForgeryToken()
        },
        body: JSON.stringify({ imageUrl: imageUrl })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                updateImagePreview(imageUrl, input);
                UIUtils.showSuccess('Valid image URL', 'Success');
            } else {
                UIUtils.showError(data.message || 'Invalid image URL', 'Validation Error');
                input.classList.add('is-invalid');
            }
        })
        .catch(error => {
            console.error('🚨 Error validating image URL:', error);
            updateImagePreview(imageUrl, input);
            UIUtils.showWarning('Could not validate URL, but proceeding anyway', 'Warning');
        });
}

function updateImagePreview(imageUrl, input) {
    console.log('🖼️ Updating image preview:', imageUrl ? 'with image' : 'clearing');

    const isAddForm = input && input.id.includes('add');
    const currentImage = document.getElementById(isAddForm ? 'addClientPreviewImage' : 'currentClientImage');
    const placeholder = document.getElementById(isAddForm ? 'addClientImagePlaceholder' : 'currentClientImagePlaceholder');
    const container = document.getElementById(isAddForm ? 'addClientImagePreview' : 'currentClientImagePreview');

    if (!currentImage || !placeholder) {
        console.warn('⚠️ Image preview elements not found');
        return;
    }

    if (imageUrl && imageUrl.trim()) {
        currentImage.src = imageUrl;
        currentImage.style.display = 'block';
        placeholder.style.display = 'none';
        if (container) container.classList.add('has-image');

        currentImage.onerror = function () {
            console.warn('⚠️ Image failed to load:', imageUrl);
            currentImage.style.display = 'none';
            placeholder.style.display = 'flex';
            if (container) container.classList.remove('has-image');
            UIUtils.showError('Failed to load image. Please check the URL or try a different image.', 'Image Load Error');
        };

        console.log('✅ Image preview updated with URL');
    } else {
        currentImage.style.display = 'none';
        placeholder.style.display = 'flex';
        currentImage.src = '';
        if (container) container.classList.remove('has-image');
        console.log('✅ Image preview cleared');
    }

    // Remove any validation states
    if (input) {
        input.classList.remove('is-invalid', 'is-valid');
    }
}

function clearAddImagePreview() {
    console.log('🧹 Clearing add image preview');

    const fileInput = document.getElementById('addClientImageFile');
    const urlInput = document.getElementById('addClientImageURL');

    if (fileInput) fileInput.value = '';
    if (urlInput) urlInput.value = '';

    updateImagePreview('', fileInput || urlInput);
    UIUtils.showInfo('Image cleared', 'Info');
}

function clearEditImagePreview() {
    console.log('🧹 Clearing edit image preview');

    const fileInput = document.getElementById('editClientImageFile');
    const urlInput = document.getElementById('editClientImageURL');

    if (fileInput) fileInput.value = '';
    if (urlInput) urlInput.value = '';

    updateImagePreview('', fileInput || urlInput);
    UIUtils.showInfo('Image cleared', 'Info');
}

function removeClientImage() {
    console.log('🗑️ Removing client image');

    if (!confirm('Are you sure you want to remove the client image? This action cannot be undone.')) {
        return;
    }

    clearEditImagePreview();

    // Add a hidden field to indicate image removal
    const form = document.getElementById('editClientForm');
    if (form) {
        let removeField = form.querySelector('input[name="RemoveImage"]');
        if (!removeField) {
            removeField = document.createElement('input');
            removeField.type = 'hidden';
            removeField.name = 'RemoveImage';
            form.appendChild(removeField);
        }
        removeField.value = 'true';
    }

    UIUtils.showInfo('Image will be removed when you save the client', 'Info');
}

function validateImageFile(file) {
    const maxFileSize = 5 * 1024 * 1024; // 5MB
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp', 'image/bmp'];
    const allowedExtensions = ['.jpg', '.jpeg', '.png', '.gif', '.webp', '.bmp'];

    if (file.size > maxFileSize) {
        return {
            isValid: false,
            errorMessage: `File size (${formatFileSize(file.size)}) exceeds maximum allowed size (5MB)`
        };
    }

    if (!allowedTypes.includes(file.type.toLowerCase())) {
        return {
            isValid: false,
            errorMessage: 'Invalid file type. Allowed types: JPG, PNG, GIF, WebP, BMP'
        };
    }

    const extension = '.' + file.name.split('.').pop().toLowerCase();
    if (!allowedExtensions.includes(extension)) {
        return {
            isValid: false,
            errorMessage: 'Invalid file extension. Allowed extensions: .jpg, .png, .gif, .webp, .bmp'
        };
    }

    return { isValid: true };
}

function formatFileSize(bytes) {
    const sizes = ['B', 'KB', 'MB', 'GB'];
    if (bytes === 0) return '0 B';
    const i = Math.floor(Math.log(bytes) / Math.log(1024));
    return Math.round(bytes / Math.pow(1024, i) * 100) / 100 + ' ' + sizes[i];
}

// ========== SUCCESS/ERROR HANDLERS ==========
function handleSaveSuccess(result, formData, submitBtn) {
    console.log('✅ Handling save success');

    if (submitBtn) {
        resetSaveButton(submitBtn);
    }

    if (result.success) {
        UIUtils.showSuccess(result.message || 'Client updated successfully', 'Success');

        const formDataObj = {};
        for (let [key, value] of formData.entries()) {
            formDataObj[key] = value;
        }

        // Use enhanced refresh system
        ClientTableRefresh.afterUpdate(formDataObj);

        setTimeout(() => {
            const modal = bootstrap.Modal.getInstance(document.getElementById('editClientModal'));
            if (modal) {
                modal.hide();
            }
        }, 1500);
    } else {
        UIUtils.showError(result.message || 'Failed to save client', 'Error');
    }
}

function handleSaveError(error, submitBtn) {
    console.error('🚨 Handling save error:', error);

    if (submitBtn) {
        resetSaveButton(submitBtn);
    }

    UIUtils.showError(`Error saving client: ${error.message}`, 'Error');
}

// ========== BUTTON MANAGEMENT ==========
function resetSaveButton(button) {
    if (!button) return;

    try {
        UIUtils.setButtonLoading(button, false);
        button.disabled = false;

        if (button.dataset.originalText) {
            button.textContent = button.dataset.originalText;
            delete button.dataset.originalText;
        } else {
            button.textContent = 'Save Changes';
        }

        button.classList.remove('loading');
        console.log('✅ Save button reset successfully');
    } catch (error) {
        console.error('🚨 Error resetting save button:', error);
    }
}

// ========== DATA LOADING FUNCTIONS ==========
function loadClientData(clientId) {
    console.log('📥 Loading client data for ID:', clientId);

    if (!clientId) {
        console.error('🚨 No client ID provided');
        return;
    }

    const row = findClientRowById(clientId);
    if (row) {
        console.log('📋 Found table row, extracting data...');
        const tableData = extractTableData(row);
        populateFromTableData(tableData);
    }

    if (!window.clientUrls?.getClientData) {
        console.warn('⚠️ GetClientData API URL not configured, using table data only');
        return;
    }

    fetch(`${window.clientUrls.getClientData}?id=${encodeURIComponent(clientId)}`)
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }
            return response.json();
        })
        .then(data => {
            console.log('📦 Received client data:', data);

            if (data.success !== false) {
                populateFromAPIData(data);
                console.log('✅ Client data loaded and populated successfully');
            } else {
                console.warn('⚠️ API returned success=false:', data.message);
                UIUtils.showWarning(`Limited client data loaded: ${data.message || 'API error'}`, 'Warning');
            }
        })
        .catch(error => {
            console.error('🚨 Error loading client data:', error);
            UIUtils.showWarning(`Using table data only. Error: ${error.message}`, 'Warning');
        });
}

function extractTableData(row) {
    if (!row) return {};

    console.log('📋 Extracting data from table row');

    const dataFromAttributes = {};

    const editBtn = row.querySelector('button[data-client-id]');
    if (editBtn) {
        dataFromAttributes.clientId = editBtn.getAttribute('data-client-id');
    }

    const cells = row.querySelectorAll('td');
    if (cells.length >= 4) {
        const clientCell = cells[0];
        const nameEl = clientCell.querySelector('.fw-semibold');
        const numberEl = clientCell.querySelector('.text-muted.small');
        const imageEl = clientCell.querySelector('.client-image');

        if (nameEl) {
            dataFromAttributes.name = nameEl.textContent.trim();
        }
        if (numberEl) {
            const match = numberEl.textContent.match(/ID: #(\S+)/);
            if (match) dataFromAttributes.clientNumber = match[1];
        }
        if (imageEl && imageEl.src) {
            dataFromAttributes.imageUrl = imageEl.src;
        }

        if (cells[1]) {
            dataFromAttributes.createdDate = cells[1].textContent.trim();
        }

        if (cells[2]) {
            const addressText = cells[2].textContent.trim();
            const addressParts = addressText.split(',').map(part => part.trim());
            if (addressParts.length >= 4) {
                dataFromAttributes.address = addressParts[0];
                dataFromAttributes.city = addressParts[1];
                dataFromAttributes.state = addressParts[2];
                dataFromAttributes.zip = addressParts[3];
            }
        }

        if (cells[3]) {
            dataFromAttributes.phoneNumber = cells[3].textContent.trim();
        }
    }

    console.log('📋 Extracted table data:', dataFromAttributes);
    return dataFromAttributes;
}

function populateFromTableData(data) {
    console.log('📝 Populating form from table data:', data);

    try {
        if (data.clientId) safeSetValue('editClientId', data.clientId);
        if (data.clientNumber) safeSetValue('editClientNumber', data.clientNumber);
        if (data.name) safeSetValue('editName', data.name);
        if (data.address) safeSetValue('editAddress', data.address);
        if (data.city) safeSetValue('editCity', data.city);
        if (data.state) safeSetValue('editState', data.state);
        if (data.zip) safeSetValue('editZip', data.zip);
        if (data.phoneNumber) safeSetValue('editPhoneNumber', data.phoneNumber);
        if (data.createdDate) safeSetValue('editCreatedDate', data.createdDate);
        if (data.imageUrl) {
            safeSetValue('editClientImageURL', data.imageUrl);
            updateImagePreview(data.imageUrl, document.getElementById('editClientImageURL'));
        }

        console.log('✅ Table data populated successfully');
    } catch (error) {
        console.error('🚨 Error populating from table data:', error);
    }
}

function populateFromAPIData(data) {
    console.log('📝 Populating form from API data:', data);

    try {
        const client = data.client || data;

        if (client.clientId) safeSetValue('editClientId', client.clientId);
        if (client.clientNumber) safeSetValue('editClientNumber', client.clientNumber);
        if (client.name) safeSetValue('editName', client.name);
        if (client.address) safeSetValue('editAddress', client.address);
        if (client.city) safeSetValue('editCity', client.city);
        if (client.state) safeSetValue('editState', client.state);
        if (client.zip) safeSetValue('editZip', client.zip);
        if (client.phoneNumber) safeSetValue('editPhoneNumber', client.phoneNumber);
        if (client.notes) safeSetValue('editNotes', client.notes);
        if (client.createdDate) safeSetValue('editCreatedDate', client.createdDate);
        if (client.imageUrl) {
            safeSetValue('editClientImageURL', client.imageUrl);
            updateImagePreview(client.imageUrl, document.getElementById('editClientImageURL'));
        }

        if (client.status) {
            safeSetSelect('editStatus', client.status);
        }

        if (data.courtList) {
            displayClientCourts(data.courtList);
        }

        console.log('✅ API data populated successfully');
    } catch (error) {
        console.error('🚨 Error populating from API data:', error);
    }
}

function findClientRowById(clientId) {
    if (!clientId) return null;

    console.log('🔍 Looking for row with client ID:', clientId);

    let row = document.querySelector(`tr[data-client-id="${clientId}"]`);
    if (row) return row;

    const button = document.querySelector(`button[data-client-id="${clientId}"]`);
    if (button) {
        row = button.closest('tr');
        if (row) return row;
    }

    const tableBody = document.querySelector('#clientsTable tbody');
    if (tableBody) {
        const allRows = tableBody.querySelectorAll('tr');
        for (const tr of allRows) {
            const editBtn = tr.querySelector(`[data-client-id="${clientId}"]`);
            if (editBtn) return tr;
        }
    }

    console.warn('⚠️ Row not found for client ID:', clientId);
    return null;
}

function loadClientCourts(clientId) {
    console.log('🏀 Loading courts for client:', clientId);

    const courtsTableBody = document.getElementById('courtsTableBody');
    if (!courtsTableBody) return;

    courtsTableBody.innerHTML = `
        <tr>
            <td colspan="3" class="text-center py-4 text-muted">
                <div class="spinner-border spinner-border-sm text-secondary me-2" role="status"></div>
                Loading courts...
            </td>
        </tr>`;

    if (!window.clientUrls?.getClientData) {
        courtsTableBody.innerHTML = `
            <tr>
                <td colspan="3" class="text-center py-4 text-muted">
                    No courts data available.
                </td>
            </tr>`;
        return;
    }

    fetch(`${window.clientUrls.getClientData}?id=${encodeURIComponent(clientId)}`)
        .then(response => response.json())
        .then(data => {
            if (data.success !== false && data.courtList) {
                displayClientCourts(data.courtList);
            } else {
                displayClientCourts([]);
            }
        })
        .catch(error => {
            console.error('🚨 Error loading courts:', error);
            courtsTableBody.innerHTML = `
                <tr>
                    <td colspan="3" class="text-center py-4 text-danger">
                        Error loading courts. Please try again.
                    </td>
                </tr>`;
        });
}

function loadClientUsers(clientId) {
    console.log('👥 Loading users for client:', clientId);

    const usersTableBody = document.getElementById('usersTableBody');
    if (!usersTableBody) return;

    usersTableBody.innerHTML = `
        <tr>
            <td colspan="5" class="text-center py-4 text-muted">
                <div class="spinner-border spinner-border-sm text-secondary me-2" role="status"></div>
                Loading users...
            </td>
        </tr>`;

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
    console.log('📊 Loading business data for client:', clientId);

    if (!window.clientUrls?.getClientBusinessData) {
        UIUtils.showWarning('Business data API not available', 'Warning');
        return;
    }

    fetch(`/Client/GetClientBusinessData?id=${encodeURIComponent(clientId)}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                updateClientBusinessDisplay(data.business);
            } else {
                UIUtils.showWarning('Failed to load business data', 'Warning');
            }
        })
        .catch(error => {
            console.error('🚨 Error loading business data:', error);
            UIUtils.showError('Error loading business data', 'Error');
        });
}

// ========== DISPLAY FUNCTIONS ==========
function displayClientCourts(courts) {
    console.log('🏀 Displaying courts:', courts);
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
    console.log('👥 Displaying users:', users);
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
    console.log('📊 Updating business display:', business);

    safeUpdateElement('businessName', business.businessName);
    safeUpdateElement('businessType', business.businessType);
    safeUpdateElement('establishedDate', formatDate(business.establishedDate));
    safeUpdateElement('employeeCount', business.employeeCount);
    safeUpdateElement('annualRevenue', business.annualRevenue);
    safeUpdateElement('operatingHours', business.operatingHours);
    safeUpdateElement('website', business.website);

    if (business.stats) {
        safeUpdateElement('totalBookings', business.stats.totalBookings);
        safeUpdateElement('monthlyUsers', business.stats.monthlyUsers);
        safeUpdateElement('averageRating', business.stats.averageRating);
        safeUpdateElement('repeatCustomers', business.stats.repeatCustomers + '%');
    }

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
    console.log('➕ Adding court for client:', clientId);

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
                UIUtils.showSuccess('Court added successfully', 'Success');
                loadClientCourts(clientId);
            } else {
                UIUtils.showError(`Error adding court: ${data.message || 'Unknown error'}`, 'Error');
            }
        })
        .catch(error => {
            console.error('🚨 Error adding court:', error);
            UIUtils.showError('Error adding court. Please try again.', 'Error');
        });
}

function editClientCourt(clientId, courtId) {
    console.log('✏️ Editing court:', courtId, 'for client:', clientId);

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
                UIUtils.showSuccess('Court updated successfully', 'Success');
                loadClientCourts(clientId);
            } else {
                UIUtils.showError(`Error updating court: ${data.message || 'Unknown error'}`, 'Error');
            }
        })
        .catch(error => {
            console.error('🚨 Error updating court:', error);
            UIUtils.showError('Error updating court. Please try again.', 'Error');
        });
}

function removeClientCourt(clientId, courtId) {
    console.log('🗑️ Removing court:', courtId, 'for client:', clientId);

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
                UIUtils.showSuccess('Court removed successfully', 'Success');
                loadClientCourts(clientId);
            } else {
                UIUtils.showError(`Error removing court: ${data.message || 'Unknown error'}`, 'Error');
            }
        })
        .catch(error => {
            console.error('🚨 Error removing court:', error);
            UIUtils.showError('Error removing court. Please try again.', 'Error');
        });
}

// ========== USER MANAGEMENT ==========
function addClientUser(clientId) {
    console.log('➕ Adding user for client:', clientId);

    const email = prompt('Enter user email address:');
    if (!email) return;

    UIUtils.showInfo('User management functionality coming soon', 'Info');
}

function editClientUser(clientId, userId) {
    console.log('✏️ Editing user:', userId, 'for client:', clientId);
    UIUtils.showInfo('User management functionality coming soon', 'Info');
}

function removeClientUser(clientId, userId) {
    console.log('🗑️ Removing user:', userId, 'for client:', clientId);

    if (!confirm('Are you sure you want to remove this user from the client?')) return;

    UIUtils.showInfo('User management functionality coming soon', 'Info');
}

// ========== VALIDATION FUNCTIONS ==========
function validateClientForm(form) {
    const errors = [];

    const nameField = form.querySelector('[name="Name"]');
    if (!nameField?.value?.trim()) {
        errors.push('Client name is required');
        nameField?.classList.add('is-invalid');
    } else {
        nameField?.classList.remove('is-invalid');
    }

    const addressField = form.querySelector('[name="Address"]');
    if (!addressField?.value?.trim()) {
        errors.push('Address is required');
        addressField?.classList.add('is-invalid');
    } else {
        addressField?.classList.remove('is-invalid');
    }

    const cityField = form.querySelector('[name="City"]');
    if (!cityField?.value?.trim()) {
        errors.push('City is required');
        cityField?.classList.add('is-invalid');
    } else {
        cityField?.classList.remove('is-invalid');
    }

    const zipField = form.querySelector('[name="Zip"]');
    if (!zipField?.value?.trim()) {
        errors.push('Zip code is required');
        zipField?.classList.add('is-invalid');
    } else {
        zipField?.classList.remove('is-invalid');
    }

    return errors;
}

// ========== UI STATE MANAGEMENT ==========
function clearAllForms() {
    clearClientDetailsForm();
    clearCourtsDisplay();
    clearUsersDisplay();
    clearBusinessDisplay();
    clearImagePreviews();
}

function clearClientDetailsForm() {
    const fields = [
        'editClientNumber', 'editName', 'editAddress', 'editCity',
        'editState', 'editZip', 'editPhoneNumber', 'editNotes', 'editCreatedDate',
        'editClientImageURL'
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

function clearImagePreviews() {
    // Clear edit image preview
    updateImagePreview('', document.getElementById('editClientImageURL'));

    // Clear add image preview
    updateImagePreview('', document.getElementById('addClientImageURL'));

    // Reset file inputs
    const editFileInput = document.getElementById('editClientImageFile');
    const addFileInput = document.getElementById('addClientImageFile');

    if (editFileInput) editFileInput.value = '';
    if (addFileInput) addFileInput.value = '';
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

function getClientInitials(clientName) {
    if (!clientName) return 'NA';

    const names = clientName.trim().split(' ');
    if (names.length >= 2) {
        return `${names[0][0]}${names[1][0]}`.toUpperCase();
    } else if (names.length === 1 && names[0].length > 0) {
        return names[0][0].toUpperCase();
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

function formatDate(dateString) {
    if (!dateString) return '--';
    try {
        const date = new Date(dateString);
        return date.toLocaleDateString();
    } catch (e) {
        return '--';
    }
}

// ========== DEBUGGING AND TESTING ==========
window.clientDebug = {
    loadClientData,
    findClientRowById,
    extractTableData,
    populateFromTableData,
    populateFromAPIData,
    loadClientCourts,
    loadClientUsers,
    loadClientBusinessData,
    handleEditClientFormSubmit,
    resetSaveButton,
    updateImagePreview,
    validateImageFile,
    clearImagePreviews
};

window.testClientRefresh = {
    testUpdate: () => {
        const mockClient = {
            ClientId: 'test-update-123',
            Name: 'Updated Test Client',
            Address: '456 Updated St',
            City: 'Updated City',
            State: 'UC',
            Zip: '54321',
            PhoneNumber: '555-9999',
            imageUrl: 'https://via.placeholder.com/150'
        };
        ClientTableRefresh.afterUpdate(mockClient);
    },

    testCreate: () => {
        const mockClient = {
            ClientId: 'test-create-456',
            Name: 'New Test Client',
            Address: '789 New St',
            City: 'New City',
            State: 'NC',
            Zip: '99999',
            PhoneNumber: '555-1111',
            imageUrl: 'https://via.placeholder.com/150'
        };
        ClientTableRefresh.afterCreate(mockClient);
    },

    testDelete: () => {
        ClientTableRefresh.afterDelete({ ClientId: 'test-delete-789' });
    },

    testForceReload: () => {
        ClientTableRefresh.forceReload();
    },

    testLoading: () => {
        ClientTableRefresh.showLoading('Testing loading state...');
        setTimeout(() => {
            ClientTableRefresh.hideLoading();
        }, 3000);
    }
};

console.log('🎯 Enhanced Client Management with Image Upload loaded successfully');
console.log('🖼️ Image upload functionality enabled');
console.log('🐛 Debug functions: window.clientDebug');
console.log('🧪 Test functions: window.testClientRefresh');
console.log('🔧 Refresh API: window.ClientTableRefresh');