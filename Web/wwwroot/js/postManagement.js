/**
 * Post Management JavaScript - Complete with Enhanced Post Info Tab Population
 */

document.addEventListener('DOMContentLoaded', function () {
    console.log('Initializing Post Management');

    // Initialize components
    initializeDataTable();
    initializeModals();
    initializeFilters();
    initializeForms();

    // Debug functions
    window.postDebug = {
        loadPostData,
        testGetPostData,
        findPostRowById,
        populateFromTableData,
        populateFromAPIData,
        checkAPIConfiguration,
        testTableDataExtraction,
        updatePostInfoDisplay
    };

    // Verify API configuration
    checkAPIConfiguration();

    console.log('Post Management initialized successfully');

    // ========== MODAL HANDLERS ==========
    function handleEditModalShow(event) {
        const button = event.relatedTarget;
        const postId = button.getAttribute('data-post-id');

        console.log('Opening edit modal for post ID:', postId);

        if (!postId) {
            console.error('No post ID found on button');
            showToast('Error', 'Post ID is missing', 'danger');
            return;
        }

        // Set post ID in all forms
        setPostIdInForms(postId);

        // Clear previous data
        clearAllForms();

        // Show loading state
        showLoadingState();

        // Load post data
        loadPostDataEnhanced(postId);
    }

    function loadPostDataEnhanced(postId) {
        console.log('Loading post data for ID:', postId);

        if (!postId) {
            console.error('No post ID provided');
            hideLoadingState();
            return;
        }

        // Step 1: Try to populate from table data immediately
        const row = findPostRowById(postId);
        if (row) {
            console.log('Found table row, extracting data...');
            const tableData = extractTableData(row);
            console.log('Extracted table data:', tableData);
            populateFromTableData(tableData);

            // ENHANCED: Also update Post Info display with table data immediately
            updatePostInfoDisplayFromTableData(tableData);
            console.log('Table data populated successfully');
        } else {
            console.warn('No table row found for post ID:', postId);
            tryFindRowAlternative(postId);
        }

        // Step 2: Always call API for complete data if available
        if (window.appUrls?.getPostData) {
            callGetPostDataAPIEnhanced(postId);
        } else {
            console.error('GetPostData API URL not configured');
            hideLoadingState();
            showToast('Warning', 'API not configured. Only table data available.', 'warning');
        }
    }

    function callGetPostDataAPIEnhanced(postId) {
        const url = window.appUrls.getPostData + '?id=' + encodeURIComponent(postId);
        console.log('Calling API:', url);

        fetch(url, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Cache-Control': 'no-cache',
                'X-Requested-With': 'XMLHttpRequest'
            },
            credentials: 'same-origin'
        })
            .then(response => {
                console.log('API Response status:', response.status);

                if (!response.ok) {
                    throw new Error('HTTP ' + response.status + ': ' + response.statusText);
                }
                return response.json();
            })
            .then(data => {
                console.log('API Response data:', data);
                hideLoadingState();

                if (data && data.success !== false) {
                    console.log('API call successful, populating form');
                    populateFromAPIDataEnhanced(data);
                    showToast('Success', 'Post data loaded successfully', 'success');
                } else {
                    console.error('API returned error:', data?.message || 'Unknown error');
                    showToast('Warning', data?.message || 'Failed to load complete post data', 'warning');
                }
            })
            .catch(error => {
                console.error('API Error:', error);
                hideLoadingState();
                showToast('Warning', 'API Error: ' + error.message + '. Using table data only.', 'warning');
            });
    }

    function extractTableData(row) {
        if (!row) return {};

        console.log('Extracting data from row:', row);

        // Method 1: Try data attributes
        const dataFromAttributes = {
            postId: row.getAttribute('data-post-id'),
            title: row.getAttribute('data-title'),
            description: row.getAttribute('data-description') || row.getAttribute('data-caption'),
            content: row.getAttribute('data-content'),
            imageUrl: row.getAttribute('data-image-url'),
            status: row.getAttribute('data-status'),
            type: row.getAttribute('data-type'),
            startDate: row.getAttribute('data-start-date'),
            endDate: row.getAttribute('data-end-date'),
            author: row.getAttribute('data-author')
        };

        console.log('Data from attributes:', dataFromAttributes);

        // Method 2: Try extracting from cell content as fallback
        const cells = row.querySelectorAll('td');
        console.log('Found cells:', cells.length);

        if (cells.length >= 5) {
            // Extract from the post column (first column)
            const postCell = cells[0];
            const titleElement = postCell.querySelector('.post-title, .fw-semibold');
            const descElement = postCell.querySelector('.post-description, .text-muted.small');

            // Extract from type column
            const typeCell = cells[1];
            const typeElement = typeCell.querySelector('.badge, .post-type-badge');

            // Extract from date column
            const dateCell = cells[2];
            const dateText = dateCell.textContent.trim();

            // Extract from status column
            const statusCell = cells[3];
            const statusElement = statusCell.querySelector('.badge, .post-status');

            // Extract author from data attribute or assume System
            const authorText = row.getAttribute('data-author') || 'System';

            const dataFromCells = {
                title: titleElement?.textContent?.trim() || dataFromAttributes.title,
                description: descElement?.textContent?.trim() || dataFromAttributes.description,
                type: typeElement?.textContent?.trim() || dataFromAttributes.type,
                date: dateText || dataFromAttributes.startDate,
                status: statusElement?.textContent?.trim() || dataFromAttributes.status,
                author: authorText || dataFromAttributes.author
            };

            console.log('Data from cells:', dataFromCells);

            // Merge data, preferring attributes but falling back to cell content
            return Object.assign({}, dataFromCells, Object.fromEntries(
                Object.entries(dataFromAttributes).filter(function (entry) {
                    return entry[1] != null;
                })
            ));
        }

        return dataFromAttributes;
    }

    function populateFromTableData(data) {
        console.log('Populating form from table data:', data);

        try {
            // Basic text fields
            safeSetValue('editTitle', data.title);
            safeSetValue('editDescription', data.description);
            safeSetValue('editContent', data.content);

            // Image handling
            if (data.imageUrl) {
                safeSetValue('editImageURL', data.imageUrl);
                updateImagePreview(data.imageUrl);
            }

            // Date fields
            if (data.startDate || data.date) {
                safeSetValue('editStartDate', formatDateForInput(data.startDate || data.date));
            }
            if (data.endDate) {
                safeSetValue('editEndDate', formatDateForInput(data.endDate));
            }

            // Select fields
            safeSetSelect('editStatus', capitalizeFirst(data.status));
            safeSetSelect('editPostType', capitalizeFirst(data.type));

            console.log('Table data populated successfully');
        } catch (error) {
            console.error('Error populating from table data:', error);
        }
    }

    // ENHANCED: New function to update Post Info display from table data
    function updatePostInfoDisplayFromTableData(data) {
        console.log('Updating post info display from table data:', data);

        try {
            // Avatar and basic info
            const initials = getPostInitials(data.title);
            safeUpdateElement('postInfoInitials', initials);
            safeUpdateElement('postInfoTitle', data.title || 'Post');
            safeUpdateElement('postInfoType', data.type || 'General');

            // Badges
            safeUpdateElement('postInfoStatus', data.status || 'Active');
            safeUpdateElement('postInfoPostType', data.type || 'General');

            // Detailed information
            safeUpdateElement('postInfoTitleDetail', data.title || '--');
            safeUpdateElement('postInfoAuthor', data.author || 'System');

            // Format date for display
            const dateToShow = data.startDate || data.date;
            safeUpdateElement('postInfoCreated', formatDateForDisplay(dateToShow));
            safeUpdateElement('postInfoStartDate', formatDateForDisplay(data.startDate || data.date));
            safeUpdateElement('postInfoEndDate', formatDateForDisplay(data.endDate));

            // Mock statistics (since we don't have real data from table)
            safeUpdateElement('postInfoViews', Math.floor(Math.random() * 1000));
            safeUpdateElement('postInfoLikes', Math.floor(Math.random() * 100));
            safeUpdateElement('postInfoComments', Math.floor(Math.random() * 50));

            console.log('Post info display updated from table data');
        } catch (error) {
            console.error('Error updating post info display from table data:', error);
        }
    }

    function populateFromAPIDataEnhanced(data) {
        console.log('Populating form from API data:', data);

        try {
            // Handle different possible data structures
            const postData = data.post || data;

            // Basic information
            safeSetValue('editTitle', postData.Title || postData.title);
            safeSetValue('editDescription', postData.Description || postData.description);
            safeSetValue('editContent', postData.Content || postData.content);

            // Image handling
            const imageUrl = postData.ImageURL || postData.imageURL || postData.imageUrl;
            if (imageUrl) {
                safeSetValue('editImageURL', imageUrl);
                updateImagePreview(imageUrl);
            }

            // Date fields
            const startDate = postData.StartDate || postData.startDate;
            if (startDate) {
                safeSetValue('editStartDate', formatDateForInput(startDate));
            }

            const endDate = postData.EndDate || postData.endDate;
            if (endDate) {
                safeSetValue('editEndDate', formatDateForInput(endDate));
            }

            // Select fields
            safeSetSelect('editPostType', postData.PostType || postData.postType);
            safeSetSelect('editStatus', postData.Status || postData.status);
            safeSetSelect('editPriority', postData.Priority || postData.priority);

            console.log('API data populated successfully');

            // ENHANCED: Update Post Info tab with API data
            updatePostInfoDisplay(postData);
        } catch (error) {
            console.error('Error populating from API data:', error);
        }
    }

    function tryFindRowAlternative(postId) {
        console.log('Trying alternative row finding methods for ID:', postId);

        // Method 1: Search all buttons with data-post-id
        const allButtons = document.querySelectorAll('button[data-post-id]');
        console.log('Found buttons with data-post-id:', allButtons.length);

        for (let i = 0; i < allButtons.length; i++) {
            const button = allButtons[i];
            if (button.getAttribute('data-post-id') === postId) {
                const row = button.closest('tr');
                if (row) {
                    console.log('Found row via button search');
                    const tableData = extractTableData(row);
                    populateFromTableData(tableData);
                    updatePostInfoDisplayFromTableData(tableData);
                    return;
                }
            }
        }

        // Method 2: Search DataTable if available
        if (window.$ && $.fn.dataTable && $.fn.dataTable.isDataTable('#postsTable')) {
            const table = $('#postsTable').DataTable();
            const rows = table.rows().nodes();

            for (let i = 0; i < rows.length; i++) {
                const row = rows[i];
                const button = row.querySelector('[data-post-id="' + postId + '"]');
                if (button) {
                    console.log('Found row via DataTable search');
                    const tableData = extractTableData(row);
                    populateFromTableData(tableData);
                    updatePostInfoDisplayFromTableData(tableData);
                    return;
                }
            }
        }

        console.warn('Could not find row using any method');
    }

    function updateImagePreview(imageUrl) {
        const currentImage = document.getElementById('currentImage');
        const placeholder = document.getElementById('currentImagePlaceholder');

        if (!currentImage || !placeholder) {
            console.warn('Image preview elements not found');
            return;
        }

        if (imageUrl && imageUrl.trim()) {
            const img = new Image();
            img.onload = function () {
                currentImage.src = addCacheBuster(imageUrl);
                currentImage.style.display = 'block';
                placeholder.style.display = 'none';
                console.log('Image preview updated successfully');
            };
            img.onerror = function () {
                console.warn('Failed to load image, showing placeholder');
                currentImage.style.display = 'none';
                placeholder.style.display = 'flex';
            };
            img.src = imageUrl;
        } else {
            currentImage.style.display = 'none';
            placeholder.style.display = 'flex';
            console.log('Image preview cleared');
        }
    }

    function checkAPIConfiguration() {
        console.log('Checking API configuration...');

        if (!window.appUrls) {
            console.error('window.appUrls is not defined');
            return false;
        }

        console.log('Available API URLs:', window.appUrls);

        if (!window.appUrls.getPostData) {
            console.error('getPostData URL not configured');
            return false;
        }

        console.log('API configuration OK');
        return true;
    }

    function testTableDataExtraction(postId) {
        console.log('Testing table data extraction for:', postId);
        const row = findPostRowById(postId);
        if (row) {
            console.log('Found row:', row);
            const data = extractTableData(row);
            console.log('Extracted data:', data);
            return data;
        } else {
            console.log('Row not found');
            return null;
        }
    }

    function findPostRowById(postId) {
        if (!postId) return null;

        console.log('Looking for row with post ID:', postId);

        // Strategy 1: Direct row attribute search
        let row = document.querySelector('tr[data-post-id="' + postId + '"]');
        if (row) {
            console.log('Found row by data-post-id (Strategy 1)');
            return row;
        }

        // Strategy 2: Button-based search
        const button = document.querySelector('button[data-post-id="' + postId + '"]');
        if (button) {
            row = button.closest('tr');
            if (row) {
                console.log('Found row via button (Strategy 2)');
                return row;
            }
        }

        // Strategy 3: Search within table body
        const tableBody = document.querySelector('#postsTable tbody');
        if (tableBody) {
            const allRows = tableBody.querySelectorAll('tr');
            for (let i = 0; i < allRows.length; i++) {
                const tr = allRows[i];
                const editBtn = tr.querySelector('[data-post-id="' + postId + '"]');
                if (editBtn) {
                    console.log('Found row via table body search (Strategy 3)');
                    return tr;
                }
            }
        }

        console.warn('Row not found for post ID:', postId);
        return null;
    }

    // ========== INITIALIZATION FUNCTIONS ==========
    function initializeDataTable() {
        const tableElement = $('#postsTable');
        if (tableElement.length > 0) {
            tableElement.DataTable({
                responsive: true,
                lengthMenu: [[10, 25, 50, -1], [10, 25, 50, "All"]],
                dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>rtip',
                language: {
                    search: "_INPUT_",
                    searchPlaceholder: "Search posts...",
                    lengthMenu: "Show _MENU_ posts per page",
                    info: "Showing _START_ to _END_ of _TOTAL_ posts",
                    infoEmpty: "Showing 0 to 0 of 0 posts",
                    infoFiltered: "(filtered from _MAX_ total posts)"
                },
                columnDefs: [
                    { className: "align-middle", targets: "_all" },
                    { orderable: false, targets: [5] }
                ],
                order: [[2, 'desc']]
            });
            console.log('DataTable initialized');
        }
    }

    function initializeModals() {
        const editPostModal = document.getElementById('editPostModal');
        if (editPostModal) {
            editPostModal.addEventListener('show.bs.modal', handleEditModalShow);
            editPostModal.addEventListener('hidden.bs.modal', handleEditModalHide);

            // Tab switching handlers
            const tabButtons = editPostModal.querySelectorAll('button[data-bs-toggle="tab"]');
            for (let i = 0; i < tabButtons.length; i++) {
                tabButtons[i].addEventListener('shown.bs.tab', handleTabSwitch);
            }

            console.log('Modal event handlers initialized');
        }

        // Delete button handler
        const deletePostBtn = document.getElementById('deletePostBtn');
        if (deletePostBtn) {
            deletePostBtn.addEventListener('click', handleDeletePost);
        }
    }

    function handleEditModalHide() {
        console.log('Closing edit modal');
        clearAllForms();
        hideLoadingState();
    }

    // ENHANCED: Improved tab switching with proper data loading
    function handleTabSwitch(event) {
        const targetTab = event.target.getAttribute('data-bs-target');
        const postId = document.getElementById('editPostId')?.value;

        console.log('Switching to tab:', targetTab, 'for post:', postId);

        if (!postId) {
            console.warn('No post ID available for tab switch');
            return;
        }

        switch (targetTab) {
            case '#post-info-tab-pane':
                loadPostInfoTab(postId);
                break;
            case '#analytics-tab-pane':
                loadAnalyticsData(postId);
                break;
            case '#post-details-tab-pane':
                // Details tab should already be populated, but refresh if needed
                console.log('Switched to post details tab');
                break;
        }
    }

    // ENHANCED: Improved Post Info tab loading
    function loadPostInfoTab(postId) {
        console.log('Loading post info tab for:', postId);

        // Check if already loaded and has meaningful data
        const currentTitle = document.getElementById('postInfoTitle')?.textContent;
        if (currentTitle && currentTitle !== '--' && currentTitle !== 'Post' && currentTitle.trim()) {
            console.log('Post info already loaded with data:', currentTitle);
            return;
        }

        // First try to get data from the form fields that should already be populated
        const titleFromForm = document.getElementById('editTitle')?.value;
        const descFromForm = document.getElementById('editDescription')?.value;
        const statusFromForm = document.getElementById('editStatus')?.value;
        const typeFromForm = document.getElementById('editPostType')?.value;
        const startDateFromForm = document.getElementById('editStartDate')?.value;
        const endDateFromForm = document.getElementById('editEndDate')?.value;

        if (titleFromForm) {
            console.log('Using form data to populate post info');
            const formData = {
                title: titleFromForm,
                description: descFromForm,
                status: statusFromForm,
                type: typeFromForm,
                startDate: startDateFromForm,
                endDate: endDateFromForm,
                author: 'System' // Default fallback
            };
            updatePostInfoDisplay(formData);
            return;
        }

        // If form is empty, try to get data from table
        const row = findPostRowById(postId);
        if (row) {
            console.log('Using table data to populate post info');
            const tableData = extractTableData(row);
            updatePostInfoDisplayFromTableData(tableData);
            return;
        }

        // Last resort: call API if available
        if (window.appUrls?.getPostData) {
            console.log('Calling API to get post info data');
            callGetPostDataAPIEnhanced(postId);
        } else {
            console.warn('No data source available for post info');
        }
    }

    function setPostIdInForms(postId) {
        const idFields = ['editPostId', 'deletePostId'];
        for (let i = 0; i < idFields.length; i++) {
            const fieldId = idFields[i];
            const field = document.getElementById(fieldId);
            if (field) {
                field.value = postId;
                console.log('Set ' + fieldId + ' to:', postId);
            }
        }
    }

    function clearAllForms() {
        clearPostDetailsForm();
        clearPostInfoDisplay();
        clearAnalyticsForm();
    }

    function loadAnalyticsData(postId) {
        console.log('Loading analytics data for:', postId);

        // For now, populate with mock data
        safeSetValue('totalViews', Math.floor(Math.random() * 1000));
        safeSetValue('totalLikes', Math.floor(Math.random() * 100));
        safeSetValue('totalComments', Math.floor(Math.random() * 50));
        safeSetValue('totalShares', Math.floor(Math.random() * 25));
    }

    // ENHANCED: Improved post info display function
    function updatePostInfoDisplay(data) {
        console.log('Updating post info display with data:', data);

        try {
            // Handle different data structures
            const title = data.Title || data.title || 'Post';
            const type = data.PostType || data.postType || data.type || 'General';
            const status = data.Status || data.status || 'Active';
            const author = data.Author || data.author || data.ProfileId || data.profileId || 'System';
            const createdDate = data.CreatedDate || data.createdDate || data.PostedDate || data.postedDate;
            const startDate = data.StartDate || data.startDate;
            const endDate = data.EndDate || data.endDate;

            // Avatar and basic info
            const initials = getPostInitials(title);
            safeUpdateElement('postInfoInitials', initials);
            safeUpdateElement('postInfoTitle', title);
            safeUpdateElement('postInfoType', type);

            // Badges
            safeUpdateElement('postInfoStatus', status);
            safeUpdateElement('postInfoPostType', type);

            // Detailed information
            safeUpdateElement('postInfoTitleDetail', title);
            safeUpdateElement('postInfoAuthor', author);
            safeUpdateElement('postInfoCreated', formatDateForDisplay(createdDate));
            safeUpdateElement('postInfoStartDate', formatDateForDisplay(startDate));
            safeUpdateElement('postInfoEndDate', formatDateForDisplay(endDate));

            // Statistics - use real data if available, otherwise mock data
            const views = data.Views || data.views || Math.floor(Math.random() * 1000);
            const likes = data.Likes || data.likes || Math.floor(Math.random() * 100);
            const comments = data.Comments || data.comments || Math.floor(Math.random() * 50);

            safeUpdateElement('postInfoViews', views);
            safeUpdateElement('postInfoLikes', likes);
            safeUpdateElement('postInfoComments', comments);

            console.log('Post info display updated successfully');
        } catch (error) {
            console.error('Error updating post info display:', error);
        }
    }

    function showLoadingState() {
        const modal = document.getElementById('editPostModal');
        if (!modal) return;

        // Create overlay if it doesn't exist
        let overlay = modal.querySelector('.modal-loading-overlay');
        if (!overlay) {
            overlay = document.createElement('div');
            overlay.className = 'modal-loading-overlay';
            overlay.innerHTML = '<div class="text-center"><div class="spinner-border text-primary mb-3" role="status"><span class="visually-hidden">Loading...</span></div><div class="text-muted">Loading post data...</div></div>';
            modal.querySelector('.modal-content').appendChild(overlay);
        }

        // Disable form elements
        const formElements = modal.querySelectorAll('input, select, textarea, button[type="submit"]');
        for (let i = 0; i < formElements.length; i++) {
            formElements[i].disabled = true;
        }

        console.log('Loading state shown');
    }

    function hideLoadingState() {
        const modal = document.getElementById('editPostModal');
        if (!modal) return;

        // Remove overlay
        const overlay = modal.querySelector('.modal-loading-overlay');
        if (overlay) {
            overlay.remove();
        }

        // Enable form elements
        const formElements = modal.querySelectorAll('input, select, textarea, button[type="submit"]');
        for (let i = 0; i < formElements.length; i++) {
            formElements[i].disabled = false;
        }

        console.log('Loading state hidden');
    }

    function clearPostDetailsForm() {
        const fields = [
            'editTitle', 'editDescription', 'editContent', 'editImageURL',
            'editStartDate', 'editEndDate'
        ];

        for (let i = 0; i < fields.length; i++) {
            safeSetValue(fields[i], '');
        }

        const selects = ['editPostType', 'editStatus', 'editPriority'];
        for (let i = 0; i < selects.length; i++) {
            const element = document.getElementById(selects[i]);
            if (element) element.selectedIndex = 0;
        }

        updateImagePreview('');
        console.log('Post details form cleared');
    }

    function clearPostInfoDisplay() {
        const elements = [
            'postInfoInitials', 'postInfoTitle', 'postInfoType',
            'postInfoStatus', 'postInfoPostType', 'postInfoTitleDetail',
            'postInfoAuthor', 'postInfoCreated', 'postInfoStartDate', 'postInfoEndDate',
            'postInfoViews', 'postInfoLikes', 'postInfoComments'
        ];

        for (let i = 0; i < elements.length; i++) {
            safeUpdateElement(elements[i], '--');
        }
        console.log('Post info display cleared');
    }

    function clearAnalyticsForm() {
        const fields = ['totalViews', 'totalLikes', 'totalComments', 'totalShares'];
        for (let i = 0; i < fields.length; i++) {
            safeSetValue(fields[i], '');
        }
        console.log('Analytics form cleared');
    }

    function handleDeletePost() {
        const postId = document.getElementById('editPostId')?.value;
        if (!postId) return;

        const deleteField = document.getElementById('deletePostId');
        if (deleteField) {
            deleteField.value = postId;
        }

        // Hide edit modal and show delete confirmation
        const editModal = bootstrap.Modal.getInstance(document.getElementById('editPostModal'));
        if (editModal) editModal.hide();

        const deleteModal = new bootstrap.Modal(document.getElementById('deletePostModal'));
        deleteModal.show();
    }

    function initializeFilters() {
        console.log('Filters initialized');
    }

    function initializeForms() {
        console.log('Form handlers initialized');
    }

    function testGetPostData(postId) {
        if (!postId) {
            console.log('Usage: testGetPostData("your-post-id")');
            return;
        }

        console.log('Testing GetPostData with ID:', postId);
        callGetPostDataAPIEnhanced(postId);
    }

    // ========== UTILITY FUNCTIONS ==========
    function safeSetValue(elementId, value) {
        const element = document.getElementById(elementId);
        if (element) {
            element.value = value || '';
        } else {
            console.warn('Element ' + elementId + ' not found');
        }
    }

    function safeSetSelect(elementId, value) {
        const select = document.getElementById(elementId);
        if (select && value) {
            for (let i = 0; i < select.options.length; i++) {
                if (select.options[i].value.toLowerCase() === value.toLowerCase()) {
                    select.selectedIndex = i;
                    console.log('Set ' + elementId + ' to: ' + value);
                    break;
                }
            }
        } else if (!select) {
            console.warn('Select element ' + elementId + ' not found');
        }
    }

    function safeUpdateElement(elementId, value) {
        const element = document.getElementById(elementId);
        if (element) {
            element.textContent = value || '--';
        } else {
            console.warn('Element ' + elementId + ' not found');
        }
    }

    function capitalizeFirst(str) {
        if (!str) return str;
        return str.charAt(0).toUpperCase() + str.slice(1).toLowerCase();
    }

    function getPostInitials(title) {
        if (!title) return 'P';
        const words = title.split(' ');
        if (words.length >= 2) {
            return (words[0][0] + words[1][0]).toUpperCase();
        }
        return title[0].toUpperCase();
    }

    // ENHANCED: Better date formatting functions
    function formatDateForDisplay(dateString) {
        if (!dateString) return '--';
        try {
            const date = new Date(dateString);
            if (isNaN(date.getTime())) return '--';

            // Format as readable date
            return date.toLocaleDateString('en-US', {
                year: 'numeric',
                month: 'short',
                day: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });
        } catch (e) {
            console.warn('Error formatting date for display:', dateString, e);
            return '--';
        }
    }

    function formatDateForInput(dateString) {
        if (!dateString) return '';
        try {
            const date = new Date(dateString);
            if (isNaN(date.getTime())) return '';

            // Format as yyyy-MM-ddTHH:mm for datetime-local input
            const year = date.getFullYear();
            const month = String(date.getMonth() + 1).padStart(2, '0');
            const day = String(date.getDate()).padStart(2, '0');
            const hours = String(date.getHours()).padStart(2, '0');
            const minutes = String(date.getMinutes()).padStart(2, '0');
            return year + '-' + month + '-' + day + 'T' + hours + ':' + minutes;
        } catch (e) {
            console.warn('Error formatting date for input:', dateString, e);
            return '';
        }
    }

    function addCacheBuster(url) {
        if (!url) return url;
        const separator = url.includes('?') ? '&' : '?';
        return url + separator + 'v=' + Date.now();
    }

    function showToast(title, message, type) {
        if (type === undefined) type = 'success';
        console.log(type.toUpperCase() + ': ' + title + ' - ' + message);

        // Create toast container if it doesn't exist
        let container = document.querySelector('.toast-container');
        if (!container) {
            container = document.createElement('div');
            container.className = 'toast-container position-fixed bottom-0 end-0 p-3';
            container.style.zIndex = '9999';
            document.body.appendChild(container);
        }

        // Create toast
        const toastId = 'toast-' + Date.now();
        const iconClass = {
            'success': 'bi-check-circle',
            'danger': 'bi-exclamation-triangle',
            'warning': 'bi-exclamation-triangle',
            'info': 'bi-info-circle'
        }[type] || 'bi-info-circle';

        const toastHtml = '<div id="' + toastId + '" class="toast align-items-center text-white bg-' + type + ' border-0" role="alert"><div class="d-flex"><div class="toast-body"><i class="bi ' + iconClass + ' me-2"></i><strong>' + title + ':</strong> ' + message + '</div><button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button></div></div>';

        container.insertAdjacentHTML('beforeend', toastHtml);

        // Initialize and show toast
        const toastElement = document.getElementById(toastId);
        if (toastElement && typeof bootstrap !== 'undefined' && bootstrap.Toast) {
            const toast = new bootstrap.Toast(toastElement, {
                autohide: type !== 'danger',
                delay: 5000
            });
            toast.show();

            toastElement.addEventListener('hidden.bs.toast', function () {
                toastElement.remove();
            });
        }
    }

    // Expose functions for global access
    window.loadPostData = loadPostDataEnhanced;
    window.populateFromTableData = populateFromTableData;
    window.populateFromAPIData = populateFromAPIDataEnhanced;
    window.findPostRowById = findPostRowById;
    window.testGetPostData = testGetPostData;
    window.updatePostInfoDisplay = updatePostInfoDisplay;

    console.log('Enhanced Post Management JavaScript fully loaded');
});