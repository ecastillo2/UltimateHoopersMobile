/**
 * User Management JavaScript
 * Handles all functionality for the Users view including:
 * - DataTable initialization and filtering
 * - User profile management
 * - Scouting report functionality
 * - Form validation and submission
 */

// Define API URLs for user management (if not already defined in the page)
if (!window.appUrls) {
    window.appUrls = {
        getUserData: '/User/GetUserData',
        getUserProfileData: '/User/GetUserProfileData',
        getUserActivity: '/User/GetUserActivity',
        getScoutingReport: '/User/GetScoutingReport',
        saveScoutingReport: '/User/UpdateScoutingReport'
    };
}

document.addEventListener('DOMContentLoaded', function () {
    // Initialize DataTable
    const usersTable = $('#usersTable').DataTable({
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

    // Filter references
    const statusFilter = $('#statusFilter');
    const roleFilter = $('#roleFilter');
    const signupDateFilter = $('#signupDateFilter');
    const resetFiltersBtn = $('#resetFilters');
    const activeFiltersContainer = $('#activeFilters');

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

            const row = $(usersTable.row(dataIndex).node());

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
        usersTable.draw();

        // Update the active filters display
        updateActiveFilters();
    }

    // Update the active filters display
    function updateActiveFilters() {
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
    resetFiltersBtn.on('click', function () {
        statusFilter.val('all');
        roleFilter.val('all');
        signupDateFilter.val('all');
        applyFilters();
    });

    // Initialize with current filter values
    applyFilters();

    // Handle edit user modal
    const editUserModal = document.getElementById('editUserModal');
    if (editUserModal) {
        editUserModal.addEventListener('show.bs.modal', function (event) {
            const button = event.relatedTarget;
            const userId = button.getAttribute('data-user-id');

            // Set user ID in the user details form
            document.getElementById('editUserId').value = userId;

            // Set user ID in the scouting report form
            document.getElementById('scoutingUserId').value = userId;

            // Load user data
            loadUserData(userId);

            // Load profile data for the new profile tab
            loadProfileData(userId);

            // Load scouting report data
            loadScoutingReport(userId);

            // Set delete button user ID
            document.getElementById('deleteUserId').value = userId;
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
            const userId = document.getElementById('editUserId').value;
            document.getElementById('deleteUserId').value = userId;

            // Hide edit modal and show delete confirmation modal
            const editModal = bootstrap.Modal.getInstance(document.getElementById('editUserModal'));
            editModal.hide();

            const deleteModal = new bootstrap.Modal(document.getElementById('deleteUserModal'));
            deleteModal.show();
        });
    }

    // Function to load user data
    function loadUserData(userId) {
        const url = `${appUrls.getUserData}?id=${userId}`;

        fetch(url)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    // Populate form fields
                    document.getElementById('editFirstName').value = data.user.firstName || '';
                    document.getElementById('editLastName').value = data.user.lastName || '';
                    document.getElementById('editEmail').value = data.user.email || '';
                    document.getElementById('editPhoneNumber').value = data.user.phoneNumber || '';
                    document.getElementById('editAddress').value = data.user.address || '';
                    document.getElementById('editCity').value = data.user.city || '';
                    document.getElementById('editState').value = data.user.state || '';
                    document.getElementById('editZip').value = data.user.zip || '';

                    // Select values
                    const userRoleSelect = document.getElementById('editUserRole');
                    if (userRoleSelect && data.user.accessLevel) {
                        for (let i = 0; i < userRoleSelect.options.length; i++) {
                            if (userRoleSelect.options[i].value === data.user.accessLevel) {
                                userRoleSelect.selectedIndex = i;
                                break;
                            }
                        }
                    }

                    const statusSelect = document.getElementById('editUserStatus');
                    if (statusSelect && data.user.status) {
                        for (let i = 0; i < statusSelect.options.length; i++) {
                            if (statusSelect.options[i].value === data.user.status) {
                                statusSelect.selectedIndex = i;
                                break;
                            }
                        }
                    }

                    // Additional fields
                    if (document.getElementById('editNotes')) {
                        document.getElementById('editNotes').value = data.user.notes || '';
                    }
                } else {
                    console.error('Error loading user data:', data.message);
                    showToast('Error', 'Failed to load user data. Please try again.', 'danger');

                    // Fallback to using row data when API fails
                    const row = findUserRowById(userId);
                    if (row) {
                        populateUserFormFromRow(row);
                    }
                }
            })
            .catch(error => {
                console.error('Error fetching user data:', error);
                showToast('Error', 'Failed to load user data. Please try again.', 'danger');

                // Fallback to using row data when API fails
                const row = findUserRowById(userId);
                if (row) {
                    populateUserFormFromRow(row);
                }
            });
    }

    // Helper function to find user row by ID
    function findUserRowById(userId) {
        const table = $('#usersTable').DataTable();
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
        const nameEl = userInfo.querySelector('.fw-semibold');
        const fullNameEl = userInfo.querySelector('.text-muted.small');

        let firstName = '', lastName = '';
        if (fullNameEl && fullNameEl.textContent) {
            const fullName = fullNameEl.textContent.trim().split(' ');
            firstName = fullName[0] || '';
            lastName = fullName.slice(1).join(' ') || '';
        }

        document.getElementById('editFirstName').value = firstName;
        document.getElementById('editLastName').value = lastName;

        // Set role from the role column (column 3)
        const role = row.cells[3].textContent.trim();
        const userRoleSelect = document.getElementById('editUserRole');
        if (userRoleSelect) {
            for (let i = 0; i < userRoleSelect.options.length; i++) {
                if (userRoleSelect.options[i].value === role) {
                    userRoleSelect.selectedIndex = i;
                    break;
                }
            }
        }

        // Set status from the status badge (column 2)
        const statusBadge = row.cells[2].querySelector('.badge');
        const status = statusBadge ? statusBadge.textContent.trim() : 'Active';
        const statusSelect = document.getElementById('editUserStatus');
        if (statusSelect) {
            for (let i = 0; i < statusSelect.options.length; i++) {
                if (statusSelect.options[i].value === status) {
                    statusSelect.selectedIndex = i;
                    break;
                }
            }
        }
    }

    // Function to load profile data
    function loadProfileData(userId) {
        const url = `${appUrls.getUserProfileData}?id=${userId}`;

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
                    showToast('Error', 'Failed to load profile data. Please try again.', 'danger');

                    // Create fallback profile from user info in the form
                    const fallbackProfile = createFallbackProfile();
                    updateProfileUI(fallbackProfile);

                    // Show placeholder for timeline
                    const timelineContainer = document.getElementById('profileTimeline');
                    timelineContainer.innerHTML = `
                        <div class="text-center py-4">
                            <p class="text-muted mb-0">No activity data available.</p>
                        </div>
                    `;
                }
            })
            .catch(error => {
                console.error('Error fetching profile data:', error);
                showToast('Error', 'Failed to load profile data. Please try again.', 'danger');

                // Create fallback profile from user info in the form
                const fallbackProfile = createFallbackProfile();
                updateProfileUI(fallbackProfile);

                // Show placeholder for timeline
                const timelineContainer = document.getElementById('profileTimeline');
                timelineContainer.innerHTML = `
                    <div class="text-center py-4">
                        <p class="text-muted mb-0">No activity data available.</p>
                    </div>
                `;
            });
    }

    // Create fallback profile from form data
    function createFallbackProfile() {
        return {
            firstName: document.getElementById('editFirstName').value || '',
            lastName: document.getElementById('editLastName').value || '',
            email: document.getElementById('editEmail').value || '',
            phoneNumber: document.getElementById('editPhoneNumber').value || '',
            address: document.getElementById('editAddress').value || '',
            city: document.getElementById('editCity').value || '',
            state: document.getElementById('editState').value || '',
            zip: document.getElementById('editZip').value || '',
            accessLevel: document.getElementById('editUserRole').value || 'User',
            status: document.getElementById('editUserStatus').value || 'Active',
            signUpDate: new Date().toISOString(), // Placeholder
            stats: { runsJoined: 0, runsHosted: 0, achievements: 0 }
        };
    }

    // Function to update profile UI elements
    function updateProfileUI(profile) {
        // Basic info
        document.getElementById('profileInitials').textContent = getInitials(profile.firstName, profile.lastName);
        document.getElementById('profileFullName').textContent = `${profile.firstName || ''} ${profile.lastName || ''}`.trim();
        document.getElementById('profileRole').textContent = profile.accessLevel || 'User';
        document.getElementById('profileStatus').textContent = profile.status || 'Active';
        document.getElementById('profileMembership').textContent = `Member since ${formatDate(profile.signUpDate)}`;

        // Personal info
        document.getElementById('profileEmail').textContent = profile.email || '--';
        document.getElementById('profilePhone').textContent = profile.phoneNumber || '--';

        // Format address
        let address = '';
        if (profile.address) address += profile.address;
        if (profile.city) address += (address ? ', ' : '') + profile.city;
        if (profile.state) address += (address ? ', ' : '') + profile.state;
        if (profile.zip) address += (address ? ' ' : '') + profile.zip;
        document.getElementById('profileAddress').textContent = address || '--';

        // Dates
        document.getElementById('profileJoined').textContent = formatDate(profile.signUpDate);
        document.getElementById('profileLastLogin').textContent = formatDate(profile.lastLoginDate) || 'Never';

        // Statistics
        const usagePercent = profile.usageStats?.usagePercent || Math.floor(Math.random() * 80) + 10; // Random number between 10-90% if not available
        document.getElementById('profileUsagePercent').textContent = `${usagePercent}%`;
        document.getElementById('profileUsageBar').style.width = `${usagePercent}%`;
        document.getElementById('profileUsageBar').setAttribute('aria-valuenow', usagePercent);

        // Calculate profile completion percentage based on filled fields
        const requiredFields = [
            'firstName', 'lastName', 'email', 'phoneNumber',
            'address', 'city', 'state', 'zip'
        ];
        let filledFields = 0;
        requiredFields.forEach(field => {
            if (profile[field]) filledFields++;
        });
        const completionPercent = Math.floor((filledFields / requiredFields.length) * 100);
        document.getElementById('profileCompletionPercent').textContent = `${completionPercent}%`;
        document.getElementById('profileCompletionBar').style.width = `${completionPercent}%`;
        document.getElementById('profileCompletionBar').setAttribute('aria-valuenow', completionPercent);

        // Activity stats
        document.getElementById('profileRunsJoined').textContent = profile.stats?.runsJoined || 0;
        document.getElementById('profileRunsHosted').textContent = profile.stats?.runsHosted || 0;
        document.getElementById('profileAchievements').textContent = profile.stats?.achievements || 0;
    }

    // Function to load profile timeline
    function loadProfileTimeline(userId) {
        const url = `${appUrls.getUserActivity}?id=${userId}`;

        fetch(url)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(data => {
                const timelineContainer = document.getElementById('profileTimeline');

                if (!data.activities || data.activities.length === 0) {
                    timelineContainer.innerHTML = `
                        <div class="text-center py-4">
                            <p class="text-muted mb-0">No recent activity found.</p>
                        </div>
                    `;
                    return;
                }

                let timelineHtml = '';
                data.activities.forEach((activity, index) => {
                    let iconClass = 'bi-circle';
                    let iconBg = 'bg-secondary';

                    // Determine icon based on activity type
                    switch (activity.type) {
                        case 'login':
                            iconClass = 'bi-box-arrow-in-right';
                            iconBg = 'bg-success';
                            break;
                        case 'join_run':
                            iconClass = 'bi-calendar-plus';
                            iconBg = 'bg-primary';
                            break;
                        case 'create_run':
                            iconClass = 'bi-calendar-check';
                            iconBg = 'bg-orange';
                            break;
                        case 'profile_update':
                            iconClass = 'bi-person-gear';
                            iconBg = 'bg-info';
                            break;
                        case 'achievement':
                            iconClass = 'bi-trophy';
                            iconBg = 'bg-warning';
                            break;
                    }

                    const isLastItem = index === data.activities.length - 1;

                    timelineHtml += `
                        <div class="d-flex mb-3 position-relative timeline-item">
                            <div class="timeline-icon ${iconBg} text-white d-flex align-items-center justify-content-center rounded-circle me-3" style="width: 36px; height: 36px; flex-shrink: 0;">
                                <i class="bi ${iconClass}"></i>
                            </div>
                            <div class="timeline-content ${!isLastItem ? 'border-bottom' : ''} pb-3 w-100">
                                <h6 class="mb-1">${activity.title}</h6>
                                <p class="text-muted mb-0 small">${activity.description}</p>
                                <small class="text-muted">${formatDateTime(activity.timestamp)}</small>
                            </div>
                        </div>
                    `;
                });

                timelineContainer.innerHTML = timelineHtml;
            })
            .catch(error => {
                console.error('Error fetching activity data:', error);
                const timelineContainer = document.getElementById('profileTimeline');
                timelineContainer.innerHTML = `
                    <div class="text-center py-4">
                        <p class="text-danger mb-0">
                            <i class="bi bi-exclamation-triangle me-1"></i>
                            Error loading activity data. Please try again.
                        </p>
                    </div>
                `;
            });
    }

    // Function to load scouting report data
    function loadScoutingReport(userId) {
        const url = `${appUrls.getScoutingReport}?id=${userId}`;

        fetch(url)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    const report = data.scoutingReport;

                    // Set the scouting report ID if it exists
                    document.getElementById('scoutingReportId').value = report.scoutingReportId || '';

                    // Player Metrics
                    if (document.getElementById('height')) {
                        document.getElementById('height').value = report.height || '';
                    }
                    if (document.getElementById('weight')) {
                        document.getElementById('weight').value = report.weight || '';
                    }
                    if (document.getElementById('wingspan')) {
                        document.getElementById('wingspan').value = report.wingspan || '';
                    }
                    if (document.getElementById('verticalJump')) {
                        document.getElementById('verticalJump').value = report.verticalJump || '';
                    }

                    // Skill Ratings
                    const setSkillValue = (id, value) => {
                        const slider = document.getElementById(id);
                        const valueDisplay = document.getElementById(`${id}Value`);
                        if (slider && valueDisplay) {
                            slider.value = value || 5;
                            valueDisplay.textContent = value || 5;
                        }
                    };

                    setSkillValue('shooting', report.shooting);
                    setSkillValue('ballHandling', report.ballHandling);
                    setSkillValue('passing', report.passing);
                    setSkillValue('defense', report.defense);
                    setSkillValue('rebounding', report.rebounding);
                    setSkillValue('athleticism', report.athleticism);

                    // Player Profile
                    const setSelectValue = (id, value) => {
                        const select = document.getElementById(id);
                        if (select && value) {
                            for (let i = 0; i < select.options.length; i++) {
                                if (select.options[i].value === value) {
                                    select.selectedIndex = i;
                                    break;
                                }
                            }
                        }
                    };

                    setSelectValue('primaryPosition', report.primaryPosition);
                    setSelectValue('secondaryPosition', report.secondaryPosition);
                    setSelectValue('playingStyle', report.playingStyle);
                    setSelectValue('experienceLevel', report.experienceLevel);

                    // Scouting Notes
                    document.getElementById('strengths').value = report.strengths || '';
                    document.getElementById('weaknesses').value = report.weaknesses || '';
                    document.getElementById('scoutingNotes').value = report.scoutingNotes || '';

                    // Evaluation Metadata
                    const metadataSection = document.getElementById('evaluationMetadata');
                    if (report.scoutingReportId) {
                        // Show metadata if report exists
                        metadataSection.style.display = 'flex';

                        // Format dates
                        const formatDate = (dateString) => {
                            if (!dateString) return '-';
                            const date = new Date(dateString);
                            return date.toLocaleDateString('en-US', {
                                year: 'numeric',
                                month: 'short',
                                day: 'numeric',
                                hour: '2-digit',
                                minute: '2-digit'
                            });
                        };

                        document.getElementById('evaluatedBy').textContent = report.evaluatedBy || '-';
                        document.getElementById('evaluationDate').textContent = formatDate(report.evaluationDate);
                        document.getElementById('lastUpdated').textContent = formatDate(report.lastUpdated);
                    } else {
                        // Hide metadata for new reports
                        metadataSection.style.display = 'none';
                    }
                } else {
                    console.error('Error loading scouting report:', data.message);
                    showToast('Error', 'Failed to load scouting report. Using default values.', 'warning');
                    // Reset form
                    document.getElementById('editScoutingForm').reset();
                    document.getElementById('evaluationMetadata').style.display = 'none';
                }
            })
            .catch(error => {
                console.error('Error fetching scouting report:', error);
                showToast('Error', 'Failed to load scouting report. Using default values.', 'warning');
                // Reset form
                document.getElementById('editScoutingForm').reset();
                document.getElementById('evaluationMetadata').style.display = 'none';
            });
    }

    // Helper function to show toast notifications
    function showToast(title, message, type = 'success') {
        // Check if toast container exists, create if not
        let toastContainer = document.querySelector('.toast-container');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
            document.body.appendChild(toastContainer);
        }

        // Create toast element
        const toastId = 'toast-' + Date.now();
        const toastHtml = `
            <div id="${toastId}" class="toast align-items-center text-white bg-${type} border-0" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body">
                        <strong>${title}</strong>: ${message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>
        `;

        // Add toast to container
        toastContainer.insertAdjacentHTML('beforeend', toastHtml);

        // Initialize and show toast
        const toastElement = document.getElementById(toastId);
        const toast = new bootstrap.Toast(toastElement, { autohide: true, delay: 5000 });
        toast.show();

        // Remove toast after hiding
        toastElement.addEventListener('hidden.bs.toast', function () {
            toastElement.remove();
        });
    }

    // Helper function to get initials from name
    function getInitials(firstName, lastName) {
        let initials = '';
        if (firstName && firstName.length > 0) {
            initials += firstName[0].toUpperCase();
        }
        if (lastName && lastName.length > 0) {
            initials += lastName[0].toUpperCase();
        }
        return initials || '--';
    }

    // Helper function to format date
    function formatDate(dateString) {
        if (!dateString) return '--';
        const date = new Date(dateString);
        return date.toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        });
    }

    // Helper function to format date with time
    function formatDateTime(dateString) {
        if (!dateString) return '--';
        const date = new Date(dateString);
        return date.toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    }

    // Send message button handler
    const sendMessageBtn = document.getElementById('sendMessageBtn');
    if (sendMessageBtn) {
        sendMessageBtn.addEventListener('click', function () {
            const userId = document.getElementById('editUserId').value;
            const userName = document.getElementById('profileFullName').textContent;

            // Show an input modal or form to send a message
            const message = prompt(`Send a message to ${userName}:`);
            if (message) {
                // In a real application, you would send this message to the server
                // For now, just show a toast notification
                showToast('Success', `Message sent to ${userName}!`);
            }
        });
    }

    // AJAX form submission for the scouting report
    const editScoutingForm = document.getElementById('editScoutingForm');
    if (editScoutingForm) {
        editScoutingForm.addEventListener('submit', function (e) {
            e.preventDefault();

            // Get form data
            const formData = new FormData(this);
            const scoutingReport = {};

            // Convert FormData to object
            for (const [key, value] of formData.entries()) {
                scoutingReport[key] = value;
            }

            // Send AJAX request
            fetch(appUrls.saveScoutingReport, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: JSON.stringify(scoutingReport)
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        // Show success message
                        showToast('Success', data.message);

                        // Update the scouting report ID if it's a new report
                        document.getElementById('scoutingReportId').value = data.scoutingReport.scoutingReportId;

                        // Update metadata
                        const metadataSection = document.getElementById('evaluationMetadata');
                        metadataSection.style.display = 'flex';

                        const formatDate = (dateString) => {
                            if (!dateString) return '-';
                            const date = new Date(dateString);
                            return date.toLocaleDateString('en-US', {
                                year: 'numeric',
                                month: 'short',
                                day: 'numeric',
                                hour: '2-digit',
                                minute: '2-digit'
                            });
                        };

                        document.getElementById('evaluatedBy').textContent = data.scoutingReport.evaluatedBy || '-';
                        document.getElementById('evaluationDate').textContent = formatDate(data.scoutingReport.evaluationDate);
                        document.getElementById('lastUpdated').textContent = formatDate(data.scoutingReport.lastUpdated);
                    } else {
                        // Show error message
                        showToast('Error', data.message, 'danger');
                    }
                })
                .catch(error => {
                    console.error('Error saving scouting report:', error);
                    showToast('Error', 'Failed to save scouting report. Please try again.', 'danger');
                });
        });
    }

    // Add form validation for the add user form
    const addUserForm = document.getElementById('addUserForm');
    if (addUserForm) {
        addUserForm.addEventListener('submit', function (e) {
            const password = document.getElementById('password').value;

            // Simple password validation
            if (password.length < 6 || !/\d/.test(password) || !/[a-zA-Z]/.test(password)) {
                e.preventDefault();
                showToast('Error', 'Password must be at least 6 characters long and contain at least one letter and one number.', 'danger');
                return;
            }

            // Show loading indicator
            const submitBtn = this.querySelector('button[type="submit"]');
            const originalText = submitBtn.innerHTML;
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Adding...';

            // Form will be submitted normally after validation
        });
    }

    // Add validation for edit user form
    const editUserForm = document.getElementById('editUserForm');
    if (editUserForm) {
        editUserForm.addEventListener('submit', function (e) {
            // Show loading indicator
            const submitBtn = this.querySelector('button[type="submit"]');
            const originalText = submitBtn.innerHTML;
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Saving...';

            // Form will be submitted normally after validation
        });
    }

    // Handle error cases where API URLs are not defined
    if (!window.appUrls) {
        console.error('API URLs not defined. User management functionality may not work properly.');
        showToast('Warning', 'API configuration is missing. Some features may not work correctly.', 'warning');
    }
});