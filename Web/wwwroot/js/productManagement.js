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
    const editProductModal = document.getElementById('editProductModal');
    if (editProductModal) {
        editProductModal.addEventListener('show.bs.modal', function (event) {
            const button = event.relatedTarget;
            const productId = button.getAttribute('data-product-id');

            // Set product ID in the product details form - check if elements exist
            const editProductIdField = document.getElementById('editProductId');
            if (editProductIdField) {
                editProductIdField.value = productId;
            }

            // Set product ID in the inventory form - check if element exists
            const inventoryProductIdField = document.getElementById('inventoryProductId');
            if (inventoryProductIdField) {
                inventoryProductIdField.value = productId;
            }

            // Load product data
            loadProductData(productId);

            // Load product info for the product info tab
            loadProductInfo(productId);

            // Load inventory data
            loadInventoryData(productId);

            // Set delete button product ID - check if element exists
            const deleteProductIdField = document.getElementById('deleteProductId');
            if (deleteProductIdField) {
                deleteProductIdField.value = productId;
            }
        });
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
        if (!productId || !window.appUrls?.getProductData) {
            console.error("Missing productId or API URL for loadProductData");
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
                    // Populate form fields - check if each element exists before setting values
                    safeSetValue('editTitle', data.Title);
                    safeSetValue('editDescription', data.Description);
                    safeSetValue('editPrice', data.Price);
                    safeSetValue('editPoints', data.Points);
                    safeSetValue('editProductNumber', data.ProductNumber);
                    safeSetValue('editTag', data.Tag);
                    safeSetValue('editImageURL', data.ImageURL);

                    // Select values - check if elements exist
                    safeSetSelect('editType', data.Type);
                    safeSetSelect('editCategory', data.Category);
                    safeSetSelect('editStatus', data.Status || 'Active');
                } else {
                    console.error('Error loading product data:', data.message);
                    showToast('Error', 'Failed to load product data. Please try again.', 'danger');

                    // Fallback to using row data when API fails
                    const row = findProductRowById(productId);
                    if (row) {
                        populateProductFormFromRow(row);
                    }
                }
            })
            .catch(error => {
                console.error('Error fetching product data:', error);
                showToast('Error', 'Failed to load product data. Please try again.', 'danger');

                // Fallback to using row data when API fails
                const row = findProductRowById(productId);
                if (row) {
                    populateProductFormFromRow(row);
                }
            });
    }

    // Helper function to safely set input value if element exists
    function safeSetValue(elementId, value) {
        const element = document.getElementById(elementId);
        if (element) {
            element.value = value || '';
        }
    }

    // Helper function to safely set select option if element exists
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

        const productInfo = row.querySelector('.d-flex.align-items-center');
        if (!productInfo) return;

        const titleEl = productInfo.querySelector('.fw-semibold');
        const productNumberEl = productInfo.querySelector('.text-muted.small');

        if (titleEl) {
            safeSetValue('editTitle', titleEl.textContent.trim());
        }

        if (productNumberEl) {
            safeSetValue('editProductNumber', productNumberEl.textContent.trim());
        }

        // Set price from the price column (column 1)
        if (row.cells && row.cells.length > 1) {
            const priceText = row.cells[1].textContent.trim().replace('$', '');
            safeSetValue('editPrice', priceText);
        }

        // Set points from the points column (column 2)
        if (row.cells && row.cells.length > 2) {
            const points = row.cells[2].textContent.trim();
            safeSetValue('editPoints', points);
        }

        // Set status from the status badge (column 3)
        if (row.cells && row.cells.length > 3) {
            const statusBadge = row.cells[3].querySelector('.badge');
            const status = statusBadge ? statusBadge.textContent.trim() : 'Active';
            safeSetSelect('editStatus', status);
        }

        // Set category from the category column (column 4)
        if (row.cells && row.cells.length > 4) {
            const category = row.cells[4].textContent.trim();
            safeSetSelect('editCategory', category);
        }
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
        safeUpdateElement('productInfoPrice', product.Price ? `$${parseFloat(product.Price).toFixed(2)}` : '--');
        safeUpdateElement('productInfoPoints', product.Points || '--');
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
            document.body.appendChild(toastContainer);
        }

        // Create toast element
        const toastId = 'toast-' + Date.now();
        const toastHtml = `
            <div id="${toastId}" class="toast align-items-center text-white bg-${type} border-0" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body">
                        <strong>${title}</strong>: ${message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>
        `;

        // Add toast to container
        toastContainer.insertAdjacentHTML('beforeend', toastHtml);

        // Initialize and show toast
        const toastElement = document.getElementById(toastId);
        if (toastElement && typeof bootstrap !== 'undefined' && bootstrap.Toast) {
            const toast = new bootstrap.Toast(toastElement, { autohide: true, delay: 5000 });
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
            if (!priceField) return;

            const price = parseFloat(priceField.value);

            // Simple price validation
            if (price < 0) {
                e.preventDefault();
                showToast('Error', 'Price cannot be negative.', 'danger');
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