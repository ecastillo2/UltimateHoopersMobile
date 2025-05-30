/**
 * Enhanced Product Management JavaScript
 * Auto-populates product details form with improved error handling and debugging
 */

document.addEventListener('DOMContentLoaded', function () {
    console.log('? Enhanced Product Management JavaScript initialized');

    // Initialize components
    initializeDataTable();
    initializeModals();
    initializeFilters();
    initializeForms();

    // Enhanced debugging
    window.productDebug = {
        loadProductData,
        testGetProductData,
        findProductRowById,
        populateFromTableData,
        populateFromAPIData,
        checkAPIConfiguration,
        testTableDataExtraction
    };

    // Verify API configuration
    checkAPIConfiguration();

    /**
     * Enhanced modal show handler with better debugging
     */
    function handleEditModalShow(event) {
        const button = event.relatedTarget;
        const productId = button.getAttribute('data-product-id');

        console.log('?? Opening edit modal for product ID:', productId);
        console.log('?? Button element:', button);

        if (!productId) {
            console.error('? No product ID found on button');
            console.log('Available data attributes:', Array.from(button.attributes).map(attr => `${attr.name}="${attr.value}"`));
            showToast('Error', 'Product ID is missing', 'danger');
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
        console.log('?? Loading product data for ID:', productId);

        if (!productId) {
            console.error('? No product ID provided');
            hideLoadingState();
            return;
        }

        // Step 1: Try to populate from table data immediately
        const row = findProductRowById(productId);
        if (row) {
            console.log('? Found table row, extracting data...');
            const tableData = extractTableData(row);
            console.log('?? Extracted table data:', tableData);
            populateFromTableData(tableData);
            console.log('? Table data populated successfully');
        } else {
            console.warn('?? No table row found for product ID:', productId);
            // Try alternative approach - search all rows
            tryFindRowAlternative(productId);
        }

        // Step 2: Always call API for complete data if available
        if (window.appUrls?.getProductData) {
            callGetProductDataAPIEnhanced(productId);
        } else {
            console.error('? GetProductData API URL not configured');
            hideLoadingState();
            showToast('Warning', 'API not configured. Only table data available.', 'warning');
        }
    }

    /**
     * Enhanced API call with better error handling
     */
    function callGetProductDataAPIEnhanced(productId) {
        const url = `${window.appUrls.getProductData}?id=${encodeURIComponent(productId)}`;
        console.log('?? Calling API:', url);

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
                console.log('?? API Response status:', response.status);
                console.log('?? API Response headers:', Object.fromEntries(response.headers.entries()));

                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                return response.json();
            })
            .then(data => {
                console.log('?? API Response data:', data);
                hideLoadingState();

                if (data && data.success !== false) {
                    console.log('? API call successful, populating form');
                    populateFromAPIDataEnhanced(data);
                    showToast('Success', 'Product data loaded successfully', 'success');
                } else {
                    console.error('? API returned error:', data?.message || 'Unknown error');
                    showToast('Warning', data?.message || 'Failed to load complete product data', 'warning');
                }
            })
            .catch(error => {
                console.error('?? API Error:', error);
                hideLoadingState();
                showToast('Warning', `API Error: ${error.message}. Using table data only.`, 'warning');
            });
    }

    /**
     * Enhanced table data extraction
     */
    function extractTableData(row) {
        if (!row) return {};

        console.log('?? Extracting data from row:', row);

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

        console.log('?? Data from attributes:', dataFromAttributes);

        // Method 2: Try extracting from cell content as fallback
        const cells = row.querySelectorAll('td');
        console.log('?? Found cells:', cells.length);

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

            console.log('?? Data from cells:', dataFromCells);

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
        console.log('?? Populating form from table data:', data);

        try {
            // Basic text fields
            safeSetValue('editTitle', data.title);
            safeSetValue('editProductNumber', data.productNumber);
            safeSetValue('editDescription', data.description);
            safeSetValue('editTag', data.tag);

            // Image handling
            if (data.imageUrl) {
                safeSetValue('editImageURL', data.imageUrl);
                updateImagePreview(data.imageUrl);
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

            console.log('? Table data populated successfully');
        } catch (error) {
            console.error('?? Error populating from table data:', error);
        }
    }

    /**
     * Enhanced API data population
     */
    function populateFromAPIDataEnhanced(data) {
        console.log('?? Populating form from API data:', data);

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
                updateImagePreview(imageUrl);
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

            console.log('? API data populated successfully');

            // Update Product Info tab
            updateProductInfoDisplay(productData);
        } catch (error) {
            console.error('?? Error populating from API data:', error);
        }
    }

    /**
     * Try alternative methods to find the row
     */
    function tryFindRowAlternative(productId) {
        console.log('?? Trying alternative row finding methods for ID:', productId);

        // Method 1: Search all buttons with data-product-id
        const allButtons = document.querySelectorAll('button[data-product-id]');
        console.log('?? Found buttons with data-product-id:', allButtons.length);

        for (const button of allButtons) {
            if (button.getAttribute('data-product-id') === productId) {
                const row = button.closest('tr');
                if (row) {
                    console.log('? Found row via button search');
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
                    console.log('? Found row via DataTable search');
                    const tableData = extractTableData(row);
                    populateFromTableData(tableData);
                    return;
                }
            }
        }

        console.warn('?? Could not find row using any method');
    }

    /**
     * Enhanced image preview with error handling
     */
    function updateImagePreview(imageUrl) {
        const currentImage = document.getElementById('currentImage');
        const placeholder = document.getElementById('currentImagePlaceholder');

        if (!currentImage || !placeholder) {
            console.warn('?? Image preview elements not found');
            return;
        }

        if (imageUrl?.trim()) {
            const img = new Image();
            img.onload = function () {
                currentImage.src = addCacheBuster(imageUrl);
                currentImage.style.display = 'block';
                placeholder.style.display = 'none';
                console.log('??? Image preview updated successfully');
            };
            img.onerror = function () {
                console.warn('?? Failed to load image, showing placeholder');
                currentImage.style.display = 'none';
                placeholder.style.display = 'flex';
            };
            img.src = imageUrl;
        } else {
            currentImage.style.display = 'none';
            placeholder.style.display = 'flex';
            console.log('??? Image preview cleared');
        }
    }

    /**
     * Check API configuration
     */
    function checkAPIConfiguration() {
        console.log('?? Checking API configuration...');

        if (!window.appUrls) {
            console.error('? window.appUrls is not defined');
            return false;
        }

        console.log('?? Available API URLs:', window.appUrls);

        if (!window.appUrls.getProductData) {
            console.error('? getProductData URL not configured');
            return false;
        }

        console.log('? API configuration OK');
        return true;
    }

    /**
     * Test table data extraction
     */
    function testTableDataExtraction(productId) {
        console.log('?? Testing table data extraction for:', productId);
        const row = findProductRowById(productId);
        if (row) {
            console.log('? Found row:', row);
            const data = extractTableData(row);
            console.log('?? Extracted data:', data);
            return data;
        } else {
            console.log('? Row not found');
            return null;
        }
    }

    /**
     * Enhanced row finding with multiple strategies
     */
    function findProductRowById(productId) {
        if (!productId) return null;

        console.log('?? Looking for row with product ID:', productId);

        // Strategy 1: Direct row attribute search
        let row = document.querySelector(`tr[data-product-id="${productId}"]`);
        if (row) {
            console.log('? Found row by data-product-id (Strategy 1)');
            return row;
        }

        // Strategy 2: Button-based search
        const button = document.querySelector(`button[data-product-id="${productId}"]`);
        if (button) {
            row = button.closest('tr');
            if (row) {
                console.log('? Found row via button (Strategy 2)');
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
                    console.log('? Found row via table body search (Strategy 3)');
                    return tr;
                }
            }
        }

        console.warn('?? Row not found for product ID:', productId);
        return null;
    }

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
            console.log('? DataTable initialized');
        }
    }

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

            console.log('? Modal event handlers initialized');
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
        console.log('?? Closing edit modal');
        clearAllForms();
        hideLoadingState();
    }

    /**
     * Handle tab switching
     */
    function handleTabSwitch(event) {
        const targetTab = event.target.getAttribute('data-bs-target');
        const productId = document.getElementById('editProductId')?.value;

        console.log('?? Switching to tab:', targetTab, 'for product:', productId);

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
     * Set product ID in all relevant form fields
     */
    function setProductIdInForms(productId) {
        const idFields = ['editProductId', 'deleteProductId'];
        idFields.forEach(fieldId => {
            const field = document.getElementById(fieldId);
            if (field) {
                field.value = productId;
                console.log(`? Set ${fieldId} to:`, productId);
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
     * Load product info for the Product Info tab
     */
    function loadProductInfo(productId) {
        console.log('?? Loading product info for:', productId);

        // Check if already loaded
        const currentTitle = document.getElementById('productInfoTitle')?.textContent;
        if (currentTitle && currentTitle !== '--' && currentTitle !== 'Product') {
            console.log('? Product info already loaded');
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
        console.log('?? Loading inventory data for:', productId);

        // For now, populate with mock data
        safeSetValue('stockQuantity', Math.floor(Math.random() * 100));
        safeSetValue('lowStockThreshold', 5);
        safeSetValue('reorderLevel', 10);
    }

    /**
     * Update product info display
     */
    function updateProductInfoDisplay(data) {
        console.log('?? Updating product info display');

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

        console.log('? Product info display updated');
    }

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

        console.log('? Loading state shown');
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

        console.log('? Loading state hidden');
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

        updateImagePreview('');
        console.log('?? Product details form cleared');
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
        console.log('?? Product info display cleared');
    }

    /**
     * Clear inventory form
     */
    function clearInventoryForm() {
        const fields = ['stockQuantity', 'lowStockThreshold', 'reorderLevel'];
        fields.forEach(field => safeSetValue(field, ''));
        console.log('?? Inventory form cleared');
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
     * Initialize filters
     */
    function initializeFilters() {
        console.log('?? Filters initialized');
    }

    /**
     * Initialize form handlers
     */
    function initializeForms() {
        console.log('?? Form handlers initialized');
    }

    /**
     * Test function for GetProductData API
     */
    function testGetProductData(productId) {
        if (!productId) {
            console.log('Usage: testGetProductData("your-product-id")');
            return;
        }

        console.log('?? Testing GetProductData with ID:', productId);
        callGetProductDataAPIEnhanced(productId);
    }

    // Helper functions
    function safeSetValue(elementId, value) {
        const element = document.getElementById(elementId);
        if (element) {
            element.value = value || '';
        } else {
            console.warn(`?? Element ${elementId} not found`);
        }
    }

    function safeSetSelect(elementId, value) {
        const select = document.getElementById(elementId);
        if (select && value) {
            for (let i = 0; i < select.options.length; i++) {
                if (select.options[i].value.toLowerCase() === value.toLowerCase()) {
                    select.selectedIndex = i;
                    console.log(`? Set ${elementId} to: ${value}`);
                    break;
                }
            }
        } else if (!select) {
            console.warn(`?? Select element ${elementId} not found`);
        }
    }

    function safeUpdateElement(elementId, value) {
        const element = document.getElementById(elementId);
        if (element) {
            element.textContent = value || '--';
        } else {
            console.warn(`?? Element ${elementId} not found`);
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

    function addCacheBuster(url) {
        if (!url) return url;
        const separator = url.includes('?') ? '&' : '?';
        return `${url}${separator}v=${Date.now()}`;
    }

    function showToast(title, message, type = 'success') {
        console.log(`${type.toUpperCase()}: ${title} - ${message}`);

        // Create toast container if it doesn't exist
        let container = document.querySelector('.toast-container');
        if (!container) {
            container = document.createElement('div');
            container.className = 'toast-container position-fixed bottom-0 end-0 p-3';
            container.style.zIndex = '9999';
            document.body.appendChild(container);
        }

        // Create toast
        const toastId = `toast-${Date.now()}`;
        const iconClass = {
            'success': 'bi-check-circle',
            'danger': 'bi-exclamation-triangle',
            'warning': 'bi-exclamation-triangle',
            'info': 'bi-info-circle'
        }[type] || 'bi-info-circle';

        const toastHtml = `
            <div id="${toastId}" class="toast align-items-center text-white bg-${type} border-0" role="alert">
                <div class="d-flex">
                    <div class="toast-body">
                        <i class="bi ${iconClass} me-2"></i>
                        <strong>${title}:</strong> ${message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                </div>
            </div>
        `;

        container.insertAdjacentHTML('beforeend', toastHtml);

        // Initialize and show toast
        const toastElement = document.getElementById(toastId);
        if (toastElement && typeof bootstrap !== 'undefined' && bootstrap.Toast) {
            const toast = new bootstrap.Toast(toastElement, {
                autohide: type !== 'danger',
                delay: 5000
            });
            toast.show();

            toastElement.addEventListener('hidden.bs.toast', () => {
                toastElement.remove();
            });
        }
    }

    // Expose functions for global access
    window.loadProductData = loadProductDataEnhanced;
    window.populateFromTableData = populateFromTableData;
    window.populateFromAPIData = populateFromAPIDataEnhanced;
    window.findProductRowById = findProductRowById;
    window.testGetProductData = testGetProductData;

    console.log('? Enhanced Product Management JavaScript fully loaded');
});