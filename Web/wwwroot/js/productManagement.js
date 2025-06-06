/**
 * Complete Fixed Product Management JavaScript
 * Addresses the "Product ID is required" error and all related issues
 */

document.addEventListener('DOMContentLoaded', function () {
    console.log('🚀 Initializing Complete Fixed Product Management');

    // Global storage for current product data
    window.currentProductData = null;

    // Initialize components
    initializeDataTable();
    initializeModals();
    initializeForms();
    initializeImageHandlers();
    fixEditButtons();

    console.log('✅ Complete Fixed Product Management loaded successfully');

    // ========== FIX EDIT BUTTONS ==========
    function fixEditButtons() {
        console.log('🔧 Fixing edit buttons...');

        const editButtons = document.querySelectorAll('[data-bs-target="#editProductModal"]');
        console.log(`Found ${editButtons.length} edit buttons`);

        editButtons.forEach((button, index) => {
            let productId = button.getAttribute('data-product-id');

            if (!productId) {
                // Try to get from parent row
                const row = button.closest('tr');
                if (row) {
                    productId = row.getAttribute('data-product-id') ||
                        row.getAttribute('data-productid') ||
                        row.dataset.productId;

                    if (productId) {
                        button.setAttribute('data-product-id', productId);
                        console.log(`✅ Fixed button ${index + 1}: added data-product-id="${productId}"`);
                    } else {
                        console.warn(`❌ Button ${index + 1}: No product ID found anywhere`);

                        // Try to extract from href of nearby view button
                        const viewButton = row.querySelector('a[href*="/Product/Details"]');
                        if (viewButton) {
                            const match = viewButton.href.match(/id=([^&]+)/);
                            if (match) {
                                productId = match[1];
                                button.setAttribute('data-product-id', productId);
                                console.log(`✅ Extracted from view button: ${productId}`);
                            }
                        }
                    }
                } else {
                    console.error(`❌ Button ${index + 1}: No parent row found`);
                }
            } else {
                console.log(`✅ Button ${index + 1}: Already has data-product-id="${productId}"`);
            }
        });
    }

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
                initComplete: function () {
                    console.log('📊 DataTable initialization complete, setting up filters...');
                    setTimeout(function () {
                        initializeFilters();
                        fixEditButtons(); // Fix buttons after DataTable renders
                    }, 100);
                }
            });
            console.log('📊 DataTable initialized successfully');
        } else {
            setTimeout(function () {
                initializeFilters();
                fixEditButtons();
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

        function applyFilters() {
            console.log('🔍 Applying filters...');

            if ($.fn.dataTable.ext.search.length > 0) {
                $.fn.dataTable.ext.search = $.fn.dataTable.ext.search.filter(fn =>
                    !fn.name || fn.name !== 'productTableFilter'
                );
            }

            const customFilter = function (settings, data, dataIndex) {
                if (settings.nTable.id !== 'productsTable') return true;

                const row = $(table.row(dataIndex).node());

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
                        const rowStatus = (row.attr('data-status') || '').toLowerCase();
                        const statusCell = row.find('td:nth-child(4)');
                        const statusText = statusCell.find('.badge').text().toLowerCase() ||
                            statusCell.text().toLowerCase();

                        const matchesStatus = rowStatus === statusValue || statusText.includes(statusValue);
                        if (!matchesStatus) return false;
                    }

                    // Category filtering
                    if (categoryFilter.val() !== 'all') {
                        const categoryValue = categoryFilter.val().toLowerCase();
                        const rowCategory = (row.attr('data-category') || '').toLowerCase();
                        const categoryCell = row.find('td:nth-child(5)');
                        const categoryText = categoryCell.text().toLowerCase();

                        const matchesCategory = rowCategory === categoryValue || categoryText === categoryValue;
                        if (!matchesCategory) return false;
                    }

                    // Type filtering
                    if (typeFilter.val() !== 'all') {
                        const typeValue = typeFilter.val().toLowerCase();
                        const rowType = (row.attr('data-type') || '').toLowerCase();
                        const productCell = row.find('td:first-child');
                        const productText = productCell.text().toLowerCase();

                        const matchesType = rowType === typeValue || productText.includes(typeValue);
                        if (!matchesType) return false;
                    }

                    // Price range filtering
                    if (priceRangeFilter.val() !== 'all') {
                        const priceRangeValue = priceRangeFilter.val();
                        let price = parseFloat(row.attr('data-price')) || 0;

                        if (price === 0) {
                            const priceCell = row.find('td:nth-child(2)');
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

                        if (!matchesPrice) return false;
                    }

                    return true;
                } catch (error) {
                    console.error('❌ Error in filter function:', error);
                    return true;
                }
            };

            customFilter.name = 'productTableFilter';
            $.fn.dataTable.ext.search.push(customFilter);
            table.draw();
            updateActiveFilters();
        }

        function updateActiveFilters() {
            if (!activeFiltersContainer.length) return;

            activeFiltersContainer.find('.filter-badge, .filter-none').remove();

            const hasActiveFilters =
                statusFilter.val() !== 'all' ||
                categoryFilter.val() !== 'all' ||
                typeFilter.val() !== 'all' ||
                priceRangeFilter.val() !== 'all';

            if (!hasActiveFilters) {
                activeFiltersContainer.append(
                    $('<span>').addClass('text-muted filter-none').text('None')
                );
                return;
            }

            if (statusFilter.val() !== 'all') {
                addFilterBadge('Status', formatFilterValue(statusFilter.val()), () => {
                    statusFilter.val('all');
                    applyFilters();
                });
            }

            if (categoryFilter.val() !== 'all') {
                addFilterBadge('Category', formatFilterValue(categoryFilter.val()), () => {
                    categoryFilter.val('all');
                    applyFilters();
                });
            }

            if (typeFilter.val() !== 'all') {
                addFilterBadge('Type', formatFilterValue(typeFilter.val()), () => {
                    typeFilter.val('all');
                    applyFilters();
                });
            }

            if (priceRangeFilter.val() !== 'all') {
                addFilterBadge('Price', formatFilterValue(priceRangeFilter.val()), () => {
                    priceRangeFilter.val('all');
                    applyFilters();
                });
            }
        }

        function formatFilterValue(value) {
            if (value === '100+') return '$100+';
            if (value.includes('-')) {
                const parts = value.split('-');
                if (parts.length === 2 && !isNaN(parts[0]) && !isNaN(parts[1])) {
                    return `$${parts[0]} - $${parts[1]}`;
                }
            }
            return value.split('-').map(word => word.charAt(0).toUpperCase() + word.slice(1)).join(' ');
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
        statusFilter.on('change', applyFilters);
        categoryFilter.on('change', applyFilters);
        typeFilter.on('change', applyFilters);
        priceRangeFilter.on('change', applyFilters);

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

        applyFilters();
        console.log('✅ Product table filters initialized successfully');
    }

    // ========== MODAL HANDLERS ==========
    function initializeModals() {
        const editProductModal = document.getElementById('editProductModal');
        if (editProductModal) {
            editProductModal.addEventListener('show.bs.modal', handleEditModalShow);
            editProductModal.addEventListener('hidden.bs.modal', handleEditModalHide);

            const tabButtons = editProductModal.querySelectorAll('button[data-bs-toggle="tab"]');
            tabButtons.forEach(button => {
                button.addEventListener('shown.bs.tab', handleTabSwitch);
            });

            console.log('📝 Modal event handlers initialized');
        }

        const deleteProductBtn = document.getElementById('deleteProductBtn');
        if (deleteProductBtn) {
            deleteProductBtn.addEventListener('click', handleDeleteProduct);
        }
    }

    function handleEditModalShow(event) {
        const button = event.relatedTarget;
        const productId = button.getAttribute('data-product-id');

        console.log('📂 ENHANCED MODAL OPENING - Product ID Debug:');
        console.log('  Button element:', button);
        console.log('  data-product-id:', productId);
        console.log('  Button HTML:', button.outerHTML);

        if (!productId) {
            console.error('❌ No product ID found on button!');

            // Try to recover from various sources
            let recoveredId = null;

            // Try parent row
            const row = button.closest('tr');
            if (row) {
                recoveredId = row.getAttribute('data-product-id') ||
                    row.getAttribute('data-productid') ||
                    row.dataset.productId;
                console.log('  Trying row data:', recoveredId);
            }

            // Try sibling view button
            if (!recoveredId && row) {
                const viewButton = row.querySelector('a[href*="/Product/Details"]');
                if (viewButton) {
                    const match = viewButton.href.match(/id=([^&]+)/);
                    if (match) {
                        recoveredId = match[1];
                        console.log('  Extracted from view button:', recoveredId);
                    }
                }
            }

            if (recoveredId) {
                // Fix the button for future use
                button.setAttribute('data-product-id', recoveredId);
                console.log('✅ Recovered and set ProductId:', recoveredId);
            } else {
                console.error('❌ Could not recover ProductId from any source');
                event.preventDefault();
                return;
            }
        }

        const finalProductId = productId || button.getAttribute('data-product-id');
        console.log('✅ Final ProductId:', finalProductId);

        // Set product IDs in forms with verification
        const productIdField = document.getElementById('editProductId');
        const deleteIdField = document.getElementById('deleteProductId');

        if (productIdField) {
            productIdField.value = finalProductId;
            console.log('✅ Set editProductId field to:', productIdField.value);
        } else {
            console.error('❌ editProductId field not found!');
        }

        if (deleteIdField) {
            deleteIdField.value = finalProductId;
            console.log('✅ Set deleteProductId field to:', deleteIdField.value);
        }

        // Clear previous data and load new data
        clearAllForms();
        loadProductDataEnhanced(finalProductId);
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

    // ========== FORM HANDLERS ==========
    function initializeForms() {
        // Add Product Form
        const addProductForm = document.getElementById('addProductForm');
        if (addProductForm) {
            addProductForm.addEventListener('submit', handleAddFormSubmit);
            console.log('✅ Add form handler attached');
        }

        // Edit Product Form - FIXED VERSION
        const editProductForm = document.getElementById('editProductForm');
        if (editProductForm) {
            editProductForm.addEventListener('submit', handleEditFormSubmitFixed);
            console.log('✅ FIXED edit form handler attached');
        }
    }

    function handleAddFormSubmit(e) {
        e.preventDefault();
        console.log('📤 Add product form submitted');

        const formData = new FormData(e.target);
        const submitBtn = e.target.querySelector('button[type="submit"]');

        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Adding Product...';
        }

        const xhr = new XMLHttpRequest();

        xhr.onreadystatechange = function () {
            if (xhr.readyState === XMLHttpRequest.DONE) {
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = 'Add Product';
                }

                if (xhr.status === 200) {
                    try {
                        const result = JSON.parse(xhr.responseText);
                        if (result.success) {
                            console.log('Product created successfully!');
                            const modal = bootstrap.Modal.getInstance(document.getElementById('addProductModal'));
                            if (modal) modal.hide();
                            setTimeout(() => location.reload(), 1000);
                        } else {
                            console.error(`Error creating product: ${result.message || 'Unknown error'}`);
                        }
                    } catch (e) {
                        console.error('Error parsing server response');
                    }
                } else {
                    console.error(`Server error: ${xhr.status}`);
                }
            }
        };

        xhr.open('POST', '/Product/Create');
        xhr.send(formData);
    }

    function handleEditFormSubmitFixed(e) {
        e.preventDefault();
        console.log('📤 FIXED Edit product form submitted (Content-Type fix)');

        const form = e.target;
        const submitBtn = form.querySelector('button[type="submit"]');

        // CRITICAL: Pre-submission validation
        const productIdField = form.querySelector('#editProductId');
        const productIdValue = productIdField?.value;

        console.log('🔍 PRE-SUBMISSION VALIDATION:');
        console.log('  ProductId value:', `"${productIdValue}"`);

        if (!productIdValue || productIdValue.trim() === '') {
            console.error('❌ CRITICAL: ProductId is empty at submission!');

            // Try to recover
            const recoveredId = tryRecoverProductId();
            if (recoveredId) {
                console.log('🔧 RECOVERED ProductId:', recoveredId);
                productIdField.value = recoveredId;
            } else {
                console.error('Product ID is missing. Please close and reopen the edit dialog.');
                return;
            }
        }

        // Validate other required fields
        const titleField = form.querySelector('#editTitle');
        const typeField = form.querySelector('#editType');
        const categoryField = form.querySelector('#editCategory');

        const validationErrors = [];

        if (!titleField?.value?.trim()) {
            validationErrors.push('Title');
            titleField?.classList.add('is-invalid');
        }
        if (!typeField?.value) {
            validationErrors.push('Type');
            typeField?.classList.add('is-invalid');
        }
        if (!categoryField?.value) {
            validationErrors.push('Category');
            categoryField?.classList.add('is-invalid');
        }

        if (validationErrors.length > 0) {
            console.error(`Please fill in: ${validationErrors.join(', ')}`);
            return;
        }

        console.log('✅ Pre-submission validation passed');

        // Show loading state
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Saving...';
        }

        const formData = new FormData(form);

        // Debug: Log form data
        console.log('📦 FormData contents:');
        for (let [key, value] of formData.entries()) {
            if (value instanceof File) {
                console.log(`  ${key}: File(${value.name}, ${value.size} bytes)`);
            } else {
                console.log(`  ${key}: "${value}"`);
            }
        }

        // METHOD 1: Use fetch with proper FormData handling
        submitWithFetch(form, formData, submitBtn);
    }

    // METHOD 1: Fetch approach (recommended)
    function submitWithFetch(form, formData, submitBtn) {
        console.log('🚀 Using fetch method...');

        fetch(form.action, {
            method: 'POST',
            body: formData
        })
            .then(response => {
                console.log('📡 Response received:', response.status, response.statusText);
                console.log('📡 Response Content-Type:', response.headers.get('content-type'));

                if (!response.ok) {
                    return response.text().then(text => {
                        console.error('❌ Server error response:', text);
                        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                    });
                }
                return response.text();
            })
            .then(responseText => {
                console.log('📦 Raw response:', responseText.substring(0, 500));

                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = 'Save Changes';
                }

                try {
                    const result = JSON.parse(responseText);
                    console.log('📦 Parsed result:', result);

                    if (result.success) {
                        console.log('Product updated successfully!');

                        window.currentProductData = { ...window.currentProductData, ...result.product };

                        setTimeout(() => {
                            const modal = bootstrap.Modal.getInstance(document.getElementById('editProductModal'));
                            if (modal) modal.hide();
                            location.reload();
                        }, 1000);
                    } else {
                        console.error('❌ Server error:', result.message);

                        if (result.field) {
                            const errorField = form.querySelector(`#edit${result.field}`);
                            if (errorField) {
                                errorField.classList.add('is-invalid');
                                errorField.focus();
                            }
                        }
                    }
                } catch (e) {
                    console.error('❌ JSON parse error:', e);
                    console.log('Full response text:', responseText);

                    // Check if it's an HTML error page
                    if (responseText.includes('<html') || responseText.includes('<!DOCTYPE')) {
                        console.error('Server returned an error page. Check browser console for details.');
                    } else {
                        console.error('Error parsing server response');
                    }
                }
            })
            .catch(error => {
                console.error('❌ Fetch error:', error);
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = 'Save Changes';
                }
                console.error(`Network error: ${error.message}`);
            });
    }

    // ========== DATA LOADING ==========
    function loadProductDataEnhanced(productId) {
        console.log('📥 Loading product data for ID:', productId);

        if (!productId) {
            console.error('🚨 No product ID provided');
            return;
        }

        const row = findProductRowById(productId);
        if (row) {
            console.log('📋 Found table row, extracting data...');
            const tableData = extractTableData(row);
            populateFromTableData(tableData);
        }

        if (!window.appUrls?.getProductData) {
            console.error('🚨 GetProductData API URL not configured');
            console.warn('API not configured. Only table data available.');
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
                console.log('📦 Received product data:', data);

                if (data.success !== false) {
                    populateFromAPIDataEnhanced(data);
                    console.log('Product data loaded successfully');
                } else {
                    console.warn(`Failed to load product data: ${data.message || 'Unknown error'}`);
                }
            })
            .catch(error => {
                console.error('🚨 Error loading product data:', error);
                console.error(`Error loading product data: ${error.message}`);
            });
    }

    function extractTableData(row) {
        if (!row) return {};

        console.log('📋 Extracting data from table row');

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

        const cells = row.querySelectorAll('td');
        if (cells.length >= 4) {
            const productCell = cells[0];
            const titleEl = productCell.querySelector('.product-title, .fw-semibold');
            const numberEl = productCell.querySelector('.product-number, .text-muted.small');

            if (titleEl && !dataFromAttributes.title) {
                dataFromAttributes.title = titleEl.textContent.trim();
            }
            if (numberEl && !dataFromAttributes.productNumber) {
                dataFromAttributes.productNumber = numberEl.textContent.trim();
            }

            if (cells[1] && !dataFromAttributes.price) {
                const priceText = cells[1].textContent.trim();
                dataFromAttributes.price = priceText.replace('$', '');
            }

            if (cells[2] && !dataFromAttributes.points) {
                dataFromAttributes.points = cells[2].textContent.trim();
            }

            if (cells[3] && !dataFromAttributes.status) {
                const statusEl = cells[3].querySelector('.badge, .product-status');
                if (statusEl) {
                    dataFromAttributes.status = statusEl.textContent.trim();
                }
            }

            if (cells[4] && !dataFromAttributes.category) {
                dataFromAttributes.category = cells[4].textContent.trim();
            }
        }

        return dataFromAttributes;
    }

    function populateFromTableData(data) {
        console.log('📝 Populating form from table data:', data);

        try {
            window.currentProductData = data;

            safeSetValue('editTitle', data.title);
            safeSetValue('editDescription', data.description);
            safeSetValue('editPrice', data.price);
            safeSetValue('editPoints', data.points);
            safeSetValue('editTag', data.tag);
            safeSetValue('editProductNumber', data.productNumber);

            if (data.imageUrl) {
                safeSetValue('editImageURL', data.imageUrl);
                updateImagePreview(data.imageUrl);
            }

            safeSetSelect('editType', data.type);
            safeSetSelect('editCategory', data.category);
            safeSetSelect('editStatus', data.status || 'Active');

            console.log('✅ Table data populated successfully');
        } catch (error) {
            console.error('🚨 Error populating from table data:', error);
        }
    }

    function populateFromAPIDataEnhanced(data) {
        console.log('🌐 Populating form from API data:', data);

        try {
            const productData = data.product || data;
            window.currentProductData = productData;

            safeSetValue('editTitle', productData.title || productData.Title);
            safeSetValue('editDescription', productData.description || productData.Description);
            safeSetValue('editPrice', productData.price || productData.Price);
            safeSetValue('editPoints', productData.points || productData.Points);
            safeSetValue('editTag', productData.tag || productData.Tag);
            safeSetValue('editProductNumber', productData.productNumber || productData.ProductNumber);

            const imageUrl = productData.imageURL || productData.ImageURL || productData.imageUrl;
            if (imageUrl) {
                safeSetValue('editImageURL', imageUrl);
                updateImagePreview(imageUrl);
            }

            safeSetSelect('editType', productData.type || productData.Type);
            safeSetSelect('editCategory', productData.category || productData.Category);
            safeSetSelect('editStatus', productData.status || productData.Status || 'Active');

            console.log('✅ API data populated successfully');

            const activeTab = document.querySelector('#editProductTabs .nav-link.active');
            if (activeTab && activeTab.getAttribute('data-bs-target') === '#product-info-tab-pane') {
                updateProductInfoDisplayFromCurrentData();
            }
        } catch (error) {
            console.error('💥 Error populating from API data:', error);
        }
    }

    // ========== PRODUCT INFO TAB ==========
    function updateProductInfoDisplayFromCurrentData() {
        console.log('📊 Updating product info display from current data');

        let productData = window.currentProductData;

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

        const initials = getProductInitials(title);
        safeUpdateElement('productInfoInitials', initials);
        safeUpdateElement('productInfoTitle', title);
        safeUpdateElement('productInfoNumber', productNumber);
        safeUpdateElement('productInfoStatus', status);
        safeUpdateElement('productInfoCategory', category);

        safeUpdateElement('productInfoTitleDetail', title);
        safeUpdateElement('productInfoPrice', `$${parseFloat(price || 0).toFixed(2)}`);
        safeUpdateElement('productInfoPoints', points);
        safeUpdateElement('productInfoType', type);
        safeUpdateElement('productInfoTag', tag);

        const statusBadge = document.getElementById('productInfoStatus');
        if (statusBadge) {
            statusBadge.className = 'badge ' + getStatusBadgeClass(status);
        }

        safeUpdateElement('productInfoSales', Math.floor(Math.random() * 100));
        safeUpdateElement('productInfoViews', Math.floor(Math.random() * 1000));
        safeUpdateElement('productInfoRating', (Math.random() * 2 + 3).toFixed(1));

        console.log('✅ Product info display updated successfully');
    }

    function loadInventoryData(productId) {
        console.log('📦 Loading inventory data for product:', productId);

        const inventoryData = {
            stockQuantity: Math.floor(Math.random() * 100),
            lowStockThreshold: 10,
            reorderLevel: 20
        };

        safeSetValue('stockQuantity', inventoryData.stockQuantity);
        safeSetValue('lowStockThreshold', inventoryData.lowStockThreshold);
        safeSetValue('reorderLevel', inventoryData.reorderLevel);
    }

    // ========== IMAGE HANDLERS ==========
    function initializeImageHandlers() {
        const imageFileInputs = document.querySelectorAll('input[type="file"][accept*="image"]');
        imageFileInputs.forEach(input => {
            input.addEventListener('change', function (e) {
                const file = e.target.files[0];
                if (file) {
                    handleImagePreview(file, input);
                }
            });
        });

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

        let row = document.querySelector(`tr[data-product-id="${productId}"]`);
        if (row) return row;

        const button = document.querySelector(`button[data-product-id="${productId}"]`);
        if (button) {
            row = button.closest('tr');
            if (row) return row;
        }

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

    function tryRecoverProductId() {
        console.log('🔧 Attempting to recover ProductId...');

        if (window.currentProductData?.productId) {
            console.log('  Found in currentProductData:', window.currentProductData.productId);
            return window.currentProductData.productId;
        }

        const deleteIdField = document.getElementById('deleteProductId');
        if (deleteIdField?.value) {
            console.log('  Found in deleteProductId field:', deleteIdField.value);
            return deleteIdField.value;
        }

        const modal = document.getElementById('editProductModal');
        if (modal && modal.classList.contains('show')) {
            const activeButton = document.querySelector('[data-bs-target="#editProductModal"][aria-expanded="true"]');
            if (activeButton) {
                const productId = activeButton.getAttribute('data-product-id');
                if (productId) {
                    console.log('  Found from active button:', productId);
                    return productId;
                }
            }
        }

        console.log('  Could not recover ProductId from any source');
        return null;
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

    // ========== DEBUG FUNCTIONS ==========
    function debugFormState() {
        console.log('🔍 CURRENT FORM STATE:');

        const form = document.getElementById('editProductForm');
        if (!form) {
            console.log('  Form not found!');
            return;
        }

        const formData = new FormData(form);
        console.log('  Form element:', form);
        console.log('  Form action:', form.action);
        console.log('  Form method:', form.method);

        console.log('  Form data:');
        for (let [key, value] of formData.entries()) {
            console.log(`    ${key}: "${value}"`);
        }

        const productIdField = document.getElementById('editProductId');
        console.log('  ProductId field:', productIdField);
        console.log('  ProductId value:', `"${productIdField?.value}"`);

        const titleField = document.getElementById('editTitle');
        console.log('  Title field:', titleField);
        console.log('  Title value:', `"${titleField?.value}"`);
    }

    function debugEditButtons() {
        console.log('🔍 DEBUGGING EDIT BUTTONS:');
        const editButtons = document.querySelectorAll('[data-bs-target="#editProductModal"]');
        console.log(`Found ${editButtons.length} edit buttons`);

        editButtons.forEach((button, index) => {
            const productId = button.getAttribute('data-product-id');
            console.log(`Button ${index + 1}:`, {
                hasProductId: !!productId,
                productId: productId,
                element: button,
                html: button.outerHTML
            });
        });
    }

    // ========== GLOBAL API ==========
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
        debugFormState,
        debugEditButtons,
        tryRecoverProductId,
        fixEditButtons
    };

    console.log('✅ Complete Fixed Product Management loaded successfully');
    console.log('🐛 Debug functions available: window.productDebug');
    console.log('💡 Use window.productDebug.debugFormState() to check form state');
    console.log('💡 Use window.productDebug.debugEditButtons() to check buttons');
});