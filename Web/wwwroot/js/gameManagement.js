document.addEventListener('DOMContentLoaded', function () {
    console.log('🚀 Initializing Complete Fixed Game Management');

    // Global storage for current game data
    window.currentGameData = null;

    // Initialize components
    initializeDataTable();
    initializeModals();
    initializeForms();
    fixEditButtons();

    console.log('✅ Complete Fixed Game Management loaded successfully');

    // ========== FIX EDIT BUTTONS ==========
    function fixEditButtons() {
        console.log('🔧 Fixing edit buttons...');

        const editButtons = document.querySelectorAll('[data-bs-target="#editGameModal"]');
        console.log(`Found ${editButtons.length} edit buttons`);

        editButtons.forEach((button, index) => {
            let gameId = button.getAttribute('data-game-id');

            if (!gameId) {
                // Try to get from parent row
                const row = button.closest('tr');
                if (row) {
                    gameId = row.getAttribute('data-game-id') ||
                        row.getAttribute('data-gameid') ||
                        row.dataset.gameId;

                    if (gameId) {
                        button.setAttribute('data-game-id', gameId);
                        console.log(`✅ Fixed button ${index + 1}: added data-game-id="${gameId}"`);
                    } else {
                        console.warn(`❌ Button ${index + 1}: No game ID found anywhere`);

                        // Try to extract from href of nearby view button
                        const viewButton = row.querySelector('a[href*="/Game/Details"]');
                        if (viewButton) {
                            const match = viewButton.href.match(/id=([^&]+)/);
                            if (match) {
                                gameId = match[1];
                                button.setAttribute('data-game-id', gameId);
                                console.log(`✅ Extracted from view button: ${gameId}`);
                            }
                        }
                    }
                } else {
                    console.error(`❌ Button ${index + 1}: No parent row found`);
                }
            } else {
                console.log(`✅ Button ${index + 1}: Already has data-game-id="${gameId}"`);
            }
        });
    }

    // ========== DATATABLE INITIALIZATION ==========
    function initializeDataTable() {
        const tableElement = document.getElementById('gamesTable');
        if (tableElement && tableElement.querySelector('tbody tr')) {
            console.log('📊 Initializing DataTable...');
            const gamesTable = $('#gamesTable').DataTable({
                responsive: true,
                lengthMenu: [[10, 25, 50, -1], [10, 25, 50, "All"]],
                dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>rtip',
                language: {
                    search: "_INPUT_",
                    searchPlaceholder: "Search games...",
                    lengthMenu: "Show _MENU_ games per page",
                    info: "Showing _START_ to _END_ of _TOTAL_ games",
                    infoEmpty: "Showing 0 to 0 of 0 games",
                    infoFiltered: "(filtered from _MAX_ total games)"
                },
                columnDefs: [
                    { className: "align-middle", targets: "_all" },
                    { orderable: false, targets: [5] }
                ],
                order: [[1, 'desc']],
                initComplete: function () {
                    console.log('📊 DataTable initialization complete, setting up filters...');
                    setTimeout(function () {
                        initializeFilters();
                        fixEditButtons(); // Fix buttons after DataTable renders
                    }, 100);
                }
            });
            console.log('📊 DataTable initialized successfully');
        } else {
            setTimeout(function () {
                initializeFilters();
                fixEditButtons();
            }, 500);
        }
    }

    // ========== FILTERS ==========
    function initializeFilters() {
        console.log('🔍 Initializing game table filters...');

        const gamesTable = $('#gamesTable');
        if (!gamesTable.length || !$.fn.dataTable.isDataTable(gamesTable)) {
            console.warn('⚠️ Games table not found or not initialized as DataTable');
            return;
        }

        const table = gamesTable.DataTable();
        const statusFilter = $('#statusFilter');
        const dateFilter = $('#dateFilter');
        const courtFilter = $('#courtFilter');
        const runFilter = $('#runFilter');
        const resetFiltersBtn = $('#resetFilters');
        const activeFiltersContainer = $('#activeFilters');

        if (!statusFilter.length || !dateFilter.length || !courtFilter.length || !runFilter.length) {
            console.warn('⚠️ Some filter elements not found');
            return;
        }

        function applyFilters() {
            console.log('🔍 Applying filters...');

            if ($.fn.dataTable.ext.search.length > 0) {
                $.fn.dataTable.ext.search = $.fn.dataTable.ext.search.filter(fn =>
                    !fn.name || fn.name !== 'gameTableFilter'
                );
            }

            const customFilter = function (settings, data, dataIndex) {
                if (settings.nTable.id !== 'gamesTable') return true;

                const row = $(table.row(dataIndex).node());

                if (statusFilter.val() === 'all' &&
                    dateFilter.val() === 'all' &&
                    courtFilter.val() === 'all' &&
                    runFilter.val() === 'all') {
                    return true;
                }

                try {
                    // Status filtering
                    if (statusFilter.val() !== 'all') {
                        const statusValue = statusFilter.val().toLowerCase();
                        const rowStatus = (row.attr('data-status') || '').toLowerCase();
                        const statusCell = row.find('td:nth-child(5)');
                        const statusText = statusCell.find('.badge').text().toLowerCase() ||
                            statusCell.text().toLowerCase();

                        const matchesStatus = rowStatus === statusValue || statusText.includes(statusValue);
                        if (!matchesStatus) return false;
                    }

                    // Date filtering
                    if (dateFilter.val() !== 'all') {
                        if (!filterByDate(row, data[1], dateFilter.val())) {
                            return false;
                        }
                    }

                    // Court filtering
                    if (courtFilter.val() !== 'all') {
                        const courtValue = courtFilter.val().toLowerCase();
                        const rowCourt = (row.attr('data-court-id') || '').toLowerCase();
                        const courtCell = row.find('td:nth-child(3)');
                        const courtText = courtCell.text().toLowerCase();

                        const matchesCourt = rowCourt === courtValue || courtText.includes(courtValue);
                        if (!matchesCourt) return false;
                    }

                    // Run filtering
                    if (runFilter.val() !== 'all') {
                        const runValue = runFilter.val().toLowerCase();
                        const rowRun = (row.attr('data-run-id') || '').toLowerCase();
                        const gameCell = row.find('td:first-child');
                        const gameText = gameCell.text().toLowerCase();

                        const matchesRun = rowRun === runValue || gameText.includes(runValue);
                        if (!matchesRun) return false;
                    }

                    return true;
                } catch (error) {
                    console.error('❌ Error in filter function:', error);
                    return true;
                }
            };

            customFilter.name = 'gameTableFilter';
            $.fn.dataTable.ext.search.push(customFilter);
            table.draw();
            updateActiveFilters();
        }

        function filterByDate(row, dateText, filterValue) {
            try {
                const dateString = row.attr('data-created-date') || dateText;
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
                    case 'today':
                        return date.toDateString() === today.toDateString();
                    case 'this-week':
                        const startOfWeek = new Date(today);
                        startOfWeek.setDate(today.getDate() - today.getDay());
                        return date >= startOfWeek;
                    case 'this-month':
                        const startOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);
                        return date >= startOfMonth;
                    case 'upcoming':
                        return date >= today;
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

            const hasActiveFilters =
                statusFilter.val() !== 'all' ||
                dateFilter.val() !== 'all' ||
                courtFilter.val() !== 'all' ||
                runFilter.val() !== 'all';

            if (!hasActiveFilters) {
                activeFiltersContainer.append(
                    $('<span>').addClass('text-muted filter-none').text('None')
                );
                return;
            }

            if (statusFilter.val() !== 'all') {
                addFilterBadge('Status', formatFilterValue(statusFilter.val()), () => {
                    statusFilter.val('all');
                    applyFilters();
                });
            }

            if (dateFilter.val() !== 'all') {
                addFilterBadge('Date', formatFilterValue(dateFilter.val()), () => {
                    dateFilter.val('all');
                    applyFilters();
                });
            }

            if (courtFilter.val() !== 'all') {
                addFilterBadge('Court', formatFilterValue(courtFilter.val()), () => {
                    courtFilter.val('all');
                    applyFilters();
                });
            }

            if (runFilter.val() !== 'all') {
                addFilterBadge('Run', formatFilterValue(runFilter.val()), () => {
                    runFilter.val('all');
                    applyFilters();
                });
            }
        }

        function formatFilterValue(value) {
            return value.split('-').map(word => word.charAt(0).toUpperCase() + word.slice(1)).join(' ');
        }

        function addFilterBadge(label, value, removeCallback) {
            const badge = $('<span>')
                .addClass('badge bg-primary me-2 filter-badge')
                .text(`${label}: ${value}`);

            const removeBtn = $('<button>')
                .addClass('btn-close btn-close-white ms-1')
                .css('font-size', '0.5rem')
                .on('click', function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    removeCallback();
                });

            badge.append(removeBtn);
            activeFiltersContainer.append(badge);
        }

        // Event listeners
        statusFilter.on('change', applyFilters);
        dateFilter.on('change', applyFilters);
        courtFilter.on('change', applyFilters);
        runFilter.on('change', applyFilters);

        if (resetFiltersBtn.length) {
            resetFiltersBtn.on('click', function () {
                console.log('🔄 Resetting all filters');
                statusFilter.val('all');
                dateFilter.val('all');
                courtFilter.val('all');
                runFilter.val('all');
                applyFilters();
            });
        }

        applyFilters();
        console.log('✅ Game table filters initialized successfully');
    }

    // ========== MODAL HANDLERS ==========
    function initializeModals() {
        const editGameModal = document.getElementById('editGameModal');
        if (editGameModal) {
            editGameModal.addEventListener('show.bs.modal', handleEditModalShow);
            editGameModal.addEventListener('hidden.bs.modal', handleEditModalHide);

            const tabButtons = editGameModal.querySelectorAll('button[data-bs-toggle="tab"]');
            tabButtons.forEach(button => {
                button.addEventListener('shown.bs.tab', handleTabSwitch);
            });

            console.log('📝 Modal event handlers initialized');
        }

        const deleteGameBtn = document.getElementById('deleteGameBtn');
        if (deleteGameBtn) {
            deleteGameBtn.addEventListener('click', handleDeleteGame);
        }
    }

    function handleEditModalShow(event) {
        const button = event.relatedTarget;
        const gameId = button.getAttribute('data-game-id');

        console.log('📂 ENHANCED MODAL OPENING - Game ID Debug:');
        console.log('  Button element:', button);
        console.log('  data-game-id:', gameId);
        console.log('  Button HTML:', button.outerHTML);

        if (!gameId) {
            console.error('❌ No game ID found on button!');

            // Try to recover from various sources
            let recoveredId = null;

            // Try parent row
            const row = button.closest('tr');
            if (row) {
                recoveredId = row.getAttribute('data-game-id') ||
                    row.getAttribute('data-gameid') ||
                    row.dataset.gameId;
                console.log('  Trying row data:', recoveredId);
            }

            // Try sibling view button
            if (!recoveredId && row) {
                const viewButton = row.querySelector('a[href*="/Game/Details"]');
                if (viewButton) {
                    const match = viewButton.href.match(/id=([^&]+)/);
                    if (match) {
                        recoveredId = match[1];
                        console.log('  Extracted from view button:', recoveredId);
                    }
                }
            }

            if (recoveredId) {
                // Fix the button for future use
                button.setAttribute('data-game-id', recoveredId);
                console.log('✅ Recovered and set GameId:', recoveredId);
            } else {
                console.error('❌ Could not recover GameId from any source');
                window.UIUtils.showError('Game ID is missing. Please refresh the page.');
                event.preventDefault();
                return;
            }
        }

        const finalGameId = gameId || button.getAttribute('data-game-id');
        console.log('✅ Final GameId:', finalGameId);

        // Set game IDs in forms with verification
        const gameIdField = document.getElementById('editGameId');
        const deleteIdField = document.getElementById('deleteGameId');

        if (gameIdField) {
            gameIdField.value = finalGameId;
            console.log('✅ Set editGameId field to:', gameIdField.value);
        } else {
            console.error('❌ editGameId field not found!');
        }

        if (deleteIdField) {
            deleteIdField.value = finalGameId;
            console.log('✅ Set deleteGameId field to:', deleteIdField.value);
        }

        // Clear previous data and load new data
        clearAllForms();
        loadGameDataEnhanced(finalGameId);
    }

    function handleEditModalHide() {
        console.log('🚪 Edit modal closed, clearing forms');
        clearAllForms();
        window.currentGameData = null;
    }

    function handleTabSwitch(event) {
        const targetTab = event.target.getAttribute('data-bs-target');
        const gameId = document.getElementById('editGameId')?.value;

        console.log('🔄 Switching to tab:', targetTab, 'for game:', gameId);

        if (!gameId) return;

        switch (targetTab) {
            case '#game-details-tab-pane':
                break;
            case '#game-players-tab-pane':
                console.log('👥 Loading players tab');
                loadGamePlayers(gameId);
                break;
            case '#game-info-tab-pane':
                console.log('📊 Loading game info tab');
                updateGameInfoDisplayFromCurrentData();
                break;
            case '#game-stats-tab-pane':
                console.log('📈 Loading statistics tab');
                loadGameStatistics(gameId);
                break;
        }
    }

    function handleDeleteGame() {
        const gameId = safeGetValue('editGameId');
        if (!gameId) return;

        safeSetValue('deleteGameId', gameId);

        const editModal = bootstrap.Modal.getInstance(document.getElementById('editGameModal'));
        if (editModal) editModal.hide();

        const deleteModalEl = document.getElementById('deleteGameModal');
        if (deleteModalEl) {
            const deleteModal = new bootstrap.Modal(deleteModalEl);
            deleteModal.show();
        }
    }

    // ========== FORM HANDLERS ==========
    function initializeForms() {
        // Add Game Form
        const addGameForm = document.getElementById('addGameForm');
        if (addGameForm) {
            addGameForm.addEventListener('submit', handleAddFormSubmit);
            console.log('✅ Add form handler attached');
        }

        // Edit Game Form
        const editGameForm = document.getElementById('editGameForm');
        if (editGameForm) {
            editGameForm.addEventListener('submit', handleEditFormSubmit);
            console.log('✅ Edit form handler attached');
        }

        // Initialize run/court dependencies
        initializeFormDependencies();
    }

    function initializeFormDependencies() {
        // Add form dependencies
        const addRunSelect = document.getElementById('addRunId');
        if (addRunSelect) {
            addRunSelect.addEventListener('change', function () {
                const runId = this.value;
                if (runId) {
                    loadCourtsForRun(runId, 'addCourtId');
                    loadClientForRun(runId, 'addClientId');
                } else {
                    clearSelect('addCourtId');
                    clearField('addClientId');
                }
            });
        }

        // Edit form dependencies
        const editRunSelect = document.getElementById('editRunId');
        if (editRunSelect) {
            editRunSelect.addEventListener('change', function () {
                const runId = this.value;
                if (runId) {
                    loadCourtsForRun(runId, 'editCourtId');
                    loadClientForRun(runId, 'editClientId');
                } else {
                    clearSelect('editCourtId');
                    clearField('editClientId');
                }
            });
        }

        // Load initial data for dropdowns
        loadAvailableRuns();
    }

    function handleAddFormSubmit(e) {
        e.preventDefault();
        console.log('📤 Add game form submitted');

        if (!validateGameForm(e.target)) {
            return;
        }

        const formData = new FormData(e.target);
        const gameData = {};

        // Convert FormData to object
        for (const [key, value] of formData.entries()) {
            gameData[key] = value;
        }

        console.log('📋 Game data for creation:', gameData);

        const submitBtn = e.target.querySelector('button[type="submit"]');
        if (submitBtn) {
            window.UIUtils.setButtonLoading(submitBtn, true, 'Creating Game...');
        }

        const token = getAntiForgeryToken();

        fetch('/Game/Create', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(gameData)
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                return response.json();
            })
            .then(result => {
                if (submitBtn) {
                    window.UIUtils.setButtonLoading(submitBtn, false);
                }

                if (result.success) {
                    window.UIUtils.showSuccess('Game created successfully!');
                    const modal = bootstrap.Modal.getInstance(document.getElementById('addGameModal'));
                    if (modal) modal.hide();
                    setTimeout(() => location.reload(), 1000);
                } else {
                    window.UIUtils.showError(`Error creating game: ${result.message || 'Unknown error'}`);
                }
            })
            .catch(error => {
                console.error('🚨 Error creating game:', error);
                if (submitBtn) {
                    window.UIUtils.setButtonLoading(submitBtn, false);
                }
                window.UIUtils.showError(`Error creating game: ${error.message}`);
            });
    }

    function handleEditFormSubmit(e) {
        e.preventDefault();
        console.log('📤 Edit game form submitted');

        if (!validateGameForm(e.target)) {
            return;
        }

        const formData = new FormData(e.target);
        const gameData = {};

        // Convert FormData to object
        for (const [key, value] of formData.entries()) {
            gameData[key] = value;
        }

        console.log('📋 Game data for edit:', gameData);

        const submitBtn = e.target.querySelector('button[type="submit"]');
        if (submitBtn) {
            window.UIUtils.setButtonLoading(submitBtn, true, 'Saving...');
        }

        const token = getAntiForgeryToken();

        fetch('/Game/Edit', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(gameData)
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                return response.json();
            })
            .then(result => {
                if (submitBtn) {
                    window.UIUtils.setButtonLoading(submitBtn, false);
                }

                if (result.success) {
                    window.UIUtils.showSuccess('Game updated successfully!');

                    window.currentGameData = { ...window.currentGameData, ...gameData };

                    setTimeout(() => {
                        const modal = bootstrap.Modal.getInstance(document.getElementById('editGameModal'));
                        if (modal) modal.hide();
                        location.reload();
                    }, 1000);
                } else {
                    window.UIUtils.showError(`Error updating game: ${result.message || 'Unknown error'}`);
                }
            })
            .catch(error => {
                console.error('🚨 Error updating game:', error);
                if (submitBtn) {
                    window.UIUtils.setButtonLoading(submitBtn, false);
                }
                window.UIUtils.showError(`Error updating game: ${error.message}`);
            });
    }

    // ========== DATA LOADING FUNCTIONS ==========
    function loadGameDataEnhanced(gameId) {
        console.log('📥 Loading enhanced game data for ID:', gameId);

        if (!gameId) {
            console.error('🚨 No game ID provided');
            return;
        }

        showLoadingState();

        const row = findGameRowById(gameId);
        if (row) {
            console.log('📋 Found table row, extracting data...');
            const tableData = extractTableData(row);
            populateFromTableData(tableData);
        }

        if (!window.appUrls?.getGameData) {
            console.error('🚨 GetGameData API URL not configured');
            hideLoadingState();
            window.UIUtils.showWarning('API not configured. Only table data available.');
            return;
        }

        fetch(`${window.appUrls.getGameData}?id=${encodeURIComponent(gameId)}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                return response.json();
            })
            .then(data => {
                console.log('📦 Received enhanced game data:', data);
                hideLoadingState();

                if (data.success !== false) {
                    populateFromAPIDataEnhanced(data);
                    window.UIUtils.showSuccess('Game data loaded successfully');
                } else {
                    window.UIUtils.showWarning(`Failed to load complete game data: ${data.message || 'Unknown error'}`);
                }
            })
            .catch(error => {
                console.error('🚨 Error loading game data:', error);
                hideLoadingState();
                window.UIUtils.showError(`Error loading game data: ${error.message}`);
            });
    }

    function extractTableData(row) {
        if (!row) return {};

        console.log('📋 Extracting data from table row');

        const dataFromAttributes = {
            gameId: row.getAttribute('data-game-id'),
            gameNumber: row.getAttribute('data-game-number'),
            status: row.getAttribute('data-status'),
            courtId: row.getAttribute('data-court-id'),
            runId: row.getAttribute('data-run-id'),
            clientId: row.getAttribute('data-client-id'),
            createdDate: row.getAttribute('data-created-date')
        };

        const cells = row.querySelectorAll('td');
        if (cells.length >= 4) {
            const gameCell = cells[0];
            const titleEl = gameCell.querySelector('.game-title, .fw-semibold');
            const numberEl = gameCell.querySelector('.game-id, .text-muted.small');

            if (titleEl && !dataFromAttributes.gameNumber) {
                const match = titleEl.textContent.match(/Game #(\w+)/);
                if (match) dataFromAttributes.gameNumber = match[1];
            }
            if (numberEl && !dataFromAttributes.gameId) {
                const match = numberEl.textContent.match(/ID: (\S+)/);
                if (match) dataFromAttributes.gameId = match[1];
            }

            if (cells[2] && !dataFromAttributes.courtId) {
                dataFromAttributes.courtName = cells[2].textContent.trim();
            }

            if (cells[4] && !dataFromAttributes.status) {
                const statusEl = cells[4].querySelector('.badge, .game-status');
                if (statusEl) {
                    dataFromAttributes.status = statusEl.textContent.trim();
                }
            }
        }

        return dataFromAttributes;
    }

    function populateFromTableData(data) {
        console.log('📝 Populating form from table data:', data);

        try {
            window.currentGameData = data;

            safeSetValue('editGameNumber', data.gameNumber);
            safeSetValue('editClientId', data.clientId);

            safeSetSelect('editStatus', data.status || 'Scheduled');

            // Load dependent data
            if (data.runId) {
                safeSetSelect('editRunId', data.runId);
                loadCourtsForRun(data.runId, 'editCourtId', data.courtId);
            }

            console.log('✅ Table data populated successfully');
        } catch (error) {
            console.error('🚨 Error populating from table data:', error);
        }
    }

    function populateFromAPIDataEnhanced(data) {
        console.log('🌐 Populating form from enhanced API data:', data);

        try {
            const gameData = data.game || data;
            window.currentGameData = gameData;

            safeSetValue('editGameNumber', gameData.gameNumber || gameData.GameNumber);
            safeSetValue('editClientId', gameData.clientId || gameData.ClientId);

            safeSetSelect('editStatus', gameData.status || gameData.Status || 'Scheduled');
            safeSetSelect('editRunId', gameData.runId || gameData.RunId);
            safeSetSelect('editCourtId', gameData.courtId || gameData.CourtId);

            console.log('✅ Enhanced API data populated successfully');

            const activeTab = document.querySelector('#editGameTabs .nav-link.active');
            if (activeTab && activeTab.getAttribute('data-bs-target') === '#game-info-tab-pane') {
                updateGameInfoDisplayFromCurrentData();
            }
        } catch (error) {
            console.error('💥 Error populating from API data:', error);
        }
    }

    // ========== GAME INFO TAB ==========
    function updateGameInfoDisplayFromCurrentData() {
        console.log('📊 Updating game info display from current data');

        let gameData = window.currentGameData;

        if (!gameData) {
            gameData = {
                gameNumber: safeGetValue('editGameNumber'),
                status: safeGetValue('editStatus'),
                runId: safeGetValue('editRunId'),
                courtId: safeGetValue('editCourtId'),
                clientId: safeGetValue('editClientId')
            };
        }

        updateGameInfoDisplay(gameData);
    }

    function updateGameInfoDisplay(gameData) {
        console.log('📊 Updating game info display with data:', gameData);

        if (!gameData) {
            console.warn('⚠️ No game data available for info display');
            return;
        }

        const gameNumber = gameData.gameNumber || gameData.GameNumber || '--';
        const status = gameData.status || gameData.Status || 'Scheduled';
        const runId = gameData.runId || gameData.RunId || '--';
        const courtId = gameData.courtId || gameData.CourtId || '--';
        const clientId = gameData.clientId || gameData.ClientId || '--';

        const initials = getGameInitials(gameNumber);
        safeUpdateElement('gameInfoInitials', initials);
        safeUpdateElement('gameInfoNumber', gameNumber);
        safeUpdateElement('gameInfoStatus', status);
        safeUpdateElement('gameInfoRun', runId);
        safeUpdateElement('gameInfoCourt', courtId);
        safeUpdateElement('gameInfoClient', clientId);

        const statusBadge = document.getElementById('gameInfoStatus');
        if (statusBadge) {
            statusBadge.className = 'badge ' + getStatusBadgeClass(status);
        }

        console.log('✅ Game info display updated successfully');
    }

    // ========== PLAYERS TAB ==========
    function loadGamePlayers(gameId) {
        console.log('👥 Loading players for game:', gameId);

        const playersContainer = document.getElementById('gamePlayersList');
        if (!playersContainer) return;

        playersContainer.innerHTML = `
        <div class="text-center py-4 text-muted">
            ${window.UIUtils.createSpinner('sm')} Loading players...
        </div>`;

        // Check if API URL is configured
        if (!window.appUrls?.getGamePlayers) {
            displayGamePlayers([]);
            return;
        }

        fetch(`${window.appUrls.getGamePlayers}?gameId=${encodeURIComponent(gameId)}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                return response.json();
            })
            .then(data => {
                if (data.success && data.players) {
                    displayGamePlayers(data.players);
                } else {
                    displayGamePlayers([]);
                }
            })
            .catch(error => {
                console.error('🚨 Error loading players:', error);
                playersContainer.innerHTML = `
                <div class="text-center py-4 text-danger">
                    Error loading players. Please try again.
                </div>`;
            });
    }

    function displayGamePlayers(players) {
        console.log('👥 Displaying players:', players);
        const playersContainer = document.getElementById('gamePlayersList');
        if (!playersContainer) return;

        if (!players || players.length === 0) {
            playersContainer.innerHTML = `
            <div class="text-center py-4 text-muted">
                <i class="bi bi-people"></i>
                <p class="mb-0 mt-2">No players assigned to this game.</p>
            </div>`;
            return;
        }

        let html = '';
        players.forEach(player => {
            const playerName = player.name || player.firstName + ' ' + player.lastName || 'Unknown Player';
            const initials = getPlayerInitials(playerName);
            const position = player.position || 'Not specified';
            const number = player.number || player.playerNumber || '--';

            html += `
            <div class="d-flex align-items-center p-2 border-bottom">
                <div class="player-avatar me-3">
                    ${initials}
                </div>
                <div class="flex-grow-1">
                    <div class="fw-semibold">${playerName}</div>
                    <div class="text-muted small">
                        Position: ${position} | Number: ${number}
                    </div>
                </div>
                <div class="btn-group btn-group-sm">
                    <button type="button" class="btn btn-outline-danger remove-player-btn"
                            data-player-id="${player.id || player.profileId}"
                            title="Remove Player">
                        <i class="bi bi-person-x"></i>
                    </button>
                </div>
            </div>`;
        });

        playersContainer.innerHTML = html;

        // Add event listeners
        playersContainer.querySelectorAll('.remove-player-btn').forEach(button => {
            button.addEventListener('click', function () {
                const playerId = this.getAttribute('data-player-id');
                const gameId = safeGetValue('editGameId');
                removePlayerFromGame(gameId, playerId);
            });
        });
    }

    function removePlayerFromGame(gameId, playerId) {
        if (!confirm('Are you sure you want to remove this player from the game?')) return;

        console.log('🗑️ Removing player:', playerId, 'from game:', gameId);

        const token = getAntiForgeryToken();

        fetch('/Game/RemovePlayerFromGame', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({ gameId, profileId: playerId })
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    window.UIUtils.showSuccess('Player removed successfully');
                    loadGamePlayers(gameId);
                } else {
                    window.UIUtils.showError(`Error removing player: ${data.message || 'Unknown error'}`);
                }
            })
            .catch(error => {
                console.error('🚨 Error removing player:', error);
                window.UIUtils.showError('Error removing player. Please try again.');
            });
    }

    // ========== STATISTICS TAB ==========
    function loadGameStatistics(gameId) {
        console.log('📈 Loading statistics for game:', gameId);

        // Check if API URL is configured
        if (!window.appUrls?.getGameStatistics) {
            displayMockStatistics();
            return;
        }

        fetch(`${window.appUrls.getGameStatistics}?gameId=${encodeURIComponent(gameId)}`)
            .then(response => response.json())
            .then(data => {
                if (data.success && data.statistics) {
                    displayGameStatistics(data.statistics);
                } else {
                    displayMockStatistics();
                }
            })
            .catch(error => {
                console.error('🚨 Error loading statistics:', error);
                displayMockStatistics();
            });
    }

    function displayGameStatistics(stats) {
        safeUpdateElement('totalPlayers', stats.totalPlayers || 0);
        safeUpdateElement('gameDuration', stats.gameDuration || '0:00');
        safeUpdateElement('totalScore', stats.totalScore || 0);
        safeUpdateElement('gameRating', stats.gameRating || '--');
    }

    function displayMockStatistics() {
        // Generate mock statistics for demo
        const mockStats = {
            totalPlayers: Math.floor(Math.random() * 10) + 2,
            gameDuration: `${Math.floor(Math.random() * 90) + 30}:${Math.floor(Math.random() * 60).toString().padStart(2, '0')}`,
            totalScore: Math.floor(Math.random() * 200) + 50,
            gameRating: (Math.random() * 2 + 3).toFixed(1)
        };

        displayGameStatistics(mockStats);
    }

    // ========== FORM VALIDATION ==========
    function validateGameForm(form) {
        const errors = [];
        clearValidationErrors(form);

        const runId = form.querySelector('#addRunId, #editRunId');
        if (!runId || !runId.value.trim()) {
            errors.push('Run selection is required');
            if (runId) runId.classList.add('is-invalid');
        }

        const courtId = form.querySelector('#addCourtId, #editCourtId');
        if (!courtId || !courtId.value.trim()) {
            errors.push('Court selection is required');
            if (courtId) courtId.classList.add('is-invalid');
        }

        if (errors.length > 0) {
            window.UIUtils.showError(`Please fix the following errors:\n• ${errors.join('\n• ')}`);
            return false;
        }

        return true;
    }

    function clearValidationErrors(form) {
        const inputs = form.querySelectorAll('.is-invalid, .is-valid');
        inputs.forEach(input => {
            input.classList.remove('is-invalid', 'is-valid');
        });
    }

    // ========== DEPENDENT DATA LOADING ==========
    function loadAvailableRuns() {
        console.log('🏃 Loading available runs...');

        const runSelects = document.querySelectorAll('#addRunId, #editRunId');
        runSelects.forEach(select => {
            select.innerHTML = '<option value="">Loading runs...</option>';
        });

        // Mock data for runs - replace with actual API call
        setTimeout(() => {
            const mockRuns = [
                { id: 'run-1', name: 'Morning Pickup Game' },
                { id: 'run-2', name: 'Evening League' },
                { id: 'run-3', name: 'Weekend Tournament' }
            ];

            runSelects.forEach(select => {
                select.innerHTML = '<option value="">Select a run...</option>';
                mockRuns.forEach(run => {
                    const option = document.createElement('option');
                    option.value = run.id;
                    option.textContent = run.name;
                    select.appendChild(option);
                });
            });
        }, 500);
    }

    function loadCourtsForRun(runId, targetSelectId, selectedCourtId = null) {
        console.log('🏀 Loading courts for run:', runId);

        const courtSelect = document.getElementById(targetSelectId);
        if (!courtSelect) return;

        courtSelect.innerHTML = '<option value="">Loading courts...</option>';
        courtSelect.disabled = true;

        // Mock data for courts - replace with actual API call
        setTimeout(() => {
            const mockCourts = [
                { id: 'court-1', name: 'Main Court' },
                { id: 'court-2', name: 'Practice Court' },
                { id: 'court-3', name: 'Outdoor Court' }
            ];

            courtSelect.innerHTML = '<option value="">Select a court...</option>';
            mockCourts.forEach(court => {
                const option = document.createElement('option');
                option.value = court.id;
                option.textContent = court.name;
                if (selectedCourtId && court.id === selectedCourtId) {
                    option.selected = true;
                }
                courtSelect.appendChild(option);
            });

            courtSelect.disabled = false;
        }, 300);
    }

    function loadClientForRun(runId, targetFieldId) {
        console.log('🏢 Loading client for run:', runId);

        const clientField = document.getElementById(targetFieldId);
        if (!clientField) return;

        // Mock client data - replace with actual API call
        setTimeout(() => {
            clientField.value = 'client-123';
        }, 200);
    }

    // ========== UI STATE MANAGEMENT ==========
    function clearAllForms() {
        clearGameDetailsForm();
        clearGameInfoDisplay();
        clearPlayersDisplay();
        clearStatisticsDisplay();
    }

    function clearGameDetailsForm() {
        const fields = [
            'editGameNumber', 'editClientId'
        ];

        fields.forEach(field => safeSetValue(field, ''));

        const selects = ['editStatus', 'editRunId', 'editCourtId'];
        selects.forEach(select => {
            const element = document.getElementById(select);
            if (element) element.selectedIndex = 0;
        });

        console.log('🧹 Game details form cleared');
    }

    function clearGameInfoDisplay() {
        const elements = [
            'gameInfoInitials', 'gameInfoNumber', 'gameInfoStatus',
            'gameInfoRun', 'gameInfoCourt', 'gameInfoClient'
        ];

        elements.forEach(elementId => safeUpdateElement(elementId, '--'));
        console.log('🧹 Game info display cleared');
    }

    function clearPlayersDisplay() {
        const playersContainer = document.getElementById('gamePlayersList');
        if (playersContainer) {
            playersContainer.innerHTML = `
            <div class="text-center py-4 text-muted">
                <i class="bi bi-people"></i>
                <p class="mb-0 mt-2">Loading players...</p>
            </div>`;
        }
        console.log('🧹 Players display cleared');
    }

    function clearStatisticsDisplay() {
        const elements = ['totalPlayers', 'gameDuration', 'totalScore', 'gameRating'];
        elements.forEach(elementId => safeUpdateElement(elementId, '--'));
        console.log('🧹 Statistics display cleared');
    }

    function showLoadingState() {
        window.UIUtils.showModalLoading('editGameModal', 'Loading game data...');
        console.log('⏳ Loading state shown');
    }

    function hideLoadingState() {
        window.UIUtils.hideModalLoading('editGameModal');
        console.log('✅ Loading state hidden');
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

    function clearSelect(elementId) {
        const select = document.getElementById(elementId);
        if (select) {
            select.innerHTML = '<option value="">Select...</option>';
        }
    }

    function clearField(elementId) {
        const field = document.getElementById(elementId);
        if (field) {
            field.value = '';
        }
    }

    function findGameRowById(gameId) {
        if (!gameId) return null;

        console.log('🔍 Looking for row with game ID:', gameId);

        let row = document.querySelector(`tr[data-game-id="${gameId}"]`);
        if (row) return row;

        const button = document.querySelector(`button[data-game-id="${gameId}"]`);
        if (button) {
            row = button.closest('tr');
            if (row) return row;
        }

        const tableBody = document.querySelector('#gamesTable tbody');
        if (tableBody) {
            const allRows = tableBody.querySelectorAll('tr');
            for (const tr of allRows) {
                const editBtn = tr.querySelector(`[data-game-id="${gameId}"]`);
                if (editBtn) return tr;
            }
        }

        console.warn('⚠️ Row not found for game ID:', gameId);
        return null;
    }

    function getGameInitials(gameNumber) {
        if (!gameNumber) return 'G';
        if (gameNumber.toString().length >= 2) {
            return gameNumber.toString().substring(0, 2).toUpperCase();
        }
        return 'G' + gameNumber.toString()[0];
    }

    function getPlayerInitials(name) {
        if (!name) return 'P';
        const words = name.trim().split(' ');
        if (words.length >= 2) {
            return (words[0][0] + words[1][0]).toUpperCase();
        }
        return name[0].toUpperCase();
    }

    function getStatusBadgeClass(status) {
        switch (status?.toLowerCase()) {
            case 'scheduled': return 'bg-primary';
            case 'in progress': return 'bg-warning';
            case 'completed': return 'bg-success';
            case 'cancelled': return 'bg-danger';
            default: return 'bg-secondary';
        }
    }

    function getAntiForgeryToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        if (!tokenInput) {
            console.error('❌ Anti-forgery token not found');
            return null;
        }
        return tokenInput.value;
    }

    // ========== GLOBAL FUNCTIONS ==========
    window.startGame = function (gameId) {
        console.log('▶️ Starting game:', gameId);

        const token = getAntiForgeryToken();

        fetch('/Game/UpdateGameStatus', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({ gameId, status: 'In Progress' })
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    window.UIUtils.showSuccess('Game started successfully!');
                    setTimeout(() => location.reload(), 1000);
                } else {
                    window.UIUtils.showError(`Error starting game: ${data.message || 'Unknown error'}`);
                }
            })
            .catch(error => {
                console.error('🚨 Error starting game:', error);
                window.UIUtils.showError('Error starting game. Please try again.');
            });
    };

    // ========== GLOBAL API ==========
    window.gameDebug = {
        loadGameDataEnhanced,
        findGameRowById,
        populateFromTableData,
        populateFromAPIDataEnhanced,
        extractTableData,
        updateGameInfoDisplayFromCurrentData,
        updateGameInfoDisplay,
        loadGamePlayers,
        loadGameStatistics,
        clearAllForms,
        currentGameData: () => window.currentGameData,
        initializeFilters,
        fixEditButtons
    };

    console.log('✅ Complete Fixed Game Management loaded successfully');
    console.log('🐛 Debug functions available: window.gameDebug');
});