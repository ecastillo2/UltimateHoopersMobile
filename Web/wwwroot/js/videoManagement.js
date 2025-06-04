/**
 * Enhanced Video Management with Comprehensive Video Handling
 * Complete solution for video management, similar to product management but adapted for videos
 */

document.addEventListener('DOMContentLoaded', function () {
    console.log('🚀 Initializing Enhanced Video Management');

    // Global storage for current video data
    window.currentVideoData = null;

    // Initialize components
    initializeDataTable();
    initializeModals();
    initializeForms();
    initializeVideoHandlers();

    console.log('✅ Complete Video Management loaded successfully');

    // ========== DATATABLE INITIALIZATION ==========
    function initializeDataTable() {
        const tableElement = document.getElementById('videosTable');
        if (tableElement && tableElement.querySelector('tbody tr')) {
            console.log('📊 Initializing DataTable...');
            const videosTable = $('#videosTable').DataTable({
                responsive: true,
                lengthMenu: [[10, 25, 50, -1], [10, 25, 50, "All"]],
                dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>rtip',
                language: {
                    search: "_INPUT_",
                    searchPlaceholder: "Search videos...",
                    lengthMenu: "Show _MENU_ videos per page",
                    info: "Showing _START_ to _END_ of _TOTAL_ videos",
                    infoEmpty: "Showing 0 to 0 of 0 videos",
                    infoFiltered: "(filtered from _MAX_ total videos)"
                },
                columnDefs: [
                    { className: "align-middle", targets: "_all" },
                    { orderable: false, targets: [4] }
                ],
                order: [[1, 'desc']],
                // Add callback to initialize filters after table is ready
                initComplete: function () {
                    console.log('📊 DataTable initialization complete, setting up filters...');
                    // Small delay to ensure DOM is ready
                    setTimeout(function () {
                        initializeFilters();
                    }, 100);
                }
            });
            console.log('📊 DataTable initialized successfully');
        } else {
            // If no data, still try to initialize filters after a delay
            setTimeout(function () {
                initializeFilters();
            }, 500);
        }
    }

    // ========== FILTERS ==========
    function initializeFilters() {
        console.log('🔍 Initializing video table filters...');

        const videosTable = $('#videosTable');
        if (!videosTable.length || !$.fn.dataTable.isDataTable(videosTable)) {
            console.warn('⚠️ Videos table not found or not initialized as DataTable');
            return;
        }

        const table = videosTable.DataTable();

        // Get filter elements
        const statusFilter = $('#statusFilter');
        const clientFilter = $('#clientFilter');
        const dateFilter = $('#dateFilter');
        const durationFilter = $('#durationFilter');
        const resetFiltersBtn = $('#resetFilters');
        const activeFiltersContainer = $('#activeFilters');

        if (!statusFilter.length || !clientFilter.length || !dateFilter.length || !durationFilter.length) {
            console.warn('⚠️ Some filter elements not found');
            return;
        }

        // Apply filter function
        function applyFilters() {
            console.log('🔍 Applying filters...', {
                status: statusFilter.val(),
                client: clientFilter.val(),
                date: dateFilter.val(),
                duration: durationFilter.val()
            });

            // Remove any existing custom filter to prevent stacking
            if ($.fn.dataTable.ext.search.length > 0) {
                $.fn.dataTable.ext.search = $.fn.dataTable.ext.search.filter(fn =>
                    !fn.name || fn.name !== 'videoTableFilter'
                );
            }

            // Create a new custom filter function
            const customFilter = function (settings, data, dataIndex) {
                // Only apply this filter to our videosTable
                if (settings.nTable.id !== 'videosTable') return true;

                const row = $(table.row(dataIndex).node());

                // Skip filtering if all filters are set to 'all'
                if (statusFilter.val() === 'all' &&
                    clientFilter.val() === 'all' &&
                    dateFilter.val() === 'all' &&
                    durationFilter.val() === 'all') {
                    return true;
                }

                try {
                    // Status filtering
                    if (statusFilter.val() !== 'all') {
                        const statusValue = statusFilter.val().toLowerCase();

                        // Get status from data attributes or cell content
                        const rowStatus = (row.attr('data-status') || '').toLowerCase();
                        const statusCell = row.find('td:nth-child(3)'); // Status column
                        const statusText = statusCell.find('.badge').text().toLowerCase() ||
                            statusCell.text().toLowerCase();

                        const matchesStatus = rowStatus === statusValue || statusText.includes(statusValue);

                        if (!matchesStatus) {
                            return false;
                        }
                    }

                    // Client filtering
                    if (clientFilter.val() !== 'all') {
                        const clientValue = clientFilter.val().toLowerCase();

                        // Get client from data attributes or cell content
                        const rowClient = (row.attr('data-client-id') || '').toLowerCase();
                        const clientCell = row.find('td:nth-child(4)'); // Client column
                        const clientText = clientCell.text().toLowerCase();

                        const matchesClient = rowClient === clientValue || clientText.includes(clientValue);

                        if (!matchesClient) {
                            return false;
                        }
                    }

                    // Date filtering
                    if (dateFilter.val() !== 'all') {
                        if (!filterByDate(row, data[1], dateFilter.val())) {
                            return false;
                        }
                    }

                    // Duration filtering (placeholder for now)
                    if (durationFilter.val() !== 'all') {
                        // This would require video duration data
                        // For now, just pass through
                    }

                    // If we got here, the row passes all filters
                    return true;

                } catch (error) {
                    console.error('❌ Error in filter function:', error);
                    return true; // Show row if there's an error
                }
            };

            // Mark the filter function for identification
            customFilter.name = 'videoTableFilter';

            // Add the custom filter
            $.fn.dataTable.ext.search.push(customFilter);

            // Redraw the table to apply filters
            table.draw();

            // Update the active filters display
            updateActiveFilters();
        }

        function filterByDate(row, dateText, filterValue) {
            try {
                const dateString = row.attr('data-video-date') || row.attr('data-created-date') || dateText;
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
                console.error("🚨 Date filtering error:", e);
                return false;
            }
        }

        // Update the active filters display
        function updateActiveFilters() {
            if (!activeFiltersContainer.length) return;

            // Clear the current active filters display (except the label)
            activeFiltersContainer.find('.filter-badge, .filter-none').remove();

            // Check if any filters are active
            const hasActiveFilters =
                statusFilter.val() !== 'all' ||
                clientFilter.val() !== 'all' ||
                dateFilter.val() !== 'all' ||
                durationFilter.val() !== 'all';

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

            if (clientFilter.val() !== 'all') {
                addFilterBadge('Client', formatFilterValue(clientFilter.val()), function () {
                    clientFilter.val('all');
                    applyFilters();
                });
            }

            if (dateFilter.val() !== 'all') {
                addFilterBadge('Date', formatFilterValue(dateFilter.val()), function () {
                    dateFilter.val('all');
                    applyFilters();
                });
            }

            if (durationFilter.val() !== 'all') {
                addFilterBadge('Duration', formatFilterValue(durationFilter.val()), function () {
                    durationFilter.val('all');
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
                .on('click', function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    removeCallback();
                });

            badge.append(removeBtn);
            activeFiltersContainer.append(badge);
        }

        // Add event listeners to filters
        statusFilter.on('change', function () {
            console.log('Status filter changed to:', this.value);
            applyFilters();
        });

        clientFilter.on('change', function () {
            console.log('Client filter changed to:', this.value);
            applyFilters();
        });

        dateFilter.on('change', function () {
            console.log('Date filter changed to:', this.value);
            applyFilters();
        });

        durationFilter.on('change', function () {
            console.log('Duration filter changed to:', this.value);
            applyFilters();
        });

        // Reset filters button
        if (resetFiltersBtn.length) {
            resetFiltersBtn.on('click', function () {
                console.log('🔄 Resetting all filters');
                statusFilter.val('all');
                clientFilter.val('all');
                dateFilter.val('all');
                durationFilter.val('all');
                applyFilters();
            });
        }

        // Initialize with current filter values
        applyFilters();

        console.log('✅ Video table filters initialized successfully');
    }

    // ========== MODAL HANDLERS ==========
    function initializeModals() {
        const editVideoModal = document.getElementById('editVideoModal');
        if (editVideoModal) {
            editVideoModal.addEventListener('show.bs.modal', handleEditModalShow);
            editVideoModal.addEventListener('hidden.bs.modal', handleEditModalHide);

            // Tab switching handlers
            const tabButtons = editVideoModal.querySelectorAll('button[data-bs-toggle="tab"]');
            tabButtons.forEach(button => {
                button.addEventListener('shown.bs.tab', handleTabSwitch);
            });

            console.log('📝 Modal event handlers initialized');
        }

        // Delete button handler
        const deleteVideoBtn = document.getElementById('deleteVideoBtn');
        if (deleteVideoBtn) {
            deleteVideoBtn.addEventListener('click', handleDeleteVideo);
        }
    }

    function handleEditModalShow(event) {
        const button = event.relatedTarget;
        const videoId = button.getAttribute('data-video-id');

        console.log('📂 Opening edit modal for video ID:', videoId);

        if (!videoId) {
            console.error('🚨 No video ID found on button');
            showToast('Video ID is missing', 'error');
            return;
        }

        // Set video IDs in forms
        safeSetValue('editVideoId', videoId);
        safeSetValue('deleteVideoId', videoId);

        // Clear previous data
        clearAllForms();

        // Load video data
        loadVideoDataEnhanced(videoId);
    }

    function handleEditModalHide() {
        console.log('🚪 Edit modal closed, clearing forms');
        clearAllForms();
        window.currentVideoData = null;
    }

    function handleTabSwitch(event) {
        const targetTab = event.target.getAttribute('data-bs-target');
        const videoId = document.getElementById('editVideoId')?.value;

        console.log('🔄 Switching to tab:', targetTab, 'for video:', videoId);

        if (!videoId) return;

        switch (targetTab) {
            case '#video-details-tab-pane':
                // Already handled in main load function
                break;
            case '#video-info-tab-pane':
                console.log('📊 Loading video info tab');
                updateVideoInfoDisplayFromCurrentData();
                break;
            case '#video-media-tab-pane':
                console.log('🎬 Loading video media tab');
                loadVideoMediaTab(videoId);
                break;
        }
    }

    function handleDeleteVideo() {
        const videoId = safeGetValue('editVideoId');
        if (!videoId) return;

        safeSetValue('deleteVideoId', videoId);

        const editModal = bootstrap.Modal.getInstance(document.getElementById('editVideoModal'));
        if (editModal) editModal.hide();

        const deleteModalEl = document.getElementById('deleteVideoModal');
        if (deleteModalEl) {
            const deleteModal = new bootstrap.Modal(deleteModalEl);
            deleteModal.show();
        }
    }

    // ========== DATA LOADING FUNCTIONS ==========
    function loadVideoDataEnhanced(videoId) {
        console.log('📥 Loading enhanced video data for ID:', videoId);

        if (!videoId) {
            console.error('🚨 No video ID provided');
            return;
        }

        showLoadingState();

        // Try to populate from table data first
        const row = findVideoRowById(videoId);
        if (row) {
            console.log('📋 Found table row, extracting data...');
            const tableData = extractTableData(row);
            populateFromTableData(tableData);
        }

        // Always call API for complete data
        if (!window.appUrls?.getVideoData) {
            console.error('🚨 GetVideoData API URL not configured');
            hideLoadingState();
            showToast('API not configured. Only table data available.', 'warning');
            return;
        }

        fetch(`${window.appUrls.getVideoData}?id=${encodeURIComponent(videoId)}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                return response.json();
            })
            .then(data => {
                console.log('📦 Received enhanced video data:', data);
                hideLoadingState();

                if (data.success !== false) {
                    populateFromAPIDataEnhanced(data);
                    showToast('Video data loaded successfully', 'success');
                } else {
                    showToast(`Failed to load complete video data: ${data.message || 'Unknown error'}`, 'warning');
                }
            })
            .catch(error => {
                console.error('🚨 Error loading video data:', error);
                hideLoadingState();
                showToast(`Error loading video data: ${error.message}`, 'error');
            });
    }

    function extractTableData(row) {
        if (!row) return {};

        console.log('📋 Extracting data from table row');

        // Get data attributes
        const dataFromAttributes = {
            videoId: row.getAttribute('data-video-id'),
            title: row.getAttribute('data-video-title'),
            videoName: row.getAttribute('data-video-name'),
            videoDate: row.getAttribute('data-video-date'),
            createdDate: row.getAttribute('data-created-date'),
            status: row.getAttribute('data-status'),
            clientId: row.getAttribute('data-client-id'),
            videoUrl: row.getAttribute('data-video-url'),
            thumbnailUrl: row.getAttribute('data-thumbnail-url'),
            videoNumber: row.getAttribute('data-video-number')
        };

        // Extract from cell content as fallback
        const cells = row.querySelectorAll('td');
        if (cells.length >= 4) {
            // Video info from first column
            const videoCell = cells[0];
            const titleEl = videoCell.querySelector('.video-title, .fw-semibold');
            const numberEl = videoCell.querySelector('.video-number, .text-muted.small');

            if (titleEl && !dataFromAttributes.title) {
                dataFromAttributes.title = titleEl.textContent.trim();
            }
            if (numberEl && !dataFromAttributes.videoNumber) {
                dataFromAttributes.videoNumber = numberEl.textContent.trim();
            }

            // Date from second column
            if (cells[1] && !dataFromAttributes.videoDate) {
                dataFromAttributes.videoDate = cells[1].textContent.trim();
            }

            // Status from third column
            if (cells[2] && !dataFromAttributes.status) {
                const statusEl = cells[2].querySelector('.badge, .video-status');
                if (statusEl) {
                    dataFromAttributes.status = statusEl.textContent.trim();
                }
            }

            // Client from fourth column
            if (cells[3] && !dataFromAttributes.clientId) {
                dataFromAttributes.clientId = cells[3].textContent.trim();
            }
        }

        return dataFromAttributes;
    }

    function populateFromTableData(data) {
        console.log('📝 Populating form from table data:', data);

        try {
            // Store data globally
            window.currentVideoData = data;

            // Populate form fields
            safeSetValue('editTitle', data.title);
            safeSetValue('editVideoName', data.videoName);
            safeSetValue('editClientId', data.clientId);
            safeSetValue('editVideoNumber', data.videoNumber);

            // Handle video URL
            if (data.videoUrl) {
                safeSetValue('editVideoURL', data.videoUrl);
                updateVideoPreview(data.videoUrl);
            }

            // Handle date
            if (data.videoDate) {
                safeSetValue('editVideoDate', formatDateForInput(data.videoDate));
            } else if (data.createdDate) {
                safeSetValue('editVideoDate', formatDateForInput(data.createdDate));
            }

            // Select fields
            safeSetSelect('editStatus', data.status || 'Active');

            console.log('✅ Table data populated successfully');
        } catch (error) {
            console.error('🚨 Error populating from table data:', error);
        }
    }

    function populateFromAPIDataEnhanced(data) {
        console.log('🌐 Populating form from enhanced API data:', data);

        try {
            // Handle different possible data structures
            const videoData = data.video || data;

            // Store data globally for video info tab access
            window.currentVideoData = videoData;

            // Basic information
            safeSetValue('editTitle', videoData.title || videoData.Title);
            safeSetValue('editVideoName', videoData.videoName || videoData.VideoName);
            safeSetValue('editClientId', videoData.clientId || videoData.ClientId);
            safeSetValue('editVideoNumber', videoData.videoNumber || videoData.VideoNumber);

            // Video URL handling
            const videoUrl = videoData.videoURL || videoData.VideoURL || videoData.videoUrl;
            if (videoUrl) {
                safeSetValue('editVideoURL', videoUrl);
                updateVideoPreview(videoUrl);
            }

            // Date handling
            const videoDate = videoData.videoDate || videoData.VideoDate || videoData.createdDate || videoData.CreatedDate;
            if (videoDate) {
                safeSetValue('editVideoDate', formatDateForInput(videoDate));
            }

            // Select fields
            safeSetSelect('editStatus', videoData.status || videoData.Status || 'Active');

            console.log('✅ Enhanced API data populated successfully');

            // Update Video Info tab immediately if it's active
            const activeTab = document.querySelector('#editVideoTabs .nav-link.active');
            if (activeTab && activeTab.getAttribute('data-bs-target') === '#video-info-tab-pane') {
                updateVideoInfoDisplayFromCurrentData();
            }
        } catch (error) {
            console.error('💥 Error populating from API data:', error);
        }
    }

    // ========== VIDEO INFO TAB FUNCTIONALITY ==========
    function updateVideoInfoDisplayFromCurrentData() {
        console.log('📊 Updating video info display from current data');

        let videoData = window.currentVideoData;

        // Fallback to form data if no stored data
        if (!videoData) {
            videoData = {
                title: safeGetValue('editTitle'),
                videoNumber: safeGetValue('editVideoNumber'),
                videoName: safeGetValue('editVideoName'),
                clientId: safeGetValue('editClientId'),
                status: safeGetValue('editStatus'),
                videoURL: safeGetValue('editVideoURL'),
                videoDate: safeGetValue('editVideoDate')
            };
        }

        updateVideoInfoDisplay(videoData);
    }

    function updateVideoInfoDisplay(videoData) {
        console.log('📊 Updating video info display with data:', videoData);

        if (!videoData) {
            console.warn('⚠️ No video data available for info display');
            return;
        }

        const title = videoData.title || videoData.Title || 'Video';
        const videoNumber = videoData.videoNumber || videoData.VideoNumber || '--';
        const clientId = videoData.clientId || videoData.ClientId || '--';
        const status = videoData.status || videoData.Status || 'Active';
        const videoName = videoData.videoName || videoData.VideoName || '--';
        const videoDate = videoData.videoDate || videoData.VideoDate || '--';

        // Update avatar and basic info
        const initials = getVideoInitials(title);
        safeUpdateElement('videoInfoInitials', initials);
        safeUpdateElement('videoInfoTitle', title);
        safeUpdateElement('videoInfoNumber', videoNumber);
        safeUpdateElement('videoInfoStatus', status);
        safeUpdateElement('videoInfoClient', clientId);

        // Update video information section
        safeUpdateElement('videoInfoTitleDetail', title);
        safeUpdateElement('videoInfoVideoName', videoName);
        safeUpdateElement('videoInfoDate', formatDisplayDate(videoDate));
        safeUpdateElement('videoInfoClientDetail', clientId);
        safeUpdateElement('videoInfoSize', 'Unknown'); // Would need actual file size

        // Update status badge color
        const statusBadge = document.getElementById('videoInfoStatus');
        if (statusBadge) {
            statusBadge.className = 'badge ' + getStatusBadgeClass(status);
        }

        // Update statistics (mock data for now)
        safeUpdateElement('videoInfoViews', Math.floor(Math.random() * 1000));
        safeUpdateElement('videoInfoDownloads', Math.floor(Math.random() * 100));
        safeUpdateElement('videoInfoRating', (Math.random() * 2 + 3).toFixed(1));

        console.log('✅ Video info display updated successfully');
    }

    // ========== VIDEO MEDIA TAB FUNCTIONALITY ==========
    function loadVideoMediaTab(videoId) {
        console.log('🎬 Loading video media tab for:', videoId);

        // Get current video data
        const currentVideoUrl = getCurrentVideoUrl();

        console.log('🎬 Current video URL:', currentVideoUrl);

        if (currentVideoUrl) {
            // Load existing video
            loadVideoDirectly(currentVideoUrl);

            // Update video information
            updateVideoInfoFromCurrentVideo(currentVideoUrl);

            // Hide upload area
            const uploadArea = document.getElementById('videoUploadArea');
            if (uploadArea) {
                uploadArea.style.display = 'none';
            }
        } else {
            // No video available
            showNoVideoFallback();

            // Show upload area
            const uploadArea = document.getElementById('videoUploadArea');
            if (uploadArea) {
                uploadArea.style.display = 'block';
            }
        }
    }

    function getCurrentVideoUrl() {
        // Try multiple sources for video URL
        const sources = [
            () => document.getElementById('editVideoURL')?.value,
            () => document.getElementById('currentVideo')?.src,
            () => getVideoUrlFromTableRow(),
            () => window.currentVideoData?.videoURL || window.currentVideoData?.VideoURL
        ];

        for (const getUrl of sources) {
            try {
                const url = getUrl();
                if (url && url.trim() &&
                    !url.includes('data:') &&
                    !url.includes('blob:') &&
                    !url.startsWith('new-upload:')) {
                    console.log('✅ Found video URL from source:', url);
                    return url.trim();
                }
            } catch (error) {
                console.warn('⚠️ Error getting URL from source:', error);
            }
        }

        console.log('❌ No video URL found');
        return null;
    }

    function getVideoUrlFromTableRow() {
        const videoId = document.getElementById('editVideoId')?.value;
        if (!videoId) return null;

        const row = findVideoRowById(videoId);
        if (row) {
            return row.getAttribute('data-video-url') ||
                row.getAttribute('data-thumbnail-url');
        }

        return null;
    }

    function loadVideoDirectly(videoUrl) {
        const videoContent = document.getElementById('videoContent');
        if (!videoContent) return;

        console.log('🎬 Loading video directly:', videoUrl);

        // Clear existing content
        videoContent.innerHTML = '';

        // Create video element
        const video = document.createElement('video');
        video.src = videoUrl;
        video.className = 'video-player';
        video.controls = true;
        video.style.maxWidth = '100%';
        video.style.maxHeight = '400px';
        video.style.borderRadius = '10px';
        video.style.boxShadow = '0 4px 15px rgba(0, 0, 0, 0.1)';

        video.onloadedmetadata = function () {
            updateVideoDimensions(this.videoWidth, this.videoHeight);
            updateVideoDuration(this.duration);
        };

        video.onerror = function () {
            showVideoError('Failed to load video');
        };

        videoContent.appendChild(video);
    }

    function showVideoError(message) {
        const videoContent = document.getElementById('videoContent');
        if (!videoContent) return;

        videoContent.innerHTML = `
            <div class="video-placeholder text-danger">
                <i class="bi bi-exclamation-triangle" style="font-size: 4rem; margin-bottom: 1rem; opacity: 0.5;"></i>
                <h5>Video Error</h5>
                <p class="mb-0">${message}</p>
            </div>
        `;
    }

    function showNoVideoFallback() {
        const videoContent = document.getElementById('videoContent');
        if (!videoContent) return;

        videoContent.innerHTML = `
            <div class="video-placeholder">
                <i class="bi bi-camera-video" style="font-size: 4rem; margin-bottom: 1rem; opacity: 0.5;"></i>
                <h5>No Video Available</h5>
                <p class="mb-0">Upload a video to see it here</p>
                <button type="button" class="btn btn-outline-primary mt-3" onclick="document.getElementById('mediaFileInput').click()">
                    <i class="bi bi-cloud-upload me-2"></i>Choose File
                </button>
            </div>
        `;
    }

    function updateVideoInfoFromCurrentVideo(videoUrl) {
        console.log('📝 Updating video info for:', videoUrl);

        // Update video format
        const formatElement = document.getElementById('mediaFormat');
        if (formatElement) {
            const extension = videoUrl.split('.').pop().toLowerCase();
            formatElement.textContent = extension.toUpperCase();
        }

        // Update video URL
        const urlElement = document.getElementById('mediaUrl');
        if (urlElement) {
            const shortUrl = videoUrl.length > 30 ? videoUrl.substring(0, 27) + '...' : videoUrl;
            urlElement.textContent = shortUrl;
            urlElement.title = videoUrl;
        }

        // Update file size (placeholder for now)
        const sizeElement = document.getElementById('mediaFileSize');
        if (sizeElement) {
            sizeElement.textContent = 'Unknown';
        }

        // Update dimensions (will be set when video loads)
        const resolutionElement = document.getElementById('mediaResolution');
        if (resolutionElement) {
            resolutionElement.textContent = 'Loading...';
        }

        // Update duration (will be set when video loads)
        const durationElement = document.getElementById('mediaDuration');
        if (durationElement) {
            durationElement.textContent = 'Loading...';
        }
    }

    function updateVideoDimensions(width, height) {
        const resolutionElement = document.getElementById('mediaResolution');
        if (resolutionElement) {
            resolutionElement.textContent = `${width} × ${height}`;
        }
    }

    function updateVideoDuration(duration) {
        const durationElement = document.getElementById('mediaDuration');
        if (durationElement) {
            const minutes = Math.floor(duration / 60);
            const seconds = Math.floor(duration % 60);
            durationElement.textContent = `${minutes}:${seconds.toString().padStart(2, '0')}`;
        }
    }

    // ========== FORM HANDLERS ==========
    function initializeForms() {
        // Add Video Form
        const addVideoForm = document.getElementById('addVideoForm');
        if (addVideoForm) {
            addVideoForm.addEventListener('submit', handleAddFormSubmit);
            console.log('✅ Add form handler attached');
        }

        // Edit Video Form  
        const editVideoForm = document.getElementById('editVideoForm');
        if (editVideoForm) {
            editVideoForm.addEventListener('submit', handleEditFormSubmit);
            console.log('✅ Edit form handler attached');
        }
    }

    function handleAddFormSubmit(e) {
        e.preventDefault();
        console.log('📤 Add video form submitted');

        const formData = new FormData(e.target);
        const submitBtn = e.target.querySelector('button[type="submit"]');

        if (submitBtn && window.UIUtils) {
            window.UIUtils.setButtonLoading(submitBtn, true, 'Adding Video...');
        }

        const token = getAntiForgeryToken();

        fetch('/Video/Create', {
            method: 'POST',
            headers: {
                'RequestVerificationToken': token
            },
            body: formData
        })
            .then(response => response.json())
            .then(result => {
                if (submitBtn && window.UIUtils) {
                    window.UIUtils.setButtonLoading(submitBtn, false);
                }

                if (result.success) {
                    showToast('Video created successfully!', 'success');

                    const modal = bootstrap.Modal.getInstance(document.getElementById('addVideoModal'));
                    if (modal) modal.hide();

                    setTimeout(() => location.reload(), 1000);
                } else {
                    showToast(`Error creating video: ${result.message || 'Unknown error'}`, 'error');
                }
            })
            .catch(error => {
                if (submitBtn && window.UIUtils) {
                    window.UIUtils.setButtonLoading(submitBtn, false);
                }
                console.error('Error creating video:', error);
                showToast(`Error creating video: ${error.message}`, 'error');
            });
    }

    function handleEditFormSubmit(e) {
        e.preventDefault();
        console.log('📤 Edit video form submitted');

        const formData = new FormData(e.target);
        const videoData = {};

        for (const [key, value] of formData.entries()) {
            videoData[key] = value;
        }

        const submitBtn = e.target.querySelector('button[type="submit"]');
        if (submitBtn && window.UIUtils) {
            window.UIUtils.setButtonLoading(submitBtn, true, 'Saving...');
        }

        const token = getAntiForgeryToken();

        fetch('/Video/Edit', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(videoData)
        })
            .then(response => response.json())
            .then(result => {
                if (submitBtn && window.UIUtils) {
                    window.UIUtils.setButtonLoading(submitBtn, false);
                }

                if (result.success) {
                    showToast('Video updated successfully!', 'success');

                    // Update stored data
                    window.currentVideoData = { ...window.currentVideoData, ...videoData };

                    setTimeout(() => {
                        const modal = bootstrap.Modal.getInstance(document.getElementById('editVideoModal'));
                        if (modal) modal.hide();
                        location.reload();
                    }, 1000);
                } else {
                    showToast(`Error updating video: ${result.message || 'Unknown error'}`, 'error');
                }
            })
            .catch(error => {
                if (submitBtn && window.UIUtils) {
                    window.UIUtils.setButtonLoading(submitBtn, false);
                }
                console.error('Error updating video:', error);
                showToast(`Error updating video: ${error.message}`, 'error');
            });
    }

    // ========== VIDEO HANDLERS ==========
    function initializeVideoHandlers() {
        // Video file input handlers
        const videoFileInputs = document.querySelectorAll('input[type="file"][accept*="video"]');
        videoFileInputs.forEach(input => {
            input.addEventListener('change', function (e) {
                const file = e.target.files[0];
                if (file) {
                    handleVideoPreview(file, input);
                }
            });
        });

        // Video URL input handlers
        const videoUrlInputs = document.querySelectorAll('input[name="VideoURL"]');
        videoUrlInputs.forEach(input => {
            input.addEventListener('blur', function () {
                if (this.value) {
                    updateVideoPreview(this.value);
                }
            });
        });

        // Media tab video controls
        const downloadBtn = document.getElementById('downloadVideoBtn');
        if (downloadBtn) {
            downloadBtn.addEventListener('click', downloadVideo);
        }

        const fullscreenBtn = document.getElementById('fullscreenVideoBtn');
        if (fullscreenBtn) {
            fullscreenBtn.addEventListener('click', toggleFullscreen);
        }

        const removeBtn = document.getElementById('removeVideoBtn');
        if (removeBtn) {
            removeBtn.addEventListener('click', removeVideo);
        }
    }

    function handleVideoPreview(file, input) {
        const reader = new FileReader();
        reader.onload = function (e) {
            const videoUrl = e.target.result;
            updateVideoPreview(videoUrl);
        };
        reader.readAsDataURL(file);
    }

    function downloadVideo() {
        const videoUrl = getCurrentVideoUrl();
        if (videoUrl) {
            const link = document.createElement('a');
            link.href = videoUrl;
            link.download = 'video';
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            showToast('Download started', 'success');
        } else {
            showToast('No video to download', 'warning');
        }
    }

    function toggleFullscreen() {
        const video = document.querySelector('#videoContent video');
        if (video) {
            if (video.requestFullscreen) {
                video.requestFullscreen();
            } else if (video.webkitRequestFullscreen) {
                video.webkitRequestFullscreen();
            } else if (video.msRequestFullscreen) {
                video.msRequestFullscreen();
            }
        }
    }

    function removeVideo() {
        if (confirm('Are you sure you want to remove this video?')) {
            // Clear video URL
            safeSetValue('editVideoURL', '');
            updateVideoPreview('');
            showNoVideoFallback();
            showToast('Video removed', 'success');
        }
    }

    // ========== UI STATE MANAGEMENT ==========
    function clearAllForms() {
        clearVideoDetailsForm();
        clearVideoInfoDisplay();
        clearVideoMediaTab();
    }

    function clearVideoDetailsForm() {
        const fields = [
            'editTitle', 'editVideoName', 'editClientId', 'editVideoNumber',
            'editVideoURL', 'editVideoDate'
        ];

        fields.forEach(field => safeSetValue(field, ''));

        // Reset select fields
        const selects = ['editStatus'];
        selects.forEach(select => {
            const element = document.getElementById(select);
            if (element) element.selectedIndex = 0;
        });

        updateVideoPreview('');
        console.log('🧹 Video details form cleared');
    }

    function clearVideoInfoDisplay() {
        const elements = [
            'videoInfoInitials', 'videoInfoTitle', 'videoInfoNumber', 'videoInfoStatus',
            'videoInfoClient', 'videoInfoTitleDetail', 'videoInfoVideoName', 'videoInfoDate',
            'videoInfoClientDetail', 'videoInfoSize', 'videoInfoViews', 'videoInfoDownloads', 'videoInfoRating'
        ];

        elements.forEach(elementId => safeUpdateElement(elementId, '--'));
        console.log('🧹 Video info display cleared');
    }

    function clearVideoMediaTab() {
        showNoVideoFallback();

        // Clear media info
        const mediaInfoElements = ['mediaFormat', 'mediaDuration', 'mediaResolution', 'mediaFileSize', 'mediaUrl'];
        mediaInfoElements.forEach(id => {
            const element = document.getElementById(id);
            if (element) {
                element.textContent = '--';
            }
        });

        console.log('🧹 Video media tab cleared');
    }

    function showLoadingState() {
        const modal = document.getElementById('editVideoModal');
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
                <div class="text-muted">Loading video data...</div>
            </div>
        `;
            modal.querySelector('.modal-content').appendChild(overlay);
        }

        console.log('⏳ Loading state shown');
    }

    function hideLoadingState() {
        const modal = document.getElementById('editVideoModal');
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

    function findVideoRowById(videoId) {
        if (!videoId) return null;

        console.log('🔍 Looking for row with video ID:', videoId);

        // Try different strategies to find the row
        let row = document.querySelector(`tr[data-video-id="${videoId}"]`);
        if (row) return row;

        const button = document.querySelector(`button[data-video-id="${videoId}"]`);
        if (button) {
            row = button.closest('tr');
            if (row) return row;
        }

        // Search within table body
        const tableBody = document.querySelector('#videosTable tbody');
        if (tableBody) {
            const allRows = tableBody.querySelectorAll('tr');
            for (const tr of allRows) {
                const editBtn = tr.querySelector(`[data-video-id="${videoId}"]`);
                if (editBtn) return tr;
            }
        }

        console.warn('⚠️ Row not found for video ID:', videoId);
        return null;
    }

    function updateVideoPreview(videoUrl) {
        const currentVideo = document.getElementById('currentVideo');
        const placeholder = document.getElementById('currentVideoPlaceholder');

        if (!currentVideo || !placeholder) {
            console.warn('⚠️ Video preview elements not found');
            return;
        }

        if (videoUrl && videoUrl.trim()) {
            currentVideo.src = videoUrl;
            currentVideo.style.display = 'block';
            placeholder.style.display = 'none';
            console.log('🎬 Video preview updated');
        } else {
            currentVideo.style.display = 'none';
            placeholder.style.display = 'flex';
            console.log('🎬 Video preview cleared');
        }
    }

    function getVideoInitials(title) {
        if (!title) return 'V';
        const words = title.split(' ');
        if (words.length >= 2) {
            return (words[0][0] + words[1][0]).toUpperCase();
        }
        return title[0].toUpperCase();
    }

    function getStatusBadgeClass(status) {
        switch (status?.toLowerCase()) {
            case 'inactive': return 'bg-secondary';
            case 'archived': return 'bg-danger';
            default: return 'bg-success';
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

    function formatDisplayDate(dateString) {
        if (!dateString) return '--';
        try {
            const date = new Date(dateString);
            return date.toLocaleDateString();
        } catch (e) {
            return '--';
        }
    }

    function getAntiForgeryToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : '';
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
    window.videoDebug = {
        loadVideoDataEnhanced,
        findVideoRowById,
        populateFromTableData,
        populateFromAPIDataEnhanced,
        extractTableData,
        updateVideoInfoDisplayFromCurrentData,
        updateVideoInfoDisplay,
        loadVideoMediaTab,
        getCurrentVideoUrl,
        clearAllForms,
        currentVideoData: () => window.currentVideoData,
        initializeFilters,
        applyFilters: () => {
            const table = $('#videosTable').DataTable();
            if (table) table.draw();
        }
    };

    console.log('✅ Complete Video Management loaded successfully');
    console.log('🐛 Debug functions available: window.videoDebug');
});