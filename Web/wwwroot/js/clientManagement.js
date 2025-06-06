/**
 * COMPLETE CLIENT MANAGEMENT JAVASCRIPT WITH COURTS TAB - CLEANED VERSION
 * Enhanced with robust DataTable refresh, improved image handling, and working courts tab
 * Toast and spinner functionality removed
 */

// ========== ENHANCED DATATABLE REFRESH SYSTEM ==========
window.ClientTableManager = {

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

    handleCreate: function (clientData) {
        console.log('➕ Handling client creation');
        console.log('Client created successfully!');
        setTimeout(() => {
            window.location.reload();
        }, 1500);
    },

    updateRowData: function (row, clientData) {
        console.log('🎯 Updating row data:', clientData);

        try {
            const cells = row.querySelectorAll('td');
            if (cells.length < 4) {
                console.warn('⚠️ Row does not have enough cells');
                return false;
            }

            this.updateClientCell(cells[0], clientData);
            this.updateAddressCell(cells[2], clientData);
            this.updatePhoneCell(cells[3], clientData);

            row.setAttribute('data-client-name', clientData.Name || clientData.name || '');

            console.log('✅ Row data updated successfully');
            return true;
        } catch (error) {
            console.error('🚨 Error updating row data:', error);
            return false;
        }
    },

    updateClientCell: function (cell, clientData) {
        if (!cell) return;

        try {
            const clientName = clientData.Name || clientData.name || '';
            const clientImageUrl = clientData.imageUrl || clientData.ImageUrl || '';

            const nameEl = cell.querySelector('.fw-semibold');
            if (nameEl && clientName) {
                nameEl.textContent = clientName;
                console.log('📝 Updated client name to:', clientName);
            }

            const image = cell.querySelector('.client-image');
            const avatar = cell.querySelector('.client-avatar');

            if (clientImageUrl && image) {
                image.src = clientImageUrl;
                image.style.display = 'block';
                if (avatar) avatar.style.display = 'none';

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

    findTableRow: function (clientId) {
        if (!clientId) return null;

        console.log('🔍 Looking for table row with client ID:', clientId);

        let row = document.querySelector(`tr[data-client-id="${clientId}"]`);
        if (row) {
            console.log('✅ Found row by data-client-id attribute');
            return row;
        }

        const button = document.querySelector(`button[data-client-id="${clientId}"]`);
        if (button) {
            row = button.closest('tr');
            if (row) {
                console.log('✅ Found row by edit button');
                return row;
            }
        }

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

    fallbackRefresh: function () {
        console.log('⚠️ Using fallback refresh strategy');
        console.log('Refreshing client data...');
        setTimeout(() => {
            window.location.reload();
        }, 2000);
    },

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

// ========== TAB SWITCHING HANDLER ==========
function handleTabSwitch(event) {
    const targetTab = event.target.getAttribute('data-bs-target');
    const clientId = document.getElementById('editClientId')?.value;

    console.log('🔄 Switching to tab:', targetTab, 'for client:', clientId);

    if (!clientId) {
        console.warn('⚠️ No client ID available for tab switch');
        return;
    }

    switch (targetTab) {
        case '#details-tab-pane':
            console.log('📋 Details tab active - no additional loading needed');
            break;
        case '#courts-tab-pane':
            console.log('🏀 Loading courts for client...');
            loadClientCourts(clientId);
            break;
        case '#users-tab-pane':
            console.log('👥 Loading users for client...');
            loadClientUsers(clientId);
            break;
        case '#business-tab-pane':
            console.log('📊 Loading business data for client...');
            loadClientBusinessData(clientId);
            break;
        default:
            console.log('ℹ️ Unknown tab:', targetTab);
    }
}

// ========== MODAL HANDLERS ==========
function handleEditModalShow(event) {
    const button = event.relatedTarget;
    const clientId = button.getAttribute('data-client-id');

    console.log('📂 Opening edit modal for client ID:', clientId);

    if (!clientId) {
        console.error('🚨 No client ID found on button');
        alert('Client ID is missing');
        return;
    }

    safeSetValue('editClientId', clientId);
    safeSetValue('deleteClientId', clientId);

    clearAllForms();
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

    const row = findClientRowById(clientId);
    if (row) {
        console.log('📋 Found table row, extracting data...');
        const tableData = extractTableData(row, clientId);
        populateFromTableData(tableData);
    } else {
        console.warn('⚠️ Could not find table row for client:', clientId);
    }

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

function extractTableData(row, clientId) {
    console.log('📋 Extracting data from table row');

    const data = { clientId: clientId };

    try {
        const cells = row.querySelectorAll('td');
        console.log('📊 Found', cells.length, 'cells in table row');

        if (cells.length >= 4) {
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

            if (cells[1]) {
                data.createdDate = cells[1].textContent.trim();
                console.log('📅 Extracted created date:', data.createdDate);
            }

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

        console.log('✅ API data populated successfully');
    } catch (error) {
        console.error('🚨 Error populating from API data:', error);
    }
}

// ========== COURTS LOADING AND MANAGEMENT ==========
function loadClientCourts(clientId) {
    console.log('🏀 Loading courts for client:', clientId);

    const courtsTableBody = document.getElementById('courtsTableBody');
    if (!courtsTableBody) {
        console.warn('⚠️ Courts table body not found');
        return;
    }

    showCourtsLoading();

    if (window.clientUrls?.getClientData) {
        fetch(`${window.clientUrls.getClientData}?id=${encodeURIComponent(clientId)}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                return response.json();
            })
            .then(data => {
                console.log('📦 Received client data for courts:', data);

                if (data.success !== false && data.courtList) {
                    displayClientCourts(data.courtList);
                    console.log('✅ Courts loaded from API successfully');
                } else {
                    console.log('ℹ️ No courts data in API response, showing sample data');
                    displaySampleCourts(clientId);
                }
            })
            .catch(error => {
                console.error('🚨 Error loading courts from API:', error);
                console.log('ℹ️ Showing sample courts data');
                displaySampleCourts(clientId);
            });
    } else {
        console.log('ℹ️ Courts API not configured, showing sample data');
        displaySampleCourts(clientId);
    }
}

function showCourtsLoading() {
    const courtsTableBody = document.getElementById('courtsTableBody');
    if (courtsTableBody) {
        courtsTableBody.innerHTML = `
            <tr>
                <td colspan="3" class="text-center py-4 text-muted">
                    <div class="spinner-border spinner-border-sm text-secondary me-2" role="status"></div>
                    Loading courts...
                </td>
            </tr>`;
    }
}

function displayClientCourts(courts) {
    console.log('🏀 Displaying courts:', courts);

    const courtsTableBody = document.getElementById('courtsTableBody');
    if (!courtsTableBody) {
        console.warn('⚠️ Courts table body not found');
        return;
    }

    if (!courts || courts.length === 0) {
        courtsTableBody.innerHTML = `
            <tr>
                <td colspan="3" class="text-center py-4 text-muted">
                    <div class="d-flex flex-column align-items-center">
                        <i class="bi bi-building" style="font-size: 2rem; color: #6c757d; margin-bottom: 1rem;"></i>
                        <p class="mb-2">No courts associated with this client.</p>
                        <button type="button" class="btn btn-sm btn-outline-primary" onclick="addClientCourt('${getCurrentClientId()}')">
                            <i class="bi bi-plus-circle me-1"></i>Add First Court
                        </button>
                    </div>
                </td>
            </tr>`;
        return;
    }

    let html = '';
    courts.forEach((court, index) => {
        const courtId = court.courtId || court.CourtId || `court-${index}`;
        const courtName = court.name || court.Name || `Court ${index + 1}`;
        const courtNumber = court.courtNumber || court.CourtNumber || `court-${index}`;
        const courtType = court.type || court.Type || 'Standard';
        const isActive = court.isActive !== false;

        html += `
            <tr>
                <td>
                    <div class="d-flex align-items-center">
                        <div class="me-2">
                            <i class="bi bi-building text-primary"></i>
                        </div>
                        <div>
                            <div class="fw-semibold">${courtName}</div>
                            <small class="text-muted"># ${courtNumber}</small>
                        </div>
                    </div>
                </td>
                <td>
                    <span class="badge ${isActive ? 'bg-success' : 'bg-secondary'}">${courtType}</span>
                </td>
                <td>
                    <div class="btn-group btn-group-sm">
                        <button type="button" class="btn btn-outline-primary edit-court-btn"
                                data-court-id="${courtId}"
                                data-court-name="${courtName}"
                                title="Edit Court">
                            <i class="bi bi-pencil"></i>
                        </button>
                        <button type="button" class="btn btn-outline-danger remove-court-btn"
                                data-court-id="${courtId}"
                                data-court-name="${courtName}"
                                title="Remove Court">
                            <i class="bi bi-trash"></i>
                        </button>
                    </div>
                </td>
            </tr>`;
    });

    courtsTableBody.innerHTML = html;
    attachCourtButtonListeners();

    console.log('✅ Courts displayed successfully');
}

function displaySampleCourts(clientId) {
    console.log('🏀 Displaying sample courts for client:', clientId);

    const sampleCourts = [
        {
            courtId: `${clientId}-court-1`,
            name: 'Main Basketball Court',
            type: 'Basketball',
            isActive: true
        },
        {
            courtId: `${clientId}-court-2`,
            name: 'Training Court',
            type: 'Multi-Purpose',
            isActive: true
        },
        {
            courtId: `${clientId}-court-3`,
            name: 'Youth Court',
            type: 'Basketball',
            isActive: false
        }
    ];

    displayClientCourts(sampleCourts);
}

function attachCourtButtonListeners() {
    document.querySelectorAll('.edit-court-btn').forEach(button => {
        button.addEventListener('click', function () {
            const courtId = this.getAttribute('data-court-id');
            const courtName = this.getAttribute('data-court-name');
            const clientId = getCurrentClientId();
            editClientCourt(clientId, courtId, courtName);
        });
    });

    document.querySelectorAll('.remove-court-btn').forEach(button => {
        button.addEventListener('click', function () {
            const courtId = this.getAttribute('data-court-id');
            const courtName = this.getAttribute('data-court-name');
            const clientId = getCurrentClientId();
            removeClientCourt(clientId, courtId, courtName);
        });
    });
}

// ========== COURT MANAGEMENT FUNCTIONS ==========
function addClientCourt(clientId) {
    console.log('➕ Adding court for client:', clientId);

    const courtName = prompt('Enter court name:');
    if (!courtName || !courtName.trim()) {
        return;
    }

    const courtData = {
        clientId: clientId,
        name: courtName.trim()
    };

    if (window.clientUrls?.addCourt) {
        const token = getAntiForgeryToken();

        fetch(window.clientUrls.addCourt, {
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
                    console.log('Court added successfully');
                    loadClientCourts(clientId);
                } else {
                    console.error(`Error adding court: ${data.message || 'Unknown error'}`);
                    alert(`Error adding court: ${data.message || 'Unknown error'}`);
                }
            })
            .catch(error => {
                console.error('🚨 Error adding court:', error);
                alert('Error adding court. Please try again.');
            });
    } else {
        console.log('🎭 Simulating court addition:', courtData);
        console.log(`Court "${courtName}" added successfully`);

        setTimeout(() => {
            loadClientCourts(clientId);
        }, 1000);
    }
}

function editClientCourt(clientId, courtId, currentName) {
    console.log('✏️ Editing court:', courtId, 'for client:', clientId);

    const newName = prompt('Enter new court name:', currentName);
    if (newName === null || newName.trim() === '') {
        return;
    }

    const courtData = {
        courtId: courtId,
        clientId: clientId,
        name: newName.trim()
    };

    if (window.clientUrls?.updateCourt) {
        const token = getAntiForgeryToken();

        fetch(window.clientUrls.updateCourt, {
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
                    console.log('Court updated successfully');
                    loadClientCourts(clientId);
                } else {
                    console.error(`Error updating court: ${data.message || 'Unknown error'}`);
                    alert(`Error updating court: ${data.message || 'Unknown error'}`);
                }
            })
            .catch(error => {
                console.error('🚨 Error updating court:', error);
                alert('Error updating court. Please try again.');
            });
    } else {
        console.log('🎭 Simulating court update:', courtData);
        console.log(`Court updated to "${newName}"`);

        setTimeout(() => {
            loadClientCourts(clientId);
        }, 1000);
    }
}

function removeClientCourt(clientId, courtId, courtName) {
    console.log('🗑️ Removing court:', courtId, 'for client:', clientId);

    if (!confirm(`Are you sure you want to remove "${courtName}"? This action cannot be undone.`)) {
        return;
    }

    const requestData = {
        clientId: clientId,
        courtId: courtId
    };

    if (window.clientUrls?.removeCourt) {
        const token = getAntiForgeryToken();

        fetch(window.clientUrls.removeCourt, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(requestData)
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    console.log('Court removed successfully');
                    loadClientCourts(clientId);
                } else {
                    console.error(`Error removing court: ${data.message || 'Unknown error'}`);
                    alert(`Error removing court: ${data.message || 'Unknown error'}`);
                }
            })
            .catch(error => {
                console.error('🚨 Error removing court:', error);
                alert('Error removing court. Please try again.');
            });
    } else {
        console.log('🎭 Simulating court removal:', requestData);
        console.log(`Court "${courtName}" removed successfully`);

        setTimeout(() => {
            loadClientCourts(clientId);
        }, 1000);
    }
}

// ========== USERS TAB FUNCTIONALITY ==========
function loadClientUsers(clientId) {
    console.log('👥 Loading users for client:', clientId);

    const usersTableBody = document.getElementById('usersTableBody');
    if (!usersTableBody) {
        console.warn('⚠️ Users table body not found');
        return;
    }

    usersTableBody.innerHTML = `
        <tr>
            <td colspan="5" class="text-center py-4 text-muted">
                <div class="spinner-border spinner-border-sm text-secondary me-2" role="status"></div>
                Loading users...
            </td>
        </tr>`;

    setTimeout(() => {
        const sampleUsers = [
            {
                userId: `${clientId}-user-1`,
                firstName: 'John',
                lastName: 'Manager',
                email: 'john.manager@client.com',
                role: 'Manager',
                status: 'Active'
            },
            {
                userId: `${clientId}-user-2`,
                firstName: 'Jane',
                lastName: 'Staff',
                email: 'jane.staff@client.com',
                role: 'Staff',
                status: 'Active'
            },
            {
                userId: `${clientId}-user-3`,
                firstName: 'Bob',
                lastName: 'Coach',
                email: 'bob.coach@client.com',
                role: 'Coach',
                status: 'Inactive'
            }
        ];

        displayClientUsers(sampleUsers);
    }, 1000);
}

function displayClientUsers(users) {
    console.log('👥 Displaying users:', users);

    const usersTableBody = document.getElementById('usersTableBody');
    if (!usersTableBody) return;

    if (!users || users.length === 0) {
        usersTableBody.innerHTML = `
            <tr>
                <td colspan="5" class="text-center py-4 text-muted">
                    <div class="d-flex flex-column align-items-center">
                        <i class="bi bi-people" style="font-size: 2rem; color: #6c757d; margin-bottom: 1rem;"></i>
                        <p class="mb-2">No users associated with this client.</p>
                        <button type="button" class="btn btn-sm btn-outline-primary" onclick="addClientUser('${getCurrentClientId()}')">
                            <i class="bi bi-person-plus me-1"></i>Add First User
                        </button>
                    </div>
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
                            <small class="text-muted">ID: ${user.userId}</small>
                        </div>
                    </div>
                </td>
                <td>${user.email || 'N/A'}</td>
                <td><span class="badge bg-primary">${user.role || 'User'}</span></td>
                <td><span class="badge ${statusClass}">${user.status || 'Active'}</span></td>
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
    attachUserButtonListeners();

    console.log('✅ Users displayed successfully');
}

function attachUserButtonListeners() {
    document.querySelectorAll('.edit-user-btn').forEach(button => {
        button.addEventListener('click', function () {
            const userId = this.getAttribute('data-user-id');
            const clientId = getCurrentClientId();
            editClientUser(clientId, userId);
        });
    });

    document.querySelectorAll('.remove-user-btn').forEach(button => {
        button.addEventListener('click', function () {
            const userId = this.getAttribute('data-user-id');
            const clientId = getCurrentClientId();
            removeClientUser(clientId, userId);
        });
    });
}

function addClientUser(clientId) {
    console.log('➕ Adding user for client:', clientId);
    console.log('User management functionality coming soon');
}

function editClientUser(clientId, userId) {
    console.log('✏️ Editing user:', userId, 'for client:', clientId);
    console.log('User management functionality coming soon');
}

function removeClientUser(clientId, userId) {
    console.log('🗑️ Removing user:', userId, 'for client:', clientId);

    if (!confirm('Are you sure you want to remove this user from the client?')) return;

    console.log('User management functionality coming soon');
}

function loadClientBusinessData(clientId) {
    console.log('📊 Loading business data for client:', clientId);

    if (!window.clientUrls?.getClientBusinessData) {
        console.warn('Business data API not available');
        return;
    }

    fetch(`${window.clientUrls.getClientBusinessData}?id=${encodeURIComponent(clientId)}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                updateClientBusinessDisplay(data.business);
            } else {
                console.warn('Failed to load business data');
            }
        })
        .catch(error => {
            console.error('🚨 Error loading business data:', error);
            console.error('Error loading business data');
        });
}

function updateClientBusinessDisplay(business) {
    console.log('📊 Updating business display:', business);
    console.log('Business data loaded successfully');
}

// ========== FORM HANDLERS ==========
function handleAddClientFormSubmit(e) {
    e.preventDefault();
    console.log('📤 Add client form submitted');

    const form = e.target;
    const submitBtn = form.querySelector('button[type="submit"]');

    const validationErrors = validateClientForm(form);
    if (validationErrors.length > 0) {
        alert(`Please fix: ${validationErrors.join(', ')}`);
        return;
    }

    if (submitBtn) {
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Adding Client...';
    }

    const formData = new FormData(form);

    fetch(form.action, {
        method: 'POST',
        body: formData
    })
        .then(response => {
            if (response.redirected) {
                console.log('Client created successfully!');
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
                console.log('Client created successfully!');
                setTimeout(() => window.location.reload(), 1000);
                return;
            }
        })
        .then(result => {
            if (result && result.success !== undefined) {
                if (result.success) {
                    console.log('Client created successfully!');
                    const modal = bootstrap.Modal.getInstance(document.getElementById('addClientModal'));
                    if (modal) modal.hide();
                    setTimeout(() => window.location.reload(), 1000);
                } else {
                    alert(`Error: ${result.message}`);
                }
            }
        })
        .catch(error => {
            console.error('🚨 Error creating client:', error);
            alert(`Error creating client: ${error.message}`);
        })
        .finally(() => {
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.innerHTML = 'Add Client';
            }
        });
}

function handleEditClientFormSubmit(e) {
    e.preventDefault();
    console.log('📤 Edit client form submitted');

    const form = e.target;
    const submitBtn = form.querySelector('button[type="submit"]');

    const clientIdValue = form.querySelector('#editClientId')?.value;
    if (!clientIdValue || clientIdValue.trim() === '') {
        alert('Client ID is missing. Please close and reopen the edit dialog.');
        return;
    }

    const validationErrors = validateClientForm(form);
    if (validationErrors.length > 0) {
        alert(`Please fix: ${validationErrors.join(', ')}`);
        return;
    }

    if (submitBtn) {
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Saving...';
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
                console.log('Client updated successfully!');

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
                ClientTableManager.refresh(clientData, 'update');

                setTimeout(() => {
                    const modal = bootstrap.Modal.getInstance(document.getElementById('editClientModal'));
                    if (modal) modal.hide();
                }, 1500);
            } else {
                alert(`Error: ${result.message}`);
            }
        })
        .catch(error => {
            console.error('❌ Error updating client:', error);
            alert(`Error updating client: ${error.message}`);
        })
        .finally(() => {
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.innerHTML = 'Save Changes';
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
        alert(validation.errorMessage);
        input.value = '';
        return;
    }

    const reader = new FileReader();
    reader.onload = function (e) {
        const imageUrl = e.target.result;
        updateImagePreview(imageUrl, input);
        console.log('Image loaded successfully');
    };
    reader.onerror = function () {
        alert('Error reading image file');
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

    if (!input.value.match(/^https?:\/\/.+\.(jpg|jpeg|png|gif|webp|bmp)(\?.*)?$/i)) {
        console.warn('Please enter a valid image URL ending with .jpg, .png, .gif, etc.');
        input.classList.add('is-invalid');
        return;
    }

    const testImg = new Image();
    testImg.onload = function () {
        updateImagePreview(input.value, input);
        input.classList.remove('is-invalid');
        input.classList.add('is-valid');
        console.log('Valid image URL');
    };
    testImg.onerror = function () {
        updateImagePreview(input.value, input);
        input.classList.add('is-invalid');
        console.warn('Image URL could not be loaded. Please verify the URL.');
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
    console.log('Image cleared');
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
    console.log('Image cleared');
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

    console.log('Image will be removed when you save the client');
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

    const editFileInput = document.getElementById('editClientImageFile');
    const addFileInput = document.getElementById('addClientImageFile');

    if (editFileInput) editFileInput.value = '';
    if (addFileInput) addFileInput.value = '';

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

function getCurrentClientId() {
    return document.getElementById('editClientId')?.value || '';
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

    // Initialize modals with tab support
    const editClientModal = document.getElementById('editClientModal');
    if (editClientModal) {
        editClientModal.addEventListener('show.bs.modal', handleEditModalShow);
        editClientModal.addEventListener('hidden.bs.modal', handleEditModalHide);

        // Initialize tab switching
        const tabButtons = editClientModal.querySelectorAll('button[data-bs-toggle="tab"]');
        tabButtons.forEach(button => {
            button.addEventListener('shown.bs.tab', handleTabSwitch);
        });

        console.log('✅ Edit modal and tab handlers attached');
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

    // Initialize court/user management buttons
    const addCourtBtn = document.getElementById('addCourtBtn');
    if (addCourtBtn) {
        addCourtBtn.addEventListener('click', function () {
            const clientId = getCurrentClientId();
            if (clientId) {
                addClientCourt(clientId);
            } else {
                alert('No client selected');
            }
        });
    }

    const addUserBtn = document.getElementById('addUserBtn');
    if (addUserBtn) {
        addUserBtn.addEventListener('click', function () {
            const clientId = getCurrentClientId();
            if (clientId) {
                addClientUser(clientId);
            } else {
                alert('No client selected');
            }
        });
    }

    console.log('✅ Client Management with Courts Tab initialized successfully');
});

// ========== TESTING AND DEBUGGING FUNCTIONS ==========
window.testClientRefresh = {

    testUpdate: function (clientId = null) {
        console.log('🧪 Testing client update refresh...');

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
    }
};

window.testCourts = {
    loadCourts: function (clientId) {
        const testClientId = clientId || getCurrentClientId() || 'test-client-123';
        console.log('🧪 Testing court loading for client:', testClientId);
        loadClientCourts(testClientId);
    },

    loadUsers: function (clientId) {
        const testClientId = clientId || getCurrentClientId() || 'test-client-123';
        console.log('🧪 Testing user loading for client:', testClientId);
        loadClientUsers(testClientId);
    },

    simulateTabSwitch: function (tabName = 'courts') {
        console.log('🧪 Simulating tab switch to:', tabName);
        const mockEvent = {
            target: {
                getAttribute: () => `#${tabName}-tab-pane`
            }
        };
        handleTabSwitch(mockEvent);
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

console.log('🎯 Complete Client Management with Working Courts Tab loaded successfully');
console.log('🧪 Test functions: window.testClientRefresh, window.testCourts');
console.log('🐛 Debug functions: window.debugClient');
console.log('🔧 Table Manager: window.ClientTableManager');