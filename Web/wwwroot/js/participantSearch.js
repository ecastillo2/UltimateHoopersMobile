/**
 * Participant Search Functionality - FIXED MODAL BACKDROP ISSUE
 * Handles searching for users to add as participants to runs
 * File: wwwroot/js/participantSearch.js
 */

// Global state for participant search
window.participantSearchState = {
    currentRunId: null,
    selectedUser: null,
    currentParticipants: [],
    searchTimeout: null,
    isSearching: false,
    modalInstance: null // Store modal instance
};

// Initialize participant search functionality when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    initializeParticipantSearch();
    updateAddParticipantButtonHandler();
});

// Initialize participant search functionality
function initializeParticipantSearch() {
    console.log('🔍 Initializing participant search functionality');

    // Setup event handlers
    setupParticipantSearchHandlers();

    console.log('✅ Participant search initialized');
}

function setupParticipantSearchHandlers() {
    const addParticipantModal = document.getElementById('addParticipantModal');
    if (!addParticipantModal) {
        console.warn('⚠️ Add participant modal not found');
        return;
    }

    // Modal show/hide handlers with proper cleanup
    addParticipantModal.addEventListener('show.bs.modal', handleAddParticipantModalShow);
    addParticipantModal.addEventListener('hidden.bs.modal', handleAddParticipantModalHide);

    // FIXED: Add specific event handlers for modal cleanup
    addParticipantModal.addEventListener('hide.bs.modal', handleAddParticipantModalHiding);

    // Search input handler
    const searchInput = document.getElementById('participantSearch');
    if (searchInput) {
        searchInput.addEventListener('input', handleSearchInput);
        searchInput.addEventListener('keypress', handleSearchKeypress);
    }

    // Search button handler
    const searchBtn = document.getElementById('searchUsersBtn');
    if (searchBtn) {
        searchBtn.addEventListener('click', () => performUserSearch());
    }

    // Clear selection handler
    const clearBtn = document.getElementById('clearSelectionBtn');
    if (clearBtn) {
        clearBtn.addEventListener('click', clearUserSelection);
    }

    // Confirm add participant handler
    const confirmBtn = document.getElementById('confirmAddParticipantBtn');
    if (confirmBtn) {
        confirmBtn.addEventListener('click', confirmAddParticipant);
    }

    // FIXED: Add cancel button handler for proper cleanup
    const cancelBtns = addParticipantModal.querySelectorAll('[data-bs-dismiss="modal"]');
    cancelBtns.forEach(btn => {
        btn.addEventListener('click', handleCancelClick);
    });

    console.log('✅ Participant search handlers setup complete');
}

// FIXED: New function to handle modal hiding event
function handleAddParticipantModalHiding(event) {
    console.log('🔍 Add participant modal hiding...');
    // Clean up any ongoing operations
    if (window.participantSearchState.searchTimeout) {
        clearTimeout(window.participantSearchState.searchTimeout);
        window.participantSearchState.searchTimeout = null;
    }
}

// FIXED: New function to handle cancel button clicks
function handleCancelClick(event) {
    console.log('❌ Add participant modal cancelled');

    // Force close the modal and clean up backdrop
    const modal = window.participantSearchState.modalInstance ||
        bootstrap.Modal.getInstance(document.getElementById('addParticipantModal'));

    if (modal) {
        modal.hide();
    }

    // FIXED: Force cleanup of any lingering backdrops
    setTimeout(() => {
        cleanupModalBackdrop();
    }, 100);
}

// FIXED: New function to clean up modal backdrop
function cleanupModalBackdrop() {
    console.log('🧹 Cleaning up modal backdrop...');

    // Remove any lingering modal backdrops
    const backdrops = document.querySelectorAll('.modal-backdrop');
    backdrops.forEach((backdrop, index) => {
        // Keep only the first backdrop (for the edit run modal)
        if (index > 0) {
            console.log('🧹 Removing extra backdrop', index);
            backdrop.remove();
        }
    });

    // Ensure body classes are correct
    const body = document.body;
    const openModals = document.querySelectorAll('.modal.show');

    if (openModals.length <= 1) {
        // Only edit run modal should be open
        body.classList.add('modal-open');
        body.style.overflow = '';
        body.style.paddingRight = '';
    }

    // Remove any stuck modal-static class
    body.classList.remove('modal-static');
}

