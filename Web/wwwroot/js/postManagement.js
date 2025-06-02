/**
 * Enhanced Post Management JavaScript with TinyMCE Rich Text Editor
 * Includes proper initialization, data handling, and cleanup for rich text content
 */

document.addEventListener('DOMContentLoaded', function () {
    console.log('🚀 Initializing Enhanced Post Management with Rich Text Editor');

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
        extractEnhancedTableData,
        getRichTextContent,
        setRichTextContent,
        richTextEditors
    };

    // Verify API configuration
    checkAPIConfiguration();

    console.log('✅ Enhanced Post Management with Rich Text initialized successfully');

    // ========== RICH TEXT EDITOR INITIALIZATION ==========
    function initializeRichTextEditors() {
        console.log('📝 Initializing TinyMCE rich text editors...');

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
            placeholder: 'Start writing your content here...',
            skin: 'oxide',
            content_css: 'default',
            branding: false,
            promotion: false,
            resize: 'vertical',
            browser_spellcheck: true,
            contextmenu: 'link image table',
            // Image handling
            images_upload_handler: function (blobInfo, success, failure) {
                // Handle image uploads here if needed
                console.log('📷 Image upload requested:', blobInfo.filename());

                // For now, just convert to data URL
                const reader = new FileReader();
                reader.onload = function (e) {
                    success(e.target.result);
                };
                reader.readAsDataURL(blobInfo.blob());
            },
            // Paste handling
            paste_data_images: true,
            paste_as_text: false,
            // Link handling
            link_assume_external_targets: true,
            target_list: [
                { title: 'New window', value: '_blank' },
                { title: 'Same window', value: '_self' }
            ]
        };

        // Initialize Add Post Editor
        tinymce.init({
            ...commonConfig,
            selector: '#addContent',
            setup: function (editor) {
                editor.on('init', function () {
                    console.log('✅ Add post rich text editor initialized');
                    richTextEditors.add = editor;
                });

                editor.on('change', function () {
                    // Trigger form validation if needed
                    validateForm('addPostForm');
                });
            }
        }).catch(error => {
            console.error('❌ Failed to initialize add post rich text editor:', error);
            fallbackToTextarea('addContent');
        });

        // Initialize Edit Post Editor (will be done when modal opens)
        // We'll initialize this dynamically when the edit modal is shown
    }

    function initializeEditRichTextEditor() {
        console.log('📝 Initializing edit post rich text editor...');

        if (richTextEditors.edit) {
            console.log('✅ Edit rich text editor already initialized');
            return Promise.resolve(richTextEditors.edit);
        }

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
            placeholder: 'Edit your post content here...',
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

        return tinymce.init({
            ...commonConfig,
            selector: '#editContent',
            setup: function (editor) {
                editor.on('init', function () {
                    console.log('✅ Edit post rich text editor initialized');
                    richTextEditors.edit = editor;
                });

                editor.on('change', function () {
                    validateForm('editPostForm');
                });
            }
        }).catch(error => {
            console.error('❌ Failed to initialize edit post rich text editor:', error);
            fallbackToTextarea('editContent');
            return null;
        });
    }

    function fallbackToTextarea(elementId) {
        console.warn('⚠️ Falling back to regular textarea for:', elementId);
        const textarea = document.getElementById(elementId);
        if (textarea) {
            textarea.rows = 8;
            textarea.placeholder = 'Rich text editor failed to load. You can still enter content here.';
            UIUtils.showWarning('Rich text editor failed to load. Using basic text input.', 'Warning');
        }
    }

    // ========== RICH TEXT CONTENT MANAGEMENT ==========
    function getRichTextContent(editorId) {
        const editorKey = editorId === 'addContent' ? 'add' : 'edit';
        const editor = richTextEditors[editorKey];

        if (editor) {
            const content = editor.getContent();
            console.log(`📖 Got rich text content from ${editorId}:`, content.substring(0, 100) + '...');
            return content;
        } else {
            // Fallback to textarea value
            const textarea = document.getElementById(editorId);
            const content = textarea ? textarea.value : '';
            console.log(`📖 Got fallback content from ${editorId}:`, content.substring(0, 100) + '...');
            return content;
        }
    }

    function setRichTextContent(editorId, content) {
        console.log(`📝 Setting rich text content for ${editorId}:`, content ? content.substring(0, 100) + '...' : 'empty');

        const editorKey = editorId === 'addContent' ? 'add' : 'edit';
        const editor = richTextEditors[editorKey];

        if (editor) {
            editor.setContent(content || '');
            console.log(`✅ Rich text content set for ${editorId}`);
        } else {
            // Fallback to textarea
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

        // Initialize edit rich text editor if not already done
        initializeEditRichTextEditor().then(() => {
            // Load post data with enhanced functionality
            loadPostDataEnhanced(postId);
        });
    }

    function handleEditModalHide() {
        console.log('🚪 Closing edit modal');
        clearAllForms();
        hideLoadingState();

        // Clean up edit rich text editor to prevent memory leaks
        if (richTextEditors.edit) {
            try {
                tinymce.get('editContent')?.remove();
                richTextEditors.edit = null;
                console.log('🧹 Edit rich text editor cleaned up');
            } catch (error) {
                console.warn('⚠️ Error cleaning up edit rich text editor:', error);
            }
        }
    }

    // ========== ENHANCED FORM POPULATION WITH RICH TEXT ==========
    function populateFromTableData(data) {
        console.log('📝 Populating form from enhanced table data:', data);

        try {
            // Basic text fields with fallbacks
            safeSetValue('editTitle', data.title);
            safeSetValue('editDescription', data.description || data.caption);

            // Rich text content - handle HTML content properly
            const content = data.content || data.description || data.caption || '';
            setRichTextContent('editContent', content);

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

            console.log('✅ Enhanced table data with rich text populated successfully');
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

            // Rich text content - handle HTML content properly
            const content = postData.content || postData.Content || postData.description || postData.Description || '';
            setRichTextContent('editContent', content);

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

            console.log('✅ Enhanced API data with rich text populated successfully');

            // Update Post Info tab with comprehensive API data
            updatePostInfoDisplayEnhanced(postData);
        } catch (error) {
            console.error('💥 Error populating from API data:', error);
        }
    }

    function populatePostDetailsForm(data) {
        console.log('📝 Populating post details form with rich text:', data);

        try {
            const postData = data.post || data;

            // Populate all form fields in the details tab
            safeSetValue('editPostId', postData.postId || postData.PostId);
            safeSetValue('editTitle', postData.title || postData.Title);
            safeSetValue('editDescription', postData.description || postData.Description || postData.caption || postData.Caption);

            // Rich text content
            const content = postData.content || postData.Content || postData.description || postData.Description || '';
            setRichTextContent('editContent', content);

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

            console.log('✅ Post details form with rich text populated successfully');
        } catch (error) {
            console.error('🚨 Error populating post details form with rich text:', error);
            UIUtils.showError('Error populating post details form', 'Error');
        }
    }

    // ========== ENHANCED FORM SUBMISSION ==========
    function handleFormSubmission(formId) {
        console.log(`📤 Handling form submission for: ${formId}`);

        const form = document.getElementById(formId);
        if (!form) return;

        // Get rich text content and ensure it's included in form data
        const contentFieldId = formId === 'addPostForm' ? 'addContent' : 'editContent';
        const richTextContent = getRichTextContent(contentFieldId);

        // Update the hidden field or textarea with rich text content
        const contentField = document.getElementById(contentFieldId);
        if (contentField) {
            contentField.value = richTextContent;
        }

        console.log(`✅ Rich text content prepared for submission: ${richTextContent.substring(0, 100)}...`);

        // Continue with normal form submission
        return true;
    }

    // ========== FORM VALIDATION ==========
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

        // Validate rich text content
        const contentFieldId = formId === 'addPostForm' ? 'addContent' : 'editContent';
        const richTextContent = getRichTextContent(contentFieldId);

        // Strip HTML tags for validation
        const textContent = richTextContent.replace(/<[^>]*>/g, '').trim();
        if (!textContent) {
            errors.push('Content is required');
            isValid = false;
        }

        // Show validation errors
        if (!isValid) {
            UIUtils.showError('Please fix the following errors:\n• ' + errors.join('\n• '), 'Validation Error');
        }

        return isValid;
    }

    // ========== CLEANUP FUNCTIONS ==========
    function clearAllForms() {
        clearPostDetailsForm();
        clearPostInfoDisplay();
        clearAnalyticsForm();
    }

    function clearPostDetailsForm() {
        const fields = [
            'editTitle', 'editDescription', 'editImageURL',
            'editStartDate', 'editEndDate', 'editPostedDate'
        ];

        fields.forEach(field => safeSetValue(field, ''));

        // Clear rich text content
        clearRichTextContent('editContent');

        const selects = ['editPostType', 'editType', 'editStatus'];
        selects.forEach(select => {
            const element = document.getElementById(select);
            if (element) element.selectedIndex = 0;
        });

        updateImagePreview('');
        console.log('🧹 Post details form with rich text cleared');
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
                order: [[2, 'desc']]
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

    function initializeForms() {
        console.log('📝 Form handlers initialized');

        // Add form submission handlers
        const addPostForm = document.getElementById('addPostForm');
        if (addPostForm) {
            addPostForm.addEventListener('submit', function (e) {
                if (!handleFormSubmission('addPostForm') || !validateForm('addPostForm')) {
                    e.preventDefault();
                    return false;
                }
            });
        }

        const editPostForm = document.getElementById('editPostForm');
        if (editPostForm) {
            editPostForm.addEventListener('submit', function (e) {
                if (!handleFormSubmission('editPostForm') || !validateForm('editPostForm')) {
                    e.preventDefault();
                    return false;
                }
            });
        }
    }

    function initializeFilters() {
        console.log('🔍 Filters initialized');
    }

    // ========== OTHER REQUIRED FUNCTIONS ==========
    // Include all the other functions from the previous implementation
    // (loadPostDataEnhanced, extractEnhancedTableData, findPostRowById, etc.)
    // These remain the same as in the previous artifact

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

    // ... Include all other utility functions from the previous implementation ...
    // (These are the same as before, just ensuring rich text content is handled properly)

    // Export functions for debugging
    window.loadPostData = loadPostDataEnhanced;
    window.getRichTextContent = getRichTextContent;
    window.setRichTextContent = setRichTextContent;

    console.log('✅ Enhanced Post Management with Rich Text Editor fully loaded');
});