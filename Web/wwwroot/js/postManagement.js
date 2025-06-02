/**
 * Enhanced Post Media Tab Integration
 * Updates postManagement.js to properly load media in the media tab
 */

// Add this to your postManagement.js file, replacing the existing media tab handling

// ========== ENHANCED MEDIA TAB INTEGRATION ==========
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

/**
 * Load media tab with current post data
 */
function loadPostMediaTab(postId) {
    console.log('🎬 Loading media tab for post:', postId);

    // Get current post data from the form
    const currentMediaUrl = getCurrentPostMediaUrl();
    const mediaType = determineMediaType(currentMediaUrl);

    console.log('🖼️ Current media URL:', currentMediaUrl);
    console.log('🎥 Media type:', mediaType);

    if (currentMediaUrl) {
        // Load existing media
        if (window.PostMediaHandler) {
            window.PostMediaHandler.loadExistingMedia(currentMediaUrl, mediaType);
        } else {
            // Fallback if PostMediaHandler is not available
            loadMediaDirectly(currentMediaUrl, mediaType);
        }

        // Update media information
        updateMediaInfoFromCurrentPost(currentMediaUrl, mediaType);
    } else {
        // No media available
        if (window.PostMediaHandler) {
            window.PostMediaHandler.showNoMediaPlaceholder();
        } else {
            showNoMediaFallback();
        }

        // Show upload area
        const uploadArea = document.getElementById('mediaUploadArea');
        if (uploadArea) {
            uploadArea.style.display = 'block';
        }
    }
}

/**
 * Get current post media URL from various possible sources
 */
function getCurrentPostMediaUrl() {
    // Try multiple sources for media URL
    const sources = [
        () => document.getElementById('editImageURL')?.value,
        () => document.getElementById('currentImage')?.src,
        () => getImageUrlFromTableRow(),
        () => getMediaUrlFromAPIData()
    ];

    for (const getUrl of sources) {
        try {
            const url = getUrl();
            if (url && url.trim() && !url.includes('data:') && !url.includes('blob:')) {
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

/**
 * Get image URL from table row data
 */
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

/**
 * Get media URL from stored API data (if available)
 */
function getMediaUrlFromAPIData() {
    // This would be set when API data is loaded
    return window.currentPostMediaUrl || null;
}

/**
 * Determine media type from URL
 */
function determineMediaType(url) {
    if (!url) return null;

    const extension = url.split('.').pop().toLowerCase().split('?')[0];

    const imageExtensions = ['jpg', 'jpeg', 'png', 'gif', 'webp', 'bmp', 'svg'];
    const videoExtensions = ['mp4', 'webm', 'ogg', 'avi', 'mov', 'wmv'];

    if (imageExtensions.includes(extension)) {
        return 'image';
    } else if (videoExtensions.includes(extension)) {
        return 'video';
    }

    // Default to image if we can't determine
    return 'image';
}

/**
 * Load media directly (fallback if PostMediaHandler is not available)
 */
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

        img.onload = function () {
            updateMediaDimensions(this.naturalWidth, this.naturalHeight);
        };

        img.onerror = function () {
            showMediaError('Failed to load image');
        };

        // Add hover effect
        img.addEventListener('mouseenter', function () {
            this.style.transform = 'scale(1.05)';
        });

        img.addEventListener('mouseleave', function () {
            this.style.transform = 'scale(1)';
        });

        mediaContent.appendChild(img);
    }

    // Hide upload area
    const uploadArea = document.getElementById('mediaUploadArea');
    if (uploadArea) {
        uploadArea.style.display = 'none';
    }
}

/**
 * Show media error
 */
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

/**
 * Show no media fallback
 */
function showNoMediaFallback() {
    const mediaContent = document.getElementById('mediaContent');
    if (!mediaContent) return;

    mediaContent.innerHTML = `
        <div class="media-placeholder" id="mediaPlaceholder">
            <i class="bi bi-image" style="font-size: 4rem; margin-bottom: 1rem; opacity: 0.5;"></i>
            <h5>No Media Attached</h5>
            <p class="mb-0">Upload an image or video to see it here</p>
        </div>
    `;
}

/**
 * Update media information from current post
 */
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

    // Update file size (placeholder)
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

/**
 * Update media dimensions
 */
function updateMediaDimensions(width, height) {
    const dimensionsElement = document.getElementById('mediaDimensions');
    if (dimensionsElement) {
        dimensionsElement.textContent = `${width} × ${height}`;
    }
}

/**
 * Update post info display from current form data
 */
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

/**
 * Load post analytics (placeholder)
 */
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
}

// ========== ENHANCED API DATA HANDLING ==========
// Update the existing populateFromAPIDataEnhanced function to store media URL
function populateFromAPIDataEnhanced(data) {
    console.log('🌐 Populating form from enhanced API data:', data);

    try {
        // Handle different possible data structures
        const postData = data.post || data;

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

        console.log('✅ Enhanced API data with media URL populated successfully');

        // Update Post Info tab with comprehensive API data
        updatePostInfoDisplayEnhanced(postData);
    } catch (error) {
        console.error('💥 Error populating from API data:', error);
    }
}

// ========== INITIALIZATION UPDATES ==========
// Make sure to initialize the tab switching properly
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

        console.log('📝 Modal event handlers with media tab support initialized');
    }

    // Delete button handler
    const deletePostBtn = document.getElementById('deletePostBtn');
    if (deletePostBtn) {
        deletePostBtn.addEventListener('click', handleDeletePost);
    }
}

// ========== CLEAR FUNCTIONS UPDATE ==========
function clearAllForms() {
    clearPostDetailsForm();
    clearPostInfoDisplay();
    clearAnalyticsForm();
    clearMediaTab();
}

function clearMediaTab() {
    // Clear stored media URL
    window.currentPostMediaUrl = null;

    // Clear media display
    if (window.PostMediaHandler) {
        window.PostMediaHandler.showNoMediaPlaceholder();
    } else {
        showNoMediaFallback();
    }

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

    // Update the handleTabSwitch function
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

// Add new function to load media tab
function loadPostMediaTab(postId) {
    console.log('🎬 Loading media tab for post:', postId);
    
    // Get current post data from the form
    const currentMediaUrl = getCurrentPostMediaUrl();
    const mediaType = determineMediaType(currentMediaUrl);
    
    console.log('🖼️ Current media URL:', currentMediaUrl);
    console.log('🎥 Media type:', mediaType);
    
    if (currentMediaUrl) {
        // Load existing media
        if (window.PostMediaHandler) {
            window.PostMediaHandler.loadExistingMedia(currentMediaUrl, mediaType);
        } else {
            console.warn('⚠️ PostMediaHandler not available, using fallback');
            loadMediaDirectly(currentMediaUrl, mediaType);
        }
        
        // Update media information
        updateMediaInfoFromCurrentPost(currentMediaUrl, mediaType);
    } else {
        // No media available
        if (window.PostMediaHandler) {
            window.PostMediaHandler.showNoMediaPlaceholder();
        } else {
            showNoMediaFallback();
        }
        
        // Show upload area
        const uploadArea = document.getElementById('mediaUploadArea');
        if (uploadArea) {
            uploadArea.style.display = 'block';
        }
    }
}

// Add function to get current post media URL
function getCurrentPostMediaUrl() {
    // Try multiple sources for media URL
    const sources = [
        () => document.getElementById('editImageURL')?.value,
        () => document.getElementById('currentImage')?.src,
        () => getImageUrlFromTableRow(),
        () => window.currentPostMediaUrl // Set during API data loading
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

// Add function to get image URL from table row
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

// Add function to determine media type
function determineMediaType(url) {
    if (!url) return null;
    
    const extension = url.split('.').pop().toLowerCase().split('?')[0];
    
    const imageExtensions = ['jpg', 'jpeg', 'png', 'gif', 'webp', 'bmp', 'svg'];
    const videoExtensions = ['mp4', 'webm', 'ogg', 'avi', 'mov', 'wmv'];
    
    if (imageExtensions.includes(extension)) {
        return 'image';
    } else if (videoExtensions.includes(extension)) {
        return 'video';
    }
    
    // Default to image if we can't determine
    return 'image';
}

// Add fallback media loading function
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
        
        video.onloadedmetadata = function() {
            updateMediaDimensions(this.videoWidth, this.videoHeight);
        };
        
        video.onerror = function() {
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
        
        img.onload = function() {
            updateMediaDimensions(this.naturalWidth, this.naturalHeight);
        };
        
        img.onerror = function() {
            showMediaError('Failed to load image');
        };
        
        // Add hover effect
        img.addEventListener('mouseenter', function() {
            this.style.transform = 'scale(1.02)';
        });
        
        img.addEventListener('mouseleave', function() {
            this.style.transform = 'scale(1)';
        });
        
        mediaContent.appendChild(img);
    }
    
    // Hide upload area
    const uploadArea = document.getElementById('mediaUploadArea');
    if (uploadArea) {
        uploadArea.style.display = 'none';
    }
}

// Add media error display function
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

// Add no media fallback function
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

// Add media info update function
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
    
    // Update file size (placeholder)
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

// Add media dimensions update function
function updateMediaDimensions(width, height) {
    const dimensionsElement = document.getElementById('mediaDimensions');
    if (dimensionsElement) {
        dimensionsElement.textContent = `${width} × ${height}`;
    }
}

// Update the existing populateFromAPIDataEnhanced function to store media URL
// Replace your existing populateFromAPIDataEnhanced function with this enhanced version:
function populateFromAPIDataEnhanced(data) {
    console.log('🌐 Populating form from enhanced API data:', data);

    try {
        // Handle different possible data structures
        const postData = data.post || data;

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

        console.log('✅ Enhanced API data with media URL populated successfully');

        // Update Post Info tab with comprehensive API data
        updatePostInfoDisplayEnhanced(postData);
    } catch (error) {
        console.error('💥 Error populating from API data:', error);
    }
}

// Update post info display from current form data
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

// Load post analytics (placeholder)
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

// Update the clearAllForms function to include media tab clearing
function clearAllForms() {
    clearPostDetailsForm();
    clearPostInfoDisplay();
    clearAnalyticsForm();
    clearMediaTab();
}

// Add media tab clearing function
function clearMediaTab() {
    // Clear stored media URL
    window.currentPostMediaUrl = null;
    
    // Clear media display
    if (window.PostMediaHandler) {
        window.PostMediaHandler.showNoMediaPlaceholder();
    } else {
        showNoMediaFallback();
    }
    
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

// Update the debug functions to include media functionality
window.postDebug = {
    loadPostDataEnhanced,
    findPostRowById,
    populateFromTableData,
    populateFromAPIDataEnhanced,
    extractEnhancedTableData,
    getRichTextContent,
    setRichTextContent,
    richTextEditors,
    refreshPostContent,
    // New media debug functions
    loadPostMediaTab,
    getCurrentPostMediaUrl,
    determineMediaType,
    loadMediaDirectly,
    updateMediaInfoFromCurrentPost,
    clearMediaTab
};

console.log('✅ Enhanced Post Management with Media Tab Integration loaded successfully');
console.log('🐛 Debug functions available: window.postDebug');

    console.log('🧹 Media tab cleared');
}