// Update the existing addParticipantBtn click handler
function updateAddParticipantButtonHandler() {
    const addParticipantBtn = document.getElementById('addParticipantBtn');
    if (addParticipantBtn) {
        // Remove existing listeners
        addParticipantBtn.removeEventListener('click', handleAddParticipantClick);

        // Add new listener
        addParticipantBtn.addEventListener('click', handleAddParticipantClick);
    }
}

function handleAddParticipantClick() {
    console.log('👥 Add participant button clicked');

    // Get current run info
    const runId = window.runManagementState?.currentRunId || safeGetValue('editRunId');
    if (!runId) {
        console.error('❌ No run ID available');
        alert('Error: No run selected');
        return;
    }

    // FIXED: Proper modal instance management
    const modalElement = document.getElementById('addParticipantModal');
    if (!modalElement) {
        console.error('❌ Add participant modal element not found');
        return;
    }

    // Get existing instance or create new one
    let modal = bootstrap.Modal.getInstance(modalElement);
    if (!modal) {
        modal = new bootstrap.Modal(modalElement, {
            backdrop: true,
            keyboard: true,
            focus: true
        });
    }

    // Store modal instance for later cleanup
    window.participantSearchState.modalInstance = modal;

    // Show the modal
    modal.show();
}

function handleAddParticipantModalShow() {
    console.log('🔍 Add participant modal opening');

    // Get current run info
    const runId = window.runManagementState?.currentRunId || safeGetValue('editRunId');
    const currentCount = document.getElementById('currentParticipantsCount')?.textContent || '0';
    const maxCount = document.getElementById('maxParticipantsCount')?.textContent || '0';

    // Store run ID
    window.participantSearchState.currentRunId = runId;

    // Update capacity display
    safeSetValue('addParticipantCurrentCount', currentCount);
    safeSetValue('addParticipantMaxCount', maxCount);

    // Reset search state
    resetParticipantSearch();

    // Load current participants to avoid duplicates
    loadCurrentParticipants(runId);

    // Focus on search input
    setTimeout(() => {
        const searchInput = document.getElementById('participantSearch');
        if (searchInput) {
            searchInput.focus();
        }
    }, 300);
}

function handleAddParticipantModalHide() {
    console.log('🔍 Add participant modal closing');

    // Reset search state
    resetParticipantSearch();

    // Clear modal instance reference
    window.participantSearchState.modalInstance = null;

    // FIXED: Ensure proper cleanup after modal hide
    setTimeout(() => {
        cleanupModalBackdrop();
    }, 150);
}

function handleSearchInput(event) {
    const searchTerm = event.target.value.trim();

    // Clear existing timeout
    if (window.participantSearchState.searchTimeout) {
        clearTimeout(window.participantSearchState.searchTimeout);
    }

    // Set new timeout for debounced search
    window.participantSearchState.searchTimeout = setTimeout(() => {
        if (searchTerm.length >= 2) {
            performUserSearch(searchTerm);
        } else if (searchTerm.length === 0) {
            showSearchPlaceholder();
        }
    }, 500); // 500ms debounce
}

function handleSearchKeypress(event) {
    if (event.key === 'Enter') {
        event.preventDefault();
        const searchTerm = event.target.value.trim();
        if (searchTerm.length >= 2) {
            performUserSearch(searchTerm);
        }
    }
}

