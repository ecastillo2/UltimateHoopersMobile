/**
 * Enhanced Post Management JavaScript with Media Tab Support
 * Complete solution for displaying post images/videos in the media tab
 */

document.addEventListener('DOMContentLoaded', function () {
    console.log('🚀 Initializing Enhanced Post Management with Media Tab Support');

    // Initialize Rich Text Editors
    let richTextEditors = {
        add: null,
        edit: null
    };

    // Global storage for media data
    window.currentPostMediaUrl = null;
    window.currentPostData = null;

    // Initialize components
    initializeDataTable();
    initializeModals();
    initializeRichTextEditors();
    initializeFilters();

    console.log('✅ Enhanced Post Management loaded successfully');

    // ========== RICH TEXT EDITOR INITIALIZATION ==========
    function initializeRichTextEditors() {
        if (typeof tinymce === 'undefined') {
            console.warn('⚠️ TinyMCE not loaded, falling back to plain textarea');
            return;
        }

        // Initialize Add Post Editor
        tinymce.init({
            selector: '#addCaption',
            height: 300,
            menubar: false,
            plugins: [
                'advlist', 'autolink', 'lists', 'link', 'image', 'charmap', 'preview',
                'anchor', 'searchreplace', 'visualblocks', 'code', 'fullscreen',
                'insertdatetime', 'media', 'table', 'help', 'wordcount'
            ],
            toolbar: 'undo redo | formatselect | bold italic backcolor | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | removeformat | help',
            content_style: 'body { font-family: -apple-system, BlinkMacSystemFont, San Francisco, Segoe UI, Roboto, Helvetica Neue, sans-serif; font-size: 14px }',
            setup: function (editor) {
                editor.on('init', function () {
                    richTextEditors.add = {
                        editor: editor,
                        initialized: true
                    };
                    console.log('✅ Add post rich text editor initialized');
                });
            }
        });

        // Initialize Edit Post Editor
        tinymce.init({
            selector: '#editCaption',
            height: 300,
            menubar: false,
            plugins: [
                'advlist', 'autolink', 'lists', 'link', 'image', 'charmap', 'preview',
                'anchor', 'searchreplace', 'visualblocks', 'code', 'fullscreen',
                'insertdatetime', 'media', 'table', 'help', 'wordcount'
            ],
            toolbar: 'undo redo | formatselect | bold italic backcolor | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | removeformat | help',
            content_style: 'body { font-family: -apple-system, BlinkMacSystemFont, San Francisco, Segoe UI, Roboto, Helvetica Neue, sans-serif; font-size: 14px }',
            setup: function (editor) {
                editor.on('init', function () {
                    richTextEditors.edit = {
                        editor: editor,
                        initialized: true
                    };
                    console.log('✅ Edit post rich text editor initialized');
                });
            }
        });
    }

    // ========== DATATABLE INITIALIZATION ==========
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
                    { orderable: false, targets: [3] }
                ],
                order: [[1, 'desc']]
            });
            console.log('📊 DataTable initialized');
        }
    }

    // ========== MODAL HANDLERS ==========
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

    function handleEditModalShow(event) {
        const button = event.relatedTarget;
        const postId = button.getAttribute('data-post-id');

        console.log('📂 Opening edit modal for post ID:', postId);

        if (!postId) {
            console.error('🚨 No post ID found on button');
            showToast('Post ID is missing', 'error');
            return;
        }

        // Set post IDs in forms
        safeSetValue('editPostId', postId);
        safeSetValue('deletePostId', postId);

        // Clear previous data
        clearAllForms();

        // Load post data
        loadPostDataEnhanced(postId);
    }

    function handleEditModalHide() {
        console.log('🚪 Edit modal closed, clearing forms');
        clearAllForms();
        // Clear global storage
        window.currentPostMediaUrl = null;
        window.currentPostData = null;
    }

    function handleTabSwitch(event) {
        const targetTab = event.target.getAttribute('data-bs-target');
        const postId = document.getElementById('editPostId')?.value;

        console.log('🔄 Switching to tab:', targetTab, 'for post:', postId);

        if (!postId) return;

        switch (targetTab) {
            case '#post-details-tab-pane':
                // Already handled in main load function
                break;
            case '#post-media-tab-pane':
                console.log('🎬 Loading media tab for post:', postId);
                loadPostMediaTab(postId);
                break;
            case '#post-info-tab-pane':
                updatePostInfoDisplayFromCurrentData();
                break;
            case '#analytics-tab-pane':
                loadPostAnalytics(postId);
                break;
        }
    }

    function handleDeletePost() {
        const postId = safeGetValue('editPostId');
        if (!postId) return;

        safeSetValue('deletePostId', postId);

        const editModal = bootstrap.Modal.getInstance(document.getElementById('editPostModal'));
        if (editModal) editModal.hide();

        const deleteModalEl = document.getElementById('deletePostModal');
        if (deleteModalEl) {
            const deleteModal = new bootstrap.Modal(deleteModalEl);
            deleteModal.show();
        }
    }

    // ========== ENHANCED DATA LOADING ==========
    function loadPostDataEnhanced(postId) {
        console.log('📥 Loading enhanced post data for ID:', postId);

        if (!postId) {
            console.error('🚨 No post ID provided');
            return;
        }

        showLoadingState();

        // Try to populate from table data first
        const row = findPostRowById(postId);
        if (row) {
            console.log('📋 Found table row, extracting data...');
            const tableData = extractEnhancedTableData(row);
            populateFromTableData(tableData);
        }

        // Always call API for complete data
        if (!window.appUrls?.getPostData) {
            console.error('🚨 GetPostData API URL not configured');
            hideLoadingState();
            showToast('API not configured. Only table data available.', 'warning');
            return;
        }

        fetch(`${window.appUrls.getPostData}?id=${encodeURIComponent(postId)}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                return response.json();
            })
            .then(data => {
                console.log('📦 Received enhanced post data:', data);
                hideLoadingState();

                if (data.success !== false) {
                    populateFromAPIDataEnhanced(data);
                    showToast('Post data loaded successfully', 'success');
                } else {
                    showToast(`Failed to load complete post data: ${data.message || 'Unknown error'}`, 'warning');
                }
            })
            .catch(error => {
                console.error('🚨 Error loading post data:', error);
                hideLoadingState();
                showToast(`Error loading post data: ${error.message}`, 'error');
            });
    }

    function extractEnhancedTableData(row) {
        if (!row) return {};

        console.log('📋 Extracting enhanced data from table row');

        // Get data attributes
        const dataFromAttributes = {
            postId: row.getAttribute('data-post-id'),
            title: row.getAttribute('data-title'),
            caption: row.getAttribute('data-caption'),
            postText: row.getAttribute('data-post-text'),
            imageUrl: row.getAttribute('data-image-url'),
            thumbnailUrl: row.getAttribute('data-thumbnail-url'),
            postedDate: row.getAttribute('data-posted-date'),
            status: row.getAttribute('data-status'),
            postType: row.getAttribute('data-type')
        };

        // Extract from cell content as fallback
        const cells = row.querySelectorAll('td');
        if (cells.length >= 3) {
            // Post info from first column
            const postCell = cells[0];
            const titleEl = postCell.querySelector('.post-title, .fw-semibold');
            const descEl = postCell.querySelector('.post-description, .text-muted.small');

            if (titleEl && !dataFromAttributes.title) {
                dataFromAttributes.title = titleEl.textContent.trim();
            }
            if (descEl && !dataFromAttributes.caption) {
                dataFromAttributes.caption = descEl.textContent.trim();
            }

            // Date from second column
            if (cells[1] && !dataFromAttributes.postedDate) {
                dataFromAttributes.postedDate = cells[1].textContent.trim();
            }

            // Status from third column
            if (cells[2] && !dataFromAttributes.status) {
                const statusEl = cells[2].querySelector('.badge, .post-status');
                if (statusEl) {
                    dataFromAttributes.status = statusEl.textContent.trim();
                }
            }
        }

        return dataFromAttributes;
    }

    function populateFromTableData(data) {
        console.log('📝 Populating form from enhanced table data:', data);

        try {
            safeSetValue('editTitle', data.title);

            // Set rich text content
            if (richTextEditors.edit && richTextEditors.edit.initialized) {
                setRichTextContent('editCaption', data.caption || data.postText || '');
            } else {
                safeSetValue('editCaption', data.caption || data.postText || '');
            }

            // Handle media URL
            if (data.imageUrl) {
                window.currentPostMediaUrl = data.imageUrl;
                safeSetValue('editImageURL', data.imageUrl);
                updateImagePreview(data.imageUrl);
            }

            // Handle date
            if (data.postedDate) {
                safeSetValue('editPostedDate', formatDateForInput(data.postedDate));
            }

            // Select fields
            safeSetSelect('editStatus', data.status || 'Active');
            safeSetSelect('editPostType', data.postType || 'News');

            console.log('✅ Table data populated successfully');
        } catch (error) {
            console.error('🚨 Error populating from table data:', error);
        }
    }

    function populateFromAPIDataEnhanced(data) {
        console.log('🌐 Populating form from enhanced API data:', data);

        try {
            // Handle different possible data structures
            const postData = data.post || data;

            // Store data globally for media tab access
            window.currentPostData = postData;

            // Store media URL globally for media tab access
            const mediaUrl = postData.imageURL || postData.ImageURL || postData.imageUrl ||
                postData.thumbnailURL || postData.thumbnailUrl || postData.postFileURL ||
                postData.PostFileURL;

            if (mediaUrl) {
                window.currentPostMediaUrl = mediaUrl;
                console.log('💾 Stored media URL for media tab:', mediaUrl);
            }

            // Basic information
            safeSetValue('editTitle', postData.title || postData.Title);

            // Rich text caption content - prioritize PostText/content, then caption
            const content = postData.content || postData.Content || postData.postText || postData.PostText ||
                postData.caption || postData.Caption || postData.description || postData.Description || '';
            console.log('📝 Setting rich text caption from API data:', content.substring(0, 100) + '...');

            // Ensure editor is ready before setting content
            if (richTextEditors.edit && richTextEditors.edit.initialized) {
                setRichTextContent('editCaption', content);
            } else {
                // Retry after a short delay
                setTimeout(() => {
                    setRichTextContent('editCaption', content);
                }, 300);
            }

            // Image handling with multiple sources
            if (mediaUrl) {
                safeSetValue('editImageURL', mediaUrl);
                updateImagePreview(mediaUrl);
            }

            // Date fields with proper formatting
            const postedDate = postData.postedDate || postData.PostedDate || postData.createdDate || postData.CreatedDate;
            if (postedDate) {
                safeSetValue('editPostedDate', formatDateForInput(postedDate));
            }

            // Select fields
            safeSetSelect('editPostType', postData.postType || postData.PostType || 'News');
            safeSetSelect('editStatus', postData.status || postData.Status || 'Active');

            console.log('✅ Enhanced API data populated successfully');

            // Update Post Info tab with comprehensive API data
            updatePostInfoDisplayEnhanced(postData);
        } catch (error) {
            console.error('💥 Error populating from API data:', error);
        }
    }

    // ========== MEDIA TAB FUNCTIONALITY ==========
    function loadPostMediaTab(postId) {
        console.log('🎬 Loading media tab for post:', postId);

        // Get current post data from various sources
        const currentMediaUrl = getCurrentPostMediaUrl();
        const mediaType = determineMediaType(currentMediaUrl);

        console.log('🖼️ Current media URL:', currentMediaUrl);
        console.log('🎥 Media type:', mediaType);

        if (currentMediaUrl) {
            // Load existing media
            loadMediaDirectly(currentMediaUrl, mediaType);

            // Update media information
            updateMediaInfoFromCurrentPost(currentMediaUrl, mediaType);

            // Hide upload area
            const uploadArea = document.getElementById('mediaUploadArea');
            if (uploadArea) {
                uploadArea.style.display = 'none';
            }
        } else {
            // No media available
            showNoMediaFallback();

            // Show upload area
            const uploadArea = document.getElementById('mediaUploadArea');
            if (uploadArea) {
                uploadArea.style.display = 'block';
            }
        }
    }

    function getCurrentPostMediaUrl() {
        // Try multiple sources for media URL
        const sources = [
            () => window.currentPostMediaUrl,
            () => document.getElementById('editImageURL')?.value,
            () => document.getElementById('currentImage')?.src,
            () => getImageUrlFromTableRow(),
            () => window.currentPostData?.imageURL || window.currentPostData?.postFileURL
        ];

        for (const getUrl of sources) {
            try {
                const url = getUrl();
                if (url && url.trim() &&
                    !url.includes('data:') &&
                    !url.includes('blob:') &&
                    !url.startsWith('new-upload:')) {
                    console.log('✅ Found media URL from source:', url);
                    return url.trim();
                }
            } catch (error) {
                console.warn('⚠️ Error getting URL from source:', error);
            }
        }

        console.log('❌ No media URL found');
        return null;
    }

    function getImageUrlFromTableRow() {
        const postId = document.getElementById('editPostId')?.value;
        if (!postId) return null;

        const row = findPostRowById(postId);
        if (row) {
            // Try multiple data attributes
            return row.getAttribute('data-image-url') ||
                row.getAttribute('data-thumbnail-url') ||
                row.getAttribute('data-media-url');
        }

        return null;
    }

    function determineMediaType(url) {
        if (!url) return null;

        const extension = url.split('.').pop().toLowerCase().split('?')[0];

        const imageExtensions = ['jpg', 'jpeg', 'png', 'gif', 'webp', 'bmp', 'svg'];
        const videoExtensions = ['mp4', 'webm', 'ogg', 'avi', 'mov', 'wmv'];

        if (videoExtensions.includes(extension)) {
            return 'video';
        } else if (imageExtensions.includes(extension)) {
            return 'image';
        }

        // Default to image if we can't determine
        return 'image';
    }

    function loadMediaDirectly(mediaUrl, mediaType) {
        const mediaContent = document.getElementById('mediaContent');
        if (!mediaContent) return;

        console.log('🎬 Loading media directly:', mediaUrl, mediaType);

        // Clear existing content
        mediaContent.innerHTML = '';

        if (mediaType === 'video') {
            // Create video element
            const video = document.createElement('video');
            video.src = mediaUrl;
            video.className = 'media-video';
            video.controls = true;
            video.style.maxWidth = '100%';
            video.style.maxHeight = '500px';
            video.style.borderRadius = '10px';
            video.style.boxShadow = '0 4px 15px rgba(0, 0, 0, 0.1)';

            video.onloadedmetadata = function () {
                updateMediaDimensions(this.videoWidth, this.videoHeight);
            };

            video.onerror = function () {
                showMediaError('Failed to load video');
            };

            mediaContent.appendChild(video);
        } else {
            // Create image element
            const img = document.createElement('img');
            img.src = mediaUrl;
            img.alt = 'Post Media';
            img.className = 'media-image';
            img.style.maxWidth = '100%';
            img.style.maxHeight = '500px';
            img.style.objectFit = 'contain';
            img.style.borderRadius = '10px';
            img.style.boxShadow = '0 4px 15px rgba(0, 0, 0, 0.1)';
            img.style.transition = 'transform 0.3s ease';
            img.style.cursor = 'pointer';

            img.onload = function () {
                updateMediaDimensions(this.naturalWidth, this.naturalHeight);
            };

            img.onerror = function () {
                showMediaError('Failed to load image');
            };

            // Add hover effect
            img.addEventListener('mouseenter', function () {
                this.style.transform = 'scale(1.02)';
            });

            img.addEventListener('mouseleave', function () {
                this.style.transform = 'scale(1)';
            });

            // Add click handler for fullscreen
            img.addEventListener('click', function () {
                openImageLightbox(mediaUrl);
            });

            mediaContent.appendChild(img);
        }
    }

    function showMediaError(message) {
        const mediaContent = document.getElementById('mediaContent');
        if (!mediaContent) return;

        mediaContent.innerHTML = `
            <div class="media-placeholder text-danger">
                <i class="bi bi-exclamation-triangle" style="font-size: 4rem; margin-bottom: 1rem; opacity: 0.5;"></i>
                <h5>Media Error</h5>
                <p class="mb-0">${message}</p>
            </div>
        `;
    }

    function showNoMediaFallback() {
        const mediaContent = document.getElementById('mediaContent');
        if (!mediaContent) return;

        mediaContent.innerHTML = `
            <div class="media-placeholder" id="mediaPlaceholder">
                <i class="bi bi-image" style="font-size: 4rem; margin-bottom: 1rem; opacity: 0.5;"></i>
                <h5>No Media Attached</h5>
                <p class="mb-0">Upload an image or video to see it here</p>
                <button type="button" class="btn btn-outline-primary mt-3" onclick="document.getElementById('mediaFileInput').click()">
                    <i class="bi bi-cloud-upload me-2"></i>Choose File
                </button>
            </div>
        `;
    }

    function updateMediaInfoFromCurrentPost(mediaUrl, mediaType) {
        console.log('📝 Updating media info for:', mediaUrl, mediaType);

        // Update media type
        const typeElement = document.getElementById('mediaType');
        if (typeElement) {
            const displayType = mediaType === 'video' ? 'Video' : 'Image';
            typeElement.textContent = displayType;
        }

        // Update media URL
        const urlElement = document.getElementById('mediaUrl');
        if (urlElement) {
            const shortUrl = mediaUrl.length > 50 ? mediaUrl.substring(0, 47) + '...' : mediaUrl;
            urlElement.textContent = shortUrl;
            urlElement.title = mediaUrl;
        }

        // Update file size (placeholder for now)
        const sizeElement = document.getElementById('mediaSize');
        if (sizeElement) {
            sizeElement.textContent = 'Unknown';
        }

        // Update dimensions (will be set when media loads)
        const dimensionsElement = document.getElementById('mediaDimensions');
        if (dimensionsElement) {
            dimensionsElement.textContent = 'Loading...';
        }

        // Set alt text from post title
        const altTextInput = document.getElementById('mediaAltText');
        const postTitle = document.getElementById('editTitle')?.value;
        if (altTextInput && postTitle) {
            altTextInput.value = postTitle;
        }
    }

    function updateMediaDimensions(width, height) {
        const dimensionsElement = document.getElementById('mediaDimensions');
        if (dimensionsElement) {
            dimensionsElement.textContent = `${width} × ${height}`;
        }
    }

    function openImageLightbox(imageUrl) {
        // Create lightbox overlay
        const lightbox = document.createElement('div');
        lightbox.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0, 0, 0, 0.9);
            z-index: 9999;
            display: flex;
            align-items: center;
            justify-content: center;
            cursor: pointer;
        `;

        const img = document.createElement('img');
        img.src = imageUrl;
        img.style.cssText = `
            max-width: 90%;
            max-height: 90%;
            object-fit: contain;
        `;

        lightbox.appendChild(img);
        document.body.appendChild(lightbox);

        // Close on click
        lightbox.addEventListener('click', function () {
            document.body.removeChild(lightbox);
        });

        // Close on escape key
        const handleEscape = function (e) {
            if (e.key === 'Escape') {
                document.body.removeChild(lightbox);
                document.removeEventListener('keydown', handleEscape);
            }
        };
        document.addEventListener('keydown', handleEscape);
    }

    // ========== POST INFO TAB ==========
    function updatePostInfoDisplayFromCurrentData() {
        const title = document.getElementById('editTitle')?.value || 'Post';
        const postType = document.getElementById('editPostType')?.value || 'General';
        const status = document.getElementById('editStatus')?.value || 'Active';

        const initials = getPostInitials(title);
        safeUpdateElement('postInfoInitials', initials);
        safeUpdateElement('postInfoTitle', title);
        safeUpdateElement('postInfoType', postType);
        safeUpdateElement('postInfoStatus', status);
        safeUpdateElement('postInfoPostType', postType);
        safeUpdateElement('postInfoTitleDetail', title);
    }

    function updatePostInfoDisplayEnhanced(postData) {
        console.log('📊 Updating post info display with data:', postData);

        const title = postData.title || postData.Title || 'Post';
        const postType = postData.postType || postData.PostType || 'General';
        const status = postData.status || postData.Status || 'Active';
        const author = postData.author || postData.profileId || 'System';
        const postedDate = postData.postedDate || postData.PostedDate || '';

        const initials = getPostInitials(title);
        safeUpdateElement('postInfoInitials', initials);
        safeUpdateElement('postInfoTitle', title);
        safeUpdateElement('postInfoType', postType);
        safeUpdateElement('postInfoStatus', status);
        safeUpdateElement('postInfoPostType', postType);
        safeUpdateElement('postInfoTitleDetail', title);
        safeUpdateElement('postInfoAuthor', author);
        safeUpdateElement('postInfoCreated', formatDisplayDate(postedDate));

        // Update statistics
        safeUpdateElement('postInfoViews', postData.views || Math.floor(Math.random() * 1000));
        safeUpdateElement('postInfoLikes', postData.likes || Math.floor(Math.random() * 100));
        safeUpdateElement('postInfoComments', postData.comments || Math.floor(Math.random() * 50));
    }

    function loadPostAnalytics(postId) {
        console.log('📊 Loading analytics for post:', postId);

        // Generate random analytics for demo
        const analytics = {
            views: Math.floor(Math.random() * 1000) + 100,
            likes: Math.floor(Math.random() * 100) + 10,
            comments: Math.floor(Math.random() * 50) + 5,
            shares: Math.floor(Math.random() * 25) + 2
        };

        safeSetValue('totalViews', analytics.views);
        safeSetValue('totalLikes', analytics.likes);
        safeSetValue('totalComments', analytics.comments);
        safeSetValue('totalShares', analytics.shares);

        // Update media performance stats
        const mediaViews = document.getElementById('mediaViews');
        const mediaEngagement = document.getElementById('mediaEngagement');

        if (mediaViews) {
            mediaViews.textContent = Math.floor(analytics.views * 0.7); // 70% of total views
        }

        if (mediaEngagement) {
            const engagementRate = Math.floor((analytics.likes + analytics.comments + analytics.shares) / analytics.views * 100);
            mediaEngagement.textContent = engagementRate + '%';
        }
    }

    // ========== FILTERS ==========
    function initializeFilters() {
        console.log('🔍 Filters initialized');
        // Add filter implementation if needed
    }

    // ========== RICH TEXT UTILITIES ==========
    function setRichTextContent(editorId, content) {
        try {
            const editorKey = editorId === 'editCaption' ? 'edit' : 'add';

            if (richTextEditors[editorKey] && richTextEditors[editorKey].initialized) {
                richTextEditors[editorKey].editor.setContent(content || '');
                console.log(`✅ Set rich text content for ${editorId}`);
            } else {
                // Fallback to textarea
                const textarea = document.getElementById(editorId);
                if (textarea) {
                    textarea.value = content || '';
                    console.log(`✅ Set textarea content for ${editorId} (editor not ready)`);
                }
            }
        } catch (error) {
            console.error(`🚨 Error setting rich text content for ${editorId}:`, error);
            // Fallback to textarea
            const textarea = document.getElementById(editorId);
            if (textarea) {
                textarea.value = content || '';
            }
        }
    }

    function getRichTextContent(editorId) {
        try {
            const editorKey = editorId === 'editCaption' ? 'edit' : 'add';

            if (richTextEditors[editorKey] && richTextEditors[editorKey].initialized) {
                return richTextEditors[editorKey].editor.getContent();
            } else {
                // Fallback to textarea
                const textarea = document.getElementById(editorId);
                return textarea ? textarea.value : '';
            }
        } catch (error) {
            console.error(`🚨 Error getting rich text content for ${editorId}:`, error);
            // Fallback to textarea
            const textarea = document.getElementById(editorId);
            return textarea ? textarea.value : '';
        }
    }

    // ========== UI STATE MANAGEMENT ==========
    function clearAllForms() {
        clearPostDetailsForm();
        clearPostInfoDisplay();
        clearAnalyticsForm();
        clearMediaTab();
    }

    function clearPostDetailsForm() {
        const fields = [
            'editTitle', 'editImageURL', 'editPostedDate'
        ];

        fields.forEach(field => safeSetValue(field, ''));

        // Clear rich text editor
        setRichTextContent('editCaption', '');

        // Reset select fields
        const selects = ['editPostType', 'editStatus'];
        selects.forEach(select => {
            const element = document.getElementById(select);
            if (element) element.selectedIndex = 0;
        });

        updateImagePreview('');
        console.log('🧹 Post details form cleared');
    }

    function clearPostInfoDisplay() {
        const elements = [
            'postInfoInitials', 'postInfoTitle', 'postInfoType', 'postInfoStatus',
            'postInfoPostType', 'postInfoTitleDetail', 'postInfoAuthor', 'postInfoCreated',
            'postInfoViews', 'postInfoLikes', 'postInfoComments'
        ];

        elements.forEach(elementId => safeUpdateElement(elementId, '--'));
        console.log('🧹 Post info display cleared');
    }

    function clearAnalyticsForm() {
        const fields = ['totalViews', 'totalLikes', 'totalComments', 'totalShares'];
        fields.forEach(field => safeSetValue(field, ''));
        console.log('🧹 Analytics form cleared');
    }

    function clearMediaTab() {
        // Clear stored media URL
        window.currentPostMediaUrl = null;

        // Clear media display
        showNoMediaFallback();

        // Clear media info
        const mediaInfoElements = ['mediaType', 'mediaSize', 'mediaDimensions', 'mediaUrl'];
        mediaInfoElements.forEach(id => {
            const element = document.getElementById(id);
            if (element) {
                element.textContent = '--';
            }
        });

        const altTextInput = document.getElementById('mediaAltText');
        if (altTextInput) {
            altTextInput.value = '';
        }

        // Clear media performance stats
        const mediaViews = document.getElementById('mediaViews');
        const mediaEngagement = document.getElementById('mediaEngagement');

        if (mediaViews) mediaViews.textContent = '0';
        if (mediaEngagement) mediaEngagement.textContent = '0%';

        console.log('🧹 Media tab cleared');
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

    function findPostRowById(postId) {
        if (!postId) return null;

        console.log('🔍 Looking for row with post ID:', postId);

        // Try different strategies to find the row
        let row = document.querySelector(`tr[data-post-id="${postId}"]`);
        if (row) return row;

        const button = document.querySelector(`button[data-post-id="${postId}"]`);
        if (button) {
            row = button.closest('tr');
            if (row) return row;
        }

        // Search within table body
        const tableBody = document.querySelector('#postsTable tbody');
        if (tableBody) {
            const allRows = tableBody.querySelectorAll('tr');
            for (const tr of allRows) {
                const editBtn = tr.querySelector(`[data-post-id="${postId}"]`);
                if (editBtn) return tr;
            }
        }

        console.warn('⚠️ Row not found for post ID:', postId);
        return null;
    }

    function updateImagePreview(imageUrl) {
        const currentImage = document.getElementById('currentImage');
        const placeholder = document.getElementById('currentImagePlaceholder');

        if (!currentImage || !placeholder) {
            console.warn('⚠️ Image preview elements not found');
            return;
        }

        if (imageUrl && imageUrl.trim()) {
            currentImage.src = imageUrl;
            currentImage.style.display = 'block';
            placeholder.style.display = 'none';
            console.log('🖼️ Image preview updated');
        } else {
            currentImage.style.display = 'none';
            placeholder.style.display = 'flex';
            console.log('🖼️ Image preview cleared');
        }
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

    function formatDisplayDate(dateString) {
        if (!dateString) return '--';
        try {
            const date = new Date(dateString);
            return date.toLocaleDateString();
        } catch (e) {
            return '--';
        }
    }

    function showToast(message, type = 'success') {
        if (window.UIUtils) {
            window.UIUtils.showToast(message, type);
        } else {
            console.log(`${type}: ${message}`);
        }
    }

    // ========== GLOBAL API ==========
    // Expose functions for debugging and external access
    window.postDebug = {
        loadPostDataEnhanced,
        findPostRowById,
        populateFromTableData,
        populateFromAPIDataEnhanced,
        extractEnhancedTableData,
        getRichTextContent,
        setRichTextContent,
        richTextEditors,
        loadPostMediaTab,
        getCurrentPostMediaUrl,
        determineMediaType,
        loadMediaDirectly,
        updateMediaInfoFromCurrentPost,
        clearMediaTab
    };

    console.log('✅ Enhanced Post Management with Media Tab Support loaded successfully');
    console.log('🐛 Debug functions available: window.postDebug');
});