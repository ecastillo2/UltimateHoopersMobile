/**
 * User Management JavaScript - Complete Fixed Version
 */

document.addEventListener('DOMContentLoaded', function () {
    console.log('Initializing User Management');

    // Initialize DataTable if the table exists
    const usersTable = $('#usersTable');
    if (usersTable.length > 0) {
        initializeUsersTable();
    }

    // Initialize modals and event handlers
    initializeModalHandlers();
    initializeFormHandlers();

    console.log('User Management initialized successfully');

    // ========== TABLE INITIALIZATION ==========
    function initializeUsersTable() {
        const table = usersTable.DataTable({
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
        const roleFilter = $('#roleFilter');
        const signupDateFilter = $('#signupDateFilter');
        const resetFiltersBtn = $('#resetFilters');
        const activeFiltersContainer = $('#activeFilters');

        if (!statusFilter.length || !roleFilter.length || !signupDateFilter.length) {
            console.warn('Filter elements not found');
            return;
        }

        function applyFilters() {
            // Remove existing custom filters
            $.fn.dataTable.ext.search = $.fn.dataTable.ext.search.filter(fn =>
                !fn.name || fn.name !== 'userTableFilter'
            );

            // Add new filter
            const customFilter = function (settings, data, dataIndex) {
                if (settings.nTable.id !== 'usersTable') return true;

                // Skip if all filters are 'all'
                if (statusFilter.val() === 'all' &&
                    roleFilter.val() === 'all' &&
                    signupDateFilter.val() === 'all') {
                    return true;
                }

                const row = $(table.row(dataIndex).node());

                // Status filtering
                if (statusFilter.val() !== 'all') {
                    const status = data[2].toLowerCase();
                    if (!status.includes(statusFilter.val().toLowerCase())) {
                        return false;
                    }
                }

                // Role filtering
                if (roleFilter.val() !== 'all') {
                    const role = (row.data('role') || data[3]).toLowerCase();
                    if (role !== roleFilter.val().toLowerCase()) {
                        return false;
                    }
                }

                // Date filtering
                if (signupDateFilter.val() !== 'all') {
                    if (!filterByDate(row, data[1], signupDateFilter.val())) {
                        return false;
                    }
                }

                return true;
            };

            customFilter.name = 'userTableFilter';
            $.fn.dataTable.ext.search.push(customFilter);
            table.draw();
            updateActiveFilters();
        }

        function filterByDate(row, dateText, filterValue) {
            try {
                const dateString = row.data('date') || dateText;
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
                    case 'this-year':
                        const startOfYear = new Date(today.getFullYear(), 0, 1);
                        return date >= startOfYear;
                    default:
                        return true;
                }
            } catch (e) {
                console.error("Date filtering error:", e);
                return false;
            }
        }

        function updateActiveFilters() {
            if (!activeFiltersContainer.length) return;

            activeFiltersContainer.find('.filter-badge, .filter-none').remove();

            const hasActiveFilters = statusFilter.val() !== 'all' ||
                roleFilter.val() !== 'all' ||
                signupDateFilter.val() !== 'all';

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

            if (roleFilter.val() !== 'all') {
                addFilterBadge('Role', formatFilterValue(roleFilter.val()), () => {
                    roleFilter.val('all');
                    applyFilters();
                });
            }

            if (signupDateFilter.val() !== 'all') {
                addFilterBadge('Signup Date', formatFilterValue(signupDateFilter.val()), () => {
                    signupDateFilter.val('all');
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
        roleFilter.on('change', applyFilters);
        signupDateFilter.on('change', applyFilters);

        if (resetFiltersBtn.length) {
            resetFiltersBtn.on('click', function () {
                statusFilter.val('all');
                roleFilter.val('all');
                signupDateFilter.val('all');
                applyFilters();
            });
        }

        applyFilters();
    }

    // ========== MODAL HANDLERS ==========
    function initializeModalHandlers() {
        const editUserModal = document.getElementById('editUserModal');
        if (editUserModal) {
            editUserModal.addEventListener('show.bs.modal', handleEditModalShow);
        }

        const deleteUserBtn = document.getElementById('deleteUserBtn');
        if (deleteUserBtn) {
            deleteUserBtn.addEventListener('click', handleDeleteUser);
        }
    }

    function handleEditModalShow(event) {
        const button = event.relatedTarget;
        const userId = button.getAttribute('data-user-id');

        // Set user IDs in forms
        safeSetValue('editUserId', userId);
        safeSetValue('scoutingUserId', userId);
        safeSetValue('deleteUserId', userId);

        // Load data
        loadUserData(userId);
        loadProfileData(userId);
        loadScoutingReport(userId);
    }

    function handleDeleteUser() {
        const userId = safeGetValue('editUserId');
        if (!userId) return;

        safeSetValue('deleteUserId', userId);

        const editModal = bootstrap.Modal.getInstance(document.getElementById('editUserModal'));
        if (editModal) editModal.hide();

        const deleteModalEl = document.getElementById('deleteUserModal');
        if (deleteModalEl) {
            const deleteModal = new bootstrap.Modal(deleteModalEl);
            deleteModal.show();
        }
    }

    // ========== FORM HANDLERS ==========
    function initializeFormHandlers() {
        // Skill rating sliders
        document.querySelectorAll('.form-range').forEach(slider => {
            slider.addEventListener('input', function () {
                const valueDisplay = document.getElementById(this.id + 'Value');
                if (valueDisplay) {
                    valueDisplay.textContent = this.value;
                }
            });
        });

        // Scouting form submission
        const scoutingForm = document.getElementById('editScoutingForm');
        if (scoutingForm) {
            scoutingForm.addEventListener('submit', handleScoutingFormSubmit);
        }
    }

    function handleScoutingFormSubmit(e) {
        e.preventDefault();

        const formData = new FormData(e.target);
        const scoutingData = {};

        for (const [key, value] of formData.entries()) {
            if (['Shooting', 'BallHandling', 'Passing', 'Defense', 'Rebounding', 'Athleticism'].includes(key)) {
                scoutingData[key] = parseInt(value);
            } else {
                scoutingData[key] = value;
            }
        }

        submitScoutingReport(scoutingData);
    }

    // ========== DATA LOADING FUNCTIONS ==========
    function loadUserData(userId) {
        if (!userId || !window.appUrls?.getUserData) {
            console.error("Missing userId or API URL for loadUserData");
            return;
        }

        fetch(window.appUrls.getUserData + '?id=' + userId)
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    populateUserForm(data.user);
                } else {
                    showError('Failed to load user data: ' + (data.message || 'Unknown error'));
                    fallbackToRowData(userId);
                }
            })
            .catch(error => {
                console.error('Error fetching user data:', error);
                showError('Failed to load user data');
                fallbackToRowData(userId);
            });
    }

    function populateUserForm(user) {
        safeSetValue('editFirstName', user.firstName);
        safeSetValue('editLastName', user.lastName);
        safeSetValue('editEmail', user.email);
        safeSetValue('editPhoneNumber', user.phoneNumber);
        safeSetValue('editAddress', user.address);
        safeSetValue('editCity', user.city);
        safeSetValue('editState', user.state);
        safeSetValue('editZip', user.zip);
        safeSetValue('editUserName', user.userName);
        safeSetValue('editNotes', user.notes);

        safeSetSelect('editUserRole', user.accessLevel);
        safeSetSelect('editUserStatus', user.status || 'Active');
    }

    function fallbackToRowData(userId) {
        const row = findUserRowById(userId);
        if (row) {
            populateUserFormFromRow(row);
        }
    }

    function populateUserFormFromRow(row) {
        if (!row) return;

        const userInfo = row.querySelector('.d-flex.align-items-center');
        if (!userInfo) return;

        const fullNameEl = userInfo.querySelector('.text-muted.small');
        if (fullNameEl && fullNameEl.textContent) {
            const fullName = fullNameEl.textContent.trim().split(' ');
            safeSetValue('editFirstName', fullName[0] || '');
            safeSetValue('editLastName', fullName.slice(1).join(' ') || '');
        }

        if (row.cells && row.cells.length > 3) {
            const role = row.cells[3].textContent.trim();
            safeSetSelect('editUserRole', role);
        }

        if (row.cells && row.cells.length > 2) {
            const statusBadge = row.cells[2].querySelector('.badge');
            const status = statusBadge ? statusBadge.textContent.trim() : 'Active';
            safeSetSelect('editUserStatus', status);
        }
    }

    function findUserRowById(userId) {
        if (!userId) return null;

        const table = $('#usersTable').DataTable();
        if (!table) return null;

        const rows = table.rows().nodes();
        for (let i = 0; i < rows.length; i++) {
            const editButton = rows[i].querySelector('[data-user-id="' + userId + '"]');
            if (editButton) {
                return rows[i];
            }
        }
        return null;
    }

    function loadProfileData(userId) {
        if (!userId || !window.appUrls?.getUserProfileData) {
            console.log("Missing userId or API URL for loadProfileData");
            updateProfileUI(createFallbackProfile());
            return;
        }

        fetch(window.appUrls.getUserProfileData + '?id=' + userId)
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    updateProfileUI(data.profile);
                    loadProfileTimeline(userId);
                } else {
                    console.error('Error loading profile data:', data.message);
                    updateProfileUI(createFallbackProfile());
                    showPlaceholderTimeline();
                }
            })
            .catch(error => {
                console.error('Error fetching profile data:', error);
                updateProfileUI(createFallbackProfile());
                showPlaceholderTimeline();
            });
    }

    function createFallbackProfile() {
        const firstName = safeGetValue('editFirstName') || 'User';
        const lastName = safeGetValue('editLastName') || '';
        const fullName = (firstName + ' ' + lastName).trim();

        return {
            userName: fullName,
            position: 'Not specified',
            ranking: '#--',
            starRating: '--',
            profileImage: null,
            playerNumber: '#--',
            zip: '--',
            height: '--',
            status: 'Active',
            record: '--',
            followersCount: '0',
            followingsCount: '0',
            city: '--',
            stats: { runsJoined: 0, runsHosted: 0, achievements: 0 }
        };
    }

    function updateProfileUI(profile) {
        const initials = getInitials(profile.userName);
        safeUpdateElement('profileInitials', initials);
        safeUpdateElement('profileUserName', profile.userName);
        safeUpdateElement('profileNumber', profile.playerNumber);
        safeUpdateElement('profileStatus', profile.status);
        safeUpdateElement('profileMembership', 'Active Member');
        safeUpdateElement('profileHeight', profile.height);
        safeUpdateElement('profilePosition', profile.position);
        safeUpdateElement('profileRanking', profile.ranking);
        safeUpdateElement('profileStarRating', profile.starRating);
        safeUpdateElement('profileRecord', profile.record);
        safeUpdateElement('profileFollowersCount', profile.followersCount);
        safeUpdateElement('profileFollowingsCount', profile.followingsCount);
        safeUpdateElement('profileZip', profile.zip);

        if (profile.stats) {
            safeUpdateElement('profileRunsJoined', profile.stats.runsJoined);
            safeUpdateElement('profileRunsHosted', profile.stats.runsHosted);
            safeUpdateElement('profileAchievements', profile.stats.achievements);
        }
    }

    function loadProfileTimeline(userId) {
        if (!userId || !window.appUrls?.getUserActivity) {
            showPlaceholderTimeline();
            return;
        }

        fetch(window.appUrls.getUserActivity + '?id=' + userId)
            .then(response => response.json())
            .then(data => {
                displayProfileTimeline(data.activities || []);
            })
            .catch(error => {
                console.error('Error loading profile timeline:', error);
                showPlaceholderTimeline();
            });
    }

    function showPlaceholderTimeline() {
        const timelineContainer = document.getElementById('profileTimeline');
        if (timelineContainer) {
            timelineContainer.innerHTML = '<div class="text-center py-4"><p class="text-muted mb-0">No activity data available.</p></div>';
        }
    }

    function displayProfileTimeline(activities) {
        const timelineContainer = document.getElementById('profileTimeline');
        if (!timelineContainer) return;

        if (!activities || activities.length === 0) {
            showPlaceholderTimeline();
            return;
        }

        let html = '';
        activities.forEach((activity, index) => {
            const iconClass = getActivityIcon(activity.type);
            const timeAgo = formatTimeAgo(activity.timestamp);

            html += '<div class="timeline-item mb-3">';
            html += '<div class="d-flex">';
            html += '<div class="timeline-icon bg-primary text-white rounded-circle d-flex align-items-center justify-content-center me-3" style="width: 32px; height: 32px; flex-shrink: 0;">';
            html += '<i class="bi ' + iconClass + ' small"></i>';
            html += '</div>';
            html += '<div class="timeline-content flex-grow-1">';
            html += '<h6 class="mb-1">' + (activity.title || 'Activity') + '</h6>';
            html += '<p class="text-muted small mb-1">' + (activity.description || 'No description') + '</p>';
            html += '<small class="text-muted">' + timeAgo + '</small>';
            html += '</div>';
            html += '</div>';
            html += '</div>';
        });

        timelineContainer.innerHTML = html;
    }

    function loadScoutingReport(userId) {
        if (!userId || !window.appUrls?.getScoutingReport) {
            populateScoutingReport({});
            return;
        }

        fetch(window.appUrls.getScoutingReport + '?id=' + userId)
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    populateScoutingReport(data.scoutingReport);
                } else {
                    populateScoutingReport({});
                }
            })
            .catch(error => {
                console.error('Error loading scouting report:', error);
                populateScoutingReport({});
            });
    }

    function populateScoutingReport(report) {
        safeSetValue('scoutingReportId', report.scoutingReportId || '');
        safeSetSelect('primaryPosition', report.PrimaryPosition);
        safeSetSelect('secondaryPosition', report.SecondaryPosition);
        safeSetSelect('playingStyle', report.PlayingStyle);

        setSkillRating('shooting', report.shooting || 5);
        setSkillRating('ballHandling', report.ballHandling || 5);
        setSkillRating('passing', report.passing || 5);
        setSkillRating('defense', report.defense || 5);
        setSkillRating('rebounding', report.rebounding || 5);
        setSkillRating('athleticism', report.athleticism || 5);

        safeSetValue('strengths', report.strengths);
        safeSetValue('weaknesses', report.weaknesses);
        safeSetValue('scoutingNotes', report.scoutingNotes);

        if (report.lastUpdated) {
            const metadataEl = document.getElementById('evaluationMetadata');
            const lastUpdatedEl = document.getElementById('lastUpdated');

            if (metadataEl && lastUpdatedEl) {
                lastUpdatedEl.textContent = formatDate(report.lastUpdated);
                metadataEl.style.display = 'block';
            }
        }
    }

    function submitScoutingReport(data) {
        if (!window.appUrls?.saveScoutingReport) {
            showError('Unable to save scouting report - API not configured');
            return;
        }

        const submitBtn = document.querySelector('#editScoutingForm button[type="submit"]');
        const originalText = submitBtn ? submitBtn.innerHTML : '';

        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Saving...';
        }

        const token = document.querySelector('input[name="__RequestVerificationToken"]');

        fetch(window.appUrls.saveScoutingReport, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token ? token.value : ''
            },
            body: JSON.stringify(data)
        })
            .then(response => response.json())
            .then(result => {
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalText;
                }

                if (result.success) {
                    showSuccess('Scouting report saved successfully!');

                    if (result.scoutingReport) {
                        const metadataEl = document.getElementById('evaluationMetadata');
                        const lastUpdatedEl = document.getElementById('lastUpdated');

                        if (metadataEl && lastUpdatedEl) {
                            lastUpdatedEl.textContent = formatDate(new Date());
                            metadataEl.style.display = 'block';
                        }
                    }
                } else {
                    showError('Error saving scouting report: ' + (result.message || 'Unknown error'));
                }
            })
            .catch(error => {
                console.error('Error saving scouting report:', error);
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalText;
                }
                showError('Error saving scouting report. Please try again.');
            });
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

    function setSkillRating(skillName, value) {
        const slider = document.getElementById(skillName);
        const display = document.getElementById(skillName + 'Value');

        if (slider) {
            slider.value = value;
            if (display) {
                display.textContent = value;
            }
        }
    }

    function getInitials(name) {
        if (!name) return 'U';
        const words = name.trim().split(' ');
        if (words.length >= 2) {
            return (words[0][0] + words[1][0]).toUpperCase();
        } else if (words.length === 1 && words[0].length > 0) {
            return words[0][0].toUpperCase();
        }
        return 'U';
    }

    function getActivityIcon(type) {
        const icons = {
            login: 'bi-box-arrow-in-right',
            join_run: 'bi-calendar-plus',
            create_run: 'bi-calendar-event',
            profile_update: 'bi-person-gear',
            achievement: 'bi-trophy'
        };
        return icons[type] || 'bi-circle';
    }

    function formatTimeAgo(timestamp) {
        try {
            const date = new Date(timestamp);
            const now = new Date();
            const diffInHours = Math.floor((now - date) / (1000 * 60 * 60));

            if (diffInHours < 1) return 'Less than an hour ago';
            if (diffInHours < 24) return diffInHours + ' hour' + (diffInHours > 1 ? 's' : '') + ' ago';

            const diffInDays = Math.floor(diffInHours / 24);
            if (diffInDays < 7) return diffInDays + ' day' + (diffInDays > 1 ? 's' : '') + ' ago';

            const diffInWeeks = Math.floor(diffInDays / 7);
            return diffInWeeks + ' week' + (diffInWeeks > 1 ? 's' : '') + ' ago';
        } catch (e) {
            return 'Recently';
        }
    }

    function formatDate(dateString) {
        try {
            const date = new Date(dateString);
            return date.toLocaleDateString() + ' ' + date.toLocaleTimeString();
        } catch (e) {
            return dateString;
        }
    }

    function showError(message, title) {
        if (window.UIUtils) {
            window.UIUtils.showError(message, title);
        } else {
            console.error((title || 'Error') + ': ' + message);
            alert((title || 'Error') + ': ' + message);
        }
    }

    function showSuccess(message, title) {
        if (window.UIUtils) {
            window.UIUtils.showSuccess(message, title);
        } else {
            console.log((title || 'Success') + ': ' + message);
        }
    }
});