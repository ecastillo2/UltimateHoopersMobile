/**
 * COMPLETE UPDATED CLIENT MANAGEMENT JAVASCRIPT
 * Enhanced with robust DataTable refresh, improved image handling, and proper error management
 */

// ========== TOAST NOTIFICATION SYSTEM ==========
function createToastNotification(message, type = 'info', title = '', duration = 5000) {
    console.log(`${type.toUpperCase()}: ${title} - ${message}`);

    const alertClass = {
        'success': 'success',
        'error': 'danger',
        'warning': 'warning',
        'info': 'info'
    }[type] || 'info';

    const icon = {
        'success': 'check-circle',
        'error': 'x-circle',
        'warning': 'exclamation-triangle',
        'info': 'info-circle'
    }[type] || 'info-circle';

    const toast = document.createElement('div');
    toast.className = `alert alert-${alertClass} alert-dismissible fade show position-fixed`;
    toast.style.cssText = `
        top: 20px;
        right: 20px;
        z-index: 9999;
        min-width: 300px;
        max-width: 500px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
    `;

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
    setTimeout(() => {
        if (toast.parentElement) {
            toast.remove();
        }
    }, duration);

    return toast;
}

// ========== UIUTILS SYSTEM ==========
if (typeof UIUtils === 'undefined') {
    window.UIUtils = {};
}

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
window.ClientTableManager = {

    /**
     * Main refresh function - handles all table update scenarios
     */
    refresh: function (clientData = null, action = 'update') {
        console.log(`🔄 ClientTableManager.refresh called with action: ${action}`, clientData);

        try {
            switch (action) {
                case 'delete':
                    return this.handleDelete(clientData);
                case 'update':
                    return this.handleUpdate(clientData);
                case 'create':
                    return this.handleCreate(clientData);
                default:
                    return this.simpleRedraw();
            }
        } catch (error) {
            console.error('🚨 Refresh error:', error);
            this.fallbackRefresh();
        }
    },

    /**
     * Handle client update - most common scenario
     */
    handleUpdate: function (clientData) {
        console.log('📝 Handling client update:', clientData);

        if (!clientData?.ClientId && !clientData?.clientId) {
            console.warn('⚠️ No client ID provided for update');
            return this.simpleRedraw();
        }

        const clientId = clientData.ClientId || clientData.clientId;
        const row = this.findTableRow(clientId);

        if (row) {
            console.log('✅ Found table row, updating in place...');
            const success = this.updateRowData(row, clientData);

            if (success) {
                this.simpleRedraw();
                console.log('✅ Client row updated successfully');
                return true;
            } else {
                console.warn('⚠️ Row update failed, using fallback');
                return this.fallbackRefresh();
            }
        } else {
            console.warn('⚠️ Row not found for update, using fallback');
            return this.fallbackRefresh();
        }
    },

    /**
     * Handle client deletion
     */
    handleDelete: function (clientData) {
        console.log('🗑️ Handling client deletion:', clientData);

        if (!clientData?.ClientId && !clientData?.clientId) {
            return this.simpleRedraw();
        }

        const clientId = clientData.ClientId || clientData.clientId;
        const row = this.findTableRow(clientId);

        if (row && window.clientsTable) {
            try {
                const dataTableRow = window.clientsTable.row(row);
                dataTableRow.remove();
                window.clientsTable.draw();
                console.log('✅ Client row removed successfully');
                return true;
            } catch (error) {
                console.error('🚨 Error removing row:', error);
            }
        }

        return this.simpleRedraw();
    },

    /**
     * Handle client creation
     */
    handleCreate: function (clientData) {
        console.log('➕ Handling client creation');
        // For create, always reload the page to get the new client
        UIUtils.showSuccess('Client created successfully!', 'Success');
        setTimeout(() => {
            window.location.reload();
        }, 1500);
    },

    /**
     * Update row data in the DOM
     */
    updateRowData: function (row, clientData) {
        console.log('🎯 Updating row data:', clientData);

        try {
            const cells = row.querySelectorAll('td');
            if (cells.length < 4) {
                console.warn('⚠️ Row does not have enough cells');
                return false;
            }

            // Update client name and image (cell 0)
            this.updateClientCell(cells[0], clientData);

            // Update address (cell 2)
            this.updateAddressCell(cells[2], clientData);

            // Update phone (cell 3)
            this.updatePhoneCell(cells[3], clientData);

            // Update row attributes for filtering
            row.setAttribute('data-client-name', clientData.Name || clientData.name || '');

            console.log('✅ Row data updated successfully');
            return true;
        } catch (error) {
            console.error('🚨 Error updating row data:', error);
            return false;
        }
    },

    /**
     * Update the client name/image cell
     */
    updateClientCell: function (cell, clientData) {
        if (!cell) return;

        try {
            const clientName = clientData.Name || clientData.name || '';
            const clientImageUrl = clientData.imageUrl || clientData.ImageUrl || '';

            // Update name
            const nameEl = cell.querySelector('.fw-semibold');
            if (nameEl && clientName) {
                nameEl.textContent = clientName;
                console.log('📝 Updated client name to:', clientName);
            }

            // Update image/avatar
            const image = cell.querySelector('.client-image');
            const avatar = cell.querySelector('.client-avatar');

            if (clientImageUrl && image) {
                image.src = clientImageUrl;
                image.style.display = 'block';
                if (avatar) avatar.style.display = 'none';

                // Handle image load errors
                image.onerror = () => {
                    console.warn('⚠️ Updated image failed to load:', clientImageUrl);
                    image.style.display = 'none';
                    if (avatar) {
                        avatar.style.display = 'flex';
                        avatar.textContent = this.getClientInitials(clientName);
                    }
                };

                console.log('🖼️ Updated client image to:', clientImageUrl);
            } else if (!clientImageUrl && avatar) {
                if (image) image.style.display = 'none';
                avatar.style.display = 'flex';
                avatar.textContent = this.getClientInitials(clientName);
                console.log('👤 Updated client avatar initials');
            }
        } catch (error) {
            console.error('🚨 Error updating client cell:', error);
        }
    },

    /**
     * Update the address cell
     */
    updateAddressCell: function (cell, clientData) {
        if (!cell) return;

        try {
            const addressParts = [
                clientData.Address || clientData.address,
                clientData.City || clientData.city,
                clientData.State || clientData.state,
                clientData.Zip || clientData.zip
            ].filter(part => part && part.trim()).join(', ');

            cell.textContent = addressParts;
            console.log('🏠 Updated address to:', addressParts);
        } catch (error) {
            console.error('🚨 Error updating address cell:', error);
        }
    },

    /**
     * Update the phone cell
     */
    updatePhoneCell: function (cell, clientData) {
        if (!cell) return;

        try {
            const phone = clientData.PhoneNumber || clientData.phoneNumber || '';
            cell.textContent = phone;
            console.log('📞 Updated phone to:', phone);
        } catch (error) {
            console.error('🚨 Error updating phone cell:', error);
        }
    },

    /**
     * Find table row by client ID
     */
    findTableRow: function (clientId) {
        if (!clientId) return null;

        console.log('🔍 Looking for table row with client ID:', clientId);

        // Method 1: Direct data attribute search
        let row = document.querySelector(`tr[data-client-id="${clientId}"]`);
        if (row) {
            console.log('✅ Found row by data-client-id attribute');
            return row;
        }

        // Method 2: Find by edit button
        const button = document.querySelector(`button[data-client-id="${clientId}"]`);
        if (button) {
            row = button.closest('tr');
            if (row) {
                console.log('✅ Found row by edit button');
                return row;
            }
        }

        // Method 3: Search all table rows
        const tableBody = document.querySelector('#clientsTable tbody');
        if (tableBody) {
            const allRows = tableBody.querySelectorAll('tr');
            for (const tr of allRows) {
                const editBtn = tr.querySelector(`[data-client-id="${clientId}"]`);
                if (editBtn) {
                    console.log('✅ Found row by searching all rows');
                    return tr;
                }
            }
        }

        console.warn('⚠️ Could not find table row for client ID:', clientId);
        return null;
    },

    /**
     * Simple table redraw
     */
    simpleRedraw: function () {
        console.log('🔄 Performing simple table redraw');

        try {
            if (window.clientsTable && $.fn.dataTable.isDataTable('#clientsTable')) {
                window.clientsTable.draw(false);
                console.log('✅ DataTable redrawn successfully');
                return true;
            } else {
                console.warn('⚠️ DataTable not available for redraw');
                return false;
            }
        } catch (error) {
            console.error('🚨 Error during table redraw:', error);
            return false;
        }
    },

    /**
     * Fallback refresh strategy
     */
    fallbackRefresh: function () {
        console.log('⚠️ Using fallback refresh strategy');
        UIUtils.showInfo('Refreshing client data...', 'Info');
        setTimeout(() => {
            window.location.reload();
        }, 2000);
    },

    /**
     * Get client initials for avatar
     */
    getClientInitials: function (clientName) {
        if (!clientName) return 'NA';

        const names = clientName.trim().split(' ');
        if (names.length >= 2) {
            return `${names[0][0]}${names[1][0]}`.toUpperCase();
        } else if (names.length === 1 && names[0].length > 0) {
            return names[0][0].toUpperCase();
        }
        return 'NA';
    }
};