async function performUserSearch(searchTerm) {
    if (!searchTerm) {
        searchTerm = document.getElementById('participantSearch')?.value?.trim();
    }

    if (!searchTerm || searchTerm.length < 2) {
        console.warn('⚠️ Search term too short');
        return;
    }

    console.log('🔍 Performing user search for:', searchTerm);

    // Show loading state
    showSearchLoading();
    window.participantSearchState.isSearching = true;

    try {
        const response = await fetch(`/Run/SearchUsers?query=${encodeURIComponent(searchTerm)}`, {
            method: 'GET',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            credentials: 'same-origin'
        });

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        const data = await response.json();
        console.log('🔍 Search results:', data);

        if (data.success !== false) {
            const users = data.users || data || [];
            displaySearchResults(users, searchTerm);
        } else {
            throw new Error(data.message || 'Search failed');
        }

    } catch (error) {
        console.error('❌ User search error:', error);
        showSearchError('Unable to search users. Please try again.');
    } finally {
        window.participantSearchState.isSearching = false;
    }
}

async function loadCurrentParticipants(runId) {
    console.log('👥 Loading current participants for run:', runId);

    try {
        const response = await fetch(`/Run/GetRunParticipants?runId=${encodeURIComponent(runId)}`, {
            method: 'GET',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            credentials: 'same-origin'
        });

        if (response.ok) {
            const data = await response.json();
            if (data.success !== false) {
                window.participantSearchState.currentParticipants = data.participants || data || [];
                console.log('👥 Loaded current participants:', window.participantSearchState.currentParticipants.length);
            } else {
                console.warn('⚠️ Failed to load current participants:', data.message);
                window.participantSearchState.currentParticipants = [];
            }
        } else {
            console.warn('⚠️ Failed to load current participants - HTTP', response.status);
            window.participantSearchState.currentParticipants = [];
        }
    } catch (error) {
        console.error('❌ Error loading participants:', error);
        window.participantSearchState.currentParticipants = [];
    }
}

function displaySearchResults(users, searchTerm) {
    const container = document.getElementById('searchResultsContainer');
    if (!container) return;

    console.log('📋 Displaying search results:', users.length, 'users found');

    if (users.length === 0) {
        showSearchEmpty(searchTerm);
        return;
    }

    // Create results HTML
    let resultsHtml = '<div class="search-results">';

    users.forEach(user => {
        const isAlreadyParticipant = isUserAlreadyParticipant(user);
        const userInitials = getUserInitials(user.userName || user.name || user.playerNumber);
        const profileImage = user.imageUrl;

        resultsHtml += `
            <div class="search-result-item ${isAlreadyParticipant ? 'already-participant' : ''}" 
                 data-user-id="${user.profileId || user.id}" 
                 data-user-name="${escapeHtml(user.userName || user.name || '')}"
                 data-user-first-name="${escapeHtml(user.firstName || '')}"
                 data-user-last-name="${escapeHtml(user.lastName || '')}"
                 data-user-player-number="${escapeHtml(user.playerNumber || '')}"
                 data-user-image="${escapeHtml(user.imageUrl || '')}"
                 ${!isAlreadyParticipant ? 'onclick="selectUser(this)"' : ''}>
                
                <div class="user-avatar">
                    ${profileImage ?
                `<img src="${escapeHtml(profileImage)}" alt="${escapeHtml(user.userName || 'User')}" class="profile-image" onerror="this.style.display='none'; this.nextElementSibling.style.display='flex';">
                         <div class="profile-initials" style="display: none;">${userInitials}</div>` :
                `<div class="profile-initials">${userInitials}</div>`
            }
                </div>
                
                <div class="user-info">
                    <div class="user-name">@${escapeHtml(user.userName || user.name || 'Unknown User')} - ${user.firstName} ${user.lastName}</div>
                    <div class="user-details">
                        ${user.playerNumber ? '#' + escapeHtml(user.playerNumber) : 'No Player Number'}
                        ${user.skillLevel ? ' • ' + escapeHtml(user.skillLevel) : ''}
                    </div>
                </div>
                
                <div class="user-status ${isAlreadyParticipant ? 'status-already-participant' : 'status-available'}">
                    ${isAlreadyParticipant ? 'Already Participant' : 'Available'}
                </div>
                
                ${isAlreadyParticipant ?
                '<i class="bi bi-person-check"></i>' :
                ''
            }
            </div>
        `;
    });

    resultsHtml += '</div>';

    container.innerHTML = resultsHtml;
}

