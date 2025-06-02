/**
 * Complete Enhanced Product Management JavaScript with Full AJAX Support
 * Includes both Edit and Add Product functionality with file upload support
 * All AJAX operations, form submissions, and UI interactions
 */

document.addEventListener('DOMContentLoaded', function () {
    console.log('🚀 Initializing Complete Enhanced Product Management with AJAX');

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
        validateAddForm,
        submitEditForm,
        submitAddForm
    };

    // Verify API configuration
    checkAPIConfiguration();

    console.log('✅ Complete Enhanced Product Management with AJAX loaded successfully');

    // ========== DATATABLE INITIALIZATION ==========
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

    function handleEditModalShow(event) {
        const button = event.relatedTarget;
        const productId = button.getAttribute('data-product-id');

        console.log('📂 Opening edit modal for product ID:', productId);

        if (!productId) {
            console.error('🚨 No product ID found on button');
            UIUtils.showError('Product ID is missing', 'Error');
            return;
        }

        // Set product IDs in forms
        setProductIdInForms(productId);

        // Clear previous data
        clearAllForms();

        // Show loading state immediately
        showLoadingState();

        // Load product data
        loadProductDataEnhanced(productId);
    }

    function handleEditModalHide() {
        console.log('🚪 Closing edit modal');
        clearAllForms();
        hideLoadingState();
    }

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

    // ========== PRODUCT DATA LOADING ==========
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
            populateFromTableData(tableData);
        }

        // Step 2: Always call API for complete data if available
        if (window.appUrls?.getProductData) {
            callGetProductDataAPIEnhanced(productId);
        } else {
            console.error('🚨 GetProductData API URL not configured');
            hideLoadingState();
            UIUtils.showWarning('API not configured. Only table data available.', 'Warning');
        }
    }

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
                    UIUtils.showSuccess('Product data loaded successfully');
                } else {
                    console.error('🚨 API returned error:', data?.message || 'Unknown error');
                    UIUtils.showWarning(data?.message || 'Failed to load complete product data');
                }
            })
            .catch(error => {
                console.error('💥 API Error:', error);
                hideLoadingState();
                UIUtils.showWarning(`API Error: ${error.message}. Using table data only.`);
            });
    }

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

        // Method 2: Try extracting from cell content as fallback
        const cells = row.querySelectorAll('td');

        if (cells.length >= 5) {
            // Extract from the product column (first column)
            const productCell = cells[0];
            const titleElement = productCell.querySelector('.product-title, .fw-semibold');
            const numberElement = productCell.querySelector('.product-number, .text-muted.small');

            // Extract from other columns
            const priceText = cells[1]?.textContent.replace('$', '').trim();
            const pointsText = cells[2]?.textContent.trim();
            const statusElement = cells[3]?.querySelector('.badge, .product-status');
            const categoryText = cells[4]?.textContent.trim();

            const dataFromCells = {
                title: titleElement?.textContent?.trim() || dataFromAttributes.title,
                productNumber: numberElement?.textContent?.trim() || dataFromAttributes.productNumber,
                price: priceText || dataFromAttributes.price,
                points: pointsText || dataFromAttributes.points,
                status: statusElement?.textContent?.trim() || dataFromAttributes.status,
                category: categoryText || dataFromAttributes.category
            };

            // Merge data, preferring attributes but falling back to cell content
            return {
                ...dataFromCells,
                ...Object.fromEntries(Object.entries(dataFromAttributes).filter(([_, v]) => v != null))
            };
        }

        return dataFromAttributes;
    }

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

    function findProductRowById(productId) {
        if (!productId) return null;

        console.log('🔍 Looking for row with product ID:', productId);

        // Strategy 1: Direct row attribute search
        let row = document.querySelector(`tr[data-product-id="${productId}"]`);
        if (row) {
            console.log('✅ Found row by data-product-id');
            return row;
        }

        // Strategy 2: Button-based search
        const button = document.querySelector(`button[data-product-id="${productId}"]`);
        if (button) {
            row = button.closest('tr');
            if (row) {
                console.log('✅ Found row via button');
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
                    console.log('✅ Found row via table body search');
                    return tr;
                }
            }
        }

        console.warn('⚠️ Row not found for product ID:', productId);
        return null;
    }

    // ========== FORM SUBMISSION HANDLERS ==========
    function initializeForms() {
        console.log('📝 Initializing form handlers...');

        // Edit Product Form
        const editProductForm = document.getElementById('editProductForm');
        if (editProductForm) {
            editProductForm.addEventListener('submit', handleEditFormSubmit);
            console.log('✅ Edit form handler attached');
        }

        // Add Product Form
        const addProductForm = document.getElementById('addProductForm');
        if (addProductForm) {
            addProductForm.addEventListener('submit', handleAddFormSubmit);
            console.log('✅ Add form handler attached');
        }
    }

    function handleEditFormSubmit(e) {
        e.preventDefault();
        console.log('📤 Edit form submitted');

        if (!validateEditProductForm()) {
            return false;
        }

        submitEditForm(e.target);
    }

    function handleAddFormSubmit(e) {
        e.preventDefault();
        console.log('📤 Add form submitted');

        if (!validateAddProductForm()) {
            return false;
        }

        submitAddForm(e.target);
    }

    function submitEditForm(form) {
        console.log('🚀 Submitting edit product form via AJAX');

        const formData = new FormData(form);
        const submitBtn = form.querySelector('button[type="submit"]');

        // Set button loading state
        UIUtils.setButtonLoading(submitBtn, true, 'Saving...');

        // Get anti-forgery token
        const token = getAntiForgeryToken();

        fetch('/Product/Edit', {
            method: 'POST',
            headers: {
                'RequestVerificationToken': token
            },
            body: formData
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                return response.json();
            })
            .then(result => {
                UIUtils.setButtonLoading(submitBtn, false);

                if (result.success) {
                    UIUtils.showSuccess(result.message || 'Product updated successfully!');

                    // Update the table row if visible
                    updateTableRow(result.product || getFormData(form));

                    // Close modal after short delay
                    setTimeout(() => {
                        const modal = bootstrap.Modal.getInstance(document.getElementById('editProductModal'));
                        if (modal) modal.hide();
                    }, 1000);
                } else {
                    UIUtils.showError(result.message || 'Failed to update product');
                }
            })
            .catch(error => {
                console.error('🚨 Error updating product:', error);
                UIUtils.setButtonLoading(submitBtn, false);
                UIUtils.showError(`Error updating product: ${error.message}`);
            });
    }

    function submitAddForm(form) {
        console.log('🚀 Submitting add product form via AJAX');

        const formData = new FormData(form);
        const submitBtn = form.querySelector('button[type="submit"]');

        // Set button loading state
        UIUtils.setButtonLoading(submitBtn, true, 'Adding Product...');

        // Get anti-forgery token
        const token = getAntiForgeryToken();

        fetch('/Product/Create', {
            method: 'POST',
            headers: {
                'RequestVerificationToken': token
            },
            body: formData
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                return response.json();
            })
            .then(result => {
                UIUtils.setButtonLoading(submitBtn, false);

                if (result.success) {
                    UIUtils.showSuccess(result.message || 'Product created successfully!');

                    // Clear form
                    clearAddForm();

                    // Close modal after short delay
                    setTimeout(() => {
                        const modal = bootstrap.Modal.getInstance(document.getElementById('addProductModal'));
                        if (modal) modal.hide();

                        // Refresh page to show new product
                        location.reload();
                    }, 1500);
                } else {
                    UIUtils.showError(result.message || 'Failed to create product');
                }
            })
            .catch(error => {
                console.error('🚨 Error creating product:', error);
                UIUtils.setButtonLoading(submitBtn, false);
                UIUtils.showError(`Error creating product: ${error.message}`);
            });
    }

    // ========== ADD PRODUCT MODAL FUNCTIONALITY ==========
    function initializeAddProductModal() {
        console.log('🎯 Initializing Add Product Modal');

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
                    if (fileInput) fileInput.value = '';
                }
            });

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

        console.log('✅ Add product modal initialized');
    }

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
                    UIUtils.showError('Please drop an image file');
                }
            }
        });
    }

    function handleFileSelection(file) {
        console.log('📁 File selected:', file.name);

        if (!validateImageFile(file)) {
            return;
        }

        const reader = new FileReader();
        reader.onload = function (e) {
            updateImagePreview(e.target.result, 'add');
            UIUtils.showSuccess(`Image "${file.name}" loaded successfully`);
        };
        reader.onerror = function () {
            UIUtils.showError('Error reading file. Please try again.');
        };
        reader.readAsDataURL(file);
    }

    function handleUrlInput(url) {
        console.log('🔗 URL entered:', url);

        if (!isValidImageUrl(url)) {
            UIUtils.showWarning('Please enter a valid image URL');
            return;
        }

        setPlaceholderLoading('add');

        const testImg = new Image();
        testImg.onload = function () {
            updateImagePreview(url, 'add');
            UIUtils.showSuccess('Image loaded from URL successfully');
        };
        testImg.onerror = function () {
            resetPlaceholder('add');
            UIUtils.showError('Failed to load image from URL. Please check the URL and try again.');
        };
        testImg.src = url;
    }

    // ========== VALIDATION FUNCTIONS ==========
    function validateEditProductForm() {
        const errors = [];

        const title = document.getElementById('editTitle')?.value?.trim();
        const price = document.getElementById('editPrice')?.value;
        const type = document.getElementById('editType')?.value;
        const category = document.getElementById('editCategory')?.value;

        if (!title) {
            errors.push('Product title is required');
            markFieldInvalid('editTitle');
        } else {
            markFieldValid('editTitle');
        }

        if (!price || isNaN(parseFloat(price)) || parseFloat(price) < 0) {
            errors.push('Valid price is required');
            markFieldInvalid('editPrice');
        } else {
            markFieldValid('editPrice');
        }

        if (!type) {
            errors.push('Product type is required');
            markFieldInvalid('editType');
        } else {
            markFieldValid('editType');
        }

        if (!category) {
            errors.push('Product category is required');
            markFieldInvalid('editCategory');
        } else {
            markFieldValid('editCategory');
        }

        if (errors.length > 0) {
            UIUtils.showError('Please fix the following errors:\n• ' + errors.join('\n• '));
            return false;
        }

        return true;
    }

    function validateAddProductForm() {
        const errors = [];

        const title = document.getElementById('addTitle')?.value?.trim();
        const price = document.getElementById('addPrice')?.value;
        const type = document.getElementById('addType')?.value;
        const category = document.getElementById('addCategory')?.value;

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
            UIUtils.showError('Please fix the following errors:\n• ' + errors.join('\n• '));
            return false;
        }

        return true;
    }

    function validateImageFile(file) {
        const maxSize = 5 * 1024 * 1024; // 5MB
        const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp', 'image/bmp'];

        if (file.size > maxSize) {
            UIUtils.showError(`File size (${formatFileSize(file.size)}) must be less than 5MB`);
            return false;
        }

        if (!allowedTypes.includes(file.type.toLowerCase())) {
            UIUtils.showError('File type not supported. Please use JPG, PNG, GIF, WebP, or BMP.');
            return false;
        }

        return true;
    }

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

    // ========== IMAGE PREVIEW FUNCTIONS ==========
    function updateImagePreview(src, modalType = 'edit') {
        const prefix = modalType === 'add' ? 'add' : 'current';
        const previewImg = document.getElementById(`${prefix === 'add' ? 'addPreview' : 'current'}Image`);
        const placeholder = document.getElementById(`${prefix === 'add' ? 'add' : 'current'}ImagePlaceholder`);

        if (previewImg && placeholder) {
            previewImg.src = src;
            previewImg.style.display = 'block';
            placeholder.style.display = 'none';

            const container = document.getElementById(`${prefix === 'add' ? 'add' : 'current'}ImagePreview`);
            if (container) {
                container.classList.add('has-image');
            }

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

    function clearImagePreview(modalType = 'edit') {
        console.log(`🧹 Clearing ${modalType} image preview`);

        const prefix = modalType === 'add' ? 'add' : 'edit';
        const fileInput = document.getElementById(`${prefix}ImageFile`);
        const urlInput = document.getElementById(`${prefix}ImageURL`);

        if (fileInput) fileInput.value = '';
        if (urlInput) {
            urlInput.value = '';
            urlInput.classList.remove('is-invalid');
        }

        resetPlaceholder(modalType);
        UIUtils.showInfo('Image cleared');
    }

    function resetPlaceholder(modalType = 'edit') {
        const prefix = modalType === 'add' ? 'add' : 'current';
        const previewImg = document.getElementById(`${prefix === 'add' ? 'addPreview' : 'current'}Image`);
        const placeholder = document.getElementById(`${prefix === 'add' ? 'add' : 'current'}ImagePlaceholder`);

        if (previewImg && placeholder) {
            previewImg.src = '';
            previewImg.style.display = 'none';
            placeholder.style.display = 'flex';

            const placeholderText = modalType === 'add' ? 'Image Preview' : 'Current Image';
            const placeholderSubtext = modalType === 'add' ? 'No image selected' : 'No image uploaded';

            placeholder.innerHTML = `
                <i class="bi bi-image text-muted" style="font-size: 3rem;"></i>
                <p class="text-muted mt-2 mb-0">${placeholderText}</p>
                <small class="text-muted">${placeholderSubtext}</small>
            `;
        }

        const container = document.getElementById(`${prefix === 'add' ? 'add' : 'current'}ImagePreview`);
        if (container) {
            container.classList.remove('has-image');
        }
    }

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

    // ========== TAB LOADING FUNCTIONS ==========
    function loadProductInfo(productId) {
        console.log('📊 Loading product info for:', productId);

        const currentTitle = document.getElementById('productInfoTitle')?.textContent;
        if (currentTitle && currentTitle !== '--' && currentTitle !== 'Product') {
            console.log('✅ Product info already loaded');
            return;
        }

        if (window.appUrls?.getProductData) {
            callGetProductDataAPIEnhanced(productId);
        }
    }

    function loadInventoryData(productId) {
        console.log('📦 Loading inventory data for:', productId);

        // For now, populate with mock data
        safeSetValue('stockQuantity', Math.floor(Math.random() * 100));
        safeSetValue('lowStockThreshold', 5);
        safeSetValue('reorderLevel', 10);
    }

    function updateProductInfoDisplay(data) {
        console.log('📊 Updating product info display');

        const initials = getProductInitials(data.Title || data.title);
        safeUpdateElement('productInfoInitials', initials);
        safeUpdateElement('productInfoTitle', data.Title || data.title || 'Product');
        safeUpdateElement('productInfoNumber', data.ProductNumber || data.productNumber || 'No SKU');

        safeUpdateElement('productInfoStatus', data.Status || data.status || 'Active');
        safeUpdateElement('productInfoCategory', data.Category || data.category || 'No Category');

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

    function clearAllForms() {
        clearProductDetailsForm();
        clearProductInfoDisplay();
        clearInventoryForm();
    }

    function clearAddForm() {
        console.log('🧹 Clearing add product form');

        const form = document.getElementById('addProductForm');
        if (form) {
            form.reset();

            const inputs = form.querySelectorAll('.is-invalid, .is-valid');
            inputs.forEach(input => {
                input.classList.remove('is-invalid', 'is-valid');
            });

            clearImagePreview('add');
        }
    }

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

    function clearProductInfoDisplay() {
        const elements = [
            'productInfoInitials', 'productInfoTitle', 'productInfoNumber',
            'productInfoStatus', 'productInfoCategory', 'productInfoTitleDetail',
            'productInfoPrice', 'productInfoPoints', 'productInfoType', 'productInfoTag'
        ];

        elements.forEach(elementId => safeUpdateElement(elementId, '--'));
        console.log('🧹 Product info display cleared');
    }

    function clearInventoryForm() {
        const fields = ['stockQuantity', 'lowStockThreshold', 'reorderLevel'];
        fields.forEach(field => safeSetValue(field, ''));
        console.log('🧹 Inventory form cleared');
    }

    // ========== UI STATE MANAGEMENT ==========
    function showLoadingState() {
        const modal = document.getElementById('editProductModal');
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
                    <div class="text-muted">Loading product data...</div>
                </div>
            `;
            modal.querySelector('.modal-content').appendChild(overlay);
        }

        const formElements = modal.querySelectorAll('input, select, textarea, button[type="submit"]');
        formElements.forEach(el => el.disabled = true);

        console.log('⏳ Loading state shown');
    }

    function hideLoadingState() {
        const modal = document.getElementById('editProductModal');
        if (!modal) return;

        const overlay = modal.querySelector('.modal-loading-overlay');
        if (overlay) {
            overlay.remove();
        }

        const formElements = modal.querySelectorAll('input, select, textarea, button[type="submit"]');
        formElements.forEach(el => el.disabled = false);

        console.log('✅ Loading state hidden');
    }

    function markFieldInvalid(fieldId) {
        const field = document.getElementById(fieldId);
        if (field) {
            field.classList.add('is-invalid');
            field.classList.remove('is-valid');
        }
    }

    function markFieldValid(fieldId) {
        const field = document.getElementById(fieldId);
        if (field) {
            field.classList.remove('is-invalid');
            field.classList.add('is-valid');
        }
    }

    // ========== TABLE UPDATE FUNCTIONS ==========
    function updateTableRow(productData) {
        const row = findProductRowById(productData.ProductId || productData.productId);
        if (row) {
            // Update title
            const titleEl = row.querySelector('.product-title, .fw-semibold');
            if (titleEl) titleEl.textContent = productData.Title || productData.title;

            // Update price
            const priceCell = row.cells[1];
            if (priceCell) {
                priceCell.textContent = formatPrice(productData.Price || productData.price);
            }

            // Update points
            const pointsCell = row.cells[2];
            if (pointsCell) {
                pointsCell.textContent = (productData.Points || productData.points)?.toString() || '0';
            }

            // Update status
            const statusCell = row.cells[3];
            const statusBadge = statusCell?.querySelector('.badge');
            if (statusBadge) {
                statusBadge.textContent = productData.Status || productData.status || 'Active';
            }

            // Update category
            const categoryCell = row.cells[4];
            if (categoryCell) {
                categoryCell.textContent = productData.Category || productData.category || 'N/A';
            }

            console.log('✅ Table row updated');
        }
    }

    function getFormData(form) {
        const formData = new FormData(form);
        const data = {};
        for (const [key, value] of formData.entries()) {
            data[key] = value;
        }
        return data;
    }

    // ========== FILTER INITIALIZATION ==========
    function initializeFilters() {
        console.log('🔍 Filters initialized');
        // Filter implementation can be added here if needed
    }

    // ========== UTILITY FUNCTIONS ==========
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

    function testGetProductData(productId) {
        if (!productId) {
            console.log('Usage: testGetProductData("your-product-id")');
            return;
        }

        console.log('🧪 Testing GetProductData with ID:', productId);
        callGetProductDataAPIEnhanced(productId);
    }

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

    function testAddProduct() {
        console.log('🧪 Testing add product functionality');
        const addModal = new bootstrap.Modal(document.getElementById('addProductModal'));
        addModal.show();
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

    function getAntiForgeryToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
    }

    // Expose main functions for global access
    window.loadProductData = loadProductDataEnhanced;
    window.populateFromTableData = populateFromTableData;
    window.populateFromAPIData = populateFromAPIDataEnhanced;
    window.findProductRowById = findProductRowById;
    window.testGetProductData = testGetProductData;
    window.clearAddForm = clearAddForm;
    window.validateAddForm = validateAddProductForm;
    window.submitEditForm = submitEditForm;
    window.submitAddForm = submitAddForm;

    console.log('🎯 Complete Enhanced Product Management JavaScript with AJAX fully loaded');
    console.log('🧪 Debug functions available: window.productDebug');
});