// ========== MAIN REFRESH FUNCTION ==========
function refreshClientDataTable(options = {}) {
    console.log('🔄 refreshClientDataTable called with options:', options);
    return ClientTableManager.refresh(options.clientData, options.action);
}

// ========== MODAL HANDLERS ==========
function handleEditModalShow(event) {
    const button = event.relatedTarget;
    const clientId = button.getAttribute('data-client-id');

    console.log('📂 Opening edit modal for client ID:', clientId);

    if (!clientId) {
        console.error('🚨 No client ID found on button');
        UIUtils.showError('Client ID is missing', 'Error');
        return;
    }

    // Set client ID immediately
    safeSetValue('editClientId', clientId);
    safeSetValue('deleteClientId', clientId);

    // Clear forms first
    clearAllForms();

    // Load client data
    loadClientData(clientId);
}

function handleEditModalHide() {
    console.log('🚪 Edit modal closed');
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

// ========== DATA LOADING FUNCTIONS ==========
function loadClientData(clientId) {
    console.log('📥 Loading client data for ID:', clientId);

    if (!clientId) {
        console.error('🚨 No client ID provided');
        return;
    }

    // Extract data from table row
    const row = findClientRowById(clientId);
    if (row) {
        console.log('📋 Found table row, extracting data...');
        const tableData = extractTableData(row, clientId);
        populateFromTableData(tableData);
    } else {
        console.warn('⚠️ Could not find table row for client:', clientId);
    }

    // Try to load from API if available
    if (window.clientUrls?.getClientData) {
        console.log('🌐 Loading additional data from API...');
        fetch(`${window.clientUrls.getClientData}?id=${encodeURIComponent(clientId)}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                return response.json();
            })
            .then(data => {
                console.log('📦 Received client data from API:', data);
                if (data.success !== false) {
                    populateFromAPIData(data);
                    console.log('✅ API data loaded successfully');
                } else {
                    console.warn('⚠️ API returned success=false:', data.message);
                }
            })
            .catch(error => {
                console.error('🚨 Error loading from API:', error);
                console.log('ℹ️ Using table data only');
            });
    } else {
        console.log('ℹ️ API not configured, using table data only');
    }
}

function findClientRowById(clientId) {
    if (!clientId) return null;

    console.log('🔍 Looking for row with client ID:', clientId);

    // Method 1: Row with data attribute
    let row = document.querySelector(`tr[data-client-id="${clientId}"]`);
    if (row) return row;

    // Method 2: Button with client ID
    const button = document.querySelector(`button[data-client-id="${clientId}"]`);
    if (button) {
        row = button.closest('tr');
        if (row) return row;
    }

    // Method 3: Search all rows
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

function extractTableData(row, clientId) {
    console.log('📋 Extracting data from table row');

    const data = { clientId: clientId };

    try {
        const cells = row.querySelectorAll('td');
        console.log('📊 Found', cells.length, 'cells in table row');

        if (cells.length >= 4) {
            // Cell 0: Client info (name, image, client number)
            const clientCell = cells[0];

            const nameEl = clientCell.querySelector('.fw-semibold');
            if (nameEl) {
                data.name = nameEl.textContent.trim();
                console.log('📝 Extracted name:', data.name);
            }

            const numberEl = clientCell.querySelector('.text-muted.small');
            if (numberEl) {
                const match = numberEl.textContent.match(/ID:\s*#?(\S+)/);
                if (match) {
                    data.clientNumber = match[1];
                    console.log('🔢 Extracted client number:', data.clientNumber);
                }
            }

            const imageEl = clientCell.querySelector('.client-image');
            if (imageEl && imageEl.src && !imageEl.src.includes('placeholder')) {
                data.imageUrl = imageEl.src;
                console.log('🖼️ Extracted image URL:', data.imageUrl);
            }

            // Cell 1: Date
            if (cells[1]) {
                data.createdDate = cells[1].textContent.trim();
                console.log('📅 Extracted created date:', data.createdDate);
            }

            // Cell 2: Address
            if (cells[2]) {
                const addressText = cells[2].textContent.trim();
                console.log('🏠 Extracted address text:', addressText);

                if (addressText) {
                    const addressParts = addressText.split(',').map(part => part.trim()).filter(part => part);

                    if (addressParts.length >= 1) data.address = addressParts[0];
                    if (addressParts.length >= 2) data.city = addressParts[1];
                    if (addressParts.length >= 3) data.state = addressParts[2];
                    if (addressParts.length >= 4) data.zip = addressParts[3];

                    console.log('🏠 Parsed address parts:', {
                        address: data.address,
                        city: data.city,
                        state: data.state,
                        zip: data.zip
                    });
                }
            }

            // Cell 3: Phone
            if (cells[3]) {
                data.phoneNumber = cells[3].textContent.trim();
                console.log('📞 Extracted phone:', data.phoneNumber);
            }
        }
    } catch (error) {
        console.error('🚨 Error extracting table data:', error);
    }

    console.log('✅ Final extracted table data:', data);
    return data;
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

        // Handle image
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

        // Only update fields if they have values from API
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

        // Handle image
        if (client.imageUrl) {
            safeSetValue('editClientImageURL', client.imageUrl);
            updateImagePreview(client.imageUrl, document.getElementById('editClientImageURL'));
        }

        console.log('✅ API data populated successfully');
    } catch (error) {
        console.error('🚨 Error populating from API data:', error);
    }
}

// ========== FORM HANDLERS ==========
function handleAddClientFormSubmit(e) {
    e.preventDefault();
    console.log('📤 Add client form submitted');

    const form = e.target;
    const submitBtn = form.querySelector('button[type="submit"]');

    // Validate form
    const validationErrors = validateClientForm(form);
    if (validationErrors.length > 0) {
        UIUtils.showError(`Please fix: ${validationErrors.join(', ')}`, 'Validation Error');
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
            if (response.redirected) {
                UIUtils.showSuccess('Client created successfully!', 'Success');
                setTimeout(() => window.location.reload(), 1500);
                return;
            }

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                return response.json();
            } else {
                UIUtils.showSuccess('Client created successfully!', 'Success');
                setTimeout(() => window.location.reload(), 1000);
                return;
            }
        })
        .then(result => {
            if (result && result.success !== undefined) {
                if (result.success) {
                    UIUtils.showSuccess('Client created successfully!', 'Success');
                    const modal = bootstrap.Modal.getInstance(document.getElementById('addClientModal'));
                    if (modal) modal.hide();
                    setTimeout(() => window.location.reload(), 1000);
                } else {
                    UIUtils.showError(`Error: ${result.message}`, 'Error');
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

    // Validate client ID
    const clientIdValue = form.querySelector('#editClientId')?.value;
    if (!clientIdValue || clientIdValue.trim() === '') {
        UIUtils.showError('Client ID is missing. Please close and reopen the edit dialog.', 'Error');
        return;
    }

    // Validate form
    const validationErrors = validateClientForm(form);
    if (validationErrors.length > 0) {
        UIUtils.showError(`Please fix: ${validationErrors.join(', ')}`, 'Validation Error');
        return;
    }

    if (submitBtn) {
        UIUtils.setButtonLoading(submitBtn, true, 'Saving...');
    }

    const formData = new FormData(form);

    fetch(form.action, {
        method: 'POST',
        body: formData,
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }
            return response.json();
        })
        .then(result => {
            if (result.success) {
                UIUtils.showSuccess('Client updated successfully!', 'Success');

                // Prepare comprehensive client data for table update
                const clientData = result.client || {
                    ClientId: clientIdValue,
                    Name: formData.get('Name'),
                    Address: formData.get('Address'),
                    City: formData.get('City'),
                    State: formData.get('State'),
                    Zip: formData.get('Zip'),
                    PhoneNumber: formData.get('PhoneNumber'),
                    imageUrl: formData.get('ImageUrl')
                };

                console.log('📊 Updating table with client data:', clientData);

                // Use enhanced refresh system
                ClientTableManager.refresh(clientData, 'update');

                setTimeout(() => {
                    const modal = bootstrap.Modal.getInstance(document.getElementById('editClientModal'));
                    if (modal) modal.hide();
                }, 1500);
            } else {
                UIUtils.showError(`Error: ${result.message}`, 'Error');
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

function validateClientForm(form) {
    const errors = [];

    const fields = [
        { name: 'Name', label: 'Client name' },
        { name: 'Address', label: 'Address' },
        { name: 'City', label: 'City' },
        { name: 'Zip', label: 'Zip code' }
    ];

    fields.forEach(field => {
        const input = form.querySelector(`[name="${field.name}"]`);
        if (!input?.value?.trim()) {
            errors.push(`${field.label} is required`);
            input?.classList.add('is-invalid');
        } else {
            input?.classList.remove('is-invalid');
        }
    });

    return errors;
}

// ========== IMAGE HANDLERS ==========
function handleImagePreview(file, input) {
    console.log('🖼️ Handling image preview for file:', file.name);

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
        UIUtils.showSuccess('Image loaded successfully', 'Success');
    };
    reader.onerror = function () {
        UIUtils.showError('Error reading image file', 'Error');
        input.value = '';
    };
    reader.readAsDataURL(file);
}

function handleImageUrlChange(input) {
    console.log('🔗 Handling image URL change:', input.value);

    if (!input.value.trim()) {
        updateImagePreview('', input);
        input.classList.remove('is-invalid', 'is-valid');
        return;
    }

    // Basic URL validation
    if (!input.value.match(/^https?:\/\/.+\.(jpg|jpeg|png|gif|webp|bmp)(\?.*)?$/i)) {
        UIUtils.showWarning('Please enter a valid image URL ending with .jpg, .png, .gif, etc.', 'Invalid URL');
        input.classList.add('is-invalid');
        return;
    }

    // Test image loading
    const testImg = new Image();
    testImg.onload = function () {
        updateImagePreview(input.value, input);
        input.classList.remove('is-invalid');
        input.classList.add('is-valid');
        UIUtils.showSuccess('Valid image URL', 'Success');
    };
    testImg.onerror = function () {
        updateImagePreview(input.value, input); // Still show preview
        input.classList.add('is-invalid');
        UIUtils.showWarning('Image URL could not be loaded. Please verify the URL.', 'Warning');
    };
    testImg.src = input.value;
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
        };

        console.log('✅ Image preview updated');
    } else {
        currentImage.style.display = 'none';
        placeholder.style.display = 'flex';
        currentImage.src = '';
        if (container) container.classList.remove('has-image');
        console.log('✅ Image preview cleared');
    }
}

function clearAddImagePreview() {
    console.log('🧹 Clearing add image preview');
    const fileInput = document.getElementById('addClientImageFile');
    const urlInput = document.getElementById('addClientImageURL');

    if (fileInput) fileInput.value = '';
    if (urlInput) {
        urlInput.value = '';
        urlInput.classList.remove('is-invalid', 'is-valid');
    }

    updateImagePreview('', fileInput || urlInput);
    UIUtils.showInfo('Image cleared', 'Info');
}

function clearEditImagePreview() {
    console.log('🧹 Clearing edit image preview');
    const fileInput = document.getElementById('editClientImageFile');
    const urlInput = document.getElementById('editClientImageURL');

    if (fileInput) fileInput.value = '';
    if (urlInput) {
        urlInput.value = '';
        urlInput.classList.remove('is-invalid', 'is-valid');
    }

    updateImagePreview('', fileInput || urlInput);
    UIUtils.showInfo('Image cleared', 'Info');
}

function removeClientImage() {
    console.log('🗑️ Removing client image');

    if (!confirm('Are you sure you want to remove the client image? This action cannot be undone.')) {
        return;
    }

    clearEditImagePreview();

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

    return { isValid: true };
}

function formatFileSize(bytes) {
    const sizes = ['B', 'KB', 'MB', 'GB'];
    if (bytes === 0) return '0 B';
    const i = Math.floor(Math.log(bytes) / Math.log(1024));
    return Math.round(bytes / Math.pow(1024, i) * 100) / 100 + ' ' + sizes[i];
}

// ========== UTILITY FUNCTIONS ==========
function safeSetValue(elementId, value) {
    const element = document.getElementById(elementId);
    if (element) {
        const oldValue = element.value;
        element.value = value || '';
        console.log(`📝 Set ${elementId}: "${oldValue}" → "${element.value}"`);
        return true;
    } else {
        console.warn(`⚠️ Element not found: ${elementId}`);
        return false;
    }
}

function safeGetValue(elementId) {
    const element = document.getElementById(elementId);
    return element ? element.value : '';
}

function clearAllForms() {
    console.log('🧹 Clearing all forms');

    const fields = [
        'editClientNumber', 'editName', 'editAddress', 'editCity',
        'editState', 'editZip', 'editPhoneNumber', 'editNotes',
        'editCreatedDate', 'editClientImageURL'
    ];

    fields.forEach(field => {
        const element = document.getElementById(field);
        if (element) {
            element.value = '';
            element.classList.remove('is-invalid', 'is-valid');
        }
    });

    // Clear image previews
    const editFileInput = document.getElementById('editClientImageFile');
    const addFileInput = document.getElementById('addClientImageFile');

    if (editFileInput) editFileInput.value = '';
    if (addFileInput) addFileInput.value = '';

    // Clear edit image preview
    updateImagePreview('', document.getElementById('editClientImageURL'));

    console.log('✅ Forms cleared');
}

function getAntiForgeryToken() {
    return document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
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

// ========== DATATABLE INITIALIZATION ==========
function initializeClientsTable() {
    const clientsTable = $('#clientsTable');
    if (!clientsTable.length) {
        console.warn('⚠️ Clients table not found');
        return null;
    }

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
    console.log('✅ DataTable initialized');
    return table;
}

// ========== TABLE FILTERS ==========
function initializeTableFilters(table) {
    if (!table) return;

    const statusFilter = $('#statusFilter');
    const joinDateFilter = $('#joinDateFilter');
    const resetFiltersBtn = $('#resetFilters');

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
    }

    function filterByDate(row, dateText, filterValue) {
        try {
            let date = new Date(dateText);

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

// ========== GLOBAL API FOR BACKWARD COMPATIBILITY ==========
window.ClientTableRefresh = ClientTableManager;

// Backward compatibility functions
window.ClientTableRefresh.afterUpdate = function (clientData) {
    return ClientTableManager.refresh(clientData, 'update');
};

window.ClientTableRefresh.afterCreate = function (clientData) {
    return ClientTableManager.refresh(clientData, 'create');
};

window.ClientTableRefresh.afterDelete = function (clientData) {
    return ClientTableManager.refresh(clientData, 'delete');
};

window.ClientTableRefresh.forceReload = function () {
    return ClientTableManager.fallbackRefresh();
};

window.ClientTableRefresh.redraw = function () {
    return ClientTableManager.simpleRedraw();
};

// ========== MAIN INITIALIZATION ==========
document.addEventListener('DOMContentLoaded', function () {
    console.log('🚀 Client Management initializing...');

    // Initialize DataTable
    const table = initializeClientsTable();
    if (table) {
        initializeTableFilters(table);
    }

    // Initialize forms
    const editForm = document.getElementById('editClientForm');
    if (editForm) {
        editForm.addEventListener('submit', handleEditClientFormSubmit);
        console.log('✅ Edit form handler attached');
    }

    const addForm = document.getElementById('addClientForm');
    if (addForm) {
        addForm.addEventListener('submit', handleAddClientFormSubmit);
        console.log('✅ Add form handler attached');
    }

    // Initialize modals
    const editClientModal = document.getElementById('editClientModal');
    if (editClientModal) {
        editClientModal.addEventListener('show.bs.modal', handleEditModalShow);
        editClientModal.addEventListener('hidden.bs.modal', handleEditModalHide);
        console.log('✅ Edit modal handlers attached');
    }

    // Initialize delete button
    const deleteClientBtn = document.getElementById('deleteClientBtn');
    if (deleteClientBtn) {
        deleteClientBtn.addEventListener('click', handleDeleteClient);
        console.log('✅ Delete button handler attached');
    }

    // Initialize image handlers
    document.querySelectorAll('input[type="file"][accept*="image"]').forEach(input => {
        input.addEventListener('change', function (e) {
            const file = e.target.files[0];
            if (file) {
                handleImagePreview(file, input);
            }
        });
    });

    document.querySelectorAll('input[name="ImageUrl"]').forEach(input => {
        input.addEventListener('blur', function () {
            if (this.value) {
                handleImageUrlChange(this);
            }
        });
    });

    // Initialize clear/remove buttons
    const clearAddBtn = document.getElementById('clearAddClientImage');
    if (clearAddBtn) {
        clearAddBtn.addEventListener('click', clearAddImagePreview);
    }

    const clearEditBtn = document.getElementById('clearEditClientImage');
    if (clearEditBtn) {
        clearEditBtn.addEventListener('click', clearEditImagePreview);
    }

    const removeBtn = document.getElementById('removeClientImage');
    if (removeBtn) {
        removeBtn.addEventListener('click', removeClientImage);
    }

    console.log('✅ Client Management initialized successfully');
});

// ========== TESTING AND DEBUGGING FUNCTIONS ==========
window.testClientRefresh = {

    testUpdate: function (clientId = null) {
        console.log('🧪 Testing client update refresh...');

        // Use first client in table if no ID provided
        if (!clientId) {
            const firstEditBtn = document.querySelector('button[data-client-id]');
            if (firstEditBtn) {
                clientId = firstEditBtn.getAttribute('data-client-id');
            } else {
                console.error('❌ No clients found in table');
                return;
            }
        }

        const testData = {
            ClientId: clientId,
            Name: `Updated Test Client ${Date.now()}`,
            Address: '456 Updated Avenue',
            City: 'Updated City',
            State: 'UC',
            Zip: '54321',
            PhoneNumber: '555-9999',
            imageUrl: 'https://via.placeholder.com/50/0000FF/FFFFFF?text=TEST'
        };

        console.log('🧪 Test data:', testData);
        ClientTableManager.refresh(testData, 'update');
    },

    testDelete: function (clientId = null) {
        console.log('🧪 Testing client delete refresh...');

        if (!clientId) {
            const firstEditBtn = document.querySelector('button[data-client-id]');
            if (firstEditBtn) {
                clientId = firstEditBtn.getAttribute('data-client-id');
            } else {
                console.error('❌ No clients found in table');
                return;
            }
        }

        if (confirm(`Delete test client ${clientId}?`)) {
            ClientTableManager.refresh({ ClientId: clientId }, 'delete');
        }
    },

    testRedraw: function () {
        console.log('🧪 Testing simple table redraw...');
        ClientTableManager.simpleRedraw();
    },

    debugRow: function (clientId = null) {
        console.log('🧪 Debugging table row...');

        if (!clientId) {
            const firstEditBtn = document.querySelector('button[data-client-id]');
            if (firstEditBtn) {
                clientId = firstEditBtn.getAttribute('data-client-id');
            } else {
                console.error('❌ No clients found in table');
                return;
            }
        }

        const row = ClientTableManager.findTableRow(clientId);
        console.log('🔍 Found row:', row);

        if (row) {
            const testData = {
                ClientId: clientId,
                Name: 'Debug Test Name',
                Address: '123 Debug St',
                City: 'Debug City',
                State: 'DC',
                Zip: '12345',
                PhoneNumber: '555-1234',
                imageUrl: 'https://via.placeholder.com/50/FF0000/FFFFFF?text=DEBUG'
            };

            console.log('🧪 Testing row update with data:', testData);
            ClientTableManager.updateRowData(row, testData);
            ClientTableManager.simpleRedraw();
        }
    }
};

window.debugClient = {
    testModal: function (clientId) {
        console.log('🐛 Testing modal for client:', clientId);
        const row = findClientRowById(clientId);
        console.log('Row found:', row);
        if (row) {
            const data = extractTableData(row, clientId);
            console.log('Extracted data:', data);
            populateFromTableData(data);
        }
    },

    checkElements: function () {
        const elements = [
            'editClientId', 'editName', 'editAddress', 'editCity',
            'editState', 'editZip', 'editPhoneNumber'
        ];

        elements.forEach(id => {
            const el = document.getElementById(id);
            console.log(`${id}:`, el ? `Found (value: "${el.value}")` : 'NOT FOUND');
        });
    }
};

console.log('🎯 Complete Updated Client Management loaded successfully');
console.log('🧪 Test functions: window.testClientRefresh');
console.log('🐛 Debug functions: window.debugClient');
console.log('🔧 Table Manager: window.ClientTableManager');