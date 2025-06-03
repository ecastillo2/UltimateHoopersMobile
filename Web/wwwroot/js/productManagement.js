/**
 * Enhanced Product Management with Comprehensive Debugging
 * This version includes extensive logging and error handling to identify the issue
 */

document.addEventListener('DOMContentLoaded', function () {
    console.log('🚀 Initializing Enhanced Product Management with Debug');

    // Global storage for current product data
    window.currentProductData = null;

    // Initialize components
    initializeDataTable();
    initializeModals();
    initializeForms();
    initializeImageHandlers();

    console.log('✅ Complete Product Management loaded successfully');

    // ========== DATATABLE INITIALIZATION ==========
    function initializeDataTable() {
        const tableElement = document.getElementById('productsTable');
        if (tableElement && tableElement.querySelector('tbody tr')) {
            console.log('📊 Initializing DataTable...');
            const productsTable = $('#productsTable').DataTable({
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
                order: [[0, 'asc']],
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
        console.log('🔍 Initializing product table filters...');

        const productsTable = $('#productsTable');
        if (!productsTable.length || !$.fn.dataTable.isDataTable(productsTable)) {
            console.warn('⚠️ Products table not found or not initialized as DataTable');
            return;
        }

        const table = productsTable.DataTable();

        // Get filter elements
        const statusFilter = $('#statusFilter');
        const categoryFilter = $('#categoryFilter');
        const typeFilter = $('#typeFilter');
        const priceRangeFilter = $('#priceRangeFilter');
        const resetFiltersBtn = $('#resetFilters');
        const activeFiltersContainer = $('#activeFilters');

        if (!statusFilter.length || !categoryFilter.length || !typeFilter.length || !priceRangeFilter.length) {
            console.warn('⚠️ Some filter elements not found');
            return;
        }

        // Apply filter function
        function applyFilters() {
            console.log('🔍 Applying filters...', {
                status: statusFilter.val(),
                category: categoryFilter.val(),
                type: typeFilter.val(),
                priceRange: priceRangeFilter.val()
            });

            // Remove any existing custom filter to prevent stacking
            if ($.fn.dataTable.ext.search.length > 0) {
                // Remove only our custom filters, keep others
                $.fn.dataTable.ext.search = $.fn.dataTable.ext.search.filter(fn =>
                    !fn.name || fn.name !== 'productTableFilter'
                );
            }

            // Create a new custom filter function
            const customFilter = function (settings, data, dataIndex) {
                // Only apply this filter to our productsTable
                if (settings.nTable.id !== 'productsTable') return true;

                const row = $(table.row(dataIndex).node());

                // Skip filtering if all filters are set to 'all'
                if (statusFilter.val() === 'all' &&
                    categoryFilter.val() === 'all' &&
                    typeFilter.val() === 'all' &&
                    priceRangeFilter.val() === 'all') {
                    return true;
                }

                try {
                    // Status filtering
                    if (statusFilter.val() !== 'all') {
                        const statusValue = statusFilter.val().toLowerCase();

                        // Get status from data attributes or cell content
                        const rowStatus = (row.attr('data-status') || '').toLowerCase();
                        const statusCell = row.find('td:nth-child(4)'); // Status column
                        const statusText = statusCell.find('.badge').text().toLowerCase() ||
                            statusCell.text().toLowerCase();

                        const matchesStatus = rowStatus === statusValue || statusText.includes(statusValue);

                        if (!matchesStatus) {
                            return false;
                        }
                    }

                    // Category filtering
                    if (categoryFilter.val() !== 'all') {
                        const categoryValue = categoryFilter.val().toLowerCase();

                        // Get category from data attributes or cell content
                        const rowCategory = (row.attr('data-category') || '').toLowerCase();
                        const categoryCell = row.find('td:nth-child(5)'); // Category column
                        const categoryText = categoryCell.text().toLowerCase();

                        const matchesCategory = rowCategory === categoryValue || categoryText === categoryValue;

                        if (!matchesCategory) {
                            return false;
                        }
                    }

                    // Type filtering
                    if (typeFilter.val() !== 'all') {
                        const typeValue = typeFilter.val().toLowerCase();

                        // Get type from data attributes or product info
                        const rowType = (row.attr('data-type') || '').toLowerCase();
                        const productCell = row.find('td:first-child');
                        const productText = productCell.text().toLowerCase();

                        const matchesType = rowType === typeValue || productText.includes(typeValue);

                        if (!matchesType) {
                            return false;
                        }
                    }

                    // Price range filtering
                    if (priceRangeFilter.val() !== 'all') {
                        const priceRangeValue = priceRangeFilter.val();

                        // Get price from data attributes or cell content
                        let price = parseFloat(row.attr('data-price')) || 0;

                        if (price === 0) {
                            const priceCell = row.find('td:nth-child(2)'); // Price column
                            const priceText = priceCell.text().replace(/[$,]/g, '');
                            price = parseFloat(priceText) || 0;
                        }

                        let matchesPrice = false;

                        switch (priceRangeValue) {
                            case '0-25':
                                matchesPrice = price >= 0 && price <= 25;
                                break;
                            case '25-50':
                                matchesPrice = price > 25 && price <= 50;
                                break;
                            case '50-100':
                                matchesPrice = price > 50 && price <= 100;
                                break;
                            case '100+':
                                matchesPrice = price > 100;
                                break;
                            default:
                                matchesPrice = true;
                        }

                        if (!matchesPrice) {
                            return false;
                        }
                    }

                    // If we got here, the row passes all filters
                    return true;

                } catch (error) {
                    console.error('❌ Error in filter function:', error);
                    return true; // Show row if there's an error
                }
            };

            // Mark the filter function for identification
            customFilter.name = 'productTableFilter';

            // Add the custom filter
            $.fn.dataTable.ext.search.push(customFilter);

            // Redraw the table to apply filters
            table.draw();

            // Update the active filters display
            updateActiveFilters();
        }

        // Update the active filters display
        function updateActiveFilters() {
            if (!activeFiltersContainer.length) return;

            // Clear the current active filters display (except the label)
            activeFiltersContainer.find('.filter-badge, .filter-none').remove();

            // Check if any filters are active
            const hasActiveFilters =
                statusFilter.val() !== 'all' ||
                categoryFilter.val() !== 'all' ||
                typeFilter.val() !== 'all' ||
                priceRangeFilter.val() !== 'all';

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

            if (categoryFilter.val() !== 'all') {
                addFilterBadge('Category', formatFilterValue(categoryFilter.val()), function () {
                    categoryFilter.val('all');
                    applyFilters();
                });
            }

            if (typeFilter.val() !== 'all') {
                addFilterBadge('Type', formatFilterValue(typeFilter.val()), function () {
                    typeFilter.val('all');
                    applyFilters();
                });
            }

            if (priceRangeFilter.val() !== 'all') {
                addFilterBadge('Price', formatFilterValue(priceRangeFilter.val()), function () {
                    priceRangeFilter.val('all');
                    applyFilters();
                });
            }
        }

        // Helper function to format filter values for display
        function formatFilterValue(value) {
            // Handle special cases
            if (value === '100+') return '$100+';
            if (value.includes('-')) {
                const parts = value.split('-');
                if (parts.length === 2 && !isNaN(parts[0]) && !isNaN(parts[1])) {
                    return `$${parts[0]} - $${parts[1]}`;
                }
            }

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

        categoryFilter.on('change', function () {
            console.log('Category filter changed to:', this.value);
            applyFilters();
        });

        typeFilter.on('change', function () {
            console.log('Type filter changed to:', this.value);
            applyFilters();
        });

        priceRangeFilter.on('change', function () {
            console.log('Price range filter changed to:', this.value);
            applyFilters();
        });

        // Reset filters button
        if (resetFiltersBtn.length) {
            resetFiltersBtn.on('click', function () {
                console.log('🔄 Resetting all filters');
                statusFilter.val('all');
                categoryFilter.val('all');
                typeFilter.val('all');
                priceRangeFilter.val('all');
                applyFilters();
            });
        }

        // Initialize with current filter values
        applyFilters();

        console.log('✅ Product table filters initialized successfully');
    }

    // ========== MODAL HANDLERS ==========
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

            console.log('📝 Modal event handlers initialized');
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
            showToast('Product ID is missing', 'error');
            return;
        }

        // Set product IDs in forms
        safeSetValue('editProductId', productId);
        safeSetValue('deleteProductId', productId);

        // Clear previous data
        clearAllForms();

        // Load product data
        loadProductDataEnhanced(productId);
    }

    function handleEditModalHide() {
        console.log('🚪 Edit modal closed, clearing forms');
        clearAllForms();
        window.currentProductData = null;
    }

    function handleTabSwitch(event) {
        const targetTab = event.target.getAttribute('data-bs-target');
        const productId = document.getElementById('editProductId')?.value;

        console.log('🔄 Switching to tab:', targetTab, 'for product:', productId);

        if (!productId) return;

        switch (targetTab) {
            case '#product-details-tab-pane':
                // Already handled in main load function
                break;
            case '#product-info-tab-pane':
                console.log('📊 Loading product info tab');
                updateProductInfoDisplayFromCurrentData();
                break;
            case '#inventory-tab-pane':
                console.log('📦 Loading inventory tab');
                loadInventoryData(productId);
                break;
        }
    }

    function handleDeleteProduct() {
        const productId = safeGetValue('editProductId');
        if (!productId) return;

        safeSetValue('deleteProductId', productId);

        const editModal = bootstrap.Modal.getInstance(document.getElementById('editProductModal'));
        if (editModal) editModal.hide();

        const deleteModalEl = document.getElementById('deleteProductModal');
        if (deleteModalEl) {
            const deleteModal = new bootstrap.Modal(deleteModalEl);
            deleteModal.show();
        }
    }

    // ========== DATA LOADING FUNCTIONS ==========
    function loadProductDataEnhanced(productId) {
        console.log('📥 Loading enhanced product data for ID:', productId);

        if (!productId) {
            console.error('🚨 No product ID provided');
            return;
        }

        showLoadingState();

        // Try to populate from table data first
        const row = findProductRowById(productId);
        if (row) {
            console.log('📋 Found table row, extracting data...');
            const tableData = extractTableData(row);
            populateFromTableData(tableData);
        }

        // Always call API for complete data
        if (!window.appUrls?.getProductData) {
            console.error('🚨 GetProductData API URL not configured');
            hideLoadingState();
            showToast('API not configured. Only table data available.', 'warning');
            return;
        }

        fetch(`${window.appUrls.getProductData}?id=${encodeURIComponent(productId)}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                return response.json();
            })
            .then(data => {
                console.log('📦 Received enhanced product data:', data);
                hideLoadingState();

                if (data.success !== false) {
                    populateFromAPIDataEnhanced(data);
                    showToast('Product data loaded successfully', 'success');
                } else {
                    showToast(`Failed to load complete product data: ${data.message || 'Unknown error'}`, 'warning');
                }
            })
            .catch(error => {
                console.error('🚨 Error loading product data:', error);
                hideLoadingState();
                showToast(`Error loading product data: ${error.message}`, 'error');
            });
    }

    function extractTableData(row) {
        if (!row) return {};

        console.log('📋 Extracting data from table row');

        // Get data attributes
        const dataFromAttributes = {
            productId: row.getAttribute('data-product-id'),
            title: row.getAttribute('data-product-title'),
            description: row.getAttribute('data-description'),
            price: row.getAttribute('data-price'),
            points: row.getAttribute('data-points'),
            status: row.getAttribute('data-status'),
            category: row.getAttribute('data-category'),
            type: row.getAttribute('data-type'),
            imageUrl: row.getAttribute('data-image-url'),
            productNumber: row.getAttribute('data-product-number'),
            tag: row.getAttribute('data-tag')
        };

        // Extract from cell content as fallback
        const cells = row.querySelectorAll('td');
        if (cells.length >= 4) {
            // Product info from first column
            const productCell = cells[0];
            const titleEl = productCell.querySelector('.product-title, .fw-semibold');
            const numberEl = productCell.querySelector('.product-number, .text-muted.small');

            if (titleEl && !dataFromAttributes.title) {
                dataFromAttributes.title = titleEl.textContent.trim();
            }
            if (numberEl && !dataFromAttributes.productNumber) {
                dataFromAttributes.productNumber = numberEl.textContent.trim();
            }

            // Price from second column
            if (cells[1] && !dataFromAttributes.price) {
                const priceText = cells[1].textContent.trim();
                dataFromAttributes.price = priceText.replace('$', '');
            }

            // Points from third column
            if (cells[2] && !dataFromAttributes.points) {
                dataFromAttributes.points = cells[2].textContent.trim();
            }

            // Status from fourth column
            if (cells[3] && !dataFromAttributes.status) {
                const statusEl = cells[3].querySelector('.badge, .product-status');
                if (statusEl) {
                    dataFromAttributes.status = statusEl.textContent.trim();
                }
            }

            // Category from fifth column
            if (cells[4] && !dataFromAttributes.category) {
                dataFromAttributes.category = cells[4].textContent.trim();
            }
        }

        return dataFromAttributes;
    }

    function populateFromTableData(data) {
        console.log('📝 Populating form from table data:', data);

        try {
            // Store data globally
            window.currentProductData = data;

            // Populate form fields
            safeSetValue('editTitle', data.title);
            safeSetValue('editDescription', data.description);
            safeSetValue('editPrice', data.price);
            safeSetValue('editPoints', data.points);
            safeSetValue('editTag', data.tag);
            safeSetValue('editProductNumber', data.productNumber);

            // Handle image URL
            if (data.imageUrl) {
                safeSetValue('editImageURL', data.imageUrl);
                updateImagePreview(data.imageUrl);
            }

            // Select fields
            safeSetSelect('editType', data.type);
            safeSetSelect('editCategory', data.category);
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
            const productData = data.product || data;

            // Store data globally for product info tab access
            window.currentProductData = productData;

            // Basic information
            safeSetValue('editTitle', productData.title || productData.Title);
            safeSetValue('editDescription', productData.description || productData.Description);
            safeSetValue('editPrice', productData.price || productData.Price);
            safeSetValue('editPoints', productData.points || productData.Points);
            safeSetValue('editTag', productData.tag || productData.Tag);
            safeSetValue('editProductNumber', productData.productNumber || productData.ProductNumber);

            // Image handling
            const imageUrl = productData.imageURL || productData.ImageURL || productData.imageUrl;
            if (imageUrl) {
                safeSetValue('editImageURL', imageUrl);
                updateImagePreview(imageUrl);
            }

            // Select fields
            safeSetSelect('editType', productData.type || productData.Type);
            safeSetSelect('editCategory', productData.category || productData.Category);
            safeSetSelect('editStatus', productData.status || productData.Status || 'Active');

            console.log('✅ Enhanced API data populated successfully');

            // Update Product Info tab immediately if it's active
            const activeTab = document.querySelector('#editProductTabs .nav-link.active');
            if (activeTab && activeTab.getAttribute('data-bs-target') === '#product-info-tab-pane') {
                updateProductInfoDisplayFromCurrentData();
            }
        } catch (error) {
            console.error('💥 Error populating from API data:', error);
        }
    }

    // ========== PRODUCT INFO TAB FUNCTIONALITY ==========
    function updateProductInfoDisplayFromCurrentData() {
        console.log('📊 Updating product info display from current data');

        let productData = window.currentProductData;

        // Fallback to form data if no stored data
        if (!productData) {
            productData = {
                title: safeGetValue('editTitle'),
                productNumber: safeGetValue('editProductNumber'),
                description: safeGetValue('editDescription'),
                price: safeGetValue('editPrice'),
                points: safeGetValue('editPoints'),
                type: safeGetValue('editType'),
                category: safeGetValue('editCategory'),
                status: safeGetValue('editStatus'),
                tag: safeGetValue('editTag'),
                imageURL: safeGetValue('editImageURL')
            };
        }

        updateProductInfoDisplay(productData);
    }

    function updateProductInfoDisplay(productData) {
        console.log('📊 Updating product info display with data:', productData);

        if (!productData) {
            console.warn('⚠️ No product data available for info display');
            return;
        }

        const title = productData.title || productData.Title || 'Product';
        const productNumber = productData.productNumber || productData.ProductNumber || '--';
        const category = productData.category || productData.Category || '--';
        const status = productData.status || productData.Status || 'Active';
        const type = productData.type || productData.Type || '--';
        const price = productData.price || productData.Price || '0';
        const points = productData.points || productData.Points || '0';
        const tag = productData.tag || productData.Tag || '--';

        // Update avatar and basic info
        const initials = getProductInitials(title);
        safeUpdateElement('productInfoInitials', initials);
        safeUpdateElement('productInfoTitle', title);
        safeUpdateElement('productInfoNumber', productNumber);
        safeUpdateElement('productInfoStatus', status);
        safeUpdateElement('productInfoCategory', category);

        // Update product information section
        safeUpdateElement('productInfoTitleDetail', title);
        safeUpdateElement('productInfoPrice', `$${parseFloat(price || 0).toFixed(2)}`);
        safeUpdateElement('productInfoPoints', points);
        safeUpdateElement('productInfoType', type);
        safeUpdateElement('productInfoTag', tag);

        // Update status badge color
        const statusBadge = document.getElementById('productInfoStatus');
        if (statusBadge) {
            statusBadge.className = 'badge ' + getStatusBadgeClass(status);
        }

        // Update statistics (mock data for now)
        safeUpdateElement('productInfoSales', Math.floor(Math.random() * 100));
        safeUpdateElement('productInfoViews', Math.floor(Math.random() * 1000));
        safeUpdateElement('productInfoRating', (Math.random() * 2 + 3).toFixed(1));

        console.log('✅ Product info display updated successfully');
    }

    function loadInventoryData(productId) {
        console.log('📦 Loading inventory data for product:', productId);

        // Mock inventory data for now
        const inventoryData = {
            stockQuantity: Math.floor(Math.random() * 100),
            lowStockThreshold: 10,
            reorderLevel: 20
        };

        safeSetValue('stockQuantity', inventoryData.stockQuantity);
        safeSetValue('lowStockThreshold', inventoryData.lowStockThreshold);
        safeSetValue('reorderLevel', inventoryData.reorderLevel);
    }

    // ========== FORM HANDLERS ==========
    function initializeForms() {
        // Add Product Form
        const addProductForm = document.getElementById('addProductForm');
        if (addProductForm) {
            addProductForm.addEventListener('submit', handleAddFormSubmit);
            console.log('✅ Add form handler attached');
        }

        // Edit Product Form  
        const editProductForm = document.getElementById('editProductForm');
        if (editProductForm) {
            editProductForm.addEventListener('submit', handleEditFormSubmit);
            console.log('✅ Edit form handler attached');
        }
    }

    function handleAddFormSubmit(e) {
        e.preventDefault();
        console.log('📤 Add product form submitted');

        const formData = new FormData(e.target);
        const submitBtn = e.target.querySelector('button[type="submit"]');

        if (submitBtn && window.UIUtils) {
            window.UIUtils.setButtonLoading(submitBtn, true, 'Adding Product...');
        }

        const token = getAntiForgeryToken();

        fetch('/Product/Create', {
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
                    showToast('Product created successfully!', 'success');

                    const modal = bootstrap.Modal.getInstance(document.getElementById('addProductModal'));
                    if (modal) modal.hide();

                    setTimeout(() => location.reload(), 1000);
                } else {
                    showToast(`Error creating product: ${result.message || 'Unknown error'}`, 'error');
                }
            })
            .catch(error => {
                if (submitBtn && window.UIUtils) {
                    window.UIUtils.setButtonLoading(submitBtn, false);
                }
                console.error('Error creating product:', error);
                showToast(`Error creating product: ${error.message}`, 'error');
            });
    }

    function handleEditFormSubmit(e) {
        e.preventDefault();
        console.log('📤 Edit product form submitted');

        const formData = new FormData(e.target);
        const productData = {};

        for (const [key, value] of formData.entries()) {
            productData[key] = value;
        }

        const submitBtn = e.target.querySelector('button[type="submit"]');
        if (submitBtn && window.UIUtils) {
            window.UIUtils.setButtonLoading(submitBtn, true, 'Saving...');
        }

        const token = getAntiForgeryToken();

        fetch('/Product/Edit', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(productData)
        })
            .then(response => response.json())
            .then(result => {
                if (submitBtn && window.UIUtils) {
                    window.UIUtils.setButtonLoading(submitBtn, false);
                }

                if (result.success) {
                    showToast('Product updated successfully!', 'success');

                    // Update stored data
                    window.currentProductData = { ...window.currentProductData, ...productData };

                    setTimeout(() => {
                        const modal = bootstrap.Modal.getInstance(document.getElementById('editProductModal'));
                        if (modal) modal.hide();
                        location.reload();
                    }, 1000);
                } else {
                    showToast(`Error updating product: ${result.message || 'Unknown error'}`, 'error');
                }
            })
            .catch(error => {
                if (submitBtn && window.UIUtils) {
                    window.UIUtils.setButtonLoading(submitBtn, false);
                }
                console.error('Error updating product:', error);
                showToast(`Error updating product: ${error.message}`, 'error');
            });
    }

    // ========== IMAGE HANDLERS ==========
    function initializeImageHandlers() {
        // Image file input handlers
        const imageFileInputs = document.querySelectorAll('input[type="file"][accept*="image"]');
        imageFileInputs.forEach(input => {
            input.addEventListener('change', function (e) {
                const file = e.target.files[0];
                if (file) {
                    handleImagePreview(file, input);
                }
            });
        });

        // Image URL input handlers
        const imageUrlInputs = document.querySelectorAll('input[name="ImageURL"]');
        imageUrlInputs.forEach(input => {
            input.addEventListener('blur', function () {
                if (this.value) {
                    updateImagePreview(this.value);
                }
            });
        });
    }

    function handleImagePreview(file, input) {
        const reader = new FileReader();
        reader.onload = function (e) {
            const imageUrl = e.target.result;
            updateImagePreview(imageUrl);
        };
        reader.readAsDataURL(file);
    }

    // ========== UI STATE MANAGEMENT ==========
    function clearAllForms() {
        clearProductDetailsForm();
        clearProductInfoDisplay();
        clearInventoryForm();
    }

    function clearProductDetailsForm() {
        const fields = [
            'editTitle', 'editDescription', 'editPrice', 'editPoints',
            'editTag', 'editProductNumber', 'editImageURL'
        ];

        fields.forEach(field => safeSetValue(field, ''));

        // Reset select fields
        const selects = ['editType', 'editCategory', 'editStatus'];
        selects.forEach(select => {
            const element = document.getElementById(select);
            if (element) element.selectedIndex = 0;
        });

        updateImagePreview('');
        console.log('🧹 Product details form cleared');
    }

    function clearProductInfoDisplay() {
        const elements = [
            'productInfoInitials', 'productInfoTitle', 'productInfoNumber', 'productInfoStatus',
            'productInfoCategory', 'productInfoTitleDetail', 'productInfoPrice', 'productInfoPoints',
            'productInfoType', 'productInfoTag', 'productInfoSales', 'productInfoViews', 'productInfoRating'
        ];

        elements.forEach(elementId => safeUpdateElement(elementId, '--'));
        console.log('🧹 Product info display cleared');
    }

    function clearInventoryForm() {
        const fields = ['stockQuantity', 'lowStockThreshold', 'reorderLevel'];
        fields.forEach(field => safeSetValue(field, ''));
        console.log('🧹 Inventory form cleared');
    }

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

        console.log('⏳ Loading state shown');
    }

    function hideLoadingState() {
        const modal = document.getElementById('editProductModal');
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

    function findProductRowById(productId) {
        if (!productId) return null;

        console.log('🔍 Looking for row with product ID:', productId);

        // Try different strategies to find the row
        let row = document.querySelector(`tr[data-product-id="${productId}"]`);
        if (row) return row;

        const button = document.querySelector(`button[data-product-id="${productId}"]`);
        if (button) {
            row = button.closest('tr');
            if (row) return row;
        }

        // Search within table body
        const tableBody = document.querySelector('#productsTable tbody');
        if (tableBody) {
            const allRows = tableBody.querySelectorAll('tr');
            for (const tr of allRows) {
                const editBtn = tr.querySelector(`[data-product-id="${productId}"]`);
                if (editBtn) return tr;
            }
        }

        console.warn('⚠️ Row not found for product ID:', productId);
        return null;
    }

    function updateImagePreview(imageUrl) {
        const currentImage = document.getElementById('currentImage');
        const placeholder = document.getElementById('currentImagePlaceholder');

        if (!currentImage || !placeholder) {
            console.warn('⚠️ Image preview elements not found');
            return;
        }

        if (imageUrl && imageUrl.trim()) {
            currentImage.src = imageUrl;
            currentImage.style.display = 'block';
            placeholder.style.display = 'none';
            console.log('🖼️ Image preview updated');
        } else {
            currentImage.style.display = 'none';
            placeholder.style.display = 'flex';
            console.log('🖼️ Image preview cleared');
        }
    }

    function getProductInitials(title) {
        if (!title) return 'P';
        const words = title.split(' ');
        if (words.length >= 2) {
            return (words[0][0] + words[1][0]).toUpperCase();
        }
        return title[0].toUpperCase();
    }

    function getStatusBadgeClass(status) {
        switch (status?.toLowerCase()) {
            case 'inactive': return 'bg-secondary';
            case 'discontinued': return 'bg-danger';
            default: return 'bg-success';
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
    window.productDebug = {
        loadProductDataEnhanced,
        findProductRowById,
        populateFromTableData,
        populateFromAPIDataEnhanced,
        extractTableData,
        updateProductInfoDisplayFromCurrentData,
        updateProductInfoDisplay,
        clearAllForms,
        currentProductData: () => window.currentProductData,
        initializeFilters,
        applyFilters: () => {
            const table = $('#productsTable').DataTable();
            if (table) table.draw();
        }
    };

    console.log('✅ Complete Product Management loaded successfully');
    console.log('🐛 Debug functions available: window.productDebug');
});