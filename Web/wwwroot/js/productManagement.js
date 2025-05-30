/**
 * Complete Product Management JavaScript
 * Auto-populates product details form with guaranteed API calls
 */

document.addEventListener('DOMContentLoaded', function () {
    console.log('Product Management JavaScript initialized');

    // Initialize DataTable
    initializeDataTable();

    // Initialize modal handlers
    initializeModals();

    // Initialize filters
    initializeFilters();

    // Initialize form handlers
    initializeForms();

    // Debug: Make functions available globally
    window.productDebug = {
        loadProductData,
        testGetProductData,
        findProductRowById,
        populateFromTableData,
        populateFromAPIData
    };

    // Test API configuration
    if (window.appUrls && window.appUrls.getProductData) {
        console.log('? GetProductData API URL configured:', window.appUrls.getProductData);
    } else {
        console.error('? GetProductData API URL not configured!');
        showToast('Error', 'API configuration missing. Product editing may not work properly.', 'danger');
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
     * Handle edit modal show event
     */
    function handleEditModalShow(event) {
        const button = event.relatedTarget;
        const productId = button.getAttribute('data-product-id');

        console.log('?? Opening edit modal for product ID:', productId);

        if (!productId) {
            console.error('? No product ID found');
            showToast('Error', 'Product ID is missing', 'danger');
            return;
        }

        // Set product ID in all forms
        setProductIdInForms(productId);

        // Clear previous data
        clearAllForms();

        // Load product data
        loadProductData(productId);
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
     * Main function to load product data
     */
    function loadProductData(productId) {
        console.log('?? Loading product data for ID:', productId);

        if (!productId) {
            console.error('? No product ID provided');
            return;
        }

        showLoadingState();

        // Step 1: Populate from table data immediately for instant feedback
        const row = findProductRowById(productId);
        if (row) {
            console.log('? Found table row, populating basic data');
            populateFromTableData(row);
        } else {
            console.warn('?? No table row found for product ID:', productId);
        }

        // Step 2: Always call API for complete data
        if (window.appUrls?.getProductData) {
            callGetProductDataAPI(productId);
        } else {
            hideLoadingState();
            console.error('? GetProductData API URL not configured');
            showToast('Error', 'API not configured. Only table data available.', 'danger');
        }
    }

    /**
     * Call the GetProductData API
     */
    function callGetProductDataAPI(productId) {
        const url = `${window.appUrls.getProductData}?id=${productId}`;
        console.log('?? Calling GetProductData API:', url);

        fetch(url, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Cache-Control': 'no-cache'
            }
        })
            .then(response => {
                console.log('?? API Response status:', response.status);
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                return response.json();
            })
            .then(data => {
                console.log('?? GetProductData API response:', data);
                hideLoadingState();

                if (data.success) {
                    console.log('? API call successful, populating form');
                    populateFromAPIData(data);
                    showToast('Success', 'Product data loaded successfully', 'success');
                } else {
                    console.error('? API returned error:', data.message);
                    showToast('Warning', data.message || 'Failed to load complete product data', 'warning');
                }
            })
            .catch(error => {
                console.error('?? Error calling GetProductData API:', error);
                hideLoadingState();
                showToast('Warning', `API unavailable: ${error.message}. Using table data only.`, 'warning');
            });
    }

    /**
     * Populate form from table data (immediate feedback)
     */
    function populateFromTableData(row) {
        console.log('?? Populating from table data');

        try {
            // Extract data from attributes
            const data = {
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

            console.log('?? Table data extracted:', data);

            // Populate form fields
            if (data.title) safeSetValue('editTitle', data.title);
            if (data.productNumber) safeSetValue('editProductNumber', data.productNumber);
            if (data.description) safeSetValue('editDescription', data.description);
            if (data.tag) safeSetValue('editTag', data.tag);
            if (data.imageUrl) {
                safeSetValue('editImageURL', data.imageUrl);
                updateImagePreview(data.imageUrl);
            }

            // Handle numeric fields
            if (data.price) {
                const price = parseFloat(data.price);
                if (!isNaN(price)) {
                    safeSetValue('editPrice', price.toFixed(2));
                }
            }

            if (data.points) {
                const points = parseInt(data.points, 10);
                if (!isNaN(points)) {
                    safeSetValue('editPoints', points.toString());
                }
            }

            // Handle select fields
            if (data.status) safeSetSelect('editStatus', capitalizeFirst(data.status));
            if (data.category) safeSetSelect('editCategory', capitalizeFirst(data.category));
            if (data.type) safeSetSelect('editType', capitalizeFirst(data.type));

            console.log('? Table data populated successfully');
        } catch (error) {
            console.error('? Error populating from table data:', error);
        }
    }

    /**
     * Populate form from API data (complete data)
     */
    function populateFromAPIData(data) {
        console.log('?? Populating from API data');

        // Basic information
        if (data.Title) safeSetValue('editTitle', data.Title);
        if (data.ProductNumber) safeSetValue('editProductNumber', data.ProductNumber);
        if (data.Description) safeSetValue('editDescription', data.Description);
        if (data.Tag) safeSetValue('editTag', data.Tag);

        // Image
        if (data.ImageURL) {
            safeSetValue('editImageURL', data.ImageURL);
            updateImagePreview(data.ImageURL);
        }

        // Numeric fields with validation
        if (data.Price !== null && data.Price !== undefined) {
            const price = parseFloat(data.Price);
            safeSetValue('editPrice', isNaN(price) ? '0.00' : price.toFixed(2));
        }

        if (data.Points !== null && data.Points !== undefined) {
            const points = parseInt(data.Points, 10);
            safeSetValue('editPoints', isNaN(points) ? '0' : points.toString());
        }

        // Select fields
        if (data.Type) safeSetSelect('editType', data.Type);
        if (data.Category) safeSetSelect('editCategory', data.Category);
        if (data.Status) safeSetSelect('editStatus', data.Status);

        console.log('? API data populated successfully');

        // Also update the Product Info tab
        updateProductInfoDisplay(data);
    }

    /**
     * Load product info for the Product Info tab
     */
    function loadProductInfo(productId) {
        console.log('?? Loading product info for:', productId);

        // Check if already loaded
        const currentTitle = document.getElementById('productInfoTitle')?.textContent;
        if (currentTitle && currentTitle !== '--' && currentTitle !== 'Product') {
            console.log('?? Product info already loaded');
            return;
        }

        // Call API if available
        if (window.appUrls?.getProductData) {
            callGetProductDataAPI(productId);
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
        const initials = getProductInitials(data.Title);
        safeUpdateElement('productInfoInitials', initials);
        safeUpdateElement('productInfoTitle', data.Title || 'Product');
        safeUpdateElement('productInfoNumber', data.ProductNumber || 'No SKU');

        // Badges
        safeUpdateElement('productInfoStatus', data.Status || 'Active');
        safeUpdateElement('productInfoCategory', data.Category || 'No Category');

        // Detailed information
        safeUpdateElement('productInfoTitleDetail', data.Title || '--');
        safeUpdateElement('productInfoPrice', formatPrice(data.Price));
        safeUpdateElement('productInfoPoints', data.Points?.toString() || '0');
        safeUpdateElement('productInfoType', data.Type || '--');
        safeUpdateElement('productInfoTag', data.Tag || '--');

        // Mock statistics
        safeUpdateElement('productInfoSales', Math.floor(Math.random() * 100));
        safeUpdateElement('productInfoViews', Math.floor(Math.random() * 1000));
        safeUpdateElement('productInfoRating', (Math.random() * 5).toFixed(1));

        console.log('? Product info display updated');
    }

    /**
     * Find product row by ID
     */
    function findProductRowById(productId) {
        if (!productId) return null;

        // Try data attribute first
        let row = document.querySelector(`tr[data-product-id="${productId}"]`);
        if (row) {
            console.log('? Found row by data-product-id');
            return row;
        }

        // Try finding by button
        const button = document.querySelector(`button[data-product-id="${productId}"]`);
        if (button) {
            row = button.closest('tr');
            if (row) {
                console.log('? Found row by button');
                return row;
            }
        }

        console.warn('?? Row not found for product ID:', productId);
        return null;
    }

    /**
     * Update image preview
     */
    function updateImagePreview(imageUrl) {
        const currentImage = document.getElementById('currentImage');
        const placeholder = document.getElementById('currentImagePlaceholder');

        if (currentImage && placeholder) {
            if (imageUrl?.trim()) {
                currentImage.src = addCacheBuster(imageUrl);
                currentImage.style.display = 'block';
                placeholder.style.display = 'none';
                console.log('??? Image preview updated:', imageUrl);
            } else {
                currentImage.style.display = 'none';
                placeholder.style.display = 'flex';
                console.log('??? Image preview cleared');
            }
        }
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

        console.log('?? Loading state shown');
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
        // Add filter logic here if needed
        console.log('?? Filters initialized');
    }

    /**
     * Initialize form handlers
     */
    function initializeForms() {
        // Add form validation and handlers here if needed
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
        callGetProductDataAPI(productId);
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

    console.log('? Product Management JavaScript fully loaded');
});