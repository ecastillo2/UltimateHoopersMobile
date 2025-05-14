// site.js - Custom JavaScript for Your App Website

// Wait for the document to be fully loaded
document.addEventListener('DOMContentLoaded', function () {
    // Add shadow to navbar when scrolling
    const navbar = document.querySelector('.navbar');
    if (navbar) {
        window.addEventListener('scroll', function () {
            if (window.scrollY > 10) {
                navbar.classList.add('shadow-sm');
            } else {
                navbar.classList.remove('shadow-sm');
            }
        });
    }

    // Enable tooltips everywhere
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize animation on scroll (simple implementation)
    const animatedElements = document.querySelectorAll('.fade-in-up');

    function checkInView() {
        animatedElements.forEach(element => {
            const elementTop = element.getBoundingClientRect().top;
            const elementBottom = element.getBoundingClientRect().bottom;
            const isVisible = (elementTop < window.innerHeight) && (elementBottom > 0);

            if (isVisible) {
                element.classList.add('animated');
            }
        });
    }

    // Run check on scroll and initial page load
    window.addEventListener('scroll', checkInView);
    checkInView();

    // Smooth scrolling for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();

            const targetId = this.getAttribute('href');
            if (targetId === '#') return;

            const targetElement = document.querySelector(targetId);
            if (targetElement) {
                targetElement.scrollIntoView({
                    behavior: 'smooth'
                });
            }
        });
    });

    // Newsletter form validation and submission
    const newsletterForm = document.querySelector('.newsletter-form');
    if (newsletterForm) {
        newsletterForm.addEventListener('submit', function (e) {
            e.preventDefault();

            const emailInput = this.querySelector('input[type="email"]');
            const email = emailInput.value.trim();

            // Simple validation
            if (!email || !email.includes('@') || !email.includes('.')) {
                // Show error
                emailInput.classList.add('is-invalid');
                return;
            }

            emailInput.classList.remove('is-invalid');

            // Here you would normally send an AJAX request to your backend
            // For demo purposes, we'll just show a success message
            this.innerHTML = '<div class="alert alert-success">Thank you for subscribing!</div>';
        });
    }

    // Add a class to animate app screenshot on hover
    const appScreenshot = document.querySelector('.promo-image img');
    if (appScreenshot) {
        appScreenshot.addEventListener('mouseenter', function () {
            this.classList.add('hover-effect');
        });

        appScreenshot.addEventListener('mouseleave', function () {
            this.classList.remove('hover-effect');
        });
    }
});