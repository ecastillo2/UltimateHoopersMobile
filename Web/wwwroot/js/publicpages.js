/*
 * Ultimate Hoopers - Public Pages JavaScript
 * Consolidated scripts for home, blog, and public-facing pages
 * Version: 1.0.0
 * Last Updated: 2025
 */

(function () {
    'use strict';

    // =================================================
    // CONFIGURATION & CONSTANTS
    // =================================================

    const CONFIG = {
        ANIMATION_DURATION: 300,
        SCROLL_OFFSET: 100,
        DEBOUNCE_DELAY: 300,
        BASKETBALL_ROTATION_SPEED: 30,
        BASKETBALL_ROTATION_INTERVAL: 1000,
        TOAST_DURATION: 5000,
        ANALYTICS_SCROLL_THRESHOLDS: [25, 50, 75, 90],
        EMAIL_REGEX: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
        LAZY_LOAD_ROOT_MARGIN: '50px',
        CRITICAL_IMAGES: [
            '/image/mobilescreenshots.png'
        ]
    };

    // Global state
    let maxScrollDepth = 0;
    let isInitialized = false;
    let basketballAnimationIntervals = [];
    let toastContainer = null;

    // =================================================
    // UTILITY FUNCTIONS
    // =================================================

    /**
     * Debounce function to limit function calls
     * @param {Function} func - Function to debounce
     * @param {number} wait - Wait time in milliseconds
     * @returns {Function} Debounced function
     */
    function debounce(func, wait = CONFIG.DEBOUNCE_DELAY) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func.apply(this, args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    /**
     * Throttle function to limit function calls
     * @param {Function} func - Function to throttle
     * @param {number} limit - Time limit in milliseconds
     * @returns {Function} Throttled function
     */
    function throttle(func, limit) {
        let inThrottle;
        return function () {
            const args = arguments;
            const context = this;
            if (!inThrottle) {
                func.apply(context, args);
                inThrottle = true;
                setTimeout(() => inThrottle = false, limit);
            }
        };
    }

    /**
     * Get element safely
     * @param {string} selector - CSS selector
     * @param {Element} parent - Parent element (optional)
     * @returns {Element|null} Element or null
     */
    function $(selector, parent = document) {
        return parent.querySelector(selector);
    }

    /**
     * Get elements safely
     * @param {string} selector - CSS selector
     * @param {Element} parent - Parent element (optional)
     * @returns {NodeList} NodeList of elements
     */
    function $$(selector, parent = document) {
        return parent.querySelectorAll(selector);
    }

    /**
     * Add event listener with error handling
     * @param {Element} element - Target element
     * @param {string} event - Event name
     * @param {Function} handler - Event handler
     * @param {boolean|object} options - Event options
     */
    function addEventListenerSafe(element, event, handler, options = false) {
        if (!element) return;

        const safeHandler = function (e) {
            try {
                handler.call(this, e);
            } catch (error) {
                console.error(`Error in ${event} handler:`, error);
                trackError('event_handler_error', error, { event, selector: element.tagName });
            }
        };

        element.addEventListener(event, safeHandler, options);
    }

    /**
     * Check if element is in viewport
     * @param {Element} element - Element to check
     * @returns {boolean} True if in viewport
     */
    function isInViewport(element) {
        if (!element) return false;
        const rect = element.getBoundingClientRect();
        return (
            rect.top >= 0 &&
            rect.left >= 0 &&
            rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) &&
            rect.right <= (window.innerWidth || document.documentElement.clientWidth)
        );
    }

    /**
     * Validate email address
     * @param {string} email - Email to validate
     * @returns {boolean} True if valid
     */
    function isValidEmail(email) {
        return CONFIG.EMAIL_REGEX.test(email);
    }

    /**
     * Generate unique ID
     * @returns {string} Unique ID
     */
    function generateId() {
        return 'uh-' + Math.random().toString(36).substr(2, 9);
    }

    // =================================================
    // ERROR HANDLING & ANALYTICS
    // =================================================

    /**
     * Track error for analytics
     * @param {string} type - Error type
     * @param {Error} error - Error object
     * @param {object} context - Additional context
     */
    function trackError(type, error, context = {}) {
        const errorData = {
            type,
            message: error.message || error,
            stack: error.stack,
            timestamp: new Date().toISOString(),
            url: window.location.href,
            userAgent: navigator.userAgent,
            ...context
        };

        // Log to console in development
        if (window.location.hostname === 'localhost' || window.location.hostname.includes('dev')) {
            console.error('Tracked Error:', errorData);
        }

        // Send to analytics service if available
        if (typeof gtag !== 'undefined') {
            gtag('event', 'exception', {
                description: errorData.message,
                fatal: false,
                custom_map: {
                    error_type: type,
                    error_context: JSON.stringify(context)
                }
            });
        }

        // You could also send to a custom error tracking service here
        // Example: sendToErrorTrackingService(errorData);
    }

    /**
     * Track user interaction events
     * @param {string} action - Action name
     * @param {object} data - Event data
     */
    function trackEvent(action, data = {}) {
        const eventData = {
            action,
            timestamp: new Date().toISOString(),
            page: window.location.pathname,
            ...data
        };

        // Log to console in development
        if (window.location.hostname === 'localhost') {
            console.log('Tracked Event:', eventData);
        }

        // Send to Google Analytics if available
        if (typeof gtag !== 'undefined') {
            gtag('event', action, {
                event_category: data.category || 'user_interaction',
                event_label: data.label || '',
                value: data.value || 0,
                custom_map: data.custom || {}
            });
        }

        // Send to custom analytics if available
        if (window.UltimateHoopersAnalytics) {
            window.UltimateHoopersAnalytics.track(eventData);
        }
    }

    // =================================================
    // TOAST NOTIFICATION SYSTEM
    // =================================================

    /**
     * Create toast container if it doesn't exist
     */
    function createToastContainer() {
        if (toastContainer) return toastContainer;

        toastContainer = document.createElement('div');
        toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
        toastContainer.style.zIndex = '9999';
        document.body.appendChild(toastContainer);

        return toastContainer;
    }

    /**
     * Show toast notification
     * @param {string} message - Message to show
     * @param {string} type - Toast type (success, error, info, warning)
     * @param {number} duration - Duration in milliseconds
     */
    function showToast(message, type = 'info', duration = CONFIG.TOAST_DURATION) {
        const container = createToastContainer();
        const toastId = generateId();

        const typeClasses = {
            success: 'text-bg-success',
            error: 'text-bg-danger',
            warning: 'text-bg-warning',
            info: 'text-bg-info'
        };

        const icons = {
            success: '✓',
            error: '✕',
            warning: '⚠',
            info: 'ℹ'
        };

        const toastHtml = `
            <div id="${toastId}" class="toast align-items-center ${typeClasses[type] || typeClasses.info} border-0" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body">
                        <span class="me-2">${icons[type] || icons.info}</span>
                        ${message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>
        `;

        container.insertAdjacentHTML('beforeend', toastHtml);

        const toastElement = $(`#${toastId}`);

        // Initialize Bootstrap toast
        let toast;
        if (window.bootstrap && window.bootstrap.Toast) {
            toast = new window.bootstrap.Toast(toastElement, {
                autohide: true,
                delay: duration
            });
            toast.show();
        } else {
            // Fallback if Bootstrap is not available
            toastElement.classList.add('show');
            setTimeout(() => {
                toastElement.remove();
            }, duration);
        }

        // Clean up after toast is hidden
        addEventListenerSafe(toastElement, 'hidden.bs.toast', () => {
            toastElement.remove();
        });

        // Track toast shown
        trackEvent('toast_shown', {
            category: 'ui_feedback',
            label: type,
            message: message.substring(0, 50)
        });
    }

    // =================================================
    // NAVIGATION & MODAL HANDLING
    // =================================================

    /**
     * Initialize navigation functionality
     */
    function initNavigation() {
        // Handle URL hash for login modal
        handleLoginModal();

        // Initialize responsive navigation
        initResponsiveNavigation();

        // Initialize smooth scrolling
        initSmoothScrolling();
    }

    /**
     * Handle login modal based on URL parameters
     */
    function handleLoginModal() {
        const showModal = () => {
            const loginModal = $('#loginModal');
            if (loginModal && window.bootstrap && window.bootstrap.Modal) {
                const modal = new window.bootstrap.Modal(loginModal);
                modal.show();
                trackEvent('login_modal_opened', { category: 'authentication', trigger: 'url_parameter' });
            }
        };

        // Check URL hash
        if (window.location.hash === '#login') {
            showModal();
        }

        // Check URL parameters
        const urlParams = new URLSearchParams(window.location.search);
        if (urlParams.get('scrollTo') === 'login') {
            showModal();
        }

        // Handle login button clicks
        $$('[data-bs-target="#loginModal"]').forEach(button => {
            addEventListenerSafe(button, 'click', () => {
                trackEvent('login_modal_opened', { category: 'authentication', trigger: 'button_click' });
            });
        });
    }

    /**
     * Initialize responsive navigation
     */
    function initResponsiveNavigation() {
        const navbarToggler = $('.navbar-toggler');
        const navbarCollapse = $('.navbar-collapse');

        if (!navbarToggler || !navbarCollapse) return;

        // Close mobile menu when clicking on a link
        const navLinks = $$('.nav-link', navbarCollapse);
        navLinks.forEach(link => {
            addEventListenerSafe(link, 'click', () => {
                if (window.innerWidth < 992) { // Bootstrap's lg breakpoint
                    if (window.bootstrap && window.bootstrap.Collapse) {
                        const bsCollapse = new window.bootstrap.Collapse(navbarCollapse, {
                            toggle: false
                        });
                        bsCollapse.hide();
                    }
                }

                // Track navigation
                trackEvent('nav_link_clicked', {
                    category: 'navigation',
                    label: link.textContent.trim(),
                    href: link.href
                });
            });
        });

        // Track mobile menu toggle
        addEventListenerSafe(navbarToggler, 'click', () => {
            trackEvent('mobile_menu_toggled', { category: 'navigation' });
        });
    }

    /**
     * Initialize smooth scrolling
     */
    function initSmoothScrolling() {
        // General smooth scrolling for anchor links
        $$('a[href^="#"]').forEach(anchor => {
            addEventListenerSafe(anchor, 'click', function (e) {
                const href = this.getAttribute('href');
                if (href === '#' || href === '#!') return;

                e.preventDefault();

                const targetElement = $(href);
                if (targetElement) {
                    const offsetTop = targetElement.offsetTop - CONFIG.SCROLL_OFFSET;

                    window.scrollTo({
                        top: offsetTop,
                        behavior: 'smooth'
                    });

                    // Update URL without causing scroll
                    if (history.pushState) {
                        history.pushState(null, null, href);
                    }

                    trackEvent('smooth_scroll', {
                        category: 'navigation',
                        label: href,
                        target_element: targetElement.tagName
                    });
                }
            });
        });

        // Table of contents smooth scrolling (for blog pages)
        $$('.table-of-contents a[href^="#"]').forEach(anchor => {
            addEventListenerSafe(anchor, 'click', function (e) {
                e.preventDefault();
                const target = $(this.getAttribute('href'));
                if (target) {
                    target.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });

                    trackEvent('toc_link_clicked', {
                        category: 'blog_navigation',
                        label: this.textContent.trim()
                    });
                }
            });
        });
    }

    // =================================================
    // ANIMATIONS & EFFECTS
    // =================================================

    /**
     * Initialize basketball icon animations
     */
    function initBasketballAnimation() {
        const basketballIcons = $$('.basketball-icon');

        basketballIcons.forEach(icon => {
            let rotation = 0;
            const interval = setInterval(() => {
                rotation += CONFIG.BASKETBALL_ROTATION_SPEED;
                icon.style.transform = `rotate(${rotation}deg)`;
            }, CONFIG.BASKETBALL_ROTATION_INTERVAL);

            basketballAnimationIntervals.push(interval);
        });

        if (basketballIcons.length > 0) {
            trackEvent('basketball_animation_started', {
                category: 'animation',
                count: basketballIcons.length
            });
        }
    }

    /**
     * Initialize hover effects for post cards
     */
    function initHoverEffects() {
        // Post card hover effects
        $$('.post-card').forEach(card => {
            addEventListenerSafe(card, 'mouseenter', function () {
                this.style.transform = 'translateY(-5px)';
                this.style.transition = 'transform 0.3s ease';
            });

            addEventListenerSafe(card, 'mouseleave', function () {
                this.style.transform = 'translateY(0)';
            });

            // Track post card interactions
            addEventListenerSafe(card, 'click', function () {
                const title = $('.card-title', this)?.textContent.trim();
                trackEvent('post_card_clicked', {
                    category: 'blog_interaction',
                    label: title || 'unknown',
                    card_type: 'post'
                });
            });
        });

        // Feature card hover effects
        $$('.feature-card').forEach(card => {
            addEventListenerSafe(card, 'mouseenter', function () {
                this.style.transform = 'translateY(-10px)';
                this.style.transition = 'all 0.3s ease';
            });

            addEventListenerSafe(card, 'mouseleave', function () {
                this.style.transform = 'translateY(0)';
            });
        });

        // App badge hover effects
        $$('.app-badge').forEach(badge => {
            addEventListenerSafe(badge, 'mouseenter', function () {
                this.style.transform = 'translateY(-5px)';
                this.style.transition = 'all 0.3s ease';
            });

            addEventListenerSafe(badge, 'mouseleave', function () {
                this.style.transform = 'translateY(0)';
            });

            addEventListenerSafe(badge, 'click', function () {
                trackEvent('app_download_clicked', {
                    category: 'conversion',
                    label: this.textContent.includes('App Store') ? 'ios' : 'android'
                });
            });
        });
    }

    /**
     * Initialize intersection observer for animations
     */
    function initScrollAnimations() {
        if (!window.IntersectionObserver) return;

        const animateOnScroll = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('animate-in');

                    // Track element coming into view
                    trackEvent('element_viewed', {
                        category: 'scroll_interaction',
                        label: entry.target.className.split(' ')[0],
                        threshold: entry.intersectionRatio
                    });
                }
            });
        }, {
            threshold: 0.1,
            rootMargin: '50px'
        });

        // Observe elements that should animate on scroll
        $$('.feature-card, .testimonial-card, .coach-card, .rule-card').forEach(el => {
            animateOnScroll.observe(el);
        });
    }

    // =================================================
    // SEARCH FUNCTIONALITY
    // =================================================

    /**
     * Initialize blog search functionality
     */
    function initBlogSearch() {
        const searchBoxes = $$('.search-box');

        searchBoxes.forEach(searchBox => {
            // Handle enter key
            addEventListenerSafe(searchBox, 'keypress', function (e) {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    const searchTerm = this.value.trim();
                    if (searchTerm) {
                        performBlogSearch(searchTerm);
                        trackEvent('blog_search_performed', {
                            category: 'search',
                            label: searchTerm,
                            search_length: searchTerm.length
                        });
                    }
                }
            });

            // Handle input changes with debouncing for live search
            const debouncedSearch = debounce((searchTerm) => {
                if (searchTerm.length >= 2) {
                    performBlogSearch(searchTerm, true);
                } else if (searchTerm.length === 0) {
                    clearSearchResults();
                }
            }, 500);

            addEventListenerSafe(searchBox, 'input', function () {
                debouncedSearch(this.value.trim());
            });

            // Handle search button clicks
            const searchButton = searchBox.nextElementSibling;
            if (searchButton && searchButton.classList.contains('btn')) {
                addEventListenerSafe(searchButton, 'click', function () {
                    const searchTerm = searchBox.value.trim();
                    if (searchTerm) {
                        performBlogSearch(searchTerm);
                        trackEvent('blog_search_button_clicked', {
                            category: 'search',
                            label: searchTerm
                        });
                    }
                });
            }
        });
    }

    /**
     * Perform blog search
     * @param {string} searchTerm - Search term
     * @param {boolean} isLive - Is this a live search
     */
    function performBlogSearch(searchTerm, isLive = false) {
        const searchTermLower = searchTerm.toLowerCase();
        const posts = $$('.post-card');
        let matchCount = 0;

        posts.forEach(post => {
            const title = $('.card-title', post)?.textContent.toLowerCase() || '';
            const content = $('.card-text', post)?.textContent.toLowerCase() || '';
            const tags = Array.from($$('.tag', post)).map(tag => tag.textContent.toLowerCase());
            const author = $('.post-meta', post)?.textContent.toLowerCase() || '';

            const isMatch = title.includes(searchTermLower) ||
                content.includes(searchTermLower) ||
                tags.some(tag => tag.includes(searchTermLower)) ||
                author.includes(searchTermLower);

            const postContainer = post.closest('.col-md-6, .col-lg-4, .col-12');

            if (isMatch) {
                if (postContainer) {
                    postContainer.style.display = 'block';
                    postContainer.style.order = matchCount; // Show matches first
                }
                post.classList.remove('search-hidden');
                post.classList.add('search-match');
                matchCount++;
            } else {
                if (postContainer) {
                    postContainer.style.display = 'none';
                }
                post.classList.add('search-hidden');
                post.classList.remove('search-match');
            }
        });

        // Show search results summary
        showSearchResults(searchTerm, matchCount, isLive);
    }

    /**
     * Show search results summary
     * @param {string} searchTerm - Search term
     * @param {number} matchCount - Number of matches
     * @param {boolean} isLive - Is this a live search
     */
    function showSearchResults(searchTerm, matchCount, isLive) {
        let resultsContainer = $('.search-results-summary');

        if (!resultsContainer) {
            resultsContainer = document.createElement('div');
            resultsContainer.className = 'search-results-summary alert alert-info mt-3';

            const firstPostContainer = $('.post-card')?.closest('.row');
            if (firstPostContainer) {
                firstPostContainer.parentNode.insertBefore(resultsContainer, firstPostContainer);
            }
        }

        if (matchCount === 0) {
            resultsContainer.innerHTML = `
                <i class="bi bi-search me-2"></i>
                No results found for "<strong>${searchTerm}</strong>". 
                <button class="btn btn-sm btn-outline-primary ms-2" onclick="UltimateHoopers.clearSearch()">Clear Search</button>
            `;
            resultsContainer.className = 'search-results-summary alert alert-warning mt-3';
        } else {
            resultsContainer.innerHTML = `
                <i class="bi bi-search me-2"></i>
                Found <strong>${matchCount}</strong> ${matchCount === 1 ? 'result' : 'results'} for "<strong>${searchTerm}</strong>". 
                <button class="btn btn-sm btn-outline-primary ms-2" onclick="UltimateHoopers.clearSearch()">Clear Search</button>
            `;
            resultsContainer.className = 'search-results-summary alert alert-info mt-3';
        }

        if (!isLive) {
            showToast(`Found ${matchCount} ${matchCount === 1 ? 'result' : 'results'} for "${searchTerm}"`, 'info');
        }
    }

    /**
     * Clear search results
     */
    function clearSearchResults() {
        const posts = $$('.post-card');
        posts.forEach(post => {
            const postContainer = post.closest('.col-md-6, .col-lg-4, .col-12');
            if (postContainer) {
                postContainer.style.display = 'block';
                postContainer.style.order = '';
            }
            post.classList.remove('search-hidden', 'search-match');
        });

        const resultsContainer = $('.search-results-summary');
        if (resultsContainer) {
            resultsContainer.remove();
        }

        // Clear search boxes
        $$('.search-box').forEach(box => {
            box.value = '';
        });

        trackEvent('search_cleared', { category: 'search' });
    }

    // =================================================
    // FORM HANDLING
    // =================================================

    /**
     * Initialize newsletter signup forms
     */
    function initNewsletterForms() {
        const forms = $$('form');

        forms.forEach(form => {
            // Check if it's a newsletter form (has email input but not in login modal)
            const emailInput = $('input[type="email"]', form);
            if (!emailInput || form.closest('#loginModal')) return;

            addEventListenerSafe(form, 'submit', function (e) {
                e.preventDefault();

                const email = emailInput.value.trim();
                const submitBtn = $('button[type="submit"]', this);

                if (!email) {
                    showToast('Please enter your email address.', 'warning');
                    emailInput.focus();
                    return;
                }

                if (!isValidEmail(email)) {
                    showToast('Please enter a valid email address.', 'error');
                    emailInput.focus();
                    emailInput.classList.add('is-invalid');
                    return;
                }

                emailInput.classList.remove('is-invalid');
                handleNewsletterSignup(email, this, submitBtn);
            });

            // Real-time email validation
            if (emailInput) {
                addEventListenerSafe(emailInput, 'blur', function () {
                    const email = this.value.trim();
                    if (email && !isValidEmail(email)) {
                        this.classList.add('is-invalid');
                        showToast('Please enter a valid email address.', 'warning');
                    } else {
                        this.classList.remove('is-invalid');
                    }
                });

                addEventListenerSafe(emailInput, 'input', function () {
                    this.classList.remove('is-invalid');
                });
            }
        });
    }

    /**
     * Handle newsletter signup
     * @param {string} email - Email address
     * @param {Element} form - Form element
     * @param {Element} submitBtn - Submit button
     */
    function handleNewsletterSignup(email, form, submitBtn) {
        // Show loading state
        const originalText = submitBtn.innerHTML;
        submitBtn.innerHTML = '<i class="spinner-border spinner-border-sm me-2"></i>Subscribing...';
        submitBtn.disabled = true;

        // Track signup attempt
        trackEvent('newsletter_signup_attempted', {
            category: 'conversion',
            label: 'newsletter',
            email_domain: email.split('@')[1]
        });

        // Simulate API call (replace with actual API call)
        setTimeout(() => {
            // Reset button
            submitBtn.innerHTML = originalText;
            submitBtn.disabled = false;

            // Clear form
            form.reset();

            // Show success message
            showToast('🎉 Thank you for subscribing! Check your email to confirm.', 'success');

            // Track successful signup
            trackEvent('newsletter_signup_completed', {
                category: 'conversion',
                label: 'newsletter',
                email_domain: email.split('@')[1]
            });

            // Add some visual feedback
            const emailInput = $('input[type="email"]', form);
            if (emailInput) {
                emailInput.classList.add('is-valid');
                setTimeout(() => {
                    emailInput.classList.remove('is-valid');
                }, 3000);
            }

        }, 1500);
    }

    // =================================================
    // SHARING FUNCTIONALITY
    // =================================================

    /**
     * Initialize share buttons
     */
    function initShareButtons() {
        const shareButtons = $$('.share-btn');

        shareButtons.forEach(button => {
            addEventListenerSafe(button, 'click', function (e) {
                e.preventDefault();

                const icon = $('i', this);
                if (!icon) return;

                const currentUrl = window.location.href;
                const title = document.title;
                const description = $('meta[name="description"]')?.getAttribute('content') || '';

                if (icon.classList.contains('bi-twitter')) {
                    shareOnTwitter(currentUrl, title);
                } else if (icon.classList.contains('bi-facebook')) {
                    shareOnFacebook(currentUrl);
                } else if (icon.classList.contains('bi-linkedin')) {
                    shareOnLinkedIn(currentUrl, title, description);
                } else if (icon.classList.contains('bi-link-45deg')) {
                    copyLinkToClipboard(currentUrl, icon);
                } else if (icon.classList.contains('bi-envelope')) {
                    shareByEmail(currentUrl, title, description);
                }
            });
        });
    }

    /**
     * Share on Twitter
     * @param {string} url - URL to share
     * @param {string} title - Title to share
     */
    function shareOnTwitter(url, title) {
        const text = `${title} - Ultimate Hoopers`;
        const hashtags = 'Basketball,UltimateHoopers,Sports';
        const twitterUrl = `https://twitter.com/intent/tweet?url=${encodeURIComponent(url)}&text=${encodeURIComponent(text)}&hashtags=${hashtags}`;

        openShareWindow(twitterUrl, 'Twitter Share');
        trackEvent('content_shared', { category: 'social_share', label: 'twitter', url });
    }

    /**
     * Share on Facebook
     * @param {string} url - URL to share
     */
    function shareOnFacebook(url) {
        const facebookUrl = `https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(url)}`;

        openShareWindow(facebookUrl, 'Facebook Share');
        trackEvent('content_shared', { category: 'social_share', label: 'facebook', url });
    }

    /**
     * Share on LinkedIn
     * @param {string} url - URL to share
     * @param {string} title - Title to share
     * @param {string} description - Description to share
     */
    function shareOnLinkedIn(url, title, description) {
        const linkedInUrl = `https://www.linkedin.com/sharing/share-offsite/?url=${encodeURIComponent(url)}&title=${encodeURIComponent(title)}&summary=${encodeURIComponent(description)}`;

        openShareWindow(linkedInUrl, 'LinkedIn Share');
        trackEvent('content_shared', { category: 'social_share', label: 'linkedin', url });
    }

    /**
     * Share by email
     * @param {string} url - URL to share
     * @param {string} title - Title to share
     * @param {string} description - Description to share
     */
    function shareByEmail(url, title, description) {
        const subject = `Check out: ${title}`;
        const body = `I thought you might be interested in this article:\n\n${title}\n\n${description}\n\nRead more: ${url}\n\n- Shared from Ultimate Hoopers`;
        const emailUrl = `mailto:?subject=${encodeURIComponent(subject)}&body=${encodeURIComponent(body)}`;

        window.location.href = emailUrl;
        trackEvent('content_shared', { category: 'social_share', label: 'email', url });
    }

    /**
     * Open share window
     * @param {string} url - URL to open
     * @param {string} title - Window title
     */
    function openShareWindow(url, title) {
        const width = 600;
        const height = 400;
        const left = (window.innerWidth - width) / 2;
        const top = (window.innerHeight - height) / 2;

        window.open(
            url,
            title,
            `width=${width},height=${height},left=${left},top=${top},resizable=yes,scrollbars=yes`
        );
    }

    /**
     * Copy link to clipboard
     * @param {string} url - URL to copy
     * @param {Element} iconElement - Icon element to update
     */
    function copyLinkToClipboard(url, iconElement) {
        if (navigator.clipboard && window.isSecureContext) {
            navigator.clipboard.writeText(url).then(() => {
                showCopySuccess(iconElement);
            }).catch(() => {
                fallbackCopyTextToClipboard(url, iconElement);
            });
        } else {
            fallbackCopyTextToClipboard(url, iconElement);
        }

        trackEvent('content_shared', { category: 'social_share', label: 'copy_link', url });
    }

    /**
     * Fallback copy to clipboard
     * @param {string} text - Text to copy
     * @param {Element} iconElement - Icon element to update
     */
    function fallbackCopyTextToClipboard(text, iconElement) {
        const textArea = document.createElement('textarea');
        textArea.value = text;
        textArea.style.top = '0';
        textArea.style.left = '0';
        textArea.style.position = 'fixed';
        textArea.style.opacity = '0';

        document.body.appendChild(textArea);
        textArea.focus();
        textArea.select();

        try {
            document.execCommand('copy');
            showCopySuccess(iconElement);
        } catch (err) {
            console.error('Fallback: Unable to copy', err);
            showToast('Unable to copy link. Please copy the URL from your browser.', 'error');
        }

        document.body.removeChild(textArea);
    }

    /**
     * Show copy success feedback
     * @param {Element} iconElement - Icon element to update
     */
    function showCopySuccess(iconElement) {
        const originalClass = iconElement.className;
        iconElement.className = 'bi bi-check';
        showToast('Link copied to clipboard! 📋', 'success');

        setTimeout(() => {
            iconElement.className = originalClass;
        }, 2000);
    }

    // =================================================
    // PERFORMANCE OPTIMIZATIONS
    // =================================================

    /**
     * Initialize lazy loading for images
     */
    function initLazyLoading() {
        if (!window.IntersectionObserver) {
            // Fallback for browsers without IntersectionObserver
            $$('img[data-src]').forEach(img => {
                img.src = img.dataset.src;
                img.removeAttribute('data-src');
            });
            return;
        }

        const imageObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    if (img.dataset.src) {
                        img.src = img.dataset.src;
                        img.removeAttribute('data-src');
                        img.classList.add('loaded');

                        // Add fade in animation
                        img.style.opacity = '0';
                        img.style.transition = 'opacity 0.3s ease';

                        img.onload = () => {
                            img.style.opacity = '1';
                        };

                        imageObserver.unobserve(img);

                        trackEvent('image_lazy_loaded', {
                            category: 'performance',
                            label: img.alt || img.src.split('/').pop()
                        });
                    }
                }
            });
        }, {
            rootMargin: CONFIG.LAZY_LOAD_ROOT_MARGIN
        });

        $$('img[data-src]').forEach(img => {
            imageObserver.observe(img);
        });
    }

    /**
     * Preload critical images
     */
    function preloadCriticalImages() {
        CONFIG.CRITICAL_IMAGES.forEach(src => {
            const link = document.createElement('link');
            link.rel = 'preload';
            link.as = 'image';
            link.href = src;
            document.head.appendChild(link);
        });

        trackEvent('critical_images_preloaded', {
            category: 'performance',
            count: CONFIG.CRITICAL_IMAGES.length
        });
    }

    /**
     * Initialize performance monitoring
     */
    function initPerformanceMonitoring() {
        // Monitor page load performance
        if (window.performance && window.performance.timing) {
            const timing = window.performance.timing;
            const loadTime = timing.loadEventEnd - timing.navigationStart;
            const domReady = timing.domContentLoadedEventEnd - timing.navigationStart;

            trackEvent('page_performance', {
                category: 'performance',
                load_time: loadTime,
                dom_ready: domReady,
                page: window.location.pathname
            });
        }

        // Monitor largest contentful paint
        if (window.PerformanceObserver) {
            try {
                const observer = new PerformanceObserver((list) => {
                    const entries = list.getEntries();
                    const lastEntry = entries[entries.length - 1];

                    trackEvent('largest_contentful_paint', {
                        category: 'performance',
                        value: Math.round(lastEntry.startTime),
                        page: window.location.pathname
                    });
                });

                observer.observe({ entryTypes: ['largest-contentful-paint'] });
            } catch (e) {
                // Ignore if not supported
            }
        }
    }

    // =================================================
    // SCROLL TRACKING & ANALYTICS
    // =================================================

    /**
     * Initialize scroll tracking
     */
    function initScrollTracking() {
        const trackScrollDepth = throttle(() => {
            const scrollPercent = Math.round(
                (window.scrollY / (document.body.scrollHeight - window.innerHeight)) * 100
            );

            if (scrollPercent > maxScrollDepth) {
                maxScrollDepth = scrollPercent;

                CONFIG.ANALYTICS_SCROLL_THRESHOLDS.forEach(threshold => {
                    if (maxScrollDepth >= threshold && maxScrollDepth < threshold + 5) {
                        trackEvent('scroll_depth', {
                            category: 'engagement',
                            label: `${threshold}%`,
                            value: threshold,
                            page: window.location.pathname
                        });
                    }
                });
            }
        }, 1000);

        addEventListenerSafe(window, 'scroll', trackScrollDepth);
    }

    /**
     * Initialize user engagement tracking
     */
    function initEngagementTracking() {
        let startTime = Date.now();
        let isActive = true;
        let totalActiveTime = 0;

        // Track time on page
        const trackTimeOnPage = () => {
            if (isActive) {
                totalActiveTime += Date.now() - startTime;
            }
            startTime = Date.now();
        };

        // Track when user becomes inactive
        addEventListenerSafe(document, 'visibilitychange', () => {
            if (document.hidden) {
                trackTimeOnPage();
                isActive = false;
            } else {
                isActive = true;
                startTime = Date.now();
            }
        });

        // Track when user leaves page
        addEventListenerSafe(window, 'beforeunload', () => {
            trackTimeOnPage();

            trackEvent('page_exit', {
                category: 'engagement',
                total_time: Math.round(totalActiveTime / 1000),
                max_scroll_depth: maxScrollDepth,
                page: window.location.pathname
            });
        });

        // Track clicks on important elements
        addEventListenerSafe(document, 'click', (e) => {
            const target = e.target.closest('.btn, .card, .nav-link, .tag');
            if (!target) return;

            let elementType = 'unknown';
            let elementText = target.textContent?.trim().substring(0, 50) || '';

            if (target.classList.contains('btn')) {
                elementType = 'button';
            } else if (target.classList.contains('card')) {
                elementType = 'card';
            } else if (target.classList.contains('nav-link')) {
                elementType = 'navigation';
            } else if (target.classList.contains('tag')) {
                elementType = 'tag';
            }

            trackEvent('element_clicked', {
                category: 'interaction',
                label: elementType,
                text: elementText,
                page: window.location.pathname
            });
        });
    }

    // =================================================
    // ACCESSIBILITY ENHANCEMENTS
    // =================================================

    /**
     * Initialize accessibility enhancements
     */
    function initAccessibility() {
        // Add skip to content link
        addSkipToContentLink();

        // Enhance keyboard navigation
        enhanceKeyboardNavigation();

        // Add ARIA labels where needed
        addAriaLabels();

        // Manage focus for modals
        manageFocusForModals();
    }

    /**
     * Add skip to content link
     */
    function addSkipToContentLink() {
        if ($('.skip-to-content')) return; // Already exists

        const skipLink = document.createElement('a');
        skipLink.href = '#main-content';
        skipLink.textContent = 'Skip to main content';
        skipLink.className = 'skip-to-content visually-hidden-focusable';
        skipLink.style.cssText = `
            position: absolute;
            top: 0;
            left: 0;
            z-index: 10000;
            padding: 8px 16px;
            background: #000;
            color: #fff;
            text-decoration: none;
            transform: translateY(-100%);
            transition: transform 0.3s;
        `;

        addEventListenerSafe(skipLink, 'focus', function () {
            this.style.transform = 'translateY(0)';
        });

        addEventListenerSafe(skipLink, 'blur', function () {
            this.style.transform = 'translateY(-100%)';
        });

        document.body.insertBefore(skipLink, document.body.firstChild);

        // Add main content landmark if it doesn't exist
        const main = $('main');
        if (main && !main.id) {
            main.id = 'main-content';
        }
    }

    /**
     * Enhance keyboard navigation
     */
    function enhanceKeyboardNavigation() {
        // Handle escape key for modals
        addEventListenerSafe(document, 'keydown', (e) => {
            if (e.key === 'Escape') {
                const openModal = $('.modal.show');
                if (openModal && window.bootstrap && window.bootstrap.Modal) {
                    const modal = window.bootstrap.Modal.getInstance(openModal);
                    if (modal) {
                        modal.hide();
                    }
                }
            }
        });

        // Make card elements keyboard accessible
        $$('.post-card, .feature-card, .testimonial-card').forEach(card => {
            if (!card.hasAttribute('tabindex')) {
                card.setAttribute('tabindex', '0');
            }

            if (!card.hasAttribute('role')) {
                card.setAttribute('role', 'button');
            }

            addEventListenerSafe(card, 'keydown', (e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    card.click();
                }
            });
        });
    }

    /**
     * Add ARIA labels where needed
     */
    function addAriaLabels() {
        // Add labels to search boxes
        $$('.search-box').forEach(searchBox => {
            if (!searchBox.hasAttribute('aria-label')) {
                searchBox.setAttribute('aria-label', 'Search blog articles');
            }
        });

        // Add labels to social sharing buttons
        $$('.share-btn').forEach(button => {
            const icon = $('i', button);
            if (icon && !button.hasAttribute('aria-label')) {
                let label = 'Share';
                if (icon.classList.contains('bi-twitter')) label = 'Share on Twitter';
                else if (icon.classList.contains('bi-facebook')) label = 'Share on Facebook';
                else if (icon.classList.contains('bi-linkedin')) label = 'Share on LinkedIn';
                else if (icon.classList.contains('bi-link-45deg')) label = 'Copy link';

                button.setAttribute('aria-label', label);
            }
        });

        // Add labels to newsletter forms
        $$('form input[type="email"]').forEach(input => {
            if (!input.hasAttribute('aria-label') && !input.hasAttribute('aria-labelledby')) {
                input.setAttribute('aria-label', 'Email address for newsletter subscription');
            }
        });
    }

    /**
     * Manage focus for modals
     */
    function manageFocusForModals() {
        const loginModal = $('#loginModal');
        if (!loginModal) return;

        addEventListenerSafe(loginModal, 'shown.bs.modal', () => {
            const firstInput = $('input', loginModal);
            if (firstInput) {
                firstInput.focus();
            }
        });
    }

    // =================================================
    // INITIALIZATION & CLEANUP
    // =================================================

    /**
     * Main initialization function
     */
    function init() {
        if (isInitialized) {
            console.warn('UltimateHoopers: Already initialized');
            return;
        }

        try {
            // Core functionality
            initNavigation();
            initBasketballAnimation();
            initHoverEffects();
            initScrollAnimations();

            // Search and forms
            initBlogSearch();
            initNewsletterForms();

            // Sharing and social
            initShareButtons();

            // Performance optimizations
            initLazyLoading();
            preloadCriticalImages();
            initPerformanceMonitoring();

            // Analytics and tracking
            initScrollTracking();
            initEngagementTracking();

            // Accessibility
            initAccessibility();

            isInitialized = true;

            trackEvent('app_initialized', {
                category: 'system',
                page: window.location.pathname,
                user_agent: navigator.userAgent,
                screen_resolution: `${screen.width}x${screen.height}`,
                viewport_size: `${window.innerWidth}x${window.innerHeight}`
            });

            console.log('🏀 Ultimate Hoopers: All scripts initialized successfully');

        } catch (error) {
            console.error('🚨 Ultimate Hoopers: Initialization error:', error);
            trackError('initialization_error', error);
        }
    }

    /**
     * Cleanup function
     */
    function cleanup() {
        // Clear basketball animation intervals
        basketballAnimationIntervals.forEach(interval => {
            clearInterval(interval);
        });
        basketballAnimationIntervals = [];

        // Remove toast container
        if (toastContainer && toastContainer.parentNode) {
            toastContainer.parentNode.removeChild(toastContainer);
            toastContainer = null;
        }

        isInitialized = false;

        console.log('🏀 Ultimate Hoopers: Cleanup completed');
    }

    // =================================================
    // EXTERNAL API & INTEGRATIONS
    // =================================================

    /**
     * Initialize external services
     */
    function initExternalServices() {
        // Google Analytics 4
        if (typeof gtag !== 'undefined') {
            gtag('config', 'GA_MEASUREMENT_ID', {
                page_title: document.title,
                page_location: window.location.href,
                custom_map: {
                    scroll_depth: 'max_scroll_depth'
                }
            });
        }

        // Facebook Pixel (if available)
        if (typeof fbq !== 'undefined') {
            fbq('track', 'PageView');
        }

        // Custom analytics service
        if (window.UltimateHoopersAnalytics && typeof window.UltimateHoopersAnalytics.init === 'function') {
            window.UltimateHoopersAnalytics.init({
                page: window.location.pathname,
                timestamp: new Date().toISOString()
            });
        }

        trackEvent('external_services_initialized', { category: 'system' });
    }

    // =================================================
    // ERROR HANDLERS
    // =================================================

    // Global error handler
    addEventListenerSafe(window, 'error', (event) => {
        trackError('javascript_error', {
            message: event.message,
            filename: event.filename,
            lineno: event.lineno,
            colno: event.colno,
            stack: event.error?.stack
        });
    });

    // Unhandled promise rejection handler
    addEventListenerSafe(window, 'unhandledrejection', (event) => {
        trackError('unhandled_promise_rejection', {
            message: event.reason?.message || event.reason,
            stack: event.reason?.stack
        });
    });

    // =================================================
    // DOM READY INITIALIZATION
    // =================================================

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        addEventListenerSafe(document, 'DOMContentLoaded', init);
    } else {
        // DOM is already loaded
        init();
    }

    // Initialize external services after a short delay
    setTimeout(initExternalServices, 1000);

    // Cleanup on page unload
    addEventListenerSafe(window, 'beforeunload', cleanup);

    // =================================================
    // PUBLIC API
    // =================================================

    // Expose public API
    window.UltimateHoopers = {
        // Core functions
        showToast: showToast,
        performBlogSearch: performBlogSearch,
        clearSearch: clearSearchResults,
        copyLinkToClipboard: copyLinkToClipboard,

        // Utility functions
        trackEvent: trackEvent,
        trackError: trackError,

        // State getters
        isInitialized: () => isInitialized,
        getMaxScrollDepth: () => maxScrollDepth,

        // Advanced functions
        reinitialize: () => {
            cleanup();
            setTimeout(init, 100);
        },

        // Configuration
        config: CONFIG,

        // Version
        version: '1.0.0'
    };

    // Make utility functions available globally for debugging
    if (window.location.hostname === 'localhost' || window.location.hostname.includes('dev')) {
        window.UH_DEBUG = {
            $: $,
            $$: $$,
            debounce: debounce,
            throttle: throttle,
            isInViewport: isInViewport,
            basketballAnimationIntervals: basketballAnimationIntervals
        };
    }

})();