/**
 * Enhanced Post Management JavaScript with Rich Text Caption Field
 * Content and PostText removed - Caption is now the rich text field
 */

document.addEventListener('DOMContentLoaded', function () {
    console.log('🚀 Initializing Post Management with Rich Text Caption');

    // Rich text editor instances storage
    let richTextEditors = {
        add: null,
        edit: null
    };

    // Initialize components
    initializeDataTable();
    initializeModals();
    initializeFilters();
    initializeForms();
    initializeRichTextEditors();

    console.log('✅ Post Management with Rich Text Caption initialized successfully');

    // ========== RICH TEXT EDITOR INITIALIZATION ==========
    function initializeRichTextEditors() {
        console.log('📝 Initializing TinyMCE rich text editors for caption fields...');

        // Common TinyMCE configuration
        const commonConfig = {
            height: 300,
            menubar: false,
            plugins: [
                'advlist', 'autolink', 'lists', 'link', 'image', 'charmap', 'preview',
                'anchor', 'searchreplace', 'visualblocks', 'code', 'fullscreen',
                'insertdatetime', 'media', 'table', 'help', 'wordcount', 'emoticons'
            ],
            toolbar: 'undo redo | blocks | bold italic forecolor backcolor | ' +
                'alignleft aligncenter alignright alignjustify | ' +
                'bullist numlist outdent indent | removeformat | help | ' +
                'link image media | code preview fullscreen',
            content_style: `
                body { 
                    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; 
                    font-size: 14px; 
                    line-height: 1.6;
                    margin: 1rem;
                }
                h1, h2, h3, h4, h5, h6 { margin-top: 1rem; margin-bottom: 0.5rem; }
                p { margin-bottom: 0.75rem; }
                ul, ol { margin-bottom: 0.75rem; }
            `,
            placeholder: 'Write your post content here...',
            skin: 'oxide',
            content_css: 'default',
            branding: false,
            promotion: false,
            resize: 'vertical',
            browser_spellcheck: true,
            contextmenu: 'link image table',
            images_upload_handler: function (blobInfo, success, failure) {
                console.log('📷 Image upload requested:', blobInfo.filename());
                const reader = new FileReader();
                reader.onload = function (e) {
                    success(e.target.result);
                };
                reader.readAsDataURL(blobInfo.blob());
            },
            paste_data_images: true,
            paste_as_text: false,
            link_assume_external_targets: true,
            target_list: [
                { title: 'New window', value: '_blank' },
                { title: 'Same window', value: '_self' }
            ]
        };

        // Initialize Add Post Caption Editor
        tinymce.init({
            ...commonConfig,
            selector: '#addCaption',
            setup: function (editor) {
                editor.on('init', function () {
                    console.log('✅ Add post caption rich text editor initialized');
                    richTextEditors.add = editor;
                });

                editor.on('change', function () {
                    validateForm('addPostForm');
                });
            }
        }).catch(error => {
            console.error('❌ Failed to initialize add post caption editor:', error);
            fallbackToTextarea('addCaption');
        });

        // Initialize Edit Post Caption Editor
        return initializeEditCaptionEditor(commonConfig);
    }

    function initializeEditCaptionEditor(config) {
        console.log('📝 Initializing edit post caption rich text editor...');

        return tinymce.init({
            ...config,
            selector: '#editCaption',
            setup: function (editor) {
                editor.on('init', function () {
                    console.log('✅ Edit post caption rich text editor initialized');
                    richTextEditors.edit = editor;

                    // Trigger content refresh if modal is already open
                    const modal = document.getElementById('editPostModal');
                    if (modal && modal.classList.contains('show')) {
                        console.log('📝 Modal already open, refreshing content');
                        const currentContent = editor.getContent();
                        if (!currentContent || currentContent.trim() === '') {
                            const postId = safeGetValue('editPostId');
                            if (postId) {
                                refreshPostContent(postId);
                            }
                        }
                    }
                });

                editor.on('change', function () {
                    validateForm('editPostForm');
                });
            }
        }).then(function (editors) {
            if (editors && editors.length > 0) {
                richTextEditors.edit = editors[0];
                console.log('✅ Edit caption rich text editor setup complete');
            }
            return editors;
        }).catch(error => {
            console.error('❌ Failed to initialize edit post caption editor:', error);
            fallbackToTextarea('editCaption');
            return null;
        });
    }

    function fallbackToTextarea(elementId) {
        console.warn('⚠️ Falling back to regular textarea for:', elementId);
        const textarea = document.getElementById(elementId);
        if (textarea) {
            textarea.rows = 8;
            textarea.placeholder = 'Rich text editor failed to load. You can still enter content here.';
            if (window.UIUtils) {
                window.UIUtils.showWarning('Rich text editor failed to load. Using basic text input.', 'Warning');
            }
        }
    }

    // ========== RICH TEXT CONTENT MANAGEMENT ==========
    function getRichTextContent(editorId) {
        const editorKey = editorId === 'addCaption' ? 'add' : 'edit';
        const editor = richTextEditors[editorKey];

        if (editor && editor.getContent) {
            const content = editor.getContent();
            console.log(`📖 Got rich text content from ${editorId}:`, content.substring(0, 100) + '...');
            return content;
        } else {
            const textarea = document.getElementById(editorId);
            const content = textarea ? textarea.value : '';
            console.log(`📖 Got fallback content from ${editorId}:`, content.substring(0, 100) + '...');
            return content;
        }
    }

    function setRichTextContent(editorId, content) {
        console.log(`📝 Setting rich text content for ${editorId}:`, content ? content.substring(0, 100) + '...' : 'empty');

        const editorKey = editorId === 'addCaption' ? 'add' : 'edit';
        const editor = richTextEditors[editorKey];

        if (editor && editor.setContent) {
            try {
                editor.setContent(content || '');
                console.log(`✅ Rich text content set for ${editorId}`);
            } catch (error) {
                console.error(`❌ Error setting rich text content for ${editorId}:`, error);
                // Fallback to textarea
                const textarea = document.getElementById(editorId);
                if (textarea) {
                    textarea.value = content || '';
                }
            }
        } else {
            console.warn(`⚠️ Rich text editor not available for ${editorId}, using textarea fallback`);
            const textarea = document.getElementById(editorId);
            if (textarea) {
                textarea.value = content || '';
                console.log(`✅ Fallback content set for ${editorId}`);
            }
        }
    }

    function clearRichTextContent(editorId) {
        console.log(`🧹 Clearing rich text content for ${editorId}`);
        setRichTextContent(editorId, '');
    }

    // ========== MODAL HANDLERS ==========
    function handleEditModalShow(event) {
        const button = event.relatedTarget;
        const postId = button.getAttribute('data-post-id');

        console.log('📂 Opening edit modal for post ID:', postId);

        if (!postId) {
            console.error('❌ No post ID found on button');
            if (window.UIUtils) {
                window.UIUtils.showError('Post ID is missing', 'Error');
            }
            return;
        }

        // Set post ID in all forms
        setPostIdInForms(postId);

        // Clear previous data
        clearAllForms();

        // Show loading state
        showLoadingState();

        // Small delay to ensure rich text editor is ready
        setTimeout(() => {
            loadPostDataEnhanced(postId);
        }, 100);
    }

    function handleEditModalHide() {
        console.log('🚪 Closing edit modal');
        clearAllForms();
        hideLoadingState();
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

    // ========== DATA LOADING FUNCTIONS ==========
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
        }

        // Step 2: Always call API for complete and accurate data
        if (window.appUrls?.getPostData) {
            callGetPostDataAPIEnhanced(postId);
        } else {
            console.error('❌ GetPostData API URL not configured');
            hideLoadingState();
            if (window.UIUtils) {
                window.UIUtils.showWarning('API not configured. Only table data available.', 'Warning');
            }
        }
    }

    function refreshPostContent(postId) {
        console.log('🔄 Refreshing post content for ID:', postId);
        if (window.appUrls?.getPostData) {
            callGetPostDataAPIEnhanced(postId);
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
                    console.log('✅ API call successful, populating form');
                    populateFromAPIDataEnhanced(data);
                    if (window.UIUtils) {
                        window.UIUtils.showSuccess('Post data loaded successfully', 'Success');
                    }
                } else {
                    console.error('❌ API returned error:', data?.message || 'Unknown error');
                    if (window.UIUtils) {
                        window.UIUtils.showWarning(data?.message || 'Failed to load complete post data', 'Warning');
                    }
                }
            })
            .catch(error => {
                console.error('💥 API Error:', error);
                hideLoadingState();
                if (window.UIUtils) {
                    window.UIUtils.showWarning(`API Error: ${error.message}. Using table data only.`, 'Warning');
                }
            });
    }

    function extractEnhancedTableData(row) {
        if (!row) return {};

        console.log('📋 Extracting enhanced data from row:', row);

        // Get data attributes with fallbacks
        const dataFromAttributes = {
            postId: row.getAttribute('data-post-id'),
            title: row.getAttribute('data-title'),
            caption: row.getAttribute('data-caption'),
            postText: row.getAttribute('data-post-text'),
            imageUrl: row.getAttribute('data-image-url'),
            thumbnailUrl: row.getAttribute('data-thumbnail-url'),
            status: row.getAttribute('data-status'),
            type: row.getAttribute('data-type'),
            postType: row.getAttribute('data-type'),
            author: row.getAttribute('data-author'),
            date: row.getAttribute('data-date'),
            postedDate: row.getAttribute('data-posted-date')
        };

        console.log('📊 Enhanced data from attributes:', dataFromAttributes);

        // Extract from cell content as fallback
        const cells = row.querySelectorAll('td');
        if (cells.length >= 3) {
            // Extract from the post column (first column)
            const postCell = cells[0];
            const titleElement = postCell.querySelector('.post-title, .fw-semibold');
            const descriptionElement = postCell.querySelector('.post-description, .text-muted.small');

            // Extract from date column
            const dateCell = cells[1];
            const dateText = dateCell.textContent.trim();

            // Extract from status column
            const statusCell = cells[2];
            const statusElement = statusCell.querySelector('.badge, .post-status');

            const dataFromCells = {
                title: titleElement?.textContent?.trim() || dataFromAttributes.title,
                caption: descriptionElement?.textContent?.trim() || dataFromAttributes.caption,
                date: dateText || dataFromAttributes.date,
                status: statusElement?.textContent?.trim() || dataFromAttributes.status
            };

            console.log('📊 Data from cells:', dataFromCells);

            // Merge data, preferring attributes but falling back to cell content
            return {
                ...dataFromCells,
                ...Object.fromEntries(Object.entries(dataFromAttributes).filter(([_, v]) => v != null))
            };
        }

        return dataFromAttributes;
    }

    function populateFromTableData(data) {
        console.log('📝 Populating form from enhanced table data:', data);

        try {
            // Basic text fields
            safeSetValue('editTitle', data.title);

            // Rich text caption content - prioritize postText, then caption
            const content = data.postText || data.caption || '';
            console.log('📝 Setting rich text caption from table data:', content.substring(0, 100) + '...');

            // Wait a bit for editor to be ready, then set content
            setTimeout(() => {
                setRichTextContent('editCaption', content);
            }, 200);

            // Image handling with multiple sources
            const imageUrl = data.imageUrl || data.thumbnailUrl;
            if (imageUrl) {
                safeSetValue('editImageURL', imageUrl);
                updateImagePreview(imageUrl);
            }

            // Date fields with proper formatting
            if (data.postedDate || data.date) {
                const dateToUse = data.postedDate || data.date;
                safeSetValue('editPostedDate', formatDateForInput(dateToUse));
            }

            // Select fields with proper capitalization
            safeSetSelect('editStatus', capitalizeFirst(data.status) || 'Active');
            safeSetSelect('editPostType', capitalizeFirst(data.type || data.postType) || 'News');

            console.log('✅ Enhanced table data with rich text caption populated successfully');
        } catch (error) {
            console.error('💥 Error populating from table data:', error);
        }
    }

    function populateFromAPIDataEnhanced(data) {
        console.log('🌐 Populating form from enhanced API data:', data);

        try {
            // Handle different possible data structures
            const postData = data.post || data;

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
            const imageUrl = postData.imageURL || postData.ImageURL || postData.imageUrl || postData.thumbnailURL || postData.thumbnailUrl;
            if (imageUrl) {
                safeSetValue('editImageURL', imageUrl);
                updateImagePreview(imageUrl);
            }

            // Date fields with proper formatting
            const postedDate = postData.postedDate || postData.PostedDate || postData.createdDate || postData.CreatedDate;
            if (postedDate) {
                safeSetValue('editPostedDate', formatDateForInput(postedDate));
            }

            // Select fields
            safeSetSelect('editPostType', postData.postType || postData.PostType || 'News');
            safeSetSelect('editStatus', postData.status || postData.Status || 'Active');

            console.log('✅ Enhanced API data with rich text caption populated successfully');

            // Update Post Info tab with comprehensive API data
            updatePostInfoDisplayEnhanced(postData);
        } catch (error) {
            console.error('💥 Error populating from API data:', error);
        }
    }

    function updatePostInfoDisplayEnhanced(postData) {
        console.log('📊 Updating enhanced post info display');

        // Avatar and basic info
        const initials = getPostInitials(postData.title || postData.Title);
        safeUpdateElement('postInfoInitials', initials);
        safeUpdateElement('postInfoTitle', postData.title || postData.Title || 'Post');
        safeUpdateElement('postInfoType', postData.postType || postData.PostType || 'General');

        // Badges
        safeUpdateElement('postInfoStatus', postData.status || postData.Status || 'Active');
        safeUpdateElement('postInfoPostType', postData.postType || postData.PostType || 'General');

        // Detailed information
        safeUpdateElement('postInfoTitleDetail', postData.title || postData.Title || '--');
        safeUpdateElement('postInfoAuthor', postData.author || postData.profileId || postData.ProfileId || 'System');
        safeUpdateElement('postInfoCreated', formatDate(postData.postedDate || postData.PostedDate || postData.createdDate));

        // Statistics
        safeUpdateElement('postInfoViews', postData.views || 0);
        safeUpdateElement('postInfoLikes', postData.likes || 0);
        safeUpdateElement('postInfoComments', postData.comments || 0);

        // Analytics tab
        safeSetValue('totalViews', postData.views || 0);
        safeSetValue('totalLikes', postData.likes || 0);
        safeSetValue('totalComments', postData.comments || 0);
        safeSetValue('totalShares', postData.shares || 0);

        console.log('✅ Enhanced post info display updated');
    }

    function updatePostInfoDisplayFromTableData(tableData) {
        console.log('📋 Updating post info display from table data');

        const initials = getPostInitials(tableData.title);
        safeUpdateElement('postInfoInitials', initials);
        safeUpdateElement('postInfoTitle', tableData.title || 'Post');
        safeUpdateElement('postInfoType', tableData.type || tableData.postType || 'General');
        safeUpdateElement('postInfoStatus', tableData.status || 'Active');
        safeUpdateElement('postInfoPostType', tableData.type || tableData.postType || 'General');
        safeUpdateElement('postInfoTitleDetail', tableData.title || '--');
        safeUpdateElement('postInfoAuthor', tableData.author || 'System');
        safeUpdateElement('postInfoCreated', formatDate(tableData.date || tableData.postedDate));

        console.log('✅ Post info display updated from table data');
    }

    // ========== FORM SUBMISSION ==========
    function handleFormSubmission(formId) {
        console.log(`📤 Handling form submission for: ${formId}`);

        const form = document.getElementById(formId);
        if (!form) return false;

        // Get rich text content from caption field and ensure it's included in form data
        const captionFieldId = formId === 'addPostForm' ? 'addCaption' : 'editCaption';
        const richTextContent = getRichTextContent(captionFieldId);

        // Update the textarea with rich text content for form submission
        const captionField = document.getElementById(captionFieldId);
        if (captionField) {
            captionField.value = richTextContent;
        }

        console.log(`✅ Rich text caption content prepared for submission: ${richTextContent.substring(0, 100)}...`);

        // Validate form
        return validateForm(formId);
    }

    function validateForm(formId) {
        const form = document.getElementById(formId);
        if (!form) return true;

        let isValid = true;
        const errors = [];

        // Validate title
        const titleField = form.querySelector('[name="Title"]');
        if (!titleField?.value?.trim()) {
            errors.push('Title is required');
            isValid = false;
        }

        // Validate rich text caption content
        const captionFieldId = formId === 'addPostForm' ? 'addCaption' : 'editCaption';
        const richTextContent = getRichTextContent(captionFieldId);

        // Strip HTML tags for validation
        const textContent = richTextContent.replace(/<[^>]*>/g, '').trim();
        if (!textContent) {
            errors.push('Content is required');
            isValid = false;
        }

        // Validate post type
        const postTypeField = form.querySelector('[name="PostType"]');
        if (!postTypeField?.value?.trim()) {
            errors.push('Post type is required');
            isValid = false;
        }

        // Show validation errors
        if (!isValid && window.UIUtils) {
            window.UIUtils.showError('Please fix the following errors:\n• ' + errors.join('\n• '), 'Validation Error');
        }

        return isValid;
    }

    // ========== UTILITY FUNCTIONS ==========
    function findPostRowById(postId) {
        if (!postId) return null;

        console.log('🔍 Looking for row with post ID:', postId);

        // Strategy 1: Direct row attribute search
        let row = document.querySelector(`tr[data-post-id="${postId}"]`);
        if (row) {
            console.log('✅ Found row by data-post-id');
            return row;
        }

        // Strategy 2: Button-based search
        const button = document.querySelector(`button[data-post-id="${postId}"]`);
        if (button) {
            row = button.closest('tr');
            if (row) {
                console.log('✅ Found row via button');
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
                    console.log('✅ Found row via table body search');
                    return tr;
                }
            }
        }

        console.warn('⚠️ Row not found for post ID:', postId);
        return null;
    }

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
                console.log('✅ Image preview updated successfully');
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
            console.log('✅ Image preview cleared');
        }
    }

    function showLoadingState() {
        const modal = document.getElementById('editPostModal');
        if (!modal) return;

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

        const formElements = modal.querySelectorAll('input, select, textarea, button[type="submit"]');
        formElements.forEach(el => el.disabled = true);

        console.log('⏳ Loading state shown');
    }

    function hideLoadingState() {
        const modal = document.getElementById('editPostModal');
        if (!modal) return;

        const overlay = modal.querySelector('.modal-loading-overlay');
        if (overlay) {
            overlay.remove();
        }

        const formElements = modal.querySelectorAll('input, select, textarea, button[type="submit"]');
        formElements.forEach(el => el.disabled = false);

        console.log('✅ Loading state hidden');
    }

    function clearAllForms() {
        clearPostDetailsForm();
        clearPostInfoDisplay();
        clearAnalyticsForm();
    }

    function clearPostDetailsForm() {
        const fields = [
            'editTitle', 'editImageURL', 'editPostedDate'
        ];

        fields.forEach(field => safeSetValue(field, ''));

        // Clear rich text caption content
        clearRichTextContent('editCaption');

        const selects = ['editPostType', 'editStatus'];
        selects.forEach(select => {
            const element = document.getElementById(select);
            if (element) element.selectedIndex = 0;
        });

        updateImagePreview('');
        console.log('🧹 Post details form with rich text caption cleared');
    }

    function clearPostInfoDisplay() {
        const elements = [
            'postInfoInitials', 'postInfoTitle', 'postInfoType',
            'postInfoStatus', 'postInfoPostType', 'postInfoTitleDetail',
            'postInfoAuthor', 'postInfoCreated'
        ];

        elements.forEach(elementId => safeUpdateElement(elementId, '--'));
        console.log('🧹 Post info display cleared');
    }

    function clearAnalyticsForm() {
        const fields = ['totalViews', 'totalLikes', 'totalComments', 'totalShares'];
        fields.forEach(field => safeSetValue(field, '0'));
        console.log('🧹 Analytics form cleared');
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
                    { orderable: false, targets: [-1] }
                ],
                order: [[1, 'desc']]
            });
            console.log('📊 DataTable initialized');
        }
    }

    function initializeModals() {
        const editPostModal = document.getElementById('editPostModal');
        if (editPostModal) {
            editPostModal.addEventListener('show.bs.modal', handleEditModalShow);
            editPostModal.addEventListener('hidden.bs.modal', handleEditModalHide);

            console.log('📝 Modal event handlers initialized');
        }

        // Delete button handler
        const deletePostBtn = document.getElementById('deletePostBtn');
        if (deletePostBtn) {
            deletePostBtn.addEventListener('click', handleDeletePost);
        }
    }

    function initializeForms() {
        console.log('📝 Form handlers initialized');

        // Add form submission handlers
        const addPostForm = document.getElementById('addPostForm');
        if (addPostForm) {
            addPostForm.addEventListener('submit', function (e) {
                if (!handleFormSubmission('addPostForm')) {
                    e.preventDefault();
                    return false;
                }
            });
        }

        const editPostForm = document.getElementById('editPostForm');
        if (editPostForm) {
            editPostForm.addEventListener('submit', function (e) {
                e.preventDefault();

                if (!handleFormSubmission('editPostForm')) {
                    return false;
                }

                // Handle AJAX submission for edit form
                handleEditFormSubmission(this);
            });
        }
    }

    function handleEditFormSubmission(form) {
        const formData = new FormData(form);
        const submitBtn = form.querySelector('button[type="submit"]');
        const originalText = submitBtn.innerHTML;

        if (window.UIUtils) {
            window.UIUtils.setButtonLoading(submitBtn, true, 'Saving...');
        }

        fetch(form.action, {
            method: 'POST',
            body: formData
        })
            .then(response => response.json())
            .then(data => {
                if (window.UIUtils) {
                    window.UIUtils.setButtonLoading(submitBtn, false);
                }

                if (data.success) {
                    if (window.UIUtils) {
                        window.UIUtils.showSuccess(data.message, 'Success');
                    }

                    setTimeout(() => {
                        const modal = bootstrap.Modal.getInstance(document.getElementById('editPostModal'));
                        if (modal) modal.hide();
                        location.reload(); // Refresh to show updated data
                    }, 1000);
                } else {
                    if (window.UIUtils) {
                        window.UIUtils.showError(data.message || 'Failed to save post', 'Error');
                    }
                }
            })
            .catch(error => {
                console.error('Error saving post:', error);
                if (window.UIUtils) {
                    window.UIUtils.setButtonLoading(submitBtn, false);
                    window.UIUtils.showError('Error saving post: ' + error.message, 'Error');
                }
            });
    }

    function initializeFilters() {
        console.log('🔍 Filters initialized');
        // Initialize filter functionality here if needed
    }

    // ========== HELPER FUNCTIONS ==========
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
                if (select.options[i].value.toLowerCase() === value.toLowerCase()) {
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

    function formatDate(dateString) {
        if (!dateString) return '--';
        try {
            const date = new Date(dateString);
            return date.toLocaleDateString();
        } catch (e) {
            return '--';
        }
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

    function addCacheBuster(url) {
        if (!url) return url;
        const separator = url.includes('?') ? '&' : '?';
        return `${url}${separator}v=${Date.now()}`;
    }

    // Debug functions for testing
    window.postDebug = {
        loadPostDataEnhanced,
        findPostRowById,
        populateFromTableData,
        populateFromAPIDataEnhanced,
        extractEnhancedTableData,
        getRichTextContent,
        setRichTextContent,
        richTextEditors,
        refreshPostContent
    };

    console.log('🐛 Debug functions available: window.postDebug');
});