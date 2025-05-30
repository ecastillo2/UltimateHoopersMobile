/**
 * User Management JavaScript - Updated to use UIUtils
 * 
 * This file focuses on fixing the "TypeError: document.getElementById(...) is null" errors
 * by ensuring all DOM element references are properly checked before use.
 */

document.addEventListener('DOMContentLoaded', function () {
    // Initialize DataTable if the table exists
    const usersTable = $('#usersTable');
    if (usersTable.length > 0) {
        usersTable.DataTable({
            responsive: true,
            lengthMenu: [[10, 25, 50, -1], [10, 25, 50, "All"]],
            dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>rtip',
            language: {
                search: "_INPUT_",
                searchPlaceholder: "Search users...",
                lengthMenu: "Show _MENU_ users per page",
                info: "Showing _START_ to _END_ of _TOTAL_ users",
                infoEmpty: "Showing 0 to 0 of 0 users",
                infoFiltered: "(filtered from _MAX_ total users)"
            },
            columnDefs: [
                { className: "align-middle", targets: "_all" },
                { orderable: false, targets: [4] } // Disable sorting on the Actions column
            ],
            order: [[1, 'desc']] // Sort by Sign Up Date column by default (newest first)
        });

        // Filter references - only initialize if elements exist
        const statusFilter = $('#statusFilter');
        const roleFilter = $('#roleFilter');
        const signupDateFilter = $('#signupDateFilter');
        const resetFiltersBtn = $('#resetFilters');
        const activeFiltersContainer = $('#activeFilters');

        if (statusFilter.length && roleFilter.length && signupDateFilter.length) {
            setupFilters(usersTable.DataTable(), statusFilter, roleFilter, signupDateFilter, resetFiltersBtn, activeFiltersContainer);
        }
    }

    // Function to set up filters
    function setupFilters(table, statusFilter, roleFilter, signupDateFilter, resetFiltersBtn, activeFiltersContainer) {
        // Apply filter function
        function applyFilters() {
            // Remove any existing custom filter to prevent stacking
            if ($.fn.dataTable.ext.search.length > 0) {
                $.fn.dataTable.ext.search.pop();
            }

            // Create a new custom filter function
            $.fn.dataTable.ext.search.push(function (settings, data, dataIndex) {
                // Only apply this filter to our usersTable
                if (settings.nTable.id !== 'usersTable') return true;

                // Skip filtering if all filters are set to 'all'
                if (statusFilter.val() === 'all' &&
                    roleFilter.val() === 'all' &&
                    signupDateFilter.val() === 'all') {
                    return true;
                }

                const row = $(table.row(dataIndex).node());

                // Status filtering (column 2 contains the status badge)
                if (statusFilter.val() !== 'all') {
                    const status = data[2].toLowerCase();
                    const statusValue = statusFilter.val().toLowerCase();

                    if (!status.includes(statusValue)) {
                        return false;
                    }
                }

                // Role filtering (column 3 contains the role)
                if (roleFilter.val() !== 'all') {
                    const role = row.data('role') || data[3].toLowerCase();
                    const roleValue = roleFilter.val().toLowerCase();

                    if (role !== roleValue) {
                        return false;
                    }
                }

                // Signup date filtering (column 1 contains the date)
                if (signupDateFilter.val() !== 'all') {
                    const dateString = row.data('date') || data[1];
                    let date;

                    try {
                        // Try to parse the date from the data attribute first
                        date = new Date(dateString);
                        if (isNaN(date.getTime())) {
                            // If invalid, try to parse from the displayed date format (MM/dd/yyyy)
                            const parts = data[1].split('/');
                            if (parts.length === 3) {
                                date = new Date(parts[2], parts[0] - 1, parts[1]);
                            } else {
                                return false; // Invalid date format
                            }
                        }
                    } catch (e) {
                        console.error("Error parsing date:", e);
                        return false;
                    }

                    const today = new Date();
                    today.setHours(0, 0, 0, 0); // Set to start of today

                    const startOfWeek = new Date(today);
                    startOfWeek.setDate(today.getDate() - today.getDay()); // Sunday

                    const startOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);
                    const startOfYear = new Date(today.getFullYear(), 0, 1);

                    switch (signupDateFilter.val()) {
                        case 'today':
                            if (date < today || date >= new Date(today.getTime() + 86400000)) {
                                return false;
                            }
                            break;
                        case 'this-week':
                            if (date < startOfWeek) {
                                return false;
                            }
                            break;
                        case 'this-month':
                            if (date < startOfMonth) {
                                return false;
                            }
                            break;
                        case 'this-year':
                            if (date < startOfYear) {
                                return false;
                            }
                            break;
                    }
                }

                // If we got here, the row passes all filters
                return true;
            });

            // Redraw the table to apply filters
            table.draw();

            // Update the active filters display
            updateActiveFilters();
        }

        // Update the active filters display
        function updateActiveFilters() {
            if (!activeFiltersContainer.length) return;

            // Clear the current active filters display (except the label)
            activeFiltersContainer.find('.filter-badge, .filter-none').remove();

            // Check if any filters are active
            const hasActiveFilters =
                statusFilter.val() !== 'all' ||
                roleFilter.val() !== 'all' ||
                signupDateFilter.val() !== 'all';

            // If no filters are active, show "None"
            if (!hasActiveFilters) {
                activeFiltersContainer.append(
                    $('<span>').addClass('text-muted filter-none').text('None')
                );
                return;
            }

            // Add badges for active filters
            if (statusFilter.val() !== 'all') {
                addFilterBadge('Status', formatFilterValue(statusFilter.val()), function () {
                    statusFilter.val('all');
                    applyFilters();
                });
            }

            if (roleFilter.val() !== 'all') {
                addFilterBadge('Role', formatFilterValue(roleFilter.val()), function () {
                    roleFilter.val('all');
                    applyFilters();
                });
            }

            if (signupDateFilter.val() !== 'all') {
                addFilterBadge('Signup Date', formatFilterValue(signupDateFilter.val()), function () {
                    signupDateFilter.val('all');
                    applyFilters();
                });
            }
        }

        // Helper function to format filter values for display
        function formatFilterValue(value) {
            return value
                .split('-')
                .map(word => word.charAt(0).toUpperCase() + word.slice(1))
                .join(' ');
        }

        // Add a filter badge to the display
        function addFilterBadge(label, value, removeCallback) {
            const badge = $('<span>')
                .addClass('badge bg-primary me-2 filter-badge')
                .text(`${label}: ${value}`);

            const removeBtn = $('<button>')
                .addClass('btn-close btn-close-white ms-1')
                .css('font-size', '0.5rem')
                .on('click', removeCallback);

            badge.append(removeBtn);
            activeFiltersContainer.append(badge);
        }

        // Add event listeners to filters
        statusFilter.on('change', applyFilters);
        roleFilter.on('change', applyFilters);
        signupDateFilter.on('change', applyFilters);

        // Reset filters button
        if (resetFiltersBtn.length) {
            resetFiltersBtn.on('click', function () {
                statusFilter.val('all');
                roleFilter.val('all');
                signupDateFilter.val('all');
                applyFilters();
            });
        }

        // Initialize with current filter values
        applyFilters();
    }

    // Handle edit user modal
    const editUserModal = document.getElementById('editUserModal');
    if (editUserModal) {
        editUserModal.addEventListener('show.bs.modal', function (event) {
            const button = event.relatedTarget;
            const userId = button.getAttribute('data-user-id');

            // Set user ID in the user details form - check if elements exist
            const editUserIdField = document.getElementById('editUserId');
            if (editUserIdField) {
                editUserIdField.value = userId;
            }

            // Set user ID in the scouting report form - check if element exists
            const scoutingUserIdField = document.getElementById('scoutingUserId');
            if (scoutingUserIdField) {
                scoutingUserIdField.value = userId;
            }

            // Load user data
            loadUserData(userId);

            // Load profile data for the new profile tab
            loadProfileData(userId);

            // Load scouting report data
            loadScoutingReport(userId);

            // Set delete button user ID - check if element exists
            const deleteUserIdField = document.getElementById('deleteUserId');
            if (deleteUserIdField) {
                deleteUserIdField.value = userId;
            }
        });
    }

    // Set up skill rating sliders
    const skillSliders = document.querySelectorAll('.form-range');
    skillSliders.forEach(slider => {
        slider.addEventListener('input', function () {
            const valueDisplay = document.getElementById(`${this.id}Value`);
            if (valueDisplay) {
                valueDisplay.textContent = this.value;
            }
        });
    });

    // Handle delete user button
    const deleteUserBtn = document.getElementById('deleteUserBtn');
    if (deleteUserBtn) {
        deleteUserBtn.addEventListener('click', function () {
            const userId = document.getElementById('editUserId')?.value;
            if (!userId) return;

            const deleteUserIdField = document.getElementById('deleteUserId');
            if (deleteUserIdField) {
                deleteUserIdField.value = userId;
            }

            // Hide edit modal and show delete confirmation modal
            const editModal = bootstrap.Modal.getInstance(document.getElementById('editUserModal'));
            if (editModal) {
                editModal.hide();
            }

            const deleteModalEl = document.getElementById('deleteUserModal');
            if (deleteModalEl) {
                const deleteModal = new bootstrap.Modal(deleteModalEl);
                deleteModal.show();
            }
        });
    }

    // Function to load user data
    function loadUserData(userId) {
        if (!userId || !window.appUrls?.getUserData) {
            console.error("Missing userId or API URL for loadUserData");
            return;
        }

        const url = `${window.appUrls.getUserData}?id=${userId}`;

        fetch(url)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    // Populate form fields - check if each element exists before setting values
                    safeSetValue('editFirstName', data.user.firstName);
                    safeSetValue('editLastName', data.user.lastName);
                    safeSetValue('editEmail', data.user.email);
                    safeSetValue('editPhoneNumber', data.user.phoneNumber);
                    safeSetValue('editAddress', data.user.address);
                    safeSetValue('editCity', data.user.city);
                    safeSetValue('editState', data.user.state);
                    safeSetValue('editZip', data.user.zip);
                    safeSetValue('editUserName', data.user.userName);

                    // Select values - check if elements exist
                    safeSetSelect('editUserRole', data.user.accessLevel);
                    safeSetSelect('editUserStatus', data.user.status || 'Active');

                    // Additional fields
                    safeSetValue('editNotes', data.user.notes);
                } else {
                    console.error('Error loading user data:', data.message);
                    UIUtils.showError('Failed to load user data. Please try again.', 'Error');

                    // Fallback to using row data when API fails
                    const row = findUserRowById(userId);
                    if (row) {
                        populateUserFormFromRow(row);
                    }
                }
            })
            .catch(error => {
                console.error('Error fetching user data:', error);
                UIUtils.showError('Failed to load user data. Please try again.', 'Error');

                // Fallback to using row data when API fails
                const row = findUserRowById(userId);
                if (row) {
                    populateUserFormFromRow(row);
                }
            });
    }

    // Helper function to safely set input value if element exists
    function safeSetValue(elementId, value) {
        const element = document.getElementById(elementId);
        if (element) {
            element.value = value || '';
        }
    }

    // Helper function to safely set select option if element exists
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

    // Helper function to find user row by ID
    function findUserRowById(userId) {
        if (!userId) return null;

        const table = $('#usersTable').DataTable();
        if (!table) return null;

        const rows = table.rows().nodes();

        for (let i = 0; i < rows.length; i++) {
            const editButton = rows[i].querySelector(`button[data-user-id="${userId}"]`);
            if (editButton) {
                return rows[i];
            }
        }

        return null;
    }

    // Helper function to populate user form from table row data
    function populateUserFormFromRow(row) {
        if (!row) return;

        const userInfo = row.querySelector('.d-flex.align-items-center');
        if (!userInfo) return;

        const nameEl = userInfo.querySelector('.fw-semibold');
        const fullNameEl = userInfo.querySelector('.text-muted.small');

        let firstName = '', lastName = '';
        if (fullNameEl && fullNameEl.textContent) {
            const fullName = fullNameEl.textContent.trim().split(' ');
            firstName = fullName[0] || '';
            lastName = fullName.slice(1).join(' ') || '';
        }

        safeSetValue('editFirstName', firstName);
        safeSetValue('editLastName', lastName);

        // Set role from the role column (column 3)
        if (row.cells && row.cells.length > 3) {
            const role = row.cells[3].textContent.trim();
            safeSetSelect('editUserRole', role);
        }

        // Set status from the status badge (column 2)
        if (row.cells && row.cells.length > 2) {
            const statusBadge = row.cells[2].querySelector('.badge');
            const status = statusBadge ? statusBadge.textContent.trim() : 'Active';
            safeSetSelect('editUserStatus', status);
        }
    }

    // Function to load profile data
    function loadProfileData(userId) {
        if (!userId || !window.appUrls?.getUserProfileData) {
            console.log("Missing userId or API URL for loadProfileData");
            return;
        }

        const url = `${window.appUrls.getUserProfileData}?id=${userId}`;

        fetch(url)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    updateProfileUI(data.profile);
                    loadProfileTimeline(userId);
                } else {
                    console.error('Error loading profile data:', data.message);
                    UIUtils.showError('Failed to load profile data. Please try again.', 'Error');

                    // Create fallback profile from user info in the form
                    const fallbackProfile = createFallbackProfile();
                    updateProfileUI(fallbackProfile);

                    // Show placeholder for timeline
                    const timelineContainer = document.getElementById('profileTimeline');
                    if (timelineContainer) {
                        timelineContainer.innerHTML = `
                            <div class="text-center py-4">
                                <p class="text-muted mb-0">No activity data available.</p>
                            </div>
                        `;
                    }
                }
            })
            .catch(error => {
                console.error('Error fetching profile data:', error);
                UIUtils.showError('Failed to load profile data. Please try again.', 'Error');

                // Create fallback profile from user info in the form
                const fallbackProfile = createFallbackProfile();
                updateProfileUI(fallbackProfile);

                // Show placeholder for timeline
                const timelineContainer = document.getElementById('profileTimeline');
                if (timelineContainer) {
                    timelineContainer.innerHTML = `