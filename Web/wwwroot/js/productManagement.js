/**
 * Enhanced Product Management with Comprehensive Debugging
 * This version includes extensive logging and error handling to identify the issue
 */

document.addEventListener('DOMContentLoaded', function () {
    console.log('🚀 Initializing Enhanced Product Management with Debug');

    // Enhanced debugging
    window.productDebug = {
        lastError: null,
        lastRequest: null,
        lastResponse: null,
        formData: null,
        testAddProduct: testAddProduct,
        checkFormValidation: checkFormValidation,
        checkAuthentication: checkAuthentication,
        checkAntiForgeryToken: checkAntiForgeryToken
    };

    // Initialize components
    initializeDataTable();
    initializeModals();
    initializeFilters();
    initializeForms();
    initializeAddProductModal();

    console.log('✅ Enhanced Product Management with Debug loaded successfully');

    // ========== FORM INITIALIZATION WITH DEBUG ==========
    function initializeForms() {
        console.log('📝 Initializing form handlers with debug...');

        // Add Product Form with enhanced debugging
        const addProductForm = document.getElementById('addProductForm');
        if (addProductForm) {
            console.log('✅ Found add product form:', addProductForm);

            addProductForm.addEventListener('submit', function (e) {
                console.log('🎯 Add product form submit event triggered');
                handleAddFormSubmitDebug(e);
            });

            console.log('✅ Add form handler attached with debug');
        } else {
            console.error('❌ Add product form not found!');
        }

        // Edit Product Form  
        const editProductForm = document.getElementById('editProductForm');
        if (editProductForm) {
            editProductForm.addEventListener('submit', handleEditFormSubmit);
            console.log('✅ Edit form handler attached');
        }
    }

    function handleAddFormSubmitDebug(e) {
        e.preventDefault();
        console.log('📤 Add form submitted - starting debug process');

        try {
            // Step 1: Check if UIUtils is available
            if (typeof window.UIUtils === 'undefined') {
                console.error('❌ UIUtils not available!');
                alert('Error: UIUtils not loaded. Check if utilities.js is included.');
                return false;
            }
            console.log('✅ UIUtils available');

            // Step 2: Check authentication
            const authCheck = checkAuthentication();
            if (!authCheck.valid) {
                console.error('❌ Authentication check failed:', authCheck.message);
                window.UIUtils.showError(authCheck.message);
                return false;
            }
            console.log('✅ Authentication check passed');

            // Step 3: Check anti-forgery token
            const tokenCheck = checkAntiForgeryToken();
            if (!tokenCheck.valid) {
                console.error('❌ Anti-forgery token check failed:', tokenCheck.message);
                window.UIUtils.showError(tokenCheck.message);
                return false;
            }
            console.log('✅ Anti-forgery token check passed:', tokenCheck.token.substring(0, 20) + '...');

            // Step 4: Validate form
            const validationResult = checkFormValidation();
            if (!validationResult.valid) {
                console.error('❌ Form validation failed:', validationResult.errors);
                window.UIUtils.showError('Please fix the following errors:\n• ' + validationResult.errors.join('\n• '));
                return false;
            }
            console.log('✅ Form validation passed');

            // Step 5: Submit form
            submitAddFormDebug(e.target);

        } catch (error) {
            console.error('💥 Error in handleAddFormSubmitDebug:', error);
            window.productDebug.lastError = error;

            if (window.UIUtils) {
                window.UIUtils.showError(`Unexpected error: ${error.message}`);
            } else {
                alert(`Unexpected error: ${error.message}`);
            }
        }
    }

    function submitAddFormDebug(form) {
        console.log('🚀 Starting submitAddFormDebug');

        try {
            // Get form data
            const formData = new FormData(form);
            window.productDebug.formData = formData;

            // Log form data for debugging
            console.log('📋 Form data contents:');
            for (let [key, value] of formData.entries()) {
                if (value instanceof File) {
                    console.log(`  ${key}: File(${value.name}, ${value.size} bytes, ${value.type})`);
                } else {
                    console.log(`  ${key}: "${value}"`);
                }
            }

            const submitBtn = form.querySelector('button[type="submit"]');
            if (!submitBtn) {
                throw new Error('Submit button not found');
            }

            // Set button loading state
            console.log('⏳ Setting button loading state');
            window.UIUtils.setButtonLoading(submitBtn, true, 'Adding Product...');

            // Get anti-forgery token
            const token = getAntiForgeryToken();
            console.log('🔑 Using anti-forgery token:', token.substring(0, 20) + '...');

            // Prepare request
            const requestConfig = {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': token,
                    'X-Requested-With': 'XMLHttpRequest' // Help server identify as AJAX
                },
                body: formData
            };

            console.log('📡 Making request to /Product/Create');
            console.log('📡 Request config:', {
                method: requestConfig.method,
                headers: requestConfig.headers,
                bodyType: 'FormData'
            });

            window.productDebug.lastRequest = {
                url: '/Product/Create',
                config: requestConfig,
                timestamp: new Date().toISOString()
            };

            // Make the request with detailed logging
            fetch('/Product/Create', requestConfig)
                .then(response => {
                    console.log('📨 Response received:', {
                        status: response.status,
                        statusText: response.statusText,
                        headers: Object.fromEntries(response.headers.entries()),
                        ok: response.ok,
                        redirected: response.redirected,
                        type: response.type,
                        url: response.url
                    });

                    window.productDebug.lastResponse = {
                        status: response.status,
                        statusText: response.statusText,
                        ok: response.ok,
                        timestamp: new Date().toISOString()
                    };

                    if (!response.ok) {
                        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                    }

                    // Check content type
                    const contentType = response.headers.get('content-type');
                    console.log('📄 Response content type:', contentType);

                    if (!contentType || !contentType.includes('application/json')) {
                        console.warn('⚠️ Response is not JSON, content type:', contentType);

                        // Try to get response as text for debugging
                        return response.text().then(text => {
                            console.log('📄 Response text:', text.substring(0, 500));
                            throw new Error('Server returned non-JSON response. Check server logs.');
                        });
                    }

                    return response.json();
                })
                .then(result => {
                    console.log('📦 Parsed JSON result:', result);
                    window.productDebug.lastResponse.data = result;

                    // Reset button state
                    window.UIUtils.setButtonLoading(submitBtn, false);

                    if (result && result.success) {
                        console.log('✅ Product created successfully');
                        window.UIUtils.showSuccess(result.message || 'Product created successfully!');

                        // Clear form
                        clearAddForm();

                        // Close modal after short delay
                        setTimeout(() => {
                            const modal = bootstrap.Modal.getInstance(document.getElementById('addProductModal'));
                            if (modal) {
                                modal.hide();
                                console.log('🚪 Modal closed');
                            }

                            // Refresh page to show new product
                            console.log('🔄 Refreshing page');
                            location.reload();
                        }, 1500);
                    } else {
                        console.error('❌ Server returned failure:', result);

                        let errorMessage = 'Failed to create product';
                        if (result && result.message) {
                            errorMessage = result.message;
                        } else if (result && result.errors) {
                            errorMessage = Object.values(result.errors).flat().join(', ');
                        }

                        window.UIUtils.showError(errorMessage);

                        // Handle specific error cases
                        if (result && result.requiresLogin) {
                            console.log('🔐 Login required, redirecting...');
                            setTimeout(() => {
                                window.location.href = '/Home/Index#login';
                            }, 2000);
                        }
                    }
                })
                .catch(error => {
                    console.error('💥 Error in fetch chain:', error);
                    window.productDebug.lastError = error;

                    // Reset button state
                    window.UIUtils.setButtonLoading(submitBtn, false);

                    // Show user-friendly error
                    let errorMessage = `Error creating product: ${error.message}`;

                    if (error.message.includes('Failed to fetch')) {
                        errorMessage = 'Network error. Please check your connection and try again.';
                    } else if (error.message.includes('non-JSON response')) {
                        errorMessage = 'Server error. Please try again or contact support.';
                    }

                    window.UIUtils.showError(errorMessage);
                });

        } catch (error) {
            console.error('💥 Error in submitAddFormDebug:', error);
            window.productDebug.lastError = error;

            const submitBtn = form.querySelector('button[type="submit"]');
            if (submitBtn) {
                window.UIUtils.setButtonLoading(submitBtn, false);
            }

            window.UIUtils.showError(`Error preparing request: ${error.message}`);
        }
    }

    // ========== DEBUG HELPER FUNCTIONS ==========
    function checkAuthentication() {
        // This is a client-side check - the real auth check happens on server
        // But we can verify basic session indicators if available

        console.log('🔐 Checking authentication...');

        // Check if there are any indicators in the page that user is logged in
        const userIndicators = [
            document.querySelector('[data-user-role]'),
            document.querySelector('.user-name'),
            document.querySelector('.logout-btn'),
            document.querySelector('#userDropdown')
        ];

        const hasUserIndicators = userIndicators.some(indicator => indicator !== null);

        if (!hasUserIndicators) {
            console.warn('⚠️ No user session indicators found on page');
        }

        return {
            valid: true, // We'll let the server handle the real auth check
            message: 'Client-side auth check passed',
            hasUserIndicators
        };
    }

    function checkAntiForgeryToken() {
        console.log('🔑 Checking anti-forgery token...');

        const token = getAntiForgeryToken();

        if (!token) {
            return {
                valid: false,
                message: 'Anti-forgery token not found. Please refresh the page.',
                token: null
            };
        }

        if (token.length < 10) {
            return {
                valid: false,
                message: 'Anti-forgery token appears invalid. Please refresh the page.',
                token: token
            };
        }

        return {
            valid: true,
            message: 'Anti-forgery token found',
            token: token
        };
    }

    function checkFormValidation() {
        console.log('✅ Checking form validation...');

        const errors = [];

        // Check required fields
        const title = document.getElementById('addTitle')?.value?.trim();
        const price = document.getElementById('addPrice')?.value;
        const type = document.getElementById('addType')?.value;
        const category = document.getElementById('addCategory')?.value;

        if (!title) {
            errors.push('Product title is required');
        } else if (title.length > 100) {
            errors.push('Product title cannot exceed 100 characters');
        }

        if (!price || isNaN(parseFloat(price)) || parseFloat(price) < 0) {
            errors.push('Valid price is required');
        }

        if (!type) {
            errors.push('Product type is required');
        }

        if (!category) {
            errors.push('Product category is required');
        }

        // Check points if provided
        const points = document.getElementById('addPoints')?.value;
        if (points && (isNaN(parseInt(points)) || parseInt(points) < 0)) {
            errors.push('Points must be a non-negative number');
        }

        console.log('📋 Validation results:', {
            title: title,
            price: price,
            type: type,
            category: category,
            points: points,
            errors: errors
        });

        return {
            valid: errors.length === 0,
            errors: errors
        };
    }

    function testAddProduct() {
        console.log('🧪 Testing add product functionality');

        // Fill form with test data
        const testData = {
            title: 'Test Product ' + Date.now(),
            description: 'This is a test product',
            price: '29.99',
            points: '100',
            type: 'Equipment',
            category: 'Equipment'
        };

        document.getElementById('addTitle').value = testData.title;
        document.getElementById('addDescription').value = testData.description;
        document.getElementById('addPrice').value = testData.price;
        document.getElementById('addPoints').value = testData.points;
        document.getElementById('addType').value = testData.type;
        document.getElementById('addCategory').value = testData.category;

        console.log('✅ Test data filled');

        // Open modal
        const addModal = new bootstrap.Modal(document.getElementById('addProductModal'));
        addModal.show();

        console.log('✅ Test modal opened. You can now click "Add Product" to test.');
    }

    // ========== UTILITY FUNCTIONS ==========
    function getAntiForgeryToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        if (tokenInput) {
            return tokenInput.value;
        }

        // Also check in meta tags (some implementations put it there)
        const tokenMeta = document.querySelector('meta[name="__RequestVerificationToken"]');
        if (tokenMeta) {
            return tokenMeta.getAttribute('content');
        }

        return '';
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

            // Clear image preview if exists
            const previewImg = document.getElementById('addPreviewImage');
            const placeholder = document.getElementById('addImagePlaceholder');

            if (previewImg && placeholder) {
                previewImg.style.display = 'none';
                placeholder.style.display = 'flex';
            }
        }
    }

    // ========== PLACEHOLDER FUNCTIONS FOR MISSING FUNCTIONALITY ==========
    function initializeDataTable() {
        console.log('📊 DataTable initialization placeholder');
    }

    function initializeModals() {
        console.log('📝 Modal initialization placeholder');
    }

    function initializeFilters() {
        console.log('🔍 Filter initialization placeholder');
    }

    function initializeAddProductModal() {
        console.log('🎯 Add product modal initialization placeholder');
    }

    function handleEditFormSubmit(e) {
        console.log('📝 Edit form submit placeholder');
    }

    console.log('🎯 Enhanced Product Management with Debug loaded');
    console.log('🧪 Debug functions available: window.productDebug');
    console.log('🧪 Try: window.productDebug.testAddProduct()');
});