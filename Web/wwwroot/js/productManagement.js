/**
 * Complete Enhanced Product Management JavaScript
 * Includes both Edit and Add Product functionality with file upload support
 */

document.addEventListener('DOMContentLoaded', function () {
    console.log('🚀 Initializing Complete Enhanced Product Management');

    // Initialize components
    initializeDataTable();
    initializeModals();
    initializeFilters();
    initializeForms();
    initializeAddProductModal();

    // Enhanced debugging
    window.productDebug = {
        loadProductData,
        testGetProductData,
        findProductRowById,
        populateFromTableData,
        populateFromAPIData,
        checkAPIConfiguration,
        testTableDataExtraction,
        testAddProduct,
        clearAddForm,
        validateAddForm
    };

    // Verify API configuration
    checkAPIConfiguration();

    console.log('✅ Complete Enhanced Product Management loaded successfully');

    // ========== EDIT PRODUCT MODAL HANDLERS ==========

    /**
     * Enhanced modal show handler with better debugging
     */
    function handleEditModalShow(event) {
        const button = event.relatedTarget;
        const productId = button.getAttribute('data-product-id');

        console.log('📂 Opening edit modal for product ID:', productId);
        console.log('📂 Button element:', button);

        if (!productId) {
            console.error('🚨 No product ID found on button');
            console.log('Available data attributes:', Array.from(button.attributes).map(attr => `${attr.name}="${attr.value}"`));
            showToast('Error: Product ID is missing', 'error');
            return;
        }

        // Set product ID in all forms
        setProductIdInForms(productId);

        // Clear previous data
        clearAllForms();

        // Show loading state immediately
        showLoadingState();

        // Load product data with enhanced debugging
        loadProductDataEnhanced(productId);
    }

    /**
     * Enhanced product data loading with better error handling
     */
    function loadProductDataEnhanced(productId) {
        console.log('📥 Loading product data for ID:', productId);

        if (!productId) {
            console.error('🚨 No product ID provided');
            hideLoadingState();
            return;
        }

        // Step 1: Try to populate from table data immediately
        const row = findProductRowById(productId);
        if (row) {
            console.log('📋 Found table row, extracting data...');
            const tableData = extractTableData(row);
            console.log('📦 Extracted table data:', tableData);
            populateFromTableData(tableData);
            console.log('✅ Table data populated successfully');
        } else {
            console.warn('⚠️ No table row found for product ID:', productId);
            // Try alternative approach - search all rows
            tryFindRowAlternative(productId);
        }

        // Step 2: Always call API for complete data if available
        if (window.appUrls?.getProductData) {
            callGetProductDataAPIEnhanced(productId);
        } else {
            console.error('🚨 GetProductData API URL not configured');
            hideLoadingState();
            showToast('API not configured. Only table data available.', 'warning');
        }
    }

    /**
     * Enhanced API call with better error handling
     */
    function callGetProductDataAPIEnhanced(productId) {
        const url = `${window.appUrls.getProductData}?id=${encodeURIComponent(productId)}`;
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
                console.log('📡 API Response headers:', Object.fromEntries(response.headers.entries()));

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
                    showToast('Product data loaded successfully', 'success');
                } else {
                    console.error('🚨 API returned error:', data?.message || 'Unknown error');
                    showToast(data?.message || 'Failed to load complete product data', 'warning');
                }
            })
            .catch(error => {
                console.error('💥 API Error:', error);
                hideLoadingState();
                showToast(`API Error: ${error.message}. Using table data only.`, 'warning');
            });
    }

    /**
     * Enhanced table data extraction
     */
    function extractTableData(row) {
        if (!row) return {};

        console.log('📋 Extracting data from row:', row);

        // Method 1: Try data attributes
        const dataFromAttributes = {
            productId: row.getAttribute('data-product-id'),
            title: row.getAttribute('data-product-title'),
            productNumber: row.getAttribute('data-product-number'),
            description: row.getAttribute('data-description'),
            price: row.getAttribute('data-price'),
            points: row.getAttribute('data-points'),
            tag: row.getAttribute('data-tag'),
            imageUrl: row.getAttribute('data-image-url'),
            status: row.getAttribute('data-status'),
            category: row.getAttribute('data-category'),
            type: row.getAttribute('data-type')
        };

        console.log('📊 Data from attributes:', dataFromAttributes);

        // Method 2: Try extracting from cell content as fallback
        const cells = row.querySelectorAll('td');
        console.log('📊 Found cells:', cells.length);

        if (cells.length >= 5) {
            // Extract from the product column (first column)
            const productCell = cells[0];
            const titleElement = productCell.querySelector('.product-title, .fw-semibold');
            const numberElement = productCell.querySelector('.product-number, .text-muted.small');

            // Extract from price column
            const priceCell = cells[1];
            const priceText = priceCell.textContent.replace('$', '').trim();

            // Extract from points column
            const pointsCell = cells[2];
            const pointsText = pointsCell.textContent.trim();

            // Extract from status column
            const statusCell = cells[3];
            const statusElement = statusCell.querySelector('.badge, .product-status');

            // Extract from category column
            const categoryCell = cells[4];
            const categoryText = categoryCell.textContent.trim();

            const dataFromCells = {
                title: titleElement?.textContent?.trim() || dataFromAttributes.title,
                productNumber: numberElement?.textContent?.trim() || dataFromAttributes.productNumber,
                price: priceText || dataFromAttributes.price,
                points: pointsText || dataFromAttributes.points,
                status: statusElement?.textContent?.trim() || dataFromAttributes.status,
                category: categoryText || dataFromAttributes.category
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

    /**
     * Enhanced form population from table data
     */
    function populateFromTableData(data) {
        console.log('📝 Populating form from table data:', data);

        try {
            // Basic text fields
            safeSetValue('editTitle', data.title);
            safeSetValue('editProductNumber', data.productNumber);
            safeSetValue('editDescription', data.description);
            safeSetValue('editTag', data.tag);

            // Image handling
            if (data.imageUrl) {
                safeSetValue('editImageURL', data.imageUrl);
                updateImagePreview(data.imageUrl, 'edit');
            }

            // Numeric fields with validation
            if (data.price) {
                const price = parseFloat(data.price.toString().replace(/[^0-9.]/g, ''));
                if (!isNaN(price)) {
                    safeSetValue('editPrice', price.toFixed(2));
                }
            }

            if (data.points) {
                const points = parseInt(data.points.toString().replace(/[^0-9]/g, ''), 10);
                if (!isNaN(points)) {
                    safeSetValue('editPoints', points.toString());
                }
            }

            // Select fields
            safeSetSelect('editStatus', capitalizeFirst(data.status));
            safeSetSelect('editCategory', capitalizeFirst(data.category));
            safeSetSelect('editType', capitalizeFirst(data.type));

            console.log('✅ Table data populated successfully');
        } catch (error) {
            console.error('💥 Error populating from table data:', error);
        }
    }

    /**
     * Enhanced API data population
     */
    function populateFromAPIDataEnhanced(data) {
        console.log('🌐 Populating form from API data:', data);

        try {
            // Handle different possible data structures
            const productData = data.product || data;

            // Basic information
            safeSetValue('editTitle', productData.Title || productData.title);
            safeSetValue('editProductNumber', productData.ProductNumber || productData.productNumber);
            safeSetValue('editDescription', productData.Description || productData.description);
            safeSetValue('editTag', productData.Tag || productData.tag);

            // Image handling
            const imageUrl = productData.ImageURL || productData.imageURL || productData.imageUrl;
            if (imageUrl) {
                safeSetValue('editImageURL', imageUrl);
                updateImagePreview(imageUrl, 'edit');
            }

            // Numeric fields with validation
            const price = productData.Price || productData.price;
            if (price !== null && price !== undefined) {
                const priceValue = parseFloat(price);
                safeSetValue('editPrice', isNaN(priceValue) ? '0.00' : priceValue.toFixed(2));
            }

            const points = productData.Points || productData.points;
            if (points !== null && points !== undefined) {
                const pointsValue = parseInt(points, 10);
                safeSetValue('editPoints', isNaN(pointsValue) ? '0' : pointsValue.toString());
            }

            // Select fields
            safeSetSelect('editType', productData.Type || productData.type);
            safeSetSelect('editCategory', productData.Category || productData.category);
            safeSetSelect('editStatus', productData.Status || productData.status);

            console.log('✅ API data populated successfully');

            // Update Product Info tab
            updateProductInfoDisplay(productData);
        } catch (error) {
            console.error('💥 Error populating from API data:', error);
        }
    }

    /**
     * Try alternative methods to find the row
     */
    function tryFindRowAlternative(productId) {
        console.log('🔍 Trying alternative row finding methods for ID:', productId);

        // Method 1: Search all buttons with data-product-id
        const allButtons = document.querySelectorAll('button[data-product-id]');
        console.log('🔍 Found buttons with data-product-id:', allButtons.length);

        for (const button of allButtons) {
            if (button.getAttribute('data-product-id') === productId) {
                const row = button.closest('tr');
                if (row) {
                    console.log('✅ Found row via button search');
                    const tableData = extractTableData(row);
                    populateFromTableData(tableData);
                    return;
                }
            }
        }

        // Method 2: Search DataTable if available
        if (window.$ && $.fn.dataTable && $.fn.dataTable.isDataTable('#productsTable')) {
            const table = $('#productsTable').DataTable();
            const rows = table.rows().nodes();

            for (let i = 0; i < rows.length; i++) {
                const row = rows[i];
                const button = row.querySelector(`[data-product-id="${productId}"]`);
                if (button) {
                    console.log('✅ Found row via DataTable search');
                    const tableData = extractTableData(row);
                    populateFromTableData(tableData);
                    return;
                }
            }
        }

        console.warn('⚠️ Could not find row using any method');
    }

    /**
     * Enhanced row finding with multiple strategies
     */
    function findProductRowById(productId) {
        if (!productId) return null;

        console.log('🔍 Looking for row with product ID:', productId);

        // Strategy 1: Direct row attribute search
        let row = document.querySelector(`tr[data-product-id="${productId}"]`);
        if (row) {
            console.log('✅ Found row by data-product-id (Strategy 1)');
            return row;
        }

        // Strategy 2: Button-based search
        const button = document.querySelector(`button[data-product-id="${productId}"]`);
        if (button) {
            row = button.closest('tr');
            if (row) {
                console.log('✅ Found row via button (Strategy 2)');
                return row;
            }
        }

        // Strategy 3: Search within table body
        const tableBody = document.querySelector('#productsTable tbody');
        if (tableBody) {
            const allRows = tableBody.querySelectorAll('tr');
            for (const tr of allRows) {
                const editBtn = tr.querySelector(`[data-product-id="${productId}"]`);
                if (editBtn) {
                    console.log('✅ Found row via table body search (Strategy 3)');
                    return tr;
                }
            }
        }

        console.warn('⚠️ Row not found for product ID:', productId);
        return null;
    }

    // ========== ADD PRODUCT MODAL FUNCTIONALITY ==========

    /**
     * Initialize Add Product Modal with file upload functionality
     */
    function initializeAddProductModal() {
        console.log('🎯 Initializing Add Product Modal with File Upload');

        // Get modal and form elements
        const addModal = document.getElementById('addProductModal');
        const addForm = document.getElementById('addProductForm');
        const fileInput = document.getElementById('addImageFile');
        const urlInput = document.getElementById('addImageURL');
        const clearBtn = document.getElementById('clearAddImage');

        if (!addModal || !addForm) {
            console.warn('⚠️ Add product modal elements not found');
            return;
        }

        // File input change handler
        if (fileInput) {
            fileInput.addEventListener('change', function (e) {
                const file = e.target.files[0];
                if (file) {
                    handleFileSelection(file);
                    // Clear URL input when file is selected
                    if (urlInput) urlInput.value = '';
                }
            });
        }

        // URL input change handler
        if (urlInput) {
            urlInput.addEventListener('blur', function () {
                const url = this.value.trim();
                if (url) {
                    handleUrlInput(url);
                    // Clear file input when URL is provided
                    if (fileInput) fileInput.value = '';
                }
            });

            // Real-time URL validation
            urlInput.addEventListener('input', function () {
                const url = this.value.trim();
                if (url && !isValidImageUrl(url)) {
                    this.classList.add('is-invalid');
                } else {
                    this.classList.remove('is-invalid');
                }
            });
        }

        // Clear image button
        if (clearBtn) {
            clearBtn.addEventListener('click', function () {
                clearImagePreview('add');
            });
        }

        // Drag and drop functionality
        const fileInputArea = fileInput?.parentElement;
        if (fileInputArea) {
            setupDragAndDrop(fileInputArea, fileInput, urlInput);
        }

        // Modal show/hide handlers
        addModal.addEventListener('show.bs.modal', function () {
            console.log('📝 Opening add product modal');
            clearAddForm();
        });

        addModal.addEventListener('hidden.bs.modal', function () {
            console.log('🚪 Closing add product modal');
            clearAddForm();
        });

        // Form submission handler
        addForm.addEventListener('submit', function (e) {
            if (!validateAddProductForm()) {
                e.preventDefault();
                return false;
            }

            console.log('📤 Submitting add product form');
            const submitBtn = this.querySelector('button[type="submit"]');
            if (submitBtn) {
                setButtonLoading(submitBtn, true, 'Adding Product...');
            }

            showToast('Creating product...', 'info');
        });

        console.log('✅ Add product modal initialized');
    }

    /**
     * Setup drag and drop functionality
     */
    function setupDragAndDrop(fileInputArea, fileInput, urlInput) {
        fileInputArea.addEventListener('dragover', function (e) {
            e.preventDefault();
            this.classList.add('drag-over');
        });

        fileInputArea.addEventListener('dragleave', function (e) {
            e.preventDefault();
            this.classList.remove('drag-over');
        });

        fileInputArea.addEventListener('drop', function (e) {
            e.preventDefault();
            this.classList.remove('drag-over');

            const files = e.dataTransfer.files;
            if (files.length > 0) {
                const file = files[0];
                if (file.type.startsWith('image/')) {
                    fileInput.files = files;
                    handleFileSelection(file);
                    if (urlInput) urlInput.value = '';
                } else {
                    showToast('Please drop an image file', 'error');
                }
            }
        });
    }

    /**
     * Handle file selection for add product modal
     */
    function handleFileSelection(file) {
        console.log('📁 File selected:', file.name, 'Size:', formatFileSize(file.size));

        // Validate file
        if (!validateImageFile(file)) {
            return;
        }

        // Create preview
        const reader = new FileReader();
        reader.onload = function (e) {
            updateImagePreview(e.target.result, 'add');
            showToast(`Image "${file.name}" loaded successfully`, 'success');
        };
        reader.onerror = function () {
            showToast('Error reading file. Please try again.', 'error');
        };
        reader.readAsDataURL(file);
    }

    /**
     * Handle URL input for add product modal
     */
    function handleUrlInput(url) {
        console.log('🔗 URL entered:', url);

        // Basic URL validation
        if (!isValidImageUrl(url)) {
            showToast('Please enter a valid image URL', 'warning');
            return;
        }

        // Show loading state
        setPlaceholderLoading('add');

        // Test if the image loads
        const testImg = new Image();
        testImg.onload = function () {
            updateImagePreview(url, 'add');
            showToast('Image loaded from URL successfully', 'success');
        };
        testImg.onerror = function () {
            resetPlaceholder('add');
            showToast('Failed to load image from URL. Please check the URL and try again.', 'error');
        };
        testImg.src = url;
    }

    /**
     * Validate image file
     */
    function validateImageFile(file) {
        const maxSize = 5 * 1024 * 1024; // 5MB
        const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp', 'image/bmp'];

        if (file.size > maxSize) {
            showToast(`File size (${formatFileSize(file.size)}) must be less than 5MB`, 'error');
            return false;
        }

        if (!allowedTypes.includes(file.type.toLowerCase())) {
            showToast('File type not supported. Please use JPG, PNG, GIF, WebP, or BMP.', 'error');
            return false;
        }

        return true;
    }

    /**
     * Validate image URL
     */
    function isValidImageUrl(url) {
        try {
            new URL(url);
            const imageExtensions = ['.jpg', '.jpeg', '.png', '.gif', '.webp', '.bmp'];
            const urlLower = url.toLowerCase();
            return imageExtensions.some(ext => urlLower.includes(ext)) ||
                url.includes('image') ||
                url.includes('img') ||
                /\/(photo|picture|pic)\//i.test(url);
        } catch {
            return false;
        }
    }

    /**
     * Validate Add Product Form
     */
    function validateAddProductForm() {
        const title = document.getElementById('addTitle')?.value?.trim();
        const price = document.getElementById('addPrice')?.value;
        const type = document.getElementById('addType')?.value;
        const category = document.getElementById('addCategory')?.value;

        const errors = [];

        if (!title) {
            errors.push('Product title is required');
            markFieldInvalid('addTitle');
        } else {
            markFieldValid('addTitle');
        }

        if (!price || isNaN(parseFloat(price)) || parseFloat(price) < 0) {
            errors.push('Valid price is required');
            markFieldInvalid('addPrice');
        } else {
            markFieldValid('addPrice');
        }

        if (!type) {
            errors.push('Product type is required');
            markFieldInvalid('addType');
        } else {
            markFieldValid('addType');
        }

        if (!category) {
            errors.push('Product category is required');
            markFieldInvalid('addCategory');
        } else {
            markFieldValid('addCategory');
        }

        if (errors.length > 0) {
            showToast('Please fix the following errors:\n• ' + errors.join('\n• '), 'error');
            return false;
        }

        return true;
    }

    // ========== IMAGE PREVIEW FUNCTIONS ==========

    /**
     * Update image preview for both add and edit modals
     */
    function updateImagePreview(src, modalType = 'edit') {
        const prefix = modalType === 'add' ? 'add' : 'current';
        const previewImg = document.getElementById(`${prefix === 'add' ? 'addPreview' : 'current'}Image`);
        const placeholder = document.getElementById(`${prefix === 'add' ? 'add' : 'current'}ImagePlaceholder`);

        if (previewImg && placeholder) {
            previewImg.src = src;
            previewImg.style.display = 'block';
            placeholder.style.display = 'none';

            // Add image preview container styling
            const container = document.getElementById(`${prefix === 'add' ? 'add' : 'current'}ImagePreview`);
            if (container) {
                container.classList.add('has-image');
            }

            // Add image load animation
            previewImg.style.opacity = '0';
            previewImg.style.transform = 'scale(0.8)';

            setTimeout(() => {
                previewImg.style.transition = 'all 0.3s ease';
                previewImg.style.opacity = '1';
                previewImg.style.transform = 'scale(1)';
            }, 50);

            console.log(`🖼️ ${modalType} image preview updated`);
        }
    }

    /**
     * Clear image preview for both add and edit modals
     */
    function clearImagePreview(modalType = 'edit') {
        console.log(`🧹 Clearing ${modalType} image preview`);

        const prefix = modalType === 'add' ? 'add' : 'edit';
        const fileInput = document.getElementById(`${prefix}ImageFile`);
        const urlInput = document.getElementById(`${prefix}ImageURL`);

        // Clear inputs
        if (fileInput) fileInput.value = '';
        if (urlInput) {
            urlInput.value = '';
            urlInput.classList.remove('is-invalid');
        }

        resetPlaceholder(modalType);
        showToast('Image cleared', 'info');
    }

    /**
     * Reset placeholder to default state
     */
    function resetPlaceholder(modalType = 'edit') {
        const prefix = modalType === 'add' ? 'add' : 'current';
        const previewImg = document.getElementById(`${prefix === 'add' ? 'addPreview' : 'current'}Image`);
        const placeholder = document.getElementById(`${prefix === 'add' ? 'add' : 'current'}ImagePlaceholder`);

        if (previewImg && placeholder) {
            previewImg.src = '';
            previewImg.style.display = 'none';
            placeholder.style.display = 'flex';

            // Reset placeholder content
            const placeholderText = modalType === 'add' ? 'Image Preview' : 'Current Image';
            const placeholderSubtext = modalType === 'add' ? 'No image selected' : 'No image uploaded';

            placeholder.innerHTML = `
                <i class="bi bi-image text-muted" style="font-size: 3rem;"></i>
                <p class="text-muted mt-2 mb-0">${placeholderText}</p>
                <small class="text-muted">${placeholderSubtext}</small>
            `;
        }

        // Remove container styling
        const container = document.getElementById(`${prefix === 'add' ? 'add' : 'current'}ImagePreview`);
        if (container) {
            container.classList.remove('has-image');
        }
    }

    /**
     * Set placeholder to loading state
     */
    function setPlaceholderLoading(modalType = 'edit') {
        const prefix = modalType === 'add' ? 'add' : 'current';
        const placeholder = document.getElementById(`${prefix === 'add' ? 'add' : 'current'}ImagePlaceholder`);

        if (placeholder) {
            placeholder.innerHTML = `
                <i class="bi bi-hourglass-split text-muted" style="font-size: 3rem;"></i>
                <p class="text-muted mt-2 mb-0">Loading Image...</p>
                <small class="text-muted">Please wait</small>
            `;
        }
    }

    // ========== DATATABLE INITIALIZATION ==========

    /**
     * Initialize DataTable
     */
    function initializeDataTable() {
        const tableElement = $('#productsTable');
        if (tableElement.length > 0) {
            tableElement.DataTable({
                responsive: true,
                lengthMenu: [[10, 25, 50, -1], [10, 25, 50, "All"]],
                dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>rtip',
                language: {
                    search: "_INPUT_",
                    searchPlaceholder: "Search products...",
                    lengthMenu: "Show _MENU_ products per page",
                    info: "Showing _START_ to _END_ of _TOTAL_ products",
                    infoEmpty: "Showing 0 to 0 of 0 products",
                    infoFiltered: "(filtered from _MAX_ total products)"
                },
                columnDefs: [
                    { className: "align-middle", targets: "_all" },
                    { orderable: false, targets: [5] }
                ],
                order: [[0, 'asc']]
            });
            console.log('📊 DataTable initialized');
        }
    }

    // ========== MODAL INITIALIZATION ==========

    /**
     * Initialize modal event handlers
     */
    function initializeModals() {
        const editProductModal = document.getElementById('editProductModal');
        if (editProductModal) {
            editProductModal.addEventListener('show.bs.modal', handleEditModalShow);
            editProductModal.addEventListener('hidden.bs.modal', handleEditModalHide);

            // Tab switching handlers
            const tabButtons = editProductModal.querySelectorAll('button[data-bs-toggle="tab"]');
            tabButtons.forEach(button => {
                button.addEventListener('shown.bs.tab', handleTabSwitch);
            });

            console.log('📝 Edit modal event handlers initialized');
        }

        // Delete button handler
        const deleteProductBtn = document.getElementById('deleteProductBtn');
        if (deleteProductBtn) {
            deleteProductBtn.addEventListener('click', handleDeleteProduct);
        }
    }

    /**
     * Handle edit modal hide event
     */
    function handleEditModalHide() {
        console.log('🚪 Closing edit modal');
        clearAllForms();
        hideLoadingState();
    }

    /**
     * Handle tab switching
     */
    function handleTabSwitch(event) {
        const targetTab = event.target.getAttribute('data-bs-target');
        const productId = document.getElementById('editProductId')?.value;

        console.log('🔄 Switching to tab:', targetTab, 'for product:', productId);

        if (!productId) return;

        switch (targetTab) {
            case '#product-info-tab-pane':
                loadProductInfo(productId);
                break;
            case '#inventory-tab-pane':
                loadInventoryData(productId);
                break;
        }
    }

    /**
     * Handle delete product
     */
    function handleDeleteProduct() {
        const productId = document.getElementById('editProductId')?.value;
        if (!productId) return;

        const deleteField = document.getElementById('deleteProductId');
        if (deleteField) {
            deleteField.value = productId;
        }

        // Hide edit modal and show delete confirmation
        const editModal = bootstrap.Modal.getInstance(document.getElementById('editProductModal'));
        if (editModal) editModal.hide();

        const deleteModal = new bootstrap.Modal(document.getElementById('deleteProductModal'));
        deleteModal.show();
    }

    /**
     * Load product info for the Product Info tab
     */
    function loadProductInfo(productId) {
        console.log('📊 Loading product info for:', productId);

        // Check if already loaded
        const currentTitle = document.getElementById('productInfoTitle')?.textContent;
        if (currentTitle && currentTitle !== '--' && currentTitle !== 'Product') {
            console.log('✅ Product info already loaded');
            return;
        }

        // Call API if available
        if (window.appUrls?.getProductData) {
            callGetProductDataAPIEnhanced(productId);
        }
    }

    /**
     * Load inventory data for the Inventory tab
     */
    function loadInventoryData(productId) {
        console.log('📦 Loading inventory data for:', productId);

        // For now, populate with mock data
        safeSetValue('stockQuantity', Math.floor(Math.random() * 100));
        safeSetValue('lowStockThreshold', 5);
        safeSetValue('reorderLevel', 10);
    }

    /**
     * Update product info display
     */
    function updateProductInfoDisplay(data) {
        console.log('📊 Updating product info display');

        // Avatar and basic info
        const initials = getProductInitials(data.Title || data.title);
        safeUpdateElement('productInfoInitials', initials);
        safeUpdateElement('productInfoTitle', data.Title || data.title || 'Product');
        safeUpdateElement('productInfoNumber', data.ProductNumber || data.productNumber || 'No SKU');

        // Badges
        safeUpdateElement('productInfoStatus', data.Status || data.status || 'Active');
        safeUpdateElement('productInfoCategory', data.Category || data.category || 'No Category');

        // Detailed information
        safeUpdateElement('productInfoTitleDetail', data.Title || data.title || '--');
        safeUpdateElement('productInfoPrice', formatPrice(data.Price || data.price));
        safeUpdateElement('productInfoPoints', (data.Points || data.points)?.toString() || '0');
        safeUpdateElement('productInfoType', data.Type || data.type || '--');
        safeUpdateElement('productInfoTag', data.Tag || data.tag || '--');

        // Mock statistics
        safeUpdateElement('productInfoSales', Math.floor(Math.random() * 100));
        safeUpdateElement('productInfoViews', Math.floor(Math.random() * 1000));
        safeUpdateElement('productInfoRating', (Math.random() * 5).toFixed(1));

        console.log('✅ Product info display updated');
    }

    // ========== FORM MANAGEMENT ==========

    /**
     * Set product ID in all relevant form fields
     */
    function setProductIdInForms(productId) {
        const idFields = ['editProductId', 'deleteProductId'];
        idFields.forEach(fieldId => {
            const field = document.getElementById(fieldId);
            if (field) {
                field.value = productId;
                console.log(`✅ Set ${fieldId} to:`, productId);
            }
        });
    }

    /**
     * Clear all forms
     */
    function clearAllForms() {
        clearProductDetailsForm();
        clearProductInfoDisplay();
        clearInventoryForm();
    }

    /**
     * Clear add product form
     */
    function clearAddForm() {
        console.log('🧹 Clearing add product form');

        const form = document.getElementById('addProductForm');
        if (form) {
            form.reset();

            // Clear any validation classes
            const inputs = form.querySelectorAll('.is-invalid, .is-valid');
            inputs.forEach(input => {
                input.classList.remove('is-invalid', 'is-valid');
            });

            clearImagePreview('add');
        }
    }

    /**
     * Clear product details form
     */
    function clearProductDetailsForm() {
        const fields = [
            'editTitle', 'editProductNumber', 'editDescription',
            'editPrice', 'editPoints', 'editTag', 'editImageURL'
        ];

        fields.forEach(field => safeSetValue(field, ''));

        const selects = ['editType', 'editCategory', 'editStatus'];
        selects.forEach(select => {
            const element = document.getElementById(select);
            if (element) element.selectedIndex = 0;
        });

        updateImagePreview('', 'edit');
        console.log('🧹 Product details form cleared');
    }

    /**
     * Clear product info display
     */
    function clearProductInfoDisplay() {
        const elements = [
            'productInfoInitials', 'productInfoTitle', 'productInfoNumber',
            'productInfoStatus', 'productInfoCategory', 'productInfoTitleDetail',
            'productInfoPrice', 'productInfoPoints', 'productInfoType', 'productInfoTag'
        ];

        elements.forEach(elementId => safeUpdateElement(elementId, '--'));
        console.log('🧹 Product info display cleared');
    }

    /**
     * Clear inventory form
     */
    function clearInventoryForm() {
        const fields = ['stockQuantity', 'lowStockThreshold', 'reorderLevel'];
        fields.forEach(field => safeSetValue(field, ''));
        console.log('🧹 Inventory form cleared');
    }

    // ========== UI STATE MANAGEMENT ==========

    /**
     * Show loading state
     */
    function showLoadingState() {
        const modal = document.getElementById('editProductModal');
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
                    <div class="text-muted">Loading product data...</div>
                </div>
            `;
            modal.querySelector('.modal-content').appendChild(overlay);
        }

        // Disable form elements
        const formElements = modal.querySelectorAll('input, select, textarea, button[type="submit"]');
        formElements.forEach(el => el.disabled = true);

        console.log('⏳ Loading state shown');
    }

    /**
     * Hide loading state
     */
    function hideLoadingState() {
        const modal = document.getElementById('editProductModal');
        if (!modal) return;

        // Remove overlay
        const overlay = modal.querySelector('.modal-loading-overlay');
        if (overlay) {
            overlay.remove();
        }

        // Enable form elements
        const formElements = modal.querySelectorAll('input, select, textarea, button[type="submit"]');
        formElements.forEach(el => el.disabled = false);

        console.log('✅ Loading state hidden');
    }

    // ========== VALIDATION AND FEEDBACK ==========

    /**
     * Mark field as invalid
     */
    function markFieldInvalid(fieldId) {
        const field = document.getElementById(fieldId);
        if (field) {
            field.classList.add('is-invalid');
            field.classList.remove('is-valid');
        }
    }

    /**
     * Mark field as valid
     */
    function markFieldValid(fieldId) {
        const field = document.getElementById(fieldId);
        if (field) {
            field.classList.remove('is-invalid');
            field.classList.add('is-valid');
        }
    }

    /**
     * Set button loading state
     */
    function setButtonLoading(button, loading, text = 'Loading...') {
        if (!button) return;

        if (loading) {
            button.dataset.originalText = button.innerHTML;
            button.disabled = true;
            button.innerHTML = `<span class="spinner-border spinner-border-sm me-2"></span>${text}`;
        } else {
            button.disabled = false;
            button.innerHTML = button.dataset.originalText || button.innerHTML;
        }
    }

    // ========== UTILITY FUNCTIONS ==========

    /**
     * Check API configuration
     */
    function checkAPIConfiguration() {
        console.log('🔧 Checking API configuration...');

        if (!window.appUrls) {
            console.error('🚨 window.appUrls is not defined');
            return false;
        }

        console.log('🔧 Available API URLs:', window.appUrls);

        if (!window.appUrls.getProductData) {
            console.error('🚨 getProductData URL not configured');
            return false;
        }

        console.log('✅ API configuration OK');
        return true;
    }

    /**
     * Test function for GetProductData API
     */
    function testGetProductData(productId) {
        if (!productId) {
            console.log('Usage: testGetProductData("your-product-id")');
            return;
        }

        console.log('🧪 Testing GetProductData with ID:', productId);
        callGetProductDataAPIEnhanced(productId);
    }

    /**
     * Test table data extraction
     */
    function testTableDataExtraction(productId) {
        console.log('🧪 Testing table data extraction for:', productId);
        const row = findProductRowById(productId);
        if (row) {
            console.log('✅ Found row:', row);
            const data = extractTableData(row);
            console.log('📦 Extracted data:', data);
            return data;
        } else {
            console.log('❌ Row not found');
            return null;
        }
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

    function getProductInitials(title) {
        if (!title) return 'P';
        const words = title.split(' ');
        if (words.length >= 2) {
            return (words[0][0] + words[1][0]).toUpperCase();
        }
        return title[0].toUpperCase();
    }

    function formatPrice(price) {
        if (price === null || price === undefined) return '--';
        const numPrice = parseFloat(price);
        return isNaN(numPrice) ? '--' : `$${numPrice.toFixed(2)}`;
    }

    function formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }

    function addCacheBuster(url) {
        if (!url) return url;
        const separator = url.includes('?') ? '&' : '?';
        return `${url}${separator}v=${Date.now()}`;
    }

    function showToast(message, type = 'success') {
        console.log(`${type}: ${message}`);

        // Use UIUtils if available, otherwise fallback to console/alert
        if (window.UIUtils) {
            window.UIUtils.showToast(message, type);
        } else {
            // Fallback: simple alert for critical errors
            if (type === 'error') {
                alert('Error: ' + message);
            } else {
                console.log(`${type}: ${message}`);
            }
        }
    }

    // ========== FILTER INITIALIZATION ==========

    /**
     * Initialize filters
     */
    function initializeFilters() {
        console.log('🔍 Filters initialized');
        // Add filter implementation if needed
    }

    /**
     * Initialize form handlers
     */
    function initializeForms() {
        console.log('📝 Form handlers initialized');
        // Add additional form handlers if needed
    }

    // ========== DEBUG FUNCTIONS ==========

    /**
     * Test add product functionality
     */
    function testAddProduct() {
        console.log('🧪 Testing add product functionality');
        const addModal = new bootstrap.Modal(document.getElementById('addProductModal'));
        addModal.show();
    }

    // Expose main functions for global access
    window.loadProductData = loadProductDataEnhanced;
    window.populateFromTableData = populateFromTableData;
    window.populateFromAPIData = populateFromAPIDataEnhanced;
    window.findProductRowById = findProductRowById;
    window.testGetProductData = testGetProductData;
    window.clearAddForm = clearAddForm;
    window.validateAddForm = validateAddProductForm;

    console.log('🎯 Complete Enhanced Product Management JavaScript fully loaded');
    console.log('🧪 Debug functions available: window.productDebug');
});