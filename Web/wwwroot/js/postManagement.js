/**
 * Enhanced Post Management JavaScript with Improved Details Tab Population
 */

document.addEventListener('DOMContentLoaded', function () {
    console.log('🚀 Initializing Enhanced Post Management');

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
        loadPostDetailsTab
    };

    // Verify API configuration
    checkAPIConfiguration();

    console.log('✅ Post Management initialized successfully');

    // ========== MODAL HANDLERS ==========
    function handleEditModalShow(event) {
        const button = event.relatedTarget;
        const postId = button.getAttribute('data-post-id');

        console.log('📂 Opening edit modal for post ID:', postId);

        if (!postId) {
            console.error('❌ No post ID found on button');
            showToast('Error', 'Post ID is missing', 'danger');
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
        console.log('📥 Loading post data for ID:', postId);

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
            showToast('Warning', 'API not configured. Only table data available.', 'warning');
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
                    showToast('Success', 'Post data loaded successfully', 'success');
                } else {
                    console.error('❌ API returned error:', data?.message || 'Unknown error');
                    showToast('Warning', data?.message || 'Failed to load complete post data', 'warning');
                }
            })
            .catch(error => {
                console.error('💥 API Error:', error);
                hideLoadingState();
                showToast('Warning', `API Error: ${error.message}. Using table data only.`, 'warning');
            });
    }

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

            const dataFromCells = {
                title: titleElement?.textContent?.trim() || dataFromAttributes.title,
                description: descElement?.textContent?.trim() || dataFromAttributes.caption,
                type: typeElement?.textContent?.trim() || dataFromAttributes.type,
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

    function populateFromTableData(data) {
        console.log('📝 Populating form from enhanced table data:', data);

        try {
            // Basic text fields with fallbacks
            safeSetValue('editTitle', data.title);
            safeSetValue('editDescription', data.description || data.caption);
            safeSetValue('editContent', data.content);

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
            }
            if (data.endDate) {
                safeSetValue('editEndDate', formatDateForInput(data.endDate));
            }

            // Select fields with proper capitalization
            safeSetSelect('editStatus', capitalizeFirst(data.status) || 'Active');
            safeSetSelect('editPostType', capitalizeFirst(data.type) || 'General');
          

            console.log('✅ Enhanced table data populated successfully');
        } catch (error) {
            console.error('💥 Error populating from table data:', error);
        }
    }

    function updatePostInfoDisplayFromTableData(data) {
        console.log('📊 Updating post info display from enhanced table data:', data);

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

    function populateFromAPIDataEnhanced(data) {
        console.log('🌐 Populating form from enhanced API data:', data);

        try {
            // Handle different possible data structures
            const postData = data.post || data;

            // Basic information with multiple property name fallbacks
            safeSetValue('editTitle', postData.title || postData.Title);
            safeSetValue('editDescription', postData.description || postData.Description);
            safeSetValue('editContent', postData.content || postData.Content);

            // Image handling with multiple sources
            const imageUrl = postData.imageURL || postData.ImageURL || postData.imageUrl;
            if (imageUrl) {
                safeSetValue('editImageURL', imageUrl);
                updateImagePreview(imageUrl);
            }

            // Date fields with proper formatting
            const startDate = postData.startDate || postData.StartDate;
            if (startDate) {
                safeSetValue('editStartDate', formatDateForInput(startDate));
            }

            const endDate = postData.endDate || postData.EndDate;
            if (endDate) {
                safeSetValue('editEndDate', formatDateForInput(endDate));
            }

            // Select fields
            safeSetSelect('editPostType', postData.postType || postData.PostType);
            safeSetSelect('editStatus', postData.status || postData.Status);
         

            console.log('✅ Enhanced API data populated successfully');

            // Update Post Info tab with comprehensive API data
            updatePostInfoDisplayEnhanced(postData);
        } catch (error) {
            console.error('💥 Error populating from API data:', error);
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
            case '#post-info-tab-