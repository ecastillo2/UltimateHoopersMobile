/**
 * Product Management JavaScript
 * 
 * This file handles product management functionality similar to userManagement.js
 */

document.addEventListener('DOMContentLoaded', function () {
    // Initialize DataTable if the table exists
    const productsTable = $('#productsTable');
    if (productsTable.length > 0) {
        productsTable.DataTable({
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
                { orderable: false, targets: [5] } // Disable sorting on the Actions column
            ],
            order: [[0, 'asc']] // Sort by Product name by default
        });

        // Filter references - only initialize if elements exist
        const statusFilter = $('#statusFilter');
        const categoryFilter = $('#categoryFilter');
        const typeFilter = $('#typeFilter');
        const priceRangeFilter = $('#priceRangeFilter');
        const resetFiltersBtn = $('#resetFilters');
        const activeFiltersContainer = $('#activeFilters');

        if (statusFilter.length && categoryFilter.length && typeFilter.length) {
            setupFilters(productsTable.DataTable(), statusFilter, categoryFilter, typeFilter, priceRangeFilter, resetFiltersBtn, activeFiltersContainer);
        }
    }

    // Function to set up filters
    function setupFilters(table, statusFilter, categoryFilter, typeFilter, priceRangeFilter, resetFiltersBtn, activeFiltersContainer) {
        // Apply filter function
        function applyFilters() {
            // Remove any existing custom filter to prevent stacking
            if ($.fn.dataTable.ext.search.length > 0) {
                $.fn.dataTable.ext.search.pop();
            }

            // Create a new custom filter function
            $.fn.dataTable.ext.search.push(function (settings, data, dataIndex) {
                // Only apply this filter to our productsTable
                if (settings.nTable.id !== 'productsTable') return true;

                // Skip filtering if all filters are set to 'all'
                if (statusFilter.val() === 'all' &&
                    categoryFilter.val() === 'all' &&
                    typeFilter.val() === 'all' &&
                    priceRangeFilter.val() === 'all') {
                    return true;
                }

                const row = $(table.row(dataIndex).node());

                // Status filtering (column 3 contains the status badge)
                if (statusFilter.val() !== 'all') {
                    const status = row.data('status') || data[3].toLowerCase();
                    const statusValue = statusFilter.val().toLowerCase();

                    if (status !== statusValue) {
                        return false;
                    }
                }

                // Category filtering (column 4 contains the category)
                if (categoryFilter.val() !== 'all') {
                    const category = row.data('category') || data[4].toLowerCase();
                    const categoryValue = categoryFilter.val().toLowerCase();

                    if (category !== categoryValue) {
                        return false;
                    }
                }

                // Type filtering
                if (typeFilter.val() !== 'all') {
                    const type = row.data('type') || '';
                    const typeValue = typeFilter.val().toLowerCase();

                    if (type !== typeValue) {
                        return false;
                    }
                }

                // Price range filtering (column 1 contains the price)
                if (priceRangeFilter.val() !== 'all') {
                    const priceText = data[1] || '0';
                    const price = parseFloat(priceText.replace('$', '')) || 0;
                    const priceRange = priceRangeFilter.val();

                    let minPrice = 0, maxPrice = Infinity;

                    switch (priceRange) {
                        case '0-25':
                            minPrice = 0; maxPrice = 25;
                            break;
                        case '25-50':
                            minPrice = 25; maxPrice = 50;
                            break;
                        case '50-100':
                            minPrice = 50; maxPrice = 100;
                            break;
                        case '100+':
                            minPrice = 100; maxPrice = Infinity;
                            break;
                    }

                    if (price < minPrice || price > maxPrice) {
                        return false;
                    }
                }

                // If we got here, the row passes all filters
                return true;
            });

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
                .on('click', removeCallback);

            badge.append(removeBtn);
            activeFiltersContainer.append(badge);
        }

        // Add event listeners to filters
        statusFilter.on('change', applyFilters);
        categoryFilter.on('change', applyFilters);
        typeFilter.on('change', applyFilters);
        priceRangeFilter.on('change', applyFilters);

        // Reset filters button
        if (resetFiltersBtn.length) {
            resetFiltersBtn.on('click', function () {
                statusFilter.val('all');
                categoryFilter.val('all');
                typeFilter.val('all');
                priceRangeFilter.val('all');
                applyFilters();
            });
        }

        // Initialize with current filter values
        applyFilters();
    }

    // Handle edit product modal
    var editProductModal = document.getElementById('editProductModal');
    if (editProductModal) {
        editProductModal.addEventListener('show.bs.modal', function (event) {
            var button = event.relatedTarget;
            var productId = button.getAttribute('data-product-id');

            console.log('Opening edit modal for product ID:', productId);

            // Set product ID in the product details form - check if elements exist
            var editProductIdField = document.getElementById('editProductId');
            if (editProductIdField) {
                editProductIdField.value = productId;
            }

            // Set product ID in the inventory form - check if element exists
            var inventoryProductIdField = document.getElementById('inventoryProductId');
            if (inventoryProductIdField) {
                inventoryProductIdField.value = productId;
            }

            // Set delete button product ID - check if element exists
            var deleteProductIdField = document.getElementById('deleteProductId');
            if (deleteProductIdField) {
                deleteProductIdField.value = productId;
            }

            // Clear any previous form data
            clearProductForm();

            // Load product data immediately - this will show instant feedback
            loadProductData(productId);

            // Load product info for the product info tab
            loadProductInfo(productId);

            // Load inventory data
            loadInventoryData(productId);
        });

        // Clear form when modal is hidden
        editProductModal.addEventListener('hidden.bs.modal', function () {
            clearProductForm();
            // Remove any loading overlays
            var loadingOverlay = document.getElementById('formLoadingOverlay');
            if (loadingOverlay) {
                loadingOverlay.remove();
            }
        });
    }

    // Function to clear product form
    function clearProductForm() {
        // Clear all form fields
        var formFields = [
            'editTitle', 'editDescription', 'editPrice', 'editPoints',
            'editProductNumber', 'editTag', 'editImageURL'
        ];

        formFields.forEach(function (fieldId) {
            safeSetValue(fieldId, '');
        });

        // Reset select fields to first option
        var selectFields = ['editType', 'editCategory', 'editStatus'];
        selectFields.forEach(function (fieldId) {
            var select = document.getElementById(fieldId);
            if (select && select.options.length > 0) {
                select.selectedIndex = 0;
            }
        });

        console.log('Product form cleared');
    }

    // Handle delete product button
    const deleteProductBtn = document.getElementById('deleteProductBtn');
    if (deleteProductBtn) {
        deleteProductBtn.addEventListener('click', function () {
            const productId = document.getElementById('editProductId')?.value;
            if (!productId) return;

            const deleteProductIdField = document.getElementById('deleteProductId');
            if (deleteProductIdField) {
                deleteProductIdField.value = productId;
            }

            // Hide edit modal and show delete confirmation modal
            const editModal = bootstrap.Modal.getInstance(document.getElementById('editProductModal'));
            if (editModal) {
                editModal.hide();
            }

            const deleteModalEl = document.getElementById('deleteProductModal');
            if (deleteModalEl) {
                const deleteModal = new bootstrap.Modal(deleteModalEl);
                deleteModal.show();
            }
        });
    }

    // Function to load product data
    function loadProductData(productId) {
        if (!productId) {
            console.error("Missing productId for loadProductData");
            return;
        }

        console.log("Loading product data for ID:", productId);

        // Show loading indicator in the form
        showFormLoadingState(true);

        // Always try to populate from table row data first for immediate feedback
        const row = findProductRowById(productId);
        if (row) {
            console.log("Found table row, populating from table data");
            populateProductFormFromRow(row);
        } else {
            console.warn("No table row found for product ID:", productId);
        }

        // Then try to load from API if available for more complete data
        if (window.appUrls?.getProductData) {
            const url = `${window.appUrls.getProductData}?id=${productId}`;
            console.log("Making API request to:", url);

            fetch(url)
                .then(response => {
                    console.log("API response status:", response.status);
                    if (!response.ok) {
                        throw new Error(`Network response was not ok: ${response.status}`);
                    }
                    return response.json();
                })
                .then(data => {
                    showFormLoadingState(false);
                    console.log("API response data:", data);

                    if (data.success) {
                        console.log("API data received successfully, populating form fields");

                        // Populate form fields with API data - this will override table data with more complete info
                        if (data.Title) {
                            safeSetValue('editTitle', data.Title);
                            console.log("Set title from API:", data.Title);
                        }

                        if (data.Description) {
                            safeSetValue('editDescription', data.Description);
                            console.log("Set description from API:", data.Description);
                        }

                        // Handle price as decimal with proper formatting
                        if (data.Price !== null && data.Price !== undefined) {
                            var price = parseFloat(data.Price);
                            if (!isNaN(price)) {
                                safeSetValue('editPrice', price.toFixed(2));
                                console.log("Set price from API:", price.toFixed(2));
                            } else {
                                safeSetValue('editPrice', '0.00');
                                console.warn("Invalid price from API:", data.Price);
                            }
                        }

                        // Handle points as integer
                        if (data.Points !== null && data.Points !== undefined) {
                            var points = parseInt(data.Points, 10);
                            if (!isNaN(points)) {
                                safeSetValue('editPoints', points.toString());
                                console.log("Set points from API:", points);
                            } else {
                                safeSetValue('editPoints', '0');
                                console.warn("Invalid points from API:", data.Points);
                            }
                        }

                        if (data.ProductNumber) {
                            safeSetValue('editProductNumber', data.ProductNumber);
                            console.log("Set product number from API:", data.ProductNumber);
                        }

                        if (data.Tag) {
                            safeSetValue('editTag', data.Tag);
                            console.log("Set tag from API:", data.Tag);
                        }

                        if (data.ImageURL) {
                            safeSetValue('editImageURL', data.ImageURL);
                            console.log("Set image URL from API:", data.ImageURL);
                        }

                        // Select values - check if elements exist
                        if (data.Type) {
                            safeSetSelect('editType', data.Type);
                            console.log("Set type from API:", data.Type);
                        }

                        if (data.Category) {
                            safeSetSelect('editCategory', data.Category);
                            console.log("Set category from API:", data.Category);
                        }

                        safeSetSelect('editStatus', data.Status || 'Active');
                        console.log("Set status from API:", data.Status || 'Active');

                        console.log('Product data loaded successfully from API');
                        showToast('Success', 'Product data loaded from API', 'success');
                    } else {
                        console.error('Error loading product data from API:', data.message);
                        showToast('Warning', 'Using table data. Some details may be limited.', 'warning');
                    }
                })
                .catch(error => {
                    showFormLoadingState(false);
                    console.error('Error fetching product data from API:', error);
                    showToast('Warning', 'API unavailable. Using table data.', 'warning');
                });
        } else {
            // No API available, just use table data
            showFormLoadingState(false);
            console.log('No API URL configured, using table data only');
            showToast('Info', 'Using table data only - no API configured', 'info');
        }
    }

    // Function to show/hide loading state in form
    function showFormLoadingState(isLoading) {
        const loadingOverlay = document.getElementById('formLoadingOverlay');
        const formContainer = document.querySelector('#product-details-tab-pane .mt-4');

        if (isLoading) {
            // Create loading overlay if it doesn't exist
            if (!loadingOverlay && formContainer) {
                const overlay = document.createElement('div');
                overlay.id = 'formLoadingOverlay';
                overlay.className = 'position-absolute top-0 start-0 w-100 h-100 d-flex align-items-center justify-content-center bg-white bg-opacity-75';
                overlay.style.zIndex = '1050';

                const overlayContent = document.createElement('div');
                overlayContent.className = 'text-center';
                overlayContent.innerHTML = '<div class="spinner-border text-primary mb-2" role="status">' +
                    '<span class="visually-hidden">Loading...</span>' +
                    '</div>' +
                    '<div class="text-muted">Loading product data...</div>';

                overlay.appendChild(overlayContent);

                // Make form container relative for positioning
                formContainer.style.position = 'relative';
                formContainer.appendChild(overlay);
            }
        } else {
            // Remove loading overlay
            if (loadingOverlay) {
                loadingOverlay.remove();
            }
        }
    }

    // Helper function to safely set input value if element exists
    function safeSetValue(elementId, value) {
        const element = document.getElementById(elementId);
        if (element) {
            element.value = value || '';
            console.log(`Set ${elementId}:`, value);
        } else {
            console.warn(`Element ${elementId} not found`);
        }
    }

    // Helper function to safely set select option if element exists
    function safeSetSelect(elementId, value) {
        const select = document.getElementById(elementId);
        if (select && value) {
            // First try exact match
            let optionFound = false;
            for (let i = 0; i < select.options.length; i++) {
                if (select.options[i].value.toLowerCase() === value.toLowerCase()) {
                    select.selectedIndex = i;
                    optionFound = true;
                    console.log(`Set ${elementId} to:`, value);
                    break;
                }
            }

            // If no exact match, try partial match
            if (!optionFound) {
                for (let i = 0; i < select.options.length; i++) {
                    if (select.options[i].text.toLowerCase().includes(value.toLowerCase()) ||
                        select.options[i].value.toLowerCase().includes(value.toLowerCase())) {
                        select.selectedIndex = i;
                        optionFound = true;
                        console.log(`Set ${elementId} to (partial match):`, select.options[i].value);
                        break;
                    }
                }
            }

            if (!optionFound) {
                console.warn(`No option found for ${elementId} with value:`, value);
            }
        } else if (!select) {
            console.warn(`Element ${elementId} not found`);
        }
    }

    // Helper function to find product row by ID
    function findProductRowById(productId) {
        if (!productId) return null;

        const table = $('#productsTable').DataTable();
        if (!table) return null;

        const rows = table.rows().nodes();

        for (let i = 0; i < rows.length; i++) {
            const editButton = rows[i].querySelector(`button[data-product-id="${productId}"]`);
            if (editButton) {
                return rows[i];
            }
        }

        return null;
    }

    // Helper function to populate product form from table row data
    function populateProductFormFromRow(row) {
        if (!row) return;

        console.log('Populating form from table row data', row);

        // Try multiple approaches to extract data since table structure may vary

        // Method 1: Try to extract from the first cell (product name/title column)
        if (row.cells && row.cells.length > 0) {
            const firstCell = row.cells[0];
            console.log('First cell content:', firstCell.innerHTML);

            // Look for title in various possible locations
            let titleEl = firstCell.querySelector('.fw-semibold') ||
                firstCell.querySelector('.fw-bold') ||
                firstCell.querySelector('strong') ||
                firstCell.querySelector('.product-title') ||
                firstCell.querySelector('a');

            // If no specific element found, try to get the main text content
            if (!titleEl) {
                // Get all text nodes and find the longest one (likely the title)
                const textContent = firstCell.textContent.trim();
                const lines = textContent.split('\n').map(line => line.trim()).filter(line => line.length > 0);
                if (lines.length > 0) {
                    safeSetValue('editTitle', lines[0]);
                    console.log('Set title from text content:', lines[0]);

                    // Second line might be product number
                    if (lines.length > 1) {
                        safeSetValue('editProductNumber', lines[1]);
                        console.log('Set product number from text content:', lines[1]);
                    }
                }
            } else {
                const title = titleEl.textContent.trim();
                safeSetValue('editTitle', title);
                console.log('Set title from element:', title);
            }

            // Look for product number/SKU in various possible locations
            let productNumberEl = firstCell.querySelector('.text-muted.small') ||
                firstCell.querySelector('.text-muted') ||
                firstCell.querySelector('.small') ||
                firstCell.querySelector('.product-number') ||
                firstCell.querySelector('.sku');

            if (productNumberEl) {
                const productNumber = productNumberEl.textContent.trim();
                safeSetValue('editProductNumber', productNumber);
                console.log('Set product number from element:', productNumber);
            }

            // Extract price from column 1 (remove $ symbol and any extra characters)
            if (row.cells.length > 1) {
                const priceText = row.cells[1].textContent.trim().replace(/[$,]/g, '');
                const price = parseFloat(priceText) || 0;
                safeSetValue('editPrice', price.toString());
                console.log('Set price:', price);
            }

            // Extract points from column 2
            if (row.cells.length > 2) {
                const pointsText = row.cells[2].textContent.trim().replace(/[^0-9]/g, '');
                const points = parseInt(pointsText) || 0;
                safeSetValue('editPoints', points.toString());
                console.log('Set points:', points);
            }

            // Extract status from column 3 (look for badge or text)
            if (row.cells.length > 3) {
                const statusCell = row.cells[3];
                const statusBadge = statusCell.querySelector('.badge') ||
                    statusCell.querySelector('.label') ||
                    statusCell.querySelector('.status');

                if (statusBadge) {
                    const status = statusBadge.textContent.trim();
                    safeSetSelect('editStatus', status);
                    console.log('Set status from badge:', status);
                } else {
                    const status = statusCell.textContent.trim();
                    if (status && status !== 'N/A') {
                        safeSetSelect('editStatus', status);
                        console.log('Set status from text:', status);
                    }
                }
            }

            // Extract category from column 4
            if (row.cells.length > 4) {
                const category = row.cells[4].textContent.trim();
                if (category && category !== 'N/A' && category !== '') {
                    safeSetSelect('editCategory', category);
                    console.log('Set category:', category);
                }
            }
        }

        // Method 2: Try to extract from data attributes (more reliable if available)
        const dataAttributes = {
            title: row.getAttribute('data-title') || row.getAttribute('data-product-title'),
            productNumber: row.getAttribute('data-product-number') || row.getAttribute('data-sku'),
            description: row.getAttribute('data-description'),
            status: row.getAttribute('data-status'),
            category: row.getAttribute('data-category'),
            type: row.getAttribute('data-type'),
            price: row.getAttribute('data-price'),
            points: row.getAttribute('data-points')
        };

        console.log('Data attributes found:', dataAttributes);

        // Override with data attributes if they exist
        if (dataAttributes.title) {
            safeSetValue('editTitle', dataAttributes.title);
            console.log('Set title from data attribute:', dataAttributes.title);
        }

        if (dataAttributes.productNumber) {
            safeSetValue('editProductNumber', dataAttributes.productNumber);
            console.log('Set product number from data attribute:', dataAttributes.productNumber);
        }

        if (dataAttributes.description) {
            safeSetValue('editDescription', dataAttributes.description);
            console.log('Set description from data attribute:', dataAttributes.description);
        }

        if (dataAttributes.status) {
            safeSetSelect('editStatus', capitalizeFirst(dataAttributes.status));
            console.log('Set status from data attribute:', dataAttributes.status);
        }

        if (dataAttributes.category) {
            safeSetSelect('editCategory', capitalizeFirst(dataAttributes.category));
            console.log('Set category from data attribute:', dataAttributes.category);
        }

        if (dataAttributes.type) {
            safeSetSelect('editType', capitalizeFirst(dataAttributes.type));
            console.log('Set type from data attribute:', dataAttributes.type);
        }

        if (dataAttributes.price) {
            safeSetValue('editPrice', dataAttributes.price);
            console.log('Set price from data attribute:', dataAttributes.price);
        }

        if (dataAttributes.points) {
            safeSetValue('editPoints', dataAttributes.points);
            console.log('Set points from data attribute:', dataAttributes.points);
        }

        // Set placeholder description if not found anywhere
        const descriptionField = document.getElementById('editDescription');
        if (descriptionField && !descriptionField.value) {
            safeSetValue('editDescription', 'Product description will be loaded from API or can be edited here.');
        }

        showToast('Info', 'Product details loaded from table. Edit as needed.', 'info');
    }

    // Helper function to capitalize first letter
    function capitalizeFirst(str) {
        if (!str) return str;
        return str.charAt(0).toUpperCase() + str.slice(1).toLowerCase();
    }

    // Function to load product info
    function loadProductInfo(productId) {
        if (!productId || !window.appUrls?.getProductData) {
            console.log("Missing productId or API URL for loadProductInfo");
            return;
        }

        const url = `${window.appUrls.getProductData}?id=${productId}`;

        fetch(url)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    updateProductInfoUI(data);
                } else {
                    console.error('Error loading product info:', data.message);
                    showToast('Error', 'Failed to load product info. Please try again.', 'danger');

                    // Create fallback product info from form data
                    const fallbackProduct = createFallbackProductInfo();
                    updateProductInfoUI(fallbackProduct);
                }
            })
            .catch(error => {
                console.error('Error fetching product info:', error);
                showToast('Error', 'Failed to load product info. Please try again.', 'danger');

                // Create fallback product info from form data
                const fallbackProduct = createFallbackProductInfo();
                updateProductInfoUI(fallbackProduct);
            });
    }

    // Create fallback product info from form data
    function createFallbackProductInfo() {
        // Safely get values, defaulting to empty strings if elements don't exist
        const getValueOrEmpty = (id) => {
            const el = document.getElementById(id);
            return el ? el.value || '' : '';
        };

        return {
            Title: getValueOrEmpty('editTitle'),
            Description: getValueOrEmpty('editDescription'),
            Price: getValueOrEmpty('editPrice'),
            Points: getValueOrEmpty('editPoints'),
            ProductNumber: getValueOrEmpty('editProductNumber'),
            Tag: getValueOrEmpty('editTag'),
            Type: document.getElementById('editType')?.value || '',
            Category: document.getElementById('editCategory')?.value || '',
            Status: document.getElementById('editStatus')?.value || 'Active',
            ImageURL: getValueOrEmpty('editImageURL'),
            stats: { sales: 0, views: 0, rating: 0 }
        };
    }

    // Function to update product info UI elements
    function updateProductInfoUI(product) {
        if (!product) {
            console.error("No product data provided to updateProductInfoUI");
            return;
        }

        // Safely update UI elements - only update if element exists
        safeUpdateElement('productInfoInitials', getInitials(product.Title));
        safeUpdateElement('productInfoTitle', product.Title || 'Product');
        safeUpdateElement('productInfoNumber', product.ProductNumber || 'No SKU');
        safeUpdateElement('productInfoStatus', product.Status || 'Active');
        safeUpdateElement('productInfoCategory', product.Category || 'No Category');

        // Product info details
        safeUpdateElement('productInfoTitleDetail', product.Title || '--');

        // Format price as currency with 2 decimal places
        var priceDisplay = '--';
        if (product.Price !== null && product.Price !== undefined) {
            var price = parseFloat(product.Price);
            if (!isNaN(price)) {
                priceDisplay = '$' + price.toFixed(2);
            }
        }
        safeUpdateElement('productInfoPrice', priceDisplay);

        // Format points as integer
        var pointsDisplay = '--';
        if (product.Points !== null && product.Points !== undefined) {
            var points = parseInt(product.Points, 10);
            if (!isNaN(points)) {
                pointsDisplay = points.toString();
            }
        }
        safeUpdateElement('productInfoPoints', pointsDisplay);

        safeUpdateElement('productInfoType', product.Type || '--');
        safeUpdateElement('productInfoTag', product.Tag || '--');

        // Statistics - use mock data or actual stats if available
        safeUpdateElement('productInfoSales', product.stats?.sales || Math.floor(Math.random() * 100));
        safeUpdateElement('productInfoViews', product.stats?.views || Math.floor(Math.random() * 1000));
        safeUpdateElement('productInfoRating', product.stats?.rating || (Math.random() * 5).toFixed(1));
    }

    // Function to load inventory data
    function loadInventoryData(productId) {
        if (!productId) {
            console.log("Missing productId for loadInventoryData");
            return;
        }

        // For now, we'll populate with mock data since there's no inventory API endpoint yet
        // In a real application, you would fetch this from an inventory API

        // Set mock inventory data
        safeSetValue('stockQuantity', Math.floor(Math.random() * 100));
        safeSetValue('lowStockThreshold', 5);
        safeSetValue('reorderLevel', 10);
        safeSetValue('supplier', 'Sports Equipment Co.');
        safeSetValue('sku', `SKU-${productId}-001`);
        safeSetValue('location', 'Warehouse A - Section 3');
        safeSetValue('lastRestocked', new Date().toISOString().split('T')[0]);
        safeSetValue('inventoryNotes', 'Standard storage conditions required.');

        // Show metadata
        const metadataSection = document.getElementById('inventoryMetadata');
        if (metadataSection) {
            metadataSection.style.display = 'flex';
            safeUpdateElement('inventoryLastUpdated', formatDateTime(new Date()));
        }
    }

    // Helper function to safely update element text content
    function safeUpdateElement(elementId, value) {
        const element = document.getElementById(elementId);
        if (element) {
            element.textContent = value;
        }
    }

    // Helper function to show toast notifications
    function showToast(title, message, type = 'success') {
        // Check if toast container exists, create if not
        let toastContainer = document.querySelector('.toast-container');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
            toastContainer.style.zIndex = '9999';
            document.body.appendChild(toastContainer);
        }

        // Create toast element
        const toastId = 'toast-' + Date.now();
        const iconClass = {
            'success': 'bi-check-circle',
            'danger': 'bi-exclamation-triangle',
            'warning': 'bi-exclamation-triangle',
            'info': 'bi-info-circle'
        }[type] || 'bi-info-circle';

        const toastHtml = '<div id="' + toastId + '" class="toast align-items-center text-white bg-' + type + ' border-0" role="alert" aria-live="assertive" aria-atomic="true">' +
            '<div class="d-flex">' +
            '<div class="toast-body">' +
            '<i class="bi ' + iconClass + ' me-2"></i>' +
            '<strong>' + title + ':</strong> ' + message +
            '</div>' +
            '<button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>' +
            '</div>' +
            '</div>';

        // Add toast to container
        toastContainer.insertAdjacentHTML('beforeend', toastHtml);

        // Initialize and show toast
        const toastElement = document.getElementById(toastId);
        if (toastElement && typeof bootstrap !== 'undefined' && bootstrap.Toast) {
            const toast = new bootstrap.Toast(toastElement, {
                autohide: type !== 'danger', // Keep error toasts visible until manually closed
                delay: type === 'info' ? 3000 : 5000
            });
            toast.show();

            // Remove toast after hiding
            toastElement.addEventListener('hidden.bs.toast', function () {
                toastElement.remove();
            });
        }
    }

    // Helper function to get initials from product title
    function getInitials(title) {
        if (!title || title.length === 0) return 'P';

        const words = title.split(' ');
        if (words.length >= 2) {
            return (words[0][0] + words[1][0]).toUpperCase();
        } else {
            return title[0].toUpperCase();
        }
    }

    // Helper function to format date with time
    function formatDateTime(dateString) {
        if (!dateString) return '--';
        try {
            const date = new Date(dateString);
            if (isNaN(date.getTime())) return '--';

            return date.toLocaleDateString('en-US', {
                year: 'numeric',
                month: 'short',
                day: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });
        } catch (e) {
            console.error("Error formatting date time:", e);
            return '--';
        }
    }

    // View product button handler
    const viewProductBtn = document.getElementById('viewProductBtn');
    if (viewProductBtn) {
        viewProductBtn.addEventListener('click', function () {
            const productId = document.getElementById('editProductId')?.value;
            if (productId) {
                // Navigate to product details page
                window.location.href = `/Product/Details/${productId}`;
            }
        });
    }

    // AJAX form submission for the inventory form
    const editInventoryForm = document.getElementById('editInventoryForm');
    if (editInventoryForm) {
        editInventoryForm.addEventListener('submit', function (e) {
            e.preventDefault();

            // Get form data
            const formData = new FormData(this);
            const inventoryData = {};

            // Convert FormData to object
            for (const [key, value] of formData.entries()) {
                inventoryData[key] = value;
            }

            // Check if the updateInventory API URL exists
            if (!window.appUrls?.updateInventory) {
                console.error("Missing API URL for updateInventory");
                showToast('Error', 'API configuration is missing. Cannot save inventory data.', 'danger');
                return;
            }

            // Get anti-forgery token
            const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
            if (!tokenElement) {
                console.error("Anti-forgery token not found");
                showToast('Error', 'Security token is missing. Cannot save inventory data.', 'danger');
                return;
            }

            // Send AJAX request
            fetch(window.appUrls.updateInventory, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': tokenElement.value
                },
                body: JSON.stringify(inventoryData)
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        // Show success message
                        showToast('Success', data.message || 'Inventory updated successfully');

                        // Update metadata
                        const metadataSection = document.getElementById('inventoryMetadata');
                        if (metadataSection) {
                            metadataSection.style.display = 'flex';
                            safeUpdateElement('inventoryLastUpdated', formatDateTime(new Date()));
                        }
                    } else {
                        // Show error message
                        showToast('Error', data.message || 'Failed to update inventory', 'danger');
                    }
                })
                .catch(error => {
                    console.error('Error saving inventory data:', error);
                    showToast('Error', 'Failed to save inventory data. Please try again.', 'danger');
                });
        });
    }

    // Add form validation for the add product form
    const addProductForm = document.getElementById('addProductForm');
    if (addProductForm) {
        addProductForm.addEventListener('submit', function (e) {
            const priceField = document.getElementById('addPrice');
            const pointsField = document.getElementById('addPoints');
            let hasErrors = false;

            if (priceField) {
                const price = parseFloat(priceField.value);
                if (isNaN(price) || price < 0) {
                    e.preventDefault();
                    showToast('Error', 'Price must be a valid decimal number (0.00 or greater).', 'danger');
                    hasErrors = true;
                } else {
                    priceField.value = price.toFixed(2);
                }
            }

            if (pointsField && pointsField.value) {
                const points = parseInt(pointsField.value, 10);
                if (isNaN(points) || points < 0) {
                    e.preventDefault();
                    showToast('Error', 'Points must be a valid whole number (0 or greater).', 'danger');
                    hasErrors = true;
                } else {
                    pointsField.value = points.toString();
                }
            }

            if (hasErrors) {
                return;
            }

            // Show loading indicator
            const submitBtn = this.querySelector('button[type="submit"]');
            if (submitBtn) {
                const originalText = submitBtn.innerHTML;
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Adding...';
            }

            // Form will be submitted normally after validation
        });
    }

    // Add validation for edit product form
    const editProductForm = document.getElementById('editProductForm');
    if (editProductForm) {
        editProductForm.addEventListener('submit', function (e) {
            // Show loading indicator
            const submitBtn = this.querySelector('button[type="submit"]');
            if (submitBtn) {
                const originalText = submitBtn.innerHTML;
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Saving...';
            }

            // Form will be submitted normally after validation
        });
    }

    // Handle error cases where API URLs are not defined
    if (!window.appUrls) {
        console.error('API URLs not defined. Product management functionality may not work properly.');
        showToast('Warning', 'API configuration is missing. Some features may not work correctly.', 'warning');
    }
});