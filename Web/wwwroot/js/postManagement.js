/**
 * Enhanced Post Management JavaScript with Complete Details Tab Population
 * Ensures proper data loading when switching to details tab or opening edit modal
 */

document.addEventListener('DOMContentLoaded', function () {
    console.log('🚀 Initializing Enhanced Post Management with Details Tab Population');

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
        updatePostInfoDisplay,
        loadPostDetailsTab,
        extractEnhancedTableData
    };

    // Verify API configuration
    checkAPIConfiguration();

    console.log('✅ Enhanced Post Management initialized successfully');

    // ========== ENHANCED MODAL HANDLERS ==========
    function handleEditModalShow(event) {
        const button = event.relatedTarget;
        const postId = button.getAttribute('data-post-id');

        console.log('📂 Opening edit modal for post ID:', postId);

        if (!postId) {
            console.error('❌ No post ID found on button');
            UIUtils.showError('Post ID is missing', 'Error');
            return;
        }

        // Set post ID in all forms
        setPostIdInForms(postId);

        // Clear previous data
        clearAllForms();

        // Show loading state
        showLoadingState();

        // Load post data with enhanced functionality
        loadPostDataEnhanced(postId);
    }

    function loadPostDataEnhanced(postId) {
        console.log('📥 Loading enhanced post data for ID:', postId);

        if (!postId) {
            console.error('❌ No post ID provided');
            hideLoadingState();
            return;
        }

        // Step 1: Try to populate from table data immediately for better UX
        const row = findPostRowById(postId);
        if (row) {
            console.log('📋 Found table row, extracting comprehensive data...');
            const tableData = extractEnhancedTableData(row);
            console.log('📊 Extracted table data:', tableData);

            // Populate form fields immediately
            populateFromTableData(tableData);

            // Also update Post Info display with table data
            updatePostInfoDisplayFromTableData(tableData);
            console.log('✅ Table data populated successfully');
        } else {
            console.warn('⚠️ No table row found for post ID:', postId);
            tryFindRowAlternative(postId);
        }

        // Step 2: Always call API for complete and accurate data
        if (window.appUrls?.getPostData) {
            callGetPostDataAPIEnhanced(postId);
        } else {
            console.error('❌ GetPostData API URL not configured');
            hideLoadingState();
            UIUtils.showWarning('API not configured. Only table data available.', 'Warning');
        }
    }

    function callGetPostDataAPIEnhanced(postId) {
        const url = `${window.appUrls.getPostData}?id=${encodeURIComponent(postId)}`;
        console.log('🌐 Calling API:', url);

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
                console.log('📡 API Response status:', response.status);

                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                return response.json();
            })
            .then(data => {
                console.log('📦 API Response data:', data);
                hideLoadingState();

                if (data && data.success !== false) {
                    console.log('✅ API call successful, populating with fresh data');
                    populateFromAPIDataEnhanced(data);
                    UIUtils.showSuccess('Post data loaded successfully', 'Success');
                } else {
                    console.error('❌ API returned error:', data?.message || 'Unknown error');
                    UIUtils.showWarning(data?.message || 'Failed to load complete post data', 'Warning');
                }
            })
            .catch(error => {
                console.error('💥 API Error:', error);
                hideLoadingState();
                UIUtils.showWarning(`API Error: ${error.message}. Using table data only.`, 'Warning');
            });
    }

    // ========== ENHANCED TABLE DATA EXTRACTION ==========
    function extractEnhancedTableData(row) {
        if (!row) return {};

        console.log('🔍 Extracting enhanced data from row:', row);

        // Get all available data attributes
        const dataFromAttributes = {
            postId: row.getAttribute('data-post-id'),
            title: row.getAttribute('data-title'),
            caption: row.getAttribute('data-caption'),
            content: row.getAttribute('data-content'),
            description: row.getAttribute('data-description'),
            imageUrl: row.getAttribute('data-image-url'),
            thumbnailUrl: row.getAttribute('data-thumbnail-url'),
            status: row.getAttribute('data-status'),
            type: row.getAttribute('data-type'),
            postType: row.getAttribute('data-post-type'),
            startDate: row.getAttribute('data-start-date'),
            endDate: row.getAttribute('data-end-date'),
            postedDate: row.getAttribute('data-posted-date'),
            author: row.getAttribute('data-author'),
            date: row.getAttribute('data-date')
        };

        console.log('📊 Data from attributes:', dataFromAttributes);

        // Extract from cell content as backup
        const cells = row.querySelectorAll('td');
        console.log('🔢 Found cells:', cells.length);

        if (cells.length >= 4) {
            // Extract from the post column (first column)
            const postCell = cells[0];
            const titleElement = postCell.querySelector('.post-title, .fw-semibold');
            const descElement = postCell.querySelector('.post-description, .text-muted.small');

            // Extract from type/status columns
            let typeCell, dateCell, statusCell;

            // Handle different table structures (some have type column, some don't)
            if (cells.length >= 5) {
                // Structure: Post | Type | Date | Status | Actions
                typeCell = cells[1];
                dateCell = cells[2];
                statusCell = cells[3];
            } else {
                // Structure: Post | Date | Status | Actions
                dateCell = cells[1];
                statusCell = cells[2];
            }

            const typeElement = typeCell?.querySelector('.badge, .post-type-badge');
            const dateText = dateCell?.textContent?.trim();
            const statusElement = statusCell?.querySelector('.badge, .post-status');

            const dataFromCells = {
                title: titleElement?.textContent?.trim() || dataFromAttributes.title,
                description: descElement?.textContent?.trim() || dataFromAttributes.caption,
                caption: descElement?.textContent?.trim() || dataFromAttributes.caption,
                type: typeElement?.textContent?.trim() || dataFromAttributes.type,
                postType: typeElement?.textContent?.trim() || dataFromAttributes.postType,
                date: dateText || dataFromAttributes.date || dataFromAttributes.postedDate,
                status: statusElement?.textContent?.trim() || dataFromAttributes.status,
                author: dataFromAttributes.author || 'System'
            };

            console.log('📱 Data from cells:', dataFromCells);

            // Merge data, preferring attributes but falling back to cell content
            return Object.assign({}, dataFromCells, Object.fromEntries(
                Object.entries(dataFromAttributes).filter(([_, v]) => v != null && v !== '')
            ));
        }

        return dataFromAttributes;
    }

    // ========== ENHANCED FORM POPULATION ==========
    function populateFromTableData(data) {
        console.log('📝 Populating form from enhanced table data:', data);

        try {
            // Basic text fields with fallbacks
            safeSetValue('editTitle', data.title);
            safeSetValue('editDescription', data.description || data.caption);
            safeSetValue('editContent', data.content || data.description || data.caption);

            // Image handling with multiple sources
            const imageUrl = data.imageUrl || data.thumbnailUrl;
            if (imageUrl) {
                safeSetValue('editImageURL', imageUrl);
                updateImagePreview(imageUrl);
            }

            // Date fields with proper formatting
            if (data.startDate || data.date || data.postedDate) {
                const dateToUse = data.startDate || data.postedDate || data.date;
                safeSetValue('editStartDate', formatDateForInput(dateToUse));
                safeSetValue('editPostedDate', formatDateForInput(dateToUse));
            }
            if (data.endDate) {
                safeSetValue('editEndDate', formatDateForInput(data.endDate));
            }

            // Select fields with proper capitalization
            safeSetSelect('editStatus', capitalizeFirst(data.status) || 'Active');
            safeSetSelect('editPostType', capitalizeFirst(data.type || data.postType) || 'General');
            safeSetSelect('editType', capitalizeFirst(data.type || data.postType) || 'General');

            console.log('✅ Enhanced table data populated successfully');
        } catch (error) {
            console.error('💥 Error populating from table data:', error);
        }
    }

    function populateFromAPIDataEnhanced(data) {
        console.log('🌐 Populating form from enhanced API data:', data);

        try {
            // Handle different possible data structures
            const postData = data.post || data;

            // Basic information with multiple property name fallbacks
            safeSetValue('editTitle', postData.title || postData.Title);
            safeSetValue('editDescription', postData.description || postData.Description || postData.caption || postData.Caption);
            safeSetValue('editContent', postData.content || postData.Content || postData.description || postData.Description);

            // Image handling with multiple sources
            const imageUrl = postData.imageURL || postData.ImageURL || postData.imageUrl || postData.thumbnailURL || postData.thumbnailUrl;
            if (imageUrl) {
                safeSetValue('editImageURL', imageUrl);
                updateImagePreview(imageUrl);
            }

            // Date fields with proper formatting
            const startDate = postData.startDate || postData.StartDate || postData.postedDate || postData.PostedDate;
            if (startDate) {
                safeSetValue('editStartDate', formatDateForInput(startDate));
                safeSetValue('editPostedDate', formatDateForInput(startDate));
            }

            const endDate = postData.endDate || postData.EndDate;
            if (endDate) {
                safeSetValue('editEndDate', formatDateForInput(endDate));
            }

            // Select fields
            safeSetSelect('editPostType', postData.postType || postData.PostType || 'General');
            safeSetSelect('editType', postData.type || postData.Type || postData.postType || postData.PostType || 'General');
            safeSetSelect('editStatus', postData.status || postData.Status || 'Active');

            console.log('✅ Enhanced API data populated successfully');

            // Update Post Info tab with comprehensive API data
            updatePostInfoDisplayEnhanced(postData);
        } catch (error) {
            console.error('💥 Error populating from API data:', error);
        }
    }

    // ========== POST INFO DISPLAY UPDATES ==========
    function updatePostInfoDisplayFromTableData(data) {
        console.log('📊 Updating post info display from enhanced table data:', data);

        try {
            // Avatar and basic info
            const initials = getPostInitials(data.title);
            safeUpdateElement('postInfoInitials', initials);
            safeUpdateElement('postInfoTitle', data.title || 'Post');
            safeUpdateElement('postInfoType', data.type || data.postType || 'General');

            // Badges
            safeUpdateElement('postInfoStatus', data.status || 'Active');
            safeUpdateElement('postInfoPostType', data.type || data.postType || 'General');

            // Detailed information
            safeUpdateElement('postInfoTitleDetail', data.title || '--');
            safeUpdateElement('postInfoAuthor', data.author || 'System');

            // Format dates for display
            const dateToShow = data.startDate || data.postedDate || data.date;
            safeUpdateElement('postInfoCreated', formatDateForDisplay(dateToShow));
            safeUpdateElement('postInfoStartDate', formatDateForDisplay(data.startDate || dateToShow));
            safeUpdateElement('postInfoEndDate', formatDateForDisplay(data.endDate));

            // Mock statistics (since we don't have real data from table)
            safeUpdateElement('postInfoViews', Math.floor(Math.random() * 1000));
            safeUpdateElement('postInfoLikes', Math.floor(Math.random() * 100));
            safeUpdateElement('postInfoComments', Math.floor(Math.random() * 50));

            console.log('✅ Post info display updated from enhanced table data');
        } catch (error) {
            console.error('💥 Error updating post info display from table data:', error);
        }
    }

    function updatePostInfoDisplayEnhanced(data) {
        console.log('📊 Updating post info display with enhanced API data:', data);

        try {
            // Handle different data structures
            const title = data.title || data.Title || 'Post';
            const type = data.postType || data.PostType || data.type || 'General';
            const status = data.status || data.Status || 'Active';
            const author = data.author || data.Author || data.profileId || data.ProfileId || 'System';
            const createdDate = data.createdDate || data.CreatedDate || data.postedDate || data.PostedDate;
            const startDate = data.startDate || data.StartDate;
            const endDate = data.endDate || data.EndDate;

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

            // Statistics - use API data if available, otherwise mock data
            const views = data.views || data.Views || Math.floor(Math.random() * 1000);
            const likes = data.likes || data.Likes || Math.floor(Math.random() * 100);
            const comments = data.comments || data.Comments || Math.floor(Math.random() * 50);

            safeUpdateElement('postInfoViews', views);
            safeUpdateElement('postInfoLikes', likes);
            safeUpdateElement('postInfoComments', comments);

            console.log('✅ Enhanced post info display updated successfully');
        } catch (error) {
            console.error('💥 Error updating enhanced post info display:', error);
        }
    }

    // ========== TAB SWITCHING WITH ENHANCED LOADING ==========
    function handleTabSwitch(event) {
        const targetTab = event.target.getAttribute('data-bs-target');
        const postId = document.getElementById('editPostId')?.value;

        console.log('🔄 Switching to tab:', targetTab, 'for post:', postId);

        if (!postId) {
            console.warn('⚠️ No post ID available for tab switch');
            return;
        }

        switch (targetTab) {
            case '#post-details-tab-pane':
                loadPostDetailsTab(postId);
                break;
            case '#post-info-tab-pane':
                loadPostInfoTab(postId);
                break;
            case '#analytics-tab-pane':
                loadAnalyticsTab(postId);
                break;
        }
    }

    // ========== ENHANCED TAB LOADING FUNCTIONS ==========
    function loadPostDetailsTab(postId) {
        console.log('📝 Loading post details tab for ID:', postId);

        if (!postId) {
            console.error('❌ No post ID provided for details tab');
            return;
        }

        // Check if details are already loaded (avoid unnecessary API calls)
        const titleField = document.getElementById('editTitle');
        if (titleField && titleField.value && titleField.value.trim()) {
            console.log('✅ Post details already loaded');
            return;
        }

        // Show loading on the details tab specifically
        const detailsTabPane = document.getElementById('post-details-tab-pane');
        if (detailsTabPane) {
            UIUtils.showElementLoading('#post-details-tab-pane', 'Loading post details...');
        }

        // Try to populate from table data first
        const row = findPostRowById(postId);
        if (row) {
            console.log('📋 Found table row, using table data for details tab');
            const tableData = extractEnhancedTableData(row);
            populatePostDetailsForm(tableData);
        }

        // Always try API for complete data
        if (window.appUrls?.getPostData) {
            fetch(`${window.appUrls.getPostData}?id=${encodeURIComponent(postId)}`)
                .then(response => {
                    if (!response.ok) {
                        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                    }
                    return response.json();
                })
                .then(data => {
                    if (detailsTabPane) {
                        UIUtils.hideElementLoading('#post-details-tab-pane');
                    }

                    if (data && data.success !== false) {
                        populatePostDetailsForm(data);
                        console.log('✅ Post details populated from API successfully');
                    } else {
                        UIUtils.showWarning(`Failed to load post details: ${data?.message || 'Unknown error'}`, 'Warning');
                    }
                })
                .catch(error => {
                    console.error('🚨 Error loading post details:', error);

                    if (detailsTabPane) {
                        UIUtils.hideElementLoading('#post-details-tab-pane');
                    }

                    UIUtils.showError(`Error loading post details: ${error.message}`, 'Error');
                });
        } else {
            if (detailsTabPane) {
                UIUtils.hideElementLoading('#post-details-tab-pane');
            }
        }
    }

    function populatePostDetailsForm(data) {
        console.log('📝 Populating post details form:', data);

        try {
            const postData = data.post || data;

            // Populate all form fields in the details tab
            safeSetValue('editPostId', postData.postId || postData.PostId);
            safeSetValue('editTitle', postData.title || postData.Title);
            safeSetValue('editDescription', postData.description || postData.Description || postData.caption || postData.Caption);
            safeSetValue('editContent', postData.content || postData.Content || postData.description || postData.Description);

            // Image handling
            const imageUrl = postData.imageURL || postData.ImageURL || postData.imageUrl || postData.thumbnailURL || postData.thumbnailUrl;
            if (imageUrl) {
                safeSetValue('editImageURL', imageUrl);
                updateImagePreview(imageUrl);
            }

            // Date fields
            const postedDate = postData.postedDate || postData.PostedDate || postData.startDate || postData.StartDate || postData.date;
            if (postedDate) {
                safeSetValue('editPostedDate', formatDateForInput(postedDate));
                safeSetValue('editStartDate', formatDateForInput(postedDate));
            }

            const endDate = postData.endDate || postData.EndDate;
            if (endDate) {
                safeSetValue('editEndDate', formatDateForInput(endDate));
            }

            // Select fields
            safeSetSelect('editPostType', postData.postType || postData.PostType || postData.type || postData.Type || 'General');
            safeSetSelect('editType', postData.type || postData.Type || postData.postType || postData.PostType || 'General');
            safeSetSelect('editStatus', postData.status || postData.Status || 'Active');

            console.log('✅ Post details form populated successfully');
        } catch (error) {
            console.error('🚨 Error populating post details form:', error);
            UIUtils.showError('Error populating post details form', 'Error');
        }
    }

    function loadPostInfoTab(postId) {
        console.log('📊 Loading post info tab for ID:', postId);

        // Check if data is already populated
        const titleElement = document.getElementById('postInfoTitle');
        if (titleElement && titleElement.textContent && titleElement.textContent !== '--' && titleElement.textContent !== 'Post') {
            console.log('✅ Post info already loaded');
            return;
        }

        // Try to get data from form fields first
        const title = safeGetValue('editTitle');
        const status = safeGetValue('editStatus') || safeGetSelectValue('editStatus');
        const postType = safeGetValue('editPostType') || safeGetSelectValue('editPostType');

        if (title) {
            const formData = {
                title: title,
                status: status,
                postType: postType,
                author: 'Current User'
            };
            updatePostInfoDisplayEnhanced(formData);
        } else {
            // Load from table or API
            loadPostDataEnhanced(postId);
        }
    }

    function loadAnalyticsTab(postId) {
        console.log('📈 Loading analytics tab for ID:', postId);

        // Populate with mock analytics data
        safeSetValue('totalViews', Math.floor(Math.random() * 1000));
        safeSetValue('totalLikes', Math.floor(Math.random() * 100));
        safeSetValue('totalComments', Math.floor(Math.random() * 50));
        safeSetValue('totalShares', Math.floor(Math.random() * 25));
    }

    // ========== ENHANCED ROW FINDING ==========
    function findPostRowById(postId) {
        if (!postId) return null;

        console.log('🔍 Looking for row with post ID:', postId);

        // Strategy 1: Direct row attribute search
        let row = document.querySelector(`tr[data-post-id="${postId}"]`);
        if (row) {
            console.log('✅ Found row by data-post-id (Strategy 1)');
            return row;
        }

        // Strategy 2: Button-based search
        const button = document.querySelector(`button[data-post-id="${postId}"]`);
        if (button) {
            row = button.closest('tr');
            if (row) {
                console.log('✅ Found row via button (Strategy 2)');
                return row;
            }
        }

        // Strategy 3: Search within table body
        const tableBody = document.querySelector('#postsTable tbody');
        if (tableBody) {
            const allRows = tableBody.querySelectorAll('tr');
            for (const tr of allRows) {
                const editBtn = tr.querySelector(`[data-post-id="${postId}"]`);
                if (editBtn) {
                    console.log('✅ Found row via table body search (Strategy 3)');
                    return tr;
                }
            }
        }

        // Strategy 4: DataTable API search (if available)
        if (window.$ && $.fn.dataTable && $.fn.dataTable.isDataTable('#postsTable')) {
            try {
                const table = $('#postsTable').DataTable();
                const rows = table.rows().nodes();
                for (let i = 0; i < rows.length; i++) {
                    const rowEl = rows[i];
                    const button = rowEl.querySelector(`[data-post-id="${postId}"]`);
                    if (button) {
                        console.log('✅ Found row via DataTable search (Strategy 4)');
                        return rowEl;
                    }
                }
            } catch (error) {
                console.warn('⚠️ DataTable search failed:', error);
            }
        }

        console.warn('⚠️ Row not found for post ID:', postId);
        return null;
    }

    function tryFindRowAlternative(postId) {
        console.log('🔍 Trying alternative row finding methods for ID:', postId);

        // Method 1: Search all buttons with data-post-id
        const allButtons = document.querySelectorAll('button[data-post-id]');
        console.log('🔍 Found buttons with data-post-id:', allButtons.length);

        for (const button of allButtons) {
            if (button.getAttribute('data-post-id') === postId) {
                const row = button.closest('tr');
                if (row) {
                    console.log('✅ Found row via button search');
                    const tableData = extractEnhancedTableData(row);
                    populateFromTableData(tableData);
                    return;
                }
            }
        }

        console.warn('⚠️ Could not find row using any method');
    }

    // ========== IMAGE PREVIEW ==========
    function updateImagePreview(imageUrl) {
        const currentImage = document.getElementById('currentImage');
        const placeholder = document.getElementById('currentImagePlaceholder');

        if (!currentImage || !placeholder) {
            console.warn('⚠️ Image preview elements not found');
            return;
        }

        if (imageUrl?.trim()) {
            const img = new Image();
            img.onload = function () {
                currentImage.src = addCacheBuster(imageUrl);
                currentImage.style.display = 'block';
                placeholder.style.display = 'none';
                console.log('🖼️ Image preview updated successfully');
            };
            img.onerror = function () {
                console.warn('⚠️ Failed to load image, showing placeholder');
                currentImage.style.display = 'none';
                placeholder.style.display = 'flex';
            };
            img.src = imageUrl;
        } else {
            currentImage.style.display = 'none';
            placeholder.style.display = 'flex';
            console.log('🖼️ Image preview cleared');
        }
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
                    { orderable: false, targets: [-1] } // Last column (actions) not orderable
                ],
                order: [[2, 'desc']] // Order by date column descending
            });
            console.log('📊 DataTable initialized');
        }
    }

    function initializeModals() {
        const editPostModal = document.getElementById('editPostModal');
        if (editPostModal) {
            editPostModal.addEventListener('show.bs.modal', handleEditModalShow);
            editPostModal.addEventListener('hidden.bs.modal', handleEditModalHide);

            // Tab switching handlers
            const tabButtons = editPostModal.querySelectorAll('button[data-bs-toggle="tab"]');
            tabButtons.forEach(button => {
                button.addEventListener('shown.bs.tab', handleTabSwitch);
            });

            console.log('📝 Modal event handlers initialized');
        }

        // Delete button handler
        const deletePostBtn = document.getElementById('deletePostBtn');
        if (deletePostBtn) {
            deletePostBtn.addEventListener('click', handleDeletePost);
        }
    }

    function handleEditModalHide() {
        console.log('🚪 Closing edit modal');
        clearAllForms();
        hideLoadingState();
    }

    function initializeFilters() {
        console.log('🔍 Filters initialized');
        // Filter initialization logic here
    }

    function initializeForms() {
        console.log('📝 Form handlers initialized');
        // Form initialization logic here
    }

    // ========== UTILITY FUNCTIONS ==========
    function setPostIdInForms(postId) {
        const idFields = ['editPostId', 'deletePostId'];
        idFields.forEach(fieldId => {
            const field = document.getElementById(fieldId);
            if (field) {
                field.value = postId;
                console.log(`✅ Set ${fieldId} to:`, postId);
            }
        });
    }

    function clearAllForms() {
        clearPostDetailsForm();
        clearPostInfoDisplay();
        clearAnalyticsForm();
    }

    function clearPostDetailsForm() {
        const fields = [
            'editTitle', 'editDescription', 'editContent', 'editImageURL',
            'editStartDate', 'editEndDate', 'editPostedDate'
        ];

        fields.forEach(field => safeSetValue(field, ''));

        const selects = ['editPostType', 'editType', 'editStatus'];
        selects.forEach(select => {
            const element = document.getElementById(select);
            if (element) element.selectedIndex = 0;
        });

        updateImagePreview('');
        console.log('🧹 Post details form cleared');
    }

    function clearPostInfoDisplay() {
        const elements = [
            'postInfoInitials', 'postInfoTitle', 'postInfoType',
            'postInfoStatus', 'postInfoPostType', 'postInfoTitleDetail',
            'postInfoAuthor', 'postInfoCreated', 'postInfoStartDate', 'postInfoEndDate'
        ];

        elements.forEach(elementId => safeUpdateElement(elementId, '--'));
        console.log('🧹 Post info display cleared');
    }

    function clearAnalyticsForm() {
        const fields = ['totalViews', 'totalLikes', 'totalComments', 'totalShares'];
        fields.forEach(field => safeSetValue(field, '0'));
        console.log('🧹 Analytics form cleared');
    }

    function showLoadingState() {
        const modal = document.getElementById('editPostModal');
        if (!modal) return;

        // Create overlay if it doesn't exist
        let overlay = modal.querySelector('.modal-loading-overlay');
        if (!overlay) {
            overlay = document.createElement('div');
            overlay.className = 'modal-loading-overlay';
            overlay.innerHTML = `
                <div class="text-center">
                    <div class="spinner-border text-primary mb-3" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    <div class="text-muted">Loading post data...</div>
                </div>
            `;
            modal.querySelector('.modal-content').appendChild(overlay);
        }

        console.log('⏳ Loading state shown');
    }

    function hideLoadingState() {
        const modal = document.getElementById('editPostModal');
        if (!modal) return;

        const overlay = modal.querySelector('.modal-loading-overlay');
        if (overlay) {
            overlay.remove();
        }

        console.log('✅ Loading state hidden');
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

    function checkAPIConfiguration() {
        console.log('🔧 Checking API configuration...');

        if (!window.appUrls) {
            console.error('❌ window.appUrls is not defined');
            return false;
        }

        console.log('🔧 Available API URLs:', window.appUrls);

        if (!window.appUrls.getPostData) {
            console.error('❌ getPostData URL not configured');
            return false;
        }

        console.log('✅ API configuration OK');
        return true;
    }

    // Helper functions
    function safeSetValue(elementId, value) {
        const element = document.getElementById(elementId);
        if (element) {
            element.value = value || '';
        } else {
            console.warn(`⚠️ Element ${elementId} not found`);
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
                if (select.options[i].value.toLowerCase() === value.toLowerCase()) {
                    select.selectedIndex = i;
                    console.log(`✅ Set ${elementId} to: ${value}`);
                    break;
                }
            }
        } else if (!select) {
            console.warn(`⚠️ Select element ${elementId} not found`);
        }
    }

    function safeGetSelectValue(elementId) {
        const select = document.getElementById(elementId);
        return select ? select.value : '';
    }

    function safeUpdateElement(elementId, value) {
        const element = document.getElementById(elementId);
        if (element) {
            element.textContent = value || '--';
        } else {
            console.warn(`⚠️ Element ${elementId} not found`);
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

    function formatDateForInput(dateString) {
        if (!dateString) return '';
        try {
            const date = new Date(dateString);
            return date.toISOString().split('T')[0];
        } catch (e) {
            return '';
        }
    }

    function formatDateForDisplay(dateString) {
        if (!dateString) return '--';
        try {
            const date = new Date(dateString);
            return date.toLocaleDateString() + ' ' + date.toLocaleTimeString();
        } catch (e) {
            return '--';
        }
    }

    function addCacheBuster(url) {
        if (!url) return url;
        const separator = url.includes('?') ? '&' : '?';
        return `${url}${separator}v=${Date.now()}`;
    }

    // Test functions for debugging
    function testGetPostData(postId) {
        if (!postId) {
            console.log('Usage: testGetPostData("your-post-id")');
            return;
        }

        console.log('🧪 Testing GetPostData with ID:', postId);
        callGetPostDataAPIEnhanced(postId);
    }

    function testTableDataExtraction(postId) {
        console.log('🧪 Testing table data extraction for:', postId);
        const row = findPostRowById(postId);
        if (row) {
            console.log('✅ Found row:', row);
            const data = extractEnhancedTableData(row);
            console.log('📊 Extracted data:', data);
            return data;
        } else {
            console.log('❌ Row not found');
            return null;
        }
    }

    // Expose functions for global access
    window.loadPostData = loadPostDataEnhanced;
    window.loadPostDetailsTab = loadPostDetailsTab;
    window.populateFromTableData = populateFromTableData;
    window.populateFromAPIData = populateFromAPIDataEnhanced;
    window.findPostRowById = findPostRowById;
    window.testGetPostData = testGetPostData;

    console.log('✅ Enhanced Post Management with Details Tab Population fully loaded');
    console.log('🧪 Debug functions: window.postDebug');
});