function selectUser(element) {
    console.log('👤 User selected');

    // Clear previous selection
    document.querySelectorAll('.search-result-item').forEach(item => {
        item.classList.remove('selected');
    });

    // Mark as selected
    element.classList.add('selected');

    // Store selected user
    window.participantSearchState.selectedUser = {
        profileId: element.getAttribute('data-user-id'),
        userName: element.getAttribute('data-user-name'),
        firstName: element.getAttribute('data-user-first-name'),
        lastName: element.getAttribute('data-user-last-name'),
        playerNumber: element.getAttribute('data-user-player-number'),
        imageUrl: element.getAttribute('data-user-image')
    };

    // Show selected user preview
    showSelectedUser();

    // Enable confirm button
    const confirmBtn = document.getElementById('confirmAddParticipantBtn');
    if (confirmBtn) {
        confirmBtn.disabled = false;
    }

    console.log('👤 Selected user:', window.participantSearchState.selectedUser);
}

function showSelectedUser() {
    const container = document.getElementById('selectedUserContainer');

    if (container && window.participantSearchState.selectedUser) {
        const user = window.participantSearchState.selectedUser;
        const playerNumberText = user.playerNumber ? ` (#${user.playerNumber})` : '';
        const userInitials = getUserInitials(user.userName);

        // Determine display name - prefer full name if available
        let displayName = user.userName || 'Unknown User';
        if (user.firstName && user.lastName) {
            displayName = `${user.firstName} ${user.lastName}`;
        } else if (user.firstName) {
            displayName = user.firstName;
        } else if (user.lastName) {
            displayName = user.lastName;
        }

        // Create the selected user HTML with profile image
        const selectedUserHtml = `
            <div class="alert alert-success d-flex align-items-center" role="alert">
                <i class="bi bi-check-circle-fill flex-shrink-0 me-2"></i>
                
                <!-- User Avatar -->
                <div class="selected-user-avatar me-3">
                    ${user.imageUrl ?
                `<img src="${escapeHtml(user.imageUrl)}" alt="${escapeHtml(displayName)}" class="selected-profile-image" onerror="this.style.display='none'; this.nextElementSibling.style.display='flex';">
                         <div class="selected-profile-initials" style="display: none;">${userInitials}</div>` :
                `<div class="selected-profile-initials">${userInitials}</div>`
            }
                </div>
                
                <div class="flex-grow-1">
                    <strong>Selected User:</strong>
                    <span id="selectedUserName">${escapeHtml(displayName)}${playerNumberText}</span>
                </div>
                <button type="button" class="btn btn-sm btn-outline-success ms-2" id="clearSelectionBtn" onclick="clearUserSelection()">
                    <i class="bi bi-x"></i> Clear
                </button>
            </div>
        `;

        container.innerHTML = selectedUserHtml;
        container.classList.remove('d-none');
    }
}

function clearUserSelection() {
    console.log('🧹 Clearing user selection');

    // Clear selected user
    window.participantSearchState.selectedUser = null;

    // Remove visual selection
    document.querySelectorAll('.search-result-item').forEach(item => {
        item.classList.remove('selected');
    });

    // Hide selected user preview
    const container = document.getElementById('selectedUserContainer');
    if (container) {
        container.classList.add('d-none');
    }

    // Disable confirm button
    const confirmBtn = document.getElementById('confirmAddParticipantBtn');
    if (confirmBtn) {
        confirmBtn.disabled = true;
    }
}

async function confirmAddParticipant() {
    console.log('✅ Confirming add participant');

    const selectedUser = window.participantSearchState.selectedUser;
    const runId = window.participantSearchState.currentRunId;

    if (!selectedUser || !runId) {
        console.error('❌ Missing required data for adding participant');
        alert('Error: Missing required information');
        return;
    }

    // Disable button during request
    const confirmBtn = document.getElementById('confirmAddParticipantBtn');
    if (confirmBtn) {
        confirmBtn.disabled = true;
        confirmBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Adding...';
    }

    try {
        const response = await fetch('/Run/AddParticipant', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify({
                RunId: runId,
                ProfileId: selectedUser.profileId
            })
        });

        const result = await response.json();

        if (result.success) {
            console.log('✅ Participant added successfully');

            // Show success message
            alert(`${selectedUser.userName} has been added to the run!`);

            // FIXED: Proper modal closure with cleanup
            const modal = window.participantSearchState.modalInstance ||
                bootstrap.Modal.getInstance(document.getElementById('addParticipantModal'));
            if (modal) {
                modal.hide();
            }

            // Refresh participants list
            setTimeout(() => {
                refreshParticipantsList();
                cleanupModalBackdrop(); // Ensure clean state
            }, 500);

        } else {
            throw new Error(result.message || 'Failed to add participant');
        }

    } catch (error) {
        console.error('❌ Error adding participant:', error);
        alert(`Error adding participant: ${error.message}`);
    } finally {
        // Re-enable button
        if (confirmBtn) {
            confirmBtn.disabled = false;
            confirmBtn.innerHTML = '<i class="bi bi-person-plus me-2"></i>Add Participant';
        }
    }
}

function refreshParticipantsList() {
    console.log('🔄 Refreshing participants list');

    const runId = window.participantSearchState.currentRunId;
    if (runId && window.loadRunParticipants) {
        window.loadRunParticipants(runId);
    } else {
        // Fallback: reload the page or refresh the modal
        location.reload();
    }
}

function isUserAlreadyParticipant(user) {
    const userId = user.profileId || user.id;
    return window.participantSearchState.currentParticipants.some(
        participant => participant.profileId === userId
    );
}

function getUserInitials(name) {
    if (!name) return '?';

    const parts = name.trim().split(' ');
    if (parts.length >= 2) {
        return (parts[0][0] + parts[1][0]).toUpperCase();
    } else if (parts.length === 1 && parts[0].length > 0) {
        return parts[0][0].toUpperCase();
    }
    return '?';
}

function showSearchLoading() {
    const container = document.getElementById('searchResultsContainer');
    if (container) {
        container.innerHTML = `
            <div class="search-loading">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Searching...</span>
                </div>
                <p class="mt-2 mb-0">Searching for users...</p>
            </div>
        `;
    }
}

function showSearchEmpty(searchTerm) {
    const container = document.getElementById('searchResultsContainer');
    if (container) {
        container.innerHTML = `
            <div class="search-empty">
                <i class="bi bi-person-x" style="font-size: 2rem;"></i>
                <p class="mt-2 mb-0">No users found for "${escapeHtml(searchTerm)}"</p>
                <p class="text-muted small">Try a different search term</p>
            </div>
        `;
    }
}

function showSearchError(message) {
    const container = document.getElementById('searchResultsContainer');
    if (container) {
        container.innerHTML = `
            <div class="search-error">
                <i class="bi bi-exclamation-triangle" style="font-size: 2rem;"></i>
                <p class="mt-2 mb-0">${escapeHtml(message)}</p>
            </div>
        `;
    }
}

function showSearchPlaceholder() {
    const container = document.getElementById('searchResultsContainer');
    if (container) {
        container.innerHTML = `
            <div id="searchPlaceholder" class="text-center text-muted py-4">
                <i class="bi bi-search" style="font-size: 2rem;"></i>
                <p class="mt-2 mb-0">Start typing to search for users</p>
            </div>
        `;
    }
}

function resetParticipantSearch() {
    console.log('🧹 Resetting participant search');

    // Clear search input
    const searchInput = document.getElementById('participantSearch');
    if (searchInput) {
        searchInput.value = '';
    }

    // Clear selected user
    clearUserSelection();

    // Show placeholder
    showSearchPlaceholder();

    // Clear timeouts
    if (window.participantSearchState.searchTimeout) {
        clearTimeout(window.participantSearchState.searchTimeout);
        window.participantSearchState.searchTimeout = null;
    }

    // Reset state
    window.participantSearchState.selectedUser = null;
    window.participantSearchState.isSearching = false;
}

function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function safeSetValue(elementId, value) {
    const element = document.getElementById(elementId);
    if (element) {
        if (element.tagName.toLowerCase() === 'span' || element.tagName.toLowerCase() === 'div') {
            element.textContent = value || '';
        } else {
            element.value = value || '';
        }
    }
}

function safeGetValue(elementId) {
    const element = document.getElementById(elementId);
    return element ? element.value : '';
}

function getAntiForgeryToken() {
    const token = document.querySelector('input[name="__RequestVerificationToken"]');
    return token ? token.value : '';
}

// Make selectUser globally accessible for onclick handlers
window.selectUser = selectUser;

// FIXED: Global function for manual cleanup (useful for debugging)
window.cleanupParticipantModalBackdrop = cleanupModalBackdrop;

// Export functions for debugging
window.participantSearchDebug = {
    state: () => window.participantSearchState,
    search: performUserSearch,
    loadParticipants: loadCurrentParticipants,
    reset: resetParticipantSearch,
    selectUser: selectUser,
    clearSelection: clearUserSelection,
    confirm: confirmAddParticipant,
    cleanupBackdrop: cleanupModalBackdrop, // FIXED: Add cleanup function

    // Test functions
    testSearch: function (term) {
        console.log('🧪 Testing search with term:', term);
        performUserSearch(term);
    },

    simulateUserSelection: function (userData) {
        console.log('🧪 Simulating user selection:', userData);
        window.participantSearchState.selectedUser = userData;
        showSelectedUser();

        const confirmBtn = document.getElementById('confirmAddParticipantBtn');
        if (confirmBtn) confirmBtn.disabled = false;
    },

    getCurrentParticipants: function () {
        console.log('👥 Current participants:', window.participantSearchState.currentParticipants);
        return window.participantSearchState.currentParticipants;
    },

    getSelectedUser: function () {
        console.log('👤 Selected user:', window.participantSearchState.selectedUser);
        return window.participantSearchState.selectedUser;
    },

    // FIXED: New debug functions for modal testing
    testModalCleanup: function () {
        console.log('🧪 Testing modal cleanup...');
        cleanupModalBackdrop();
        console.log('✅ Manual cleanup completed');
    },

    getModalState: function () {
        const modalElement = document.getElementById('addParticipantModal');
        const modalInstance = window.participantSearchState.modalInstance;
        const backdrops = document.querySelectorAll('.modal-backdrop');
        const bodyClasses = document.body.className;

        console.log('🔍 Modal state:', {
            modalElement: !!modalElement,
            modalInstance: !!modalInstance,
            backdropCount: backdrops.length,
            bodyClasses: bodyClasses,
            modalShown: modalElement?.classList.contains('show')
        });

        return {
            modalElement: !!modalElement,
            modalInstance: !!modalInstance,
            backdropCount: backdrops.length,
            bodyClasses: bodyClasses,
            modalShown: modalElement?.classList.contains('show')
        };
    },

    forceCloseModal: function () {
        console.log('🚨 Force closing participant modal...');

        const modal = window.participantSearchState.modalInstance ||
            bootstrap.Modal.getInstance(document.getElementById('addParticipantModal'));

        if (modal) {
            modal.hide();
        }

        // Force cleanup
        setTimeout(() => {
            cleanupModalBackdrop();
            console.log('✅ Force close completed');
        }, 200);
    }
};

console.log('🐛 PARTICIPANT SEARCH DEBUG:');
console.log('🐛 Try: window.participantSearchDebug.testModalCleanup() to test cleanup');
console.log('🐛 Try: window.participantSearchDebug.getModalState() to check modal state');
console.log('🐛 Try: window.participantSearchDebug.forceCloseModal() to force close');
console.log('🐛 Try: window.cleanupParticipantModalBackdrop() for direct cleanup');