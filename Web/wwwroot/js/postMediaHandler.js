/**
 * Enhanced Post Media Handler JavaScript
 * Updated to better integrate with post data and handle various media types
 */

document.addEventListener('DOMContentLoaded', function () {
    console.log('🎬 Initializing Enhanced Post Media Handler');

    let currentMediaData = null;
    let isFullscreen = false;

    // Initialize media tab functionality
    initializeMediaTab();

    function initializeMediaTab() {
        initializeMediaControls();
        initializeMediaUpload();
        initializeTabSwitching();
        console.log('✅ Enhanced media tab initialized');
    }

    // ========== ENHANCED MEDIA CONTROLS ==========
    function initializeMediaControls() {
        // Download button
        const downloadBtn = document.getElementById('downloadMediaBtn');
        if (downloadBtn) {
            downloadBtn.addEventListener('click', downloadMedia);
        }

        // Fullscreen button
        const fullscreenBtn = document.getElementById('fullscreenMediaBtn');
        if (fullscreenBtn) {
            fullscreenBtn.addEventListener('click', toggleFullscreen);
        }

        // Replace media button
        const replaceBtn = document.getElementById('replaceMediaBtn');
        if (replaceBtn) {
            replaceBtn.addEventListener('click', showMediaUpload);
        }

        // Edit image button
        const editBtn = document.getElementById('editImageBtn');
        if (editBtn) {
            editBtn.addEventListener('click', editImage);
        }

        // Generate thumbnail button
        const thumbnailBtn = document.getElementById('generateThumbnailBtn');
        if (thumbnailBtn) {
            thumbnailBtn.addEventListener('click', generateThumbnail);
        }

        // Remove media button
        const removeBtn = document.getElementById('removeMediaBtn');
        if (removeBtn) {
            removeBtn.addEventListener('click', removeMedia);
        }

        // Alt text input
        const altTextInput = document.getElementById('mediaAltText');
        if (altTextInput) {
            altTextInput.addEventListener('blur', updateAltText);
        }
    }

    // ========== ENHANCED MEDIA UPLOAD ==========
    function initializeMediaUpload() {
        const uploadArea = document.getElementById('mediaUploadArea');
        const fileInput = document.getElementById('mediaFileInput');

        if (!uploadArea || !fileInput) return;

        // File input change
        fileInput.addEventListener('change', handleFileSelect);

        // Drag and drop
        uploadArea.addEventListener('dragover', handleDragOver);
        uploadArea.addEventListener('dragleave', handleDragLeave);
        uploadArea.addEventListener('drop', handleDrop);
        uploadArea.addEventListener('click', () => fileInput.click());

        console.log('📤 Enhanced media upload initialized');
    }

    function handleDragOver(e) {
        e.preventDefault();
        e.stopPropagation();
        e.currentTarget.classList.add('dragover');
    }

    function handleDragLeave(e) {
        e.preventDefault();
        e.stopPropagation();
        e.currentTarget.classList.remove('dragover');
    }

    function handleDrop(e) {
        e.preventDefault();
        e.stopPropagation();
        e.currentTarget.classList.remove('dragover');

        const files = e.dataTransfer.files;
        if (files.length > 0) {
            handleFileUpload(files[0]);
        }
    }

    function handleFileSelect(e) {
        const file = e.target.files[0];
        if (file) {
            handleFileUpload(file);
        }
    }

    function handleFileUpload(file) {
        console.log('📁 Handling file upload:', file.name);

        // Validate file
        if (!validateMediaFile(file)) {
            return;
        }

        // Create preview
        createMediaPreview(file);

        // Update media info
        updateMediaInfo(file);

        // Hide upload area, show media
        hideMediaUpload();

        // Update the post form with new media
        updatePostFormWithNewMedia(file);

        // Show success message
        if (window.UIUtils) {
            window.UIUtils.showSuccess(`${file.name} uploaded successfully!`, 'Upload Complete');
        }
    }

    function validateMediaFile(file) {
        const maxSize = 10 * 1024 * 1024; // 10MB
        const allowedTypes = [
            'image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp', 'image/bmp',
            'video/mp4', 'video/webm', 'video/ogg', 'video/avi', 'video/mov'
        ];

        if (file.size > maxSize) {
            if (window.UIUtils) {
                window.UIUtils.showError('File size must be less than 10MB', 'File Too Large');
            }
            return false;
        }

        if (!allowedTypes.includes(file.type)) {
            if (window.UIUtils) {
                window.UIUtils.showError('File type not supported. Please use JPG, PNG, GIF, MP4, WebM, or other supported formats.', 'Invalid File Type');
            }
            return false;
        }

        return true;
    }

    // ========== ENHANCED MEDIA DISPLAY ==========
    function createMediaPreview(file) {
        const mediaContent = document.getElementById('mediaContent');
        if (!mediaContent) return;

        // Clear existing content
        mediaContent.innerHTML = '';

        const fileURL = URL.createObjectURL(file);

        if (file.type.startsWith('image/')) {
            createImagePreview(fileURL, file.name);
        } else if (file.type.startsWith('video/')) {
            createVideoPreview(fileURL, file.name);
        }

        // Store current media data
        currentMediaData = {
            file: file,
            url: fileURL,
            type: file.type.startsWith('image/') ? 'image' : 'video',
            name: file.name,
            size: file.size,
            isNew: true
        };
    }

    function createImagePreview(url, filename) {
        const mediaContent = document.getElementById('mediaContent');

        const img = document.createElement('img');
        img.src = url;
        img.alt = filename;
        img.className = 'media-image';
        img.loading = 'lazy';
        img.style.cursor = 'pointer';

        // Add click handler for fullscreen
        img.addEventListener('click', () => {
            toggleFullscreen();
        });

        img.onload = function () {
            // Update dimensions info
            updateMediaDimensions(this.naturalWidth, this.naturalHeight);

            // Add hover effect
            this.addEventListener('mouseenter', function () {
                this.style.transform = 'scale(1.02)';
            });

            this.addEventListener('mouseleave', function () {
                this.style.transform = 'scale(1)';
            });
        };

        img.onerror = function () {
            console.error('Failed to load image');
            showMediaError('Failed to load image');
        };

        mediaContent.appendChild(img);
        console.log('🖼️ Enhanced image preview created');
    }

    function createVideoPreview(url, filename) {
        const mediaContent = document.getElementById('mediaContent');

        const video = document.createElement('video');
        video.src = url;
        video.className = 'media-video';
        video.controls = true;
        video.preload = 'metadata';

        video.onloadedmetadata = function () {
            // Update dimensions info
            updateMediaDimensions(this.videoWidth, this.videoHeight);

            // Update file size info if available
            updateMediaSize(currentMediaData?.size);
        };

        video.onerror = function () {
            console.error('Failed to load video');
            showMediaError('Failed to load video');
        };

        mediaContent.appendChild(video);
        console.log('🎥 Enhanced video preview created');
    }

    function loadExistingMedia(mediaUrl, mediaType) {
        if (!mediaUrl) {
            showNoMediaPlaceholder();
            return;
        }

        console.log('📱 Loading existing media:', mediaUrl, 'Type:', mediaType);

        const mediaContent = document.getElementById('mediaContent');
        if (!mediaContent) return;

        // Clear existing content
        mediaContent.innerHTML = '';

        // Determine media type if not provided
        if (!mediaType) {
            mediaType = determineMediaTypeFromUrl(mediaUrl);
        }

        // Create appropriate media element
        if (mediaType === 'video' || mediaType.startsWith('video/')) {
            createVideoPreview(mediaUrl, 'Post Video');
        } else {
            // Default to image
            createImagePreview(mediaUrl, 'Post Image');
        }

        // Update media info for existing media
        updateMediaInfoFromUrl(mediaUrl, mediaType);
        hideMediaUpload();

        // Store current media data
        currentMediaData = {
            url: mediaUrl,
            type: mediaType.startsWith('image/') ? 'image' : (mediaType.startsWith('video/') ? 'video' : mediaType),
            name: getFilenameFromUrl(mediaUrl),
            isExisting: true
        };

        console.log('✅ Existing media loaded successfully');
    }

    function determineMediaTypeFromUrl(url) {
        const extension = url.split('.').pop().toLowerCase().split('?')[0];

        const imageExtensions = ['jpg', 'jpeg', 'png', 'gif', 'webp', 'bmp', 'svg'];
        const videoExtensions = ['mp4', 'webm', 'ogg', 'avi', 'mov', 'wmv'];

        if (videoExtensions.includes(extension)) {
            return 'video';
        } else if (imageExtensions.includes(extension)) {
            return 'image';
        }

        // Default to image
        return 'image';
    }

    function showNoMediaPlaceholder() {
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

        // Clear media info
        clearMediaInfo();
        currentMediaData = null;

        // Show upload area
        showMediaUpload();
    }

    function showMediaError(message) {
        const mediaContent = document.getElementById('mediaContent');
        if (!mediaContent) return;

        mediaContent.innerHTML = `
            <div class="media-placeholder text-danger">
                <i class="bi bi-exclamation-triangle" style="font-size: 4rem; margin-bottom: 1rem; opacity: 0.5;"></i>
                <h5>Media Error</h5>
                <p class="mb-0">${message}</p>
                <button type="button" class="btn btn-outline-danger mt-3" onclick="window.PostMediaHandler.showNoMediaPlaceholder()">
                    <i class="bi bi-arrow-clockwise me-2"></i>Try Again
                </button>
            </div>
        `;
    }

    // ========== ENHANCED MEDIA INFO MANAGEMENT ==========
    function updateMediaInfo(file) {
        // Update type
        const typeElement = document.getElementById('mediaType');
        if (typeElement) {
            const type = file.type.startsWith('image/') ? 'Image' : 'Video';
            typeElement.textContent = `${type} (${file.type})`;
        }

        // Update size
        updateMediaSize(file.size);

        // Update URL (will be temporary blob URL)
        const urlElement = document.getElementById('mediaUrl');
        if (urlElement) {
            urlElement.textContent = 'New upload (not saved yet)';
            urlElement.title = file.name;
        }

        // Set alt text from file name
        const altTextInput = document.getElementById('mediaAltText');
        if (altTextInput) {
            const altText = file.name.replace(/\.[^/.]+$/, "").replace(/[-_]/g, ' ');
            altTextInput.value = altText;
        }
    }

    function updateMediaInfoFromUrl(url, type) {
        // Update type
        const typeElement = document.getElementById('mediaType');
        if (typeElement) {
            const displayType = type.startsWith('image/') || type === 'image' ? 'Image' : 'Video';
            typeElement.textContent = displayType;
        }

        // Update URL
        const urlElement = document.getElementById('mediaUrl');
        if (urlElement) {
            const shortUrl = url.length > 50 ? url.substring(0, 47) + '...' : url;
            urlElement.textContent = shortUrl;
            urlElement.title = url;
        }

        // Size and dimensions will be updated when media loads
        const sizeElement = document.getElementById('mediaSize');
        if (sizeElement) {
            sizeElement.textContent = 'Loading...';
        }

        const dimensionsElement = document.getElementById('mediaDimensions');
        if (dimensionsElement) {
            dimensionsElement.textContent = 'Loading...';
        }

        // Set alt text from post title if available
        const altTextInput = document.getElementById('mediaAltText');
        const postTitle = document.getElementById('editTitle')?.value;
        if (altTextInput && postTitle) {
            altTextInput.value = postTitle;
        }
    }

    function updateMediaSize(sizeInBytes) {
        const sizeElement = document.getElementById('mediaSize');
        if (sizeElement && sizeInBytes) {
            sizeElement.textContent = formatFileSize(sizeInBytes);
        }
    }

    function updateMediaDimensions(width, height) {
        const dimensionsElement = document.getElementById('mediaDimensions');
        if (dimensionsElement) {
            dimensionsElement.textContent = `${width} × ${height}`;
        }
    }

    function clearMediaInfo() {
        const elements = ['mediaType', 'mediaSize', 'mediaDimensions', 'mediaUrl'];
        elements.forEach(id => {
            const element = document.getElementById(id);
            if (element) {
                element.textContent = '--';
            }
        });

        const altTextInput = document.getElementById('mediaAltText');
        if (altTextInput) {
            altTextInput.value = '';
        }
    }

    // ========== ENHANCED MEDIA ACTIONS ==========
    function downloadMedia() {
        if (!currentMediaData) {
            if (window.UIUtils) {
                window.UIUtils.showWarning('No media to download', 'No Media');
            }
            return;
        }

        console.log('⬇️ Downloading media:', currentMediaData.name);

        const link = document.createElement('a');
        link.href = currentMediaData.url;
        link.download = currentMediaData.name || 'post-media';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        if (window.UIUtils) {
            window.UIUtils.showSuccess('Download started', 'Download');
        }
    }

    function toggleFullscreen() {
        if (!currentMediaData) return;

        const mediaContent = document.getElementById('mediaContent');
        const mediaElement = mediaContent.querySelector('.media-image, .media-video');

        if (!mediaElement) return;

        if (!isFullscreen) {
            // Enter fullscreen
            if (mediaElement.requestFullscreen) {
                mediaElement.requestFullscreen();
            } else if (mediaElement.webkitRequestFullscreen) {
                mediaElement.webkitRequestFullscreen();
            } else if (mediaElement.msRequestFullscreen) {
                mediaElement.msRequestFullscreen();
            }
            isFullscreen = true;
        } else {
            // Exit fullscreen
            if (document.exitFullscreen) {
                document.exitFullscreen();
            } else if (document.webkitExitFullscreen) {
                document.webkitExitFullscreen();
            } else if (document.msExitFullscreen) {
                document.msExitFullscreen();
            }
            isFullscreen = false;
        }
    }

    function showMediaUpload() {
        const uploadArea = document.getElementById('mediaUploadArea');
        if (uploadArea) {
            uploadArea.style.display = 'block';
            console.log('📤 Media upload area shown');
        }
    }

    function hideMediaUpload() {
        const uploadArea = document.getElementById('mediaUploadArea');
        if (uploadArea) {
            uploadArea.style.display = 'none';
        }
    }

    function editImage() {
        if (!currentMediaData || currentMediaData.type !== 'image') {
            if (window.UIUtils) {
                window.UIUtils.showWarning('Image editing is only available for images', 'Not Available');
            }
            return;
        }

        // This would integrate with an image editor
        if (window.UIUtils) {
            window.UIUtils.showInfo('Image editing feature coming soon!', 'Feature Preview');
        }
        console.log('✏️ Image editing requested');
    }

    function generateThumbnail() {
        if (!currentMediaData) {
            if (window.UIUtils) {
                window.UIUtils.showWarning('No media available for thumbnail generation', 'No Media');
            }
            return;
        }

        // This would generate a thumbnail
        if (window.UIUtils) {
            window.UIUtils.showInfo('Thumbnail generation feature coming soon!', 'Feature Preview');
        }
        console.log('🖼️ Thumbnail generation requested');
    }

    function removeMedia() {
        if (!currentMediaData) {
            if (window.UIUtils) {
                window.UIUtils.showWarning('No media to remove', 'No Media');
            }
            return;
        }

        if (confirm('Are you sure you want to remove this media? This action cannot be undone.')) {
            console.log('🗑️ Removing media');

            // Clear current media
            currentMediaData = null;
            showNoMediaPlaceholder();
            showMediaUpload();

            // Clear file input
            const fileInput = document.getElementById('mediaFileInput');
            if (fileInput) {
                fileInput.value = '';
            }

            // Clear the main form media fields
            const imageUrlField = document.getElementById('editImageURL');
            if (imageUrlField) {
                imageUrlField.value = '';
            }

            // Clear preview in details tab
            const currentImage = document.getElementById('currentImage');
            const placeholder = document.getElementById('currentImagePlaceholder');
            if (currentImage && placeholder) {
                currentImage.style.display = 'none';
                placeholder.style.display = 'flex';
            }

            if (window.UIUtils) {
                window.UIUtils.showSuccess('Media removed successfully', 'Removed');
            }
        }
    }

    function updateAltText() {
        const altTextInput = document.getElementById('mediaAltText');
        if (!altTextInput || !currentMediaData) return;

        const altText = altTextInput.value.trim();
        console.log('📝 Alt text updated:', altText);

        // Update the actual media element
        const mediaElement = document.querySelector('.media-image');
        if (mediaElement) {
            mediaElement.alt = altText;
        }
    }

    // ========== ENHANCED FORM INTEGRATION ==========
    function updatePostFormWithNewMedia(file) {
        // Create a data URL for immediate preview in details tab
        const reader = new FileReader();
        reader.onload = function (e) {
            const dataUrl = e.target.result;

            // Update the image URL field (temporary)
            const imageUrlField = document.getElementById('editImageURL');
            if (imageUrlField) {
                imageUrlField.value = 'new-upload:' + file.name;
            }

            // Update preview in details tab
            const currentImage = document.getElementById('currentImage');
            const placeholder = document.getElementById('currentImagePlaceholder');
            if (currentImage && placeholder) {
                currentImage.src = dataUrl;
                currentImage.style.display = 'block';
                placeholder.style.display = 'none';
            }
        };
        reader.readAsDataURL(file);
    }

    // ========== TAB SWITCHING ==========
    function initializeTabSwitching() {
        const mediaTab = document.getElementById('post-media-tab');
        if (mediaTab) {
            mediaTab.addEventListener('shown.bs.tab', function () {
                console.log('🎬 Media tab activated');
                loadMediaForCurrentPost();
            });
        }
    }

    function loadMediaForCurrentPost() {
        // Get media URL from various sources
        const mediaUrl = getCurrentPostMediaUrl();

        if (mediaUrl && !mediaUrl.startsWith('new-upload:')) {
            const mediaType = determineMediaTypeFromUrl(mediaUrl);
            loadExistingMedia(mediaUrl, mediaType);
        } else {
            showNoMediaPlaceholder();
        }
    }

    function getCurrentPostMediaUrl() {
        // Try to get media URL from form fields
        const imageUrlField = document.getElementById('editImageURL');
        if (imageUrlField && imageUrlField.value && !imageUrlField.value.startsWith('new-upload:')) {
            return imageUrlField.value;
        }

        // Try to get from current image preview
        const currentImage = document.getElementById('currentImage');
        if (currentImage && currentImage.src && !currentImage.src.startsWith('data:') && !currentImage.src.startsWith('blob:')) {
            return currentImage.src;
        }

        // Try global storage
        return window.currentPostMediaUrl || null;
    }

    // ========== UTILITY FUNCTIONS ==========
    function formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';

        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));

        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }

    function getFilenameFromUrl(url) {
        try {
            return url.split('/').pop().split('?')[0] || 'media-file';
        } catch (e) {
            return 'media-file';
        }
    }

    // ========== FULLSCREEN EVENT LISTENERS ==========
    document.addEventListener('fullscreenchange', function () {
        if (!document.fullscreenElement) {
            isFullscreen = false;
            const fullscreenBtn = document.getElementById('fullscreenMediaBtn');
            if (fullscreenBtn) {
                fullscreenBtn.innerHTML = '<i class="bi bi-arrows-fullscreen"></i>';
                fullscreenBtn.title = 'Fullscreen';
            }
        }
    });

    // ========== ENHANCED PUBLIC API ==========
    window.PostMediaHandler = {
        loadExistingMedia,
        showNoMediaPlaceholder,
        getCurrentMediaData: () => currentMediaData,
        clearMedia: removeMedia,
        updateMediaFromPost: loadMediaForCurrentPost,
        determineMediaTypeFromUrl,
        validateMediaFile
    };

    console.log('✅ Enhanced Post Media Handler initialized successfully');
});