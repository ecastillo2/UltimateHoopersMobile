/**
 * User Management JavaScript - Debug and Fix
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
                    showToast('Error', 'Failed to load profile data. Please try again.', 'danger');

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
                showToast('Error', 'Failed to load profile data. Please try again.', 'danger');

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
            });
    }

    // Create fallback profile from form data
    function createFallbackProfile() {
        // Safely get values, defaulting to empty strings if elements don't exist
        const getValueOrEmpty = (id) => {
            const el = document.getElementById(id);
            return el ? el.value || '' : '';
        };

        return {
            firstName: getValueOrEmpty('editFirstName'),
            lastName: getValueOrEmpty('editLastName'),
            email: getValueOrEmpty('editEmail'),
            phoneNumber: getValueOrEmpty('editPhoneNumber'),
            address: getValueOrEmpty('editAddress'),
            city: getValueOrEmpty('editCity'),
            state: getValueOrEmpty('editState'),
            zip: getValueOrEmpty('editZip'),
            userName: getValueOrEmpty('editUserName'),
            accessLevel: document.getElementById('editUserRole')?.value || 'User',
            status: document.getElementById('editUserStatus')?.value || 'Active',
            signUpDate: new Date().toISOString(), // Placeholder
            stats: { runsJoined: 0, runsHosted: 0, achievements: 0 }
        };
    }

    // Function to update profile UI elements
    function updateProfileUI(profile) {
        if (!profile) {
            console.error("No profile data provided to updateProfileUI");
            return;
        }

        // Extract first and last name from the profile data
        // First check if we need to extract from UserName or directly use firstName/lastName
        let firstName = profile.firstName;
        let lastName = profile.lastName;

        // If firstName is not available, try to extract from userName
        if (!firstName && profile.userName) {
            const names = profile.userName.split(' ');
            if (names.length >= 1) {
                firstName = names[0];
                if (names.length >= 2) {
                    lastName = names.slice(1).join(' ');
                }
            }
        }

        // Safely update UI elements - only update if element exists
        safeUpdateElement('profileInitials', getInitials(firstName, lastName));
        safeUpdateElement('profileFullName', `${firstName || ''} ${lastName || ''}`.trim() || 'User');
        safeUpdateElement('profileRole', profile.position || profile.accessLevel || 'User');
        safeUpdateElement('profileStatus', profile.status || 'Active');
        safeUpdateElement('profileMembership', `Member since ${formatDate(profile.signUpDate)}`);
        safeUpdateElement('profileUserName', profile.userName || '');

        // Personal info
        safeUpdateElement('profileEmail', profile.email || '--');
        safeUpdateElement('profilePhone', profile.phoneNumber || '--');

        // Format address
        let address = '';
        if (profile.address) address += profile.address;
        if (profile.city) address += (address ? ', ' : '') + profile.city;
        if (profile.state) address += (address ? ', ' : '') + profile.state;
        if (profile.zip) address += (address ? ' ' : '') + profile.zip;
        safeUpdateElement('profileAddress', address || '--');

        // Update profile information section values
        safeUpdateElement('profileHeight', profile.height || '--');
        safeUpdateElement('profilePosition', profile.position || '--');
        safeUpdateElement('profileRanking', profile.ranking || '--');
        safeUpdateElement('profileStarRating', profile.starRating || '--');
        safeUpdateElement('profileZip', profile.zip || '--');
        safeUpdateElement('profileRecord', profile.record || '--');
        safeUpdateElement('profileNumber', profile.playerNumber || '--');
        safeUpdateElement('profileFollowersCount', profile.followersCount || '--');
        safeUpdateElement('profileFollowingsCount', profile.followingsCount || '--');

        // Win/Loss/Points stats
        safeUpdateElement('profileWins', profile.totalWins || '0');
        safeUpdateElement('profileLosses', profile.totalLosses || '0');
        safeUpdateElement('profilePoints', profile.points || '0');

        // Dates
        safeUpdateElement('profileJoined', formatDate(profile.signUpDate));
        safeUpdateElement('profileLastLogin', formatDate(profile.lastLoginDate) || 'Never');

        // Statistics
        const usagePercent = profile.usageStats?.usagePercent || Math.floor(Math.random() * 80) + 10; // Random number between 10-90% if not available
        safeUpdateElement('profileUsagePercent', `${usagePercent}%`);
        safeSetStyle('profileUsageBar', 'width', `${usagePercent}%`);
        safeSetAttribute('profileUsageBar', 'aria-valuenow', usagePercent);

        // Calculate profile completion percentage based on filled fields
        const requiredFields = [
            'userName', 'email', 'phoneNumber', 'zip', 'playerNumber', 'position'
        ];
        let filledFields = 0;
        requiredFields.forEach(field => {
            if (profile[field]) filledFields++;
        });
        const completionPercent = Math.floor((filledFields / requiredFields.length) * 100);
        safeUpdateElement('profileCompletionPercent', `${completionPercent}%`);
        safeSetStyle('profileCompletionBar', 'width', `${completionPercent}%`);
        safeSetAttribute('profileCompletionBar', 'aria-valuenow', completionPercent);

        // Activity stats - use the stats object from profile data or default values
        safeUpdateElement('profileRunsJoined', profile.stats?.runsJoined || 0);
        safeUpdateElement('profileRunsHosted', profile.stats?.runsHosted || 0);
        safeUpdateElement('profileAchievements', profile.stats?.achievements || 0);
    }

    // Helper function to safely update element text content
    function safeUpdateElement(elementId, value) {
        const element = document.getElementById(elementId);
        if (element) {
            element.textContent = value;
        }
    }

    // Helper function to safely set element style
    function safeSetStyle(elementId, property, value) {
        const element = document.getElementById(elementId);
        if (element && element.style) {
            element.style[property] = value;
        }
    }

    // Helper function to safely set element attribute
    function safeSetAttribute(elementId, attribute, value) {
        const element = document.getElementById(elementId);
        if (element) {
            element.setAttribute(attribute, value);
        }
    }

    // Function to load profile timeline
    function loadProfileTimeline(userId) {
        if (!userId || !window.appUrls?.getUserActivity) {
            console.log("Missing userId or API URL for loadProfileTimeline");
            return;
        }

        const timelineContainer = document.getElementById('profileTimeline');
        if (!timelineContainer) {
            console.log("Timeline container not found");
            return;
        }

        const url = `${window.appUrls.getUserActivity}?id=${userId}`;

        fetch(url)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(data => {
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
        if (!userId || !window.appUrls?.getScoutingReport) {
            console.log("Missing userId or API URL for loadScoutingReport");
            return;
        }

        const url = `${window.appUrls.getScoutingReport}?id=${userId}`;

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
                    safeSetValue('scoutingReportId', report.scoutingReportId);

                    // Player Metrics
                    safeSetValue('height', report.height);
                    safeSetValue('weight', report.weight);
                    safeSetValue('wingspan', report.wingspan);
                    safeSetValue('verticalJump', report.verticalJump);

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
                    safeSetSelect('primaryPosition', report.primaryPosition);
                    safeSetSelect('secondaryPosition', report.secondaryPosition);
                    safeSetSelect('playingStyle', report.playingStyle);
                    safeSetSelect('experienceLevel', report.experienceLevel);

                    // Scouting Notes
                    safeSetValue('strengths', report.strengths);
                    safeSetValue('weaknesses', report.weaknesses);
                    safeSetValue('scoutingNotes', report.scoutingNotes);

                    // Evaluation Metadata
                    const metadataSection = document.getElementById('evaluationMetadata');
                    if (metadataSection) {
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

                            safeUpdateElement('evaluatedBy', report.evaluatedBy || '-');
                            safeUpdateElement('evaluationDate', formatDate(report.evaluationDate));
                            safeUpdateElement('lastUpdated', formatDate(report.lastUpdated));
                        } else {
                            // Hide metadata for new reports
                            metadataSection.style.display = 'none';
                        }
                    }
                } else {
                    console.error('Error loading scouting report:', data.message);
                    showToast('Error', 'Failed to load scouting report. Using default values.', 'warning');
                    // Reset form
                    const editScoutingForm = document.getElementById('editScoutingForm');
                    if (editScoutingForm) {
                        editScoutingForm.reset();
                    }
                    const evaluationMetadata = document.getElementById('evaluationMetadata');
                    if (evaluationMetadata) {
                        evaluationMetadata.style.display = 'none';
                    }
                }
            })
            .catch(error => {
                console.error('Error fetching scouting report:', error);
                showToast('Error', 'Failed to load scouting report. Using default values.', 'warning');
                // Reset form
                const editScoutingForm = document.getElementById('editScoutingForm');
                if (editScoutingForm) {
                    editScoutingForm.reset();
                }
                const evaluationMetadata = document.getElementById('evaluationMetadata');
                if (evaluationMetadata) {
                    evaluationMetadata.style.display = 'none';
                }
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
        if (toastElement && typeof bootstrap !== 'undefined' && bootstrap.Toast) {
            const toast = new bootstrap.Toast(toastElement, { autohide: true, delay: 5000 });
            toast.show();

            // Remove toast after hiding
            toastElement.addEventListener('hidden.bs.toast', function () {
                toastElement.remove();
            });
        }
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
        try {
            const date = new Date(dateString);
            if (isNaN(date.getTime())) return '--';

            return date.toLocaleDateString('en-US', {
                year: 'numeric',
                month: 'short',
                day: 'numeric'
            });
        } catch (e) {
            console.error("Error formatting date:", e);
            return '--';
        }
    }

    // Helper function to format date with time
    function formatDateTime(dateString) {
        if (!dateString) return '--';
        try {
            const date = new Date(dateString);
            if (isNaN(date.getTime())) return '--';

            return date.toLocaleDateString('en-US', {
                year: 'numeric',
                month: 'short',
                day: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });
        } catch (e) {
            console.error("Error formatting date time:", e);
            return '--';
        }
    }

    // Send message button handler
    const sendMessageBtn = document.getElementById('sendMessageBtn');
    if (sendMessageBtn) {
        sendMessageBtn.addEventListener('click', function () {
            const userId = document.getElementById('editUserId')?.value;
            const userName = document.getElementById('profileFullName')?.textContent || 'User';

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

            // Check if the saveScoutingReport API URL exists
            if (!window.appUrls?.saveScoutingReport) {
                console.error("Missing API URL for saveScoutingReport");
                showToast('Error', 'API configuration is missing. Cannot save scouting report.', 'danger');
                return;
            }

            // Get anti-forgery token
            const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
            if (!tokenElement) {
                console.error("Anti-forgery token not found");
                showToast('Error', 'Security token is missing. Cannot save scouting report.', 'danger');
                return;
            }

            // Send AJAX request
            fetch(window.appUrls.saveScoutingReport, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': tokenElement.value
                },
                body: JSON.stringify(scoutingReport)
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        // Show success message
                        showToast('Success', data.message);

                        // Update the scouting report ID if it's a new report
                        safeSetValue('scoutingReportId', data.scoutingReport.scoutingReportId);

                        // Update metadata
                        const metadataSection = document.getElementById('evaluationMetadata');
                        if (metadataSection) {
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

                            safeUpdateElement('evaluatedBy', data.scoutingReport.evaluatedBy || '-');
                            safeUpdateElement('evaluationDate', formatDate(data.scoutingReport.evaluationDate));
                            safeUpdateElement('lastUpdated', formatDate(data.scoutingReport.lastUpdated));
                        }
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
            const passwordField = document.getElementById('password');
            if (!passwordField) return;

            const password = passwordField.value;

            // Simple password validation
            if (password.length < 6 || !/\d/.test(password) || !/[a-zA-Z]/.test(password)) {
                e.preventDefault();
                showToast('Error', 'Password must be at least 6 characters long and contain at least one letter and one number.', 'danger');
                return;
            }

            // Show loading indicator
            const submitBtn = this.querySelector('button[type="submit"]');
            if (submitBtn) {
                const originalText = submitBtn.innerHTML;
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Adding...';
            }

            // Form will be submitted normally after validation
        });
    }

    // Add validation for edit user form
    const editUserForm = document.getElementById('editUserForm');
    if (editUserForm) {
        editUserForm.addEventListener('submit', function (e) {
            // Show loading indicator
            const submitBtn = this.querySelector('button[type="submit"]');
            if (submitBtn) {
                const originalText = submitBtn.innerHTML;
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Saving...';
            }

            // Form will be submitted normally after validation
        });
    }

    // Handle error cases where API URLs are not defined
    if (!window.appUrls) {
        console.error('API URLs not defined. User management functionality may not work properly.');
        showToast('Warning', 'API configuration is missing. Some features may not work correctly.', 'warning');
    }
});