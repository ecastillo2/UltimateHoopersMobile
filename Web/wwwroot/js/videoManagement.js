/**
 * Complete Fixed Video Management JavaScript
 * Enhanced video upload functionality with comprehensive error handling
 */

document.addEventListener('DOMContentLoaded', function () {
    console.log('🚀 Initializing Complete Fixed Video Management');

    // Global storage for current video data
    window.currentVideoData = null;

    // Initialize components
    initializeDataTable();
    initializeModals();
    initializeForms();
    initializeVideoHandlers();
    initializeFormDefaults();

    console.log('✅ Complete Fixed Video Management loaded successfully');

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
                initComplete: function () {
                    console.log('📊 DataTable initialization complete, setting up filters...');
                    setTimeout(function () {
                        initializeFilters();
                    }, 100);
                }
            });
            console.log('📊 DataTable initialized successfully');
        } else {
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

            const customFilter = function (settings, data, dataIndex) {
                if (settings.nTable.id !== 'videosTable') return true;

                const row = $(table.row(dataIndex).node());

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
                        const rowStatus = (row.attr('data-status') || '').toLowerCase();
                        const statusCell = row.find('td:nth-child(3)');
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
                        const rowClient = (row.attr('data-client-id') || '').toLowerCase();
                        const clientCell = row.find('td:nth-child(4)');
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

                    return true;
                } catch (error) {
                    console.error('❌ Error in filter function:', error);
                    return true;
                }
            };

            customFilter.name = 'videoTableFilter';
            $.fn.dataTable.ext.search.push(customFilter);
            table.draw();
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

        function updateActiveFilters() {
            if (!activeFiltersContainer.length) return;

            activeFiltersContainer.find('.filter-badge, .filter-none').remove();

            const hasActiveFilters =
                statusFilter.val() !== 'all' ||
                clientFilter.val() !== 'all' ||
                dateFilter.val() !== 'all' ||
                durationFilter.val() !== 'all';

            if (!hasActiveFilters) {
                activeFiltersContainer.append(
                    $('<span>').addClass('text-muted filter-none').text('None')
                );
                return;
            }

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

        function formatFilterValue(value) {
            return value
                .split('-')
                .map(word => word.charAt(0).toUpperCase() + word.slice(1))
                .join(' ');
        }

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

        // Event listeners
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

        applyFilters();
        console.log('✅ Video table filters initialized successfully');
    }

    // ========== MODAL HANDLERS ==========
    function initializeModals() {
        const editVideoModal = document.getElementById('editVideoModal');
        if (editVideoModal) {
            editVideoModal.addEventListener('show.bs.modal', handleEditModalShow);
            editVideoModal.addEventListener('hidden.bs.modal', handleEditModalHide);

            const tabButtons = editVideoModal.querySelectorAll('button[data-bs-toggle="tab"]');
            tabButtons.forEach(button => {
                button.addEventListener('shown.bs.tab', handleTabSwitch);
            });

            console.log('📝 Edit modal event handlers initialized');
        }

        const addVideoModal = document.getElementById('addVideoModal');
        if (addVideoModal) {
            addVideoModal.addEventListener('hidden.bs.modal', handleAddModalHide);
            console.log('📝 Add modal event handlers initialized');
        }

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

        safeSetValue('editVideoId', videoId);
        safeSetValue('deleteVideoId', videoId);

        clearAllForms();
        loadVideoDataEnhanced(videoId);
    }

    function handleEditModalHide() {
        console.log('🚪 Edit modal closed, clearing forms');
        clearAllForms();
        window.currentVideoData = null;
    }

    function handleAddModalHide() {
        console.log('🚪 Add modal closed, clearing form');
        const form = document.getElementById('addVideoForm');
        if (form) {
            form.reset();
            clearValidationErrors(form);
            clearVideoPreview();

            // Reset date to today
            const videoDateInput = document.getElementById('addVideoDate');
            if (videoDateInput) {
                const today = new Date().toISOString().split('T')[0];
                videoDateInput.value = today;
            }
        }
    }

    function handleTabSwitch(event) {
        const targetTab = event.target.getAttribute('data-bs-target');
        const videoId = document.getElementById('editVideoId')?.value;

        console.log('🔄 Switching to tab:', targetTab, 'for video:', videoId);

        if (!videoId) return;

        switch (targetTab) {
            case '#video-details-tab-pane':
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

    // ========== ENHANCED FORM HANDLERS ==========
    function initializeForms() {
        // Add Video Form - FIXED
        const addVideoForm = document.getElementById('addVideoForm');
        if (addVideoForm) {
            addVideoForm.addEventListener('submit', handleAddFormSubmitFixed);
            console.log('✅ Fixed add form handler attached');
        }

        // Edit Video Form  
        const editVideoForm = document.getElementById('editVideoForm');
        if (editVideoForm) {
            editVideoForm.addEventListener('submit', handleEditFormSubmitFixed);
            console.log('✅ Fixed edit form handler attached');
        }

        // Initialize form validation
        initializeFormValidation();
    }

    function handleAddFormSubmitFixed(e) {
        e.preventDefault();
        console.log('📤 Add video form submitted (FIXED VERSION)');

        // Validate form before submission
        if (!validateVideoForm(e.target)) {
            console.log('❌ Form validation failed');
            return;
        }

        const formData = new FormData(e.target);
        const submitBtn = e.target.querySelector('button[type="submit"]');

        // Debug: Log all form data
        console.log('📋 Form data being sent:');
        for (let [key, value] of formData.entries()) {
            if (value instanceof File) {
                console.log(`${key}: File - ${value.name} (${value.size} bytes, ${value.type})`);
            } else {
                console.log(`${key}: ${value}`);
            }
        }

        // Show loading state
        if (submitBtn && window.UIUtils) {
            window.UIUtils.setButtonLoading(submitBtn, true, 'Uploading Video...');
        }

        const token = getAntiForgeryToken();
        if (!token) {
            console.error('❌ No anti-forgery token found');
            showToast('Security token missing. Please refresh the page.', 'error');
            if (submitBtn && window.UIUtils) {
                window.UIUtils.setButtonLoading(submitBtn, false);
            }
            return;
        }

        // FIXED: Use XMLHttpRequest for better upload progress and error handling
        const xhr = new XMLHttpRequest();

        // Upload progress
        xhr.upload.addEventListener('progress', function (e) {
            if (e.lengthComputable) {
                const percentComplete = Math.round((e.loaded / e.total) * 100);
                console.log(`📊 Upload progress: ${percentComplete}%`);

                if (submitBtn) {
                    submitBtn.innerHTML = `<span class="spinner-border spinner-border-sm me-2"></span>Uploading... ${percentComplete}%`;
                }

                // Update progress bar if it exists
                const progressBar = document.querySelector('#uploadProgress .progress-bar');
                const progressContainer = document.getElementById('uploadProgress');
                if (progressBar && progressContainer) {
                    progressContainer.style.display = 'block';
                    progressBar.style.width = percentComplete + '%';
                    progressBar.setAttribute('aria-valuenow', percentComplete);
                }
            }
        });

        // Handle response
        xhr.addEventListener('load', function () {
            console.log('📡 Server response received:', xhr.status);

            if (submitBtn && window.UIUtils) {
                window.UIUtils.setButtonLoading(submitBtn, false);
            }

            // Hide progress bar
            const progressContainer = document.getElementById('uploadProgress');
            if (progressContainer) {
                progressContainer.style.display = 'none';
            }

            try {
                if (xhr.status === 200) {
                    const result = JSON.parse(xhr.responseText);
                    console.log('📦 Parsed response:', result);

                    if (result.success) {
                        showToast('Video uploaded successfully!', 'success');

                        // Clear form
                        e.target.reset();
                        clearVideoPreview();

                        // Close modal
                        const modal = bootstrap.Modal.getInstance(document.getElementById('addVideoModal'));
                        if (modal) modal.hide();

                        // Reload page to show new video
                        setTimeout(() => location.reload(), 1000);
                    } else {
                        console.error('❌ Server returned error:', result.message);
                        showToast(`Upload failed: ${result.message || 'Unknown error'}`, 'error');

                        if (result.field) {
                            highlightErrorField(result.field);
                        }
                    }
                } else {
                    console.error('❌ HTTP error:', xhr.status, xhr.statusText);
                    let errorMessage = `HTTP ${xhr.status}: ${xhr.statusText}`;

                    try {
                        const errorResponse = JSON.parse(xhr.responseText);
                        if (errorResponse.message) {
                            errorMessage = errorResponse.message;
                        }
                    } catch (e) {
                        // Response is not JSON, use status text
                    }

                    showToast(`Upload failed: ${errorMessage}`, 'error');
                }
            } catch (error) {
                console.error('❌ Error parsing response:', error);
                showToast('Upload failed: Invalid server response', 'error');
            }
        });

        // Handle network errors
        xhr.addEventListener('error', function () {
            console.error('❌ Network error during upload');
            if (submitBtn && window.UIUtils) {
                window.UIUtils.setButtonLoading(submitBtn, false);
            }
            const progressContainer = document.getElementById('uploadProgress');
            if (progressContainer) {
                progressContainer.style.display = 'none';
            }
            showToast('Upload failed: Network error. Please check your connection.', 'error');
        });

        // Handle timeout
        xhr.addEventListener('timeout', function () {
            console.error('❌ Upload timeout');
            if (submitBtn && window.UIUtils) {
                window.UIUtils.setButtonLoading(submitBtn, false);
            }
            const progressContainer = document.getElementById('uploadProgress');
            if (progressContainer) {
                progressContainer.style.display = 'none';
            }
            showToast('Upload failed: Request timed out. The file might be too large.', 'error');
        });

        // Configure request
        xhr.timeout = 900000; // 15 minutes timeout for large video files (up to 800MB)
        xhr.open('POST', '/Video/Create', true);
        xhr.setRequestHeader('RequestVerificationToken', token);

        // Send the request
        console.log('🚀 Starting video upload...');
        xhr.send(formData);
    }

    function handleEditFormSubmitFixed(e) {
        e.preventDefault();
        console.log('📤 Edit video form submitted (FIXED VERSION)');

        if (!validateVideoForm(e.target)) {
            console.log('❌ Form validation failed');
            return;
        }

        const formData = new FormData(e.target);
        const videoData = {};

        // Convert FormData to object for JSON submission
        for (const [key, value] of formData.entries()) {
            if (value instanceof File && value.size === 0) {
                continue;
            }
            videoData[key] = value;
        }

        console.log('📋 Video data for edit:', videoData);

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
            .then(response => {
                console.log('📡 Edit response status:', response.status);
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                return response.json();
            })
            .then(result => {
                console.log('📦 Edit response:', result);

                if (submitBtn && window.UIUtils) {
                    window.UIUtils.setButtonLoading(submitBtn, false);
                }

                if (result.success) {
                    showToast('Video updated successfully!', 'success');

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
                console.error('❌ Error updating video:', error);
                if (submitBtn && window.UIUtils) {
                    window.UIUtils.setButtonLoading(submitBtn, false);
                }
                showToast(`Error updating video: ${error.message}`, 'error');
            });
    }

    // ========== FORM VALIDATION ==========
    function validateVideoForm(form) {
        console.log('🔍 Validating video form...');

        const title = form.querySelector('#addTitle, #editTitle');
        const videoFile = form.querySelector('#addVideoFile, #editVideoFile');
        const videoURL = form.querySelector('#addVideoURL, #editVideoURL');

        let isValid = true;
        const errors = [];

        clearValidationErrors(form);

        // Title validation
        if (!title || !title.value.trim()) {
            errors.push('Video title is required');
            if (title) highlightErrorField('Title');
            isValid = false;
        } else if (title.value.length > 100) {
            errors.push('Video title cannot exceed 100 characters');
            highlightErrorField('Title');
            isValid = false;
        }

        // Video source validation
        const hasFile = videoFile && videoFile.files && videoFile.files.length > 0;
        const hasURL = videoURL && videoURL.value.trim();
        const isEditForm = form.id === 'editVideoForm';

        if (!isEditForm && !hasFile && !hasURL) {
            errors.push('Please either upload a video file or provide a video URL');
            if (videoFile) highlightErrorField('VideoFile');
            if (videoURL) highlightErrorField('VideoURL');
            isValid = false;
        }

        // File validation
        if (hasFile) {
            const file = videoFile.files[0];
            const validation = validateVideoFileClient(file);
            if (!validation.isValid) {
                errors.push(validation.errorMessage);
                highlightErrorField('VideoFile');
                isValid = false;
            }
        }

        // URL validation
        if (hasURL) {
            if (!isValidURL(videoURL.value)) {
                errors.push('Please enter a valid video URL');
                highlightErrorField('VideoURL');
                isValid = false;
            }
        }

        if (!isValid) {
            const errorMessage = errors.length === 1
                ? errors[0]
                : `Please fix the following errors:\n• ${errors.join('\n• ')}`;
            showToast(errorMessage, 'error');
        }

        console.log(`✅ Form validation ${isValid ? 'passed' : 'failed'}`);
        return isValid;
    }

    function validateVideoFileClient(file) {
        const maxFileSize = 800 * 1024 * 1024; // 800MB
        const allowedExtensions = ['.mp4', '.webm', '.ogg', '.avi', '.mov', '.wmv', '.flv', '.mkv'];
        const allowedTypes = [
            'video/mp4', 'video/webm', 'video/ogg', 'video/avi',
            'video/quicktime', 'video/x-msvideo', 'video/x-flv', 'video/x-matroska'
        ];

        console.log('🔍 Validating video file:', file.name, file.size, file.type);

        if (file.size > maxFileSize) {
            return {
                isValid: false,
                errorMessage: `File size (${formatFileSize(file.size)}) exceeds maximum allowed size (800MB)`
            };
        }

        const extension = getFileExtension(file.name).toLowerCase();
        if (!allowedExtensions.includes(extension)) {
            return {
                isValid: false,
                errorMessage: `File type "${extension}" not supported. Allowed types: ${allowedExtensions.join(', ')}`
            };
        }

        if (file.type && !file.type.startsWith('video/') && !allowedTypes.includes(file.type)) {
            console.warn('⚠️ Unexpected content type:', file.type);
        }

        return { isValid: true };
    }

    function initializeFormValidation() {
        // Real-time title validation
        const titleInputs = document.querySelectorAll('#addTitle, #editTitle');
        titleInputs.forEach(input => {
            input.addEventListener('input', function () {
                validateTitleField(this);
            });
        });

        // File input validation
        const fileInputs = document.querySelectorAll('#addVideoFile, #editVideoFile');
        fileInputs.forEach(input => {
            input.addEventListener('change', function () {
                if (this.files.length > 0) {
                    validateVideoFileField(this);
                }
            });
        });

        // URL input validation
        const urlInputs = document.querySelectorAll('#addVideoURL, #editVideoURL');
        urlInputs.forEach(input => {
            input.addEventListener('blur', function () {
                if (this.value.trim()) {
                    validateURLField(this);
                }
            });
        });
    }

    function validateTitleField(input) {
        const value = input.value.trim();
        const isValid = value.length > 0 && value.length <= 100;

        if (isValid) {
            input.classList.remove('is-invalid');
            input.classList.add('is-valid');
        } else {
            input.classList.remove('is-valid');
            if (value.length > 0) {
                input.classList.add('is-invalid');
            }
        }

        return isValid;
    }

    function validateVideoFileField(input) {
        if (input.files.length === 0) return true;

        const file = input.files[0];
        const validation = validateVideoFileClient(file);

        if (validation.isValid) {
            input.classList.remove('is-invalid');
            input.classList.add('is-valid');

            const infoDiv = input.parentElement.querySelector('.file-info') || createFileInfoDiv(input);
            infoDiv.innerHTML = `
                <small class="text-success">
                    <i class="bi bi-check-circle me-1"></i>
                    ${file.name} (${formatFileSize(file.size)})
                </small>
            `;
        } else {
            input.classList.remove('is-valid');
            input.classList.add('is-invalid');

            const infoDiv = input.parentElement.querySelector('.file-info') || createFileInfoDiv(input);
            infoDiv.innerHTML = `
                <small class="text-danger">
                    <i class="bi bi-exclamation-triangle me-1"></i>
                    ${validation.errorMessage}
                </small>
            `;
        }

        return validation.isValid;
    }

    function validateURLField(input) {
        const value = input.value.trim();
        const isValid = !value || isValidURL(value);

        if (isValid) {
            input.classList.remove('is-invalid');
            if (value) input.classList.add('is-valid');
        } else {
            input.classList.remove('is-valid');
            input.classList.add('is-invalid');
        }

        return isValid;
    }

    function createFileInfoDiv(input) {
        const infoDiv = document.createElement('div');
        infoDiv.className = 'file-info mt-1';
        input.parentElement.appendChild(infoDiv);
        return infoDiv;
    }

    function clearValidationErrors(form) {
        const inputs = form.querySelectorAll('.is-invalid, .is-valid');
        inputs.forEach(input => {
            input.classList.remove('is-invalid', 'is-valid');
        });

        const fileInfos = form.querySelectorAll('.file-info');
        fileInfos.forEach(info => info.remove());
    }

    function highlightErrorField(fieldName) {
        const field = document.getElementById(`add${fieldName}`) || document.getElementById(`edit${fieldName}`);
        if (field) {
            field.classList.add('is-invalid');
            field.focus();
        }
    }

    // ========== VIDEO HANDLERS ==========
    function initializeVideoHandlers() {
        // Video file input handlers
        const videoFileInputs = document.querySelectorAll('input[type="file"][accept*="video"]');
        videoFileInputs.forEach(input => {
            input.addEventListener('change', function (e) {
                const file = e.target.files[0];
                if (file) {
                    handleVideoPreviewFixed(file, input);
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

        // Clear video buttons
        const clearButtons = document.querySelectorAll('#clearAddVideo, #clearEditVideo');
        clearButtons.forEach(button => {
            button.addEventListener('click', function () {
                clearVideoPreview();
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

    function handleVideoPreviewFixed(file, input) {
        console.log('🎬 Handling video preview for:', file.name);

        // Create object URL for preview
        const videoUrl = URL.createObjectURL(file);

        // Update preview
        updateVideoPreview(videoUrl);

        // Clean up object URL after a delay to prevent memory leaks
        setTimeout(() => {
            URL.revokeObjectURL(videoUrl);
        }, 10000);
    }

    function updateVideoPreview(videoUrl) {
        const currentVideo = document.getElementById('currentVideo') || document.getElementById('addPreviewVideo');
        const placeholder = document.getElementById('currentVideoPlaceholder') || document.getElementById('addVideoPlaceholder');

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
            currentVideo.src = '';
            console.log('🎬 Video preview cleared');
        }
    }

    function clearVideoPreview() {
        updateVideoPreview('');

        // Clear file inputs
        const fileInputs = document.querySelectorAll('input[type="file"][accept*="video"]');
        fileInputs.forEach(input => {
            input.value = '';
            input.classList.remove('is-invalid', 'is-valid');
        });

        // Clear URL inputs
        const urlInputs = document.querySelectorAll('input[name="VideoURL"]');
        urlInputs.forEach(input => {
            input.value = '';
            input.classList.remove('is-invalid', 'is-valid');
        });
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
            safeSetValue('editVideoURL', '');
            updateVideoPreview('');
            showNoVideoFallback();
            showToast('Video removed', 'success');
        }
    }

    // ========== FORM DEFAULTS ==========
    function initializeFormDefaults() {
        console.log('🎬 Initializing form defaults');

        // Set today's date as default for new videos
        const videoDateInput = document.getElementById('addVideoDate');
        if (videoDateInput && !videoDateInput.value) {
            const today = new Date().toISOString().split('T')[0];
            videoDateInput.value = today;
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

        const row = findVideoRowById(videoId);
        if (row) {
            console.log('📋 Found table row, extracting data...');
            const tableData = extractTableData(row);
            populateFromTableData(tableData);
        }

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

        const cells = row.querySelectorAll('td');
        if (cells.length >= 4) {
            const videoCell = cells[0];
            const titleEl = videoCell.querySelector('.video-title, .fw-semibold');
            const numberEl = videoCell.querySelector('.video-number, .text-muted.small');

            if (titleEl && !dataFromAttributes.title) {
                dataFromAttributes.title = titleEl.textContent.trim();
            }
            if (numberEl && !dataFromAttributes.videoNumber) {
                dataFromAttributes.videoNumber = numberEl.textContent.trim();
            }

            if (cells[1] && !dataFromAttributes.videoDate) {
                dataFromAttributes.videoDate = cells[1].textContent.trim();
            }

            if (cells[2] && !dataFromAttributes.status) {
                const statusEl = cells[2].querySelector('.badge, .video-status');
                if (statusEl) {
                    dataFromAttributes.status = statusEl.textContent.trim();
                }
            }

            if (cells[3] && !dataFromAttributes.clientId) {
                dataFromAttributes.clientId = cells[3].textContent.trim();
            }
        }

        return dataFromAttributes;
    }

    function populateFromTableData(data) {
        console.log('📝 Populating form from table data:', data);

        try {
            window.currentVideoData = data;

            safeSetValue('editTitle', data.title);
            safeSetValue('editVideoName', data.videoName);
            safeSetValue('editClientId', data.clientId);
            safeSetValue('editVideoNumber', data.videoNumber);

            if (data.videoUrl) {
                safeSetValue('editVideoURL', data.videoUrl);
                updateVideoPreview(data.videoUrl);
            }

            if (data.videoDate) {
                safeSetValue('editVideoDate', formatDateForInput(data.videoDate));
            } else if (data.createdDate) {
                safeSetValue('editVideoDate', formatDateForInput(data.createdDate));
            }

            safeSetSelect('editStatus', data.status || 'Active');

            console.log('✅ Table data populated successfully');
        } catch (error) {
            console.error('🚨 Error populating from table data:', error);
        }
    }

    function populateFromAPIDataEnhanced(data) {
        console.log('🌐 Populating form from enhanced API data:', data);

        try {
            const videoData = data.video || data;
            window.currentVideoData = videoData;

            safeSetValue('editTitle', videoData.title || videoData.Title);
            safeSetValue('editVideoName', videoData.videoName || videoData.VideoName);
            safeSetValue('editClientId', videoData.clientId || videoData.ClientId);
            safeSetValue('editVideoNumber', videoData.videoNumber || videoData.VideoNumber);

            const videoUrl = videoData.videoURL || videoData.VideoURL || videoData.videoUrl;
            if (videoUrl) {
                safeSetValue('editVideoURL', videoUrl);
                updateVideoPreview(videoUrl);
            }

            const videoDate = videoData.videoDate || videoData.VideoDate || videoData.createdDate || videoData.CreatedDate;
            if (videoDate) {
                safeSetValue('editVideoDate', formatDateForInput(videoDate));
            }

            safeSetSelect('editStatus', videoData.status || videoData.Status || 'Active');

            console.log('✅ Enhanced API data populated successfully');

            const activeTab = document.querySelector('#editVideoTabs .nav-link.active');
            if (activeTab && activeTab.getAttribute('data-bs-target') === '#video-info-tab-pane') {
                updateVideoInfoDisplayFromCurrentData();
            }
        } catch (error) {
            console.error('💥 Error populating from API data:', error);
        }
    }

    // ========== VIDEO INFO TAB ==========
    function updateVideoInfoDisplayFromCurrentData() {
        console.log('📊 Updating video info display from current data');

        let videoData = window.currentVideoData;

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

        const initials = getVideoInitials(title);
        safeUpdateElement('videoInfoInitials', initials);
        safeUpdateElement('videoInfoTitle', title);
        safeUpdateElement('videoInfoNumber', videoNumber);
        safeUpdateElement('videoInfoStatus', status);
        safeUpdateElement('videoInfoClient', clientId);

        safeUpdateElement('videoInfoTitleDetail', title);
        safeUpdateElement('videoInfoVideoName', videoName);
        safeUpdateElement('videoInfoDate', formatDisplayDate(videoDate));
        safeUpdateElement('videoInfoClientDetail', clientId);
        safeUpdateElement('videoInfoSize', 'Unknown');

        const statusBadge = document.getElementById('videoInfoStatus');
        if (statusBadge) {
            statusBadge.className = 'badge ' + getStatusBadgeClass(status);
        }

        safeUpdateElement('videoInfoViews', Math.floor(Math.random() * 1000));
        safeUpdateElement('videoInfoDownloads', Math.floor(Math.random() * 100));
        safeUpdateElement('videoInfoRating', (Math.random() * 2 + 3).toFixed(1));

        console.log('✅ Video info display updated successfully');
    }

    // ========== VIDEO MEDIA TAB ==========
    function loadVideoMediaTab(videoId) {
        console.log('🎬 Loading video media tab for:', videoId);

        const currentVideoUrl = getCurrentVideoUrl();

        console.log('🎬 Current video URL:', currentVideoUrl);

        if (currentVideoUrl) {
            loadVideoDirectly(currentVideoUrl);
            updateVideoInfoFromCurrentVideo(currentVideoUrl);

            const uploadArea = document.getElementById('videoUploadArea');
            if (uploadArea) {
                uploadArea.style.display = 'none';
            }
        } else {
            showNoVideoFallback();

            const uploadArea = document.getElementById('videoUploadArea');
            if (uploadArea) {
                uploadArea.style.display = 'block';
            }
        }
    }

    function getCurrentVideoUrl() {
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

        videoContent.innerHTML = '';

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

        const formatElement = document.getElementById('mediaFormat');
        if (formatElement) {
            const extension = videoUrl.split('.').pop().toLowerCase();
            formatElement.textContent = extension.toUpperCase();
        }

        const urlElement = document.getElementById('mediaUrl');
        if (urlElement) {
            const shortUrl = videoUrl.length > 30 ? videoUrl.substring(0, 27) + '...' : videoUrl;
            urlElement.textContent = shortUrl;
            urlElement.title = videoUrl;
        }

        const sizeElement = document.getElementById('mediaFileSize');
        if (sizeElement) {
            sizeElement.textContent = 'Unknown';
        }

        const resolutionElement = document.getElementById('mediaResolution');
        if (resolutionElement) {
            resolutionElement.textContent = 'Loading...';
        }

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

        let row = document.querySelector(`tr[data-video-id="${videoId}"]`);
        if (row) return row;

        const button = document.querySelector(`button[data-video-id="${videoId}"]`);
        if (button) {
            row = button.closest('tr');
            if (row) return row;
        }

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

    function isValidURL(string) {
        try {
            const url = new URL(string);
            return url.protocol === 'http:' || url.protocol === 'https:';
        } catch (_) {
            return false;
        }
    }

    function getFileExtension(filename) {
        const lastDotIndex = filename.lastIndexOf('.');
        if (lastDotIndex === -1) return '';
        return filename.slice(lastDotIndex); // Include the dot
    }

    function formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
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
        if (!tokenInput) {
            console.error('❌ Anti-forgery token not found');
            return null;
        }
        return tokenInput.value;
    }

    function showToast(message, type = 'success') {
        if (window.UIUtils) {
            window.UIUtils.showToast(message, type);
        } else {
            console.log(`${type}: ${message}`);
            alert(`${type}: ${message}`);
        }
    }

    // ========== GLOBAL API ==========
    window.videoDebugFixed = {
        validateVideoForm,
        validateVideoFileClient,
        handleVideoPreviewFixed,
        updateVideoPreview,
        clearVideoPreview,
        formatFileSize,
        getFileExtension,
        isValidURL,
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
        initializeFilters
    };

    console.log('✅ Complete Fixed Video Management loaded successfully');
    console.log('🐛 Debug functions available: window.videoDebugFixed');
});