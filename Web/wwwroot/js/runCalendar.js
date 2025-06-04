/**
 * Complete Enhanced Run Calendar Implementation - FIXED VERSION
 * Enhanced calendar view for displaying basketball runs with improved functionality
 * Integrates seamlessly with existing run management system
 * 
 * FIXES:
 * - Fixed variable declaration conflict with 'endDate' in generateMockRuns function
 * - Renamed inner loop endDate to runEndDate to avoid conflict
 */

class EnhancedRunCalendar {
    constructor() {
        this.currentDate = new Date();
        this.currentMonth = this.currentDate.getMonth();
        this.currentYear = this.currentDate.getFullYear();
        this.selectedDate = null;
        this.runs = [];
        this.filteredRuns = [];
        this.tooltip = document.getElementById('runTooltip');
        this.isLoading = false;
        this.cache = new Map();

        // Enhanced calendar configuration
        this.config = {
            maxRunsPerDay: 3,
            dateRangeMonths: 3, // Increased for better data coverage
            animationDuration: 300,
            fetchTimeout: 30000,
            refreshInterval: 5 * 60 * 1000, // 5 minutes
            cacheTimeout: 2 * 60 * 1000, // 2 minutes cache
            enableKeyboardNavigation: true,
            enableTooltips: true,
            enableAnimations: true,
            responsive: {
                mobile: 576,
                tablet: 768,
                desktop: 992
            }
        };

        // Bind context for event handlers
        this.handleKeyboardNavigation = this.handleKeyboardNavigation.bind(this);
        this.handleResize = this.handleResize.bind(this);

        this.init();
    }

    init() {
        this.bindEvents();
        this.setupAutoRefresh();
        this.initializeAccessibility();
        console.log('🗓️ Enhanced Run Calendar initialized successfully');
    }

    bindEvents() {
        // Navigation buttons
        this.bindNavigationEvents();

        // Modal events
        this.bindModalEvents();

        // Global events
        this.bindGlobalEvents();

        // Touch events for mobile
        this.bindTouchEvents();
    }

    bindNavigationEvents() {
        const prevBtn = document.getElementById('prevMonth');
        const nextBtn = document.getElementById('nextMonth');
        const todayBtn = document.getElementById('todayBtn');
        const refreshBtn = document.getElementById('refreshCalendar');

        if (prevBtn) {
            prevBtn.addEventListener('click', () => this.previousMonth());
        }

        if (nextBtn) {
            nextBtn.addEventListener('click', () => this.nextMonth());
        }

        if (todayBtn) {
            todayBtn.addEventListener('click', () => this.goToToday());
        }

        if (refreshBtn) {
            refreshBtn.addEventListener('click', () => this.loadCalendar(true));
        }
    }

    bindModalEvents() {
        const modal = document.getElementById('runCalendarModal');
        if (modal) {
            modal.addEventListener('shown.bs.modal', () => {
                this.loadCalendar();
                this.announceToScreenReader('Calendar loaded');
            });

            modal.addEventListener('hidden.bs.modal', () => {
                this.hideTooltip();
                this.clearCache();
            });
        }
    }

    bindGlobalEvents() {
        // Keyboard navigation
        if (this.config.enableKeyboardNavigation) {
            document.addEventListener('keydown', this.handleKeyboardNavigation);
        }

        // Window resize handler for responsive adjustments
        let resizeTimeout;
        window.addEventListener('resize', () => {
            clearTimeout(resizeTimeout);
            resizeTimeout = setTimeout(this.handleResize, 250);
        });

        // Page visibility change (pause/resume auto-refresh)
        document.addEventListener('visibilitychange', () => {
            if (document.hidden) {
                this.pauseAutoRefresh();
            } else {
                this.resumeAutoRefresh();
            }
        });
    }

    bindTouchEvents() {
        // Add touch support for mobile devices
        const calendarGrid = document.getElementById('calendarGrid');
        if (calendarGrid && 'ontouchstart' in window) {
            let touchStartX = null;
            let touchStartY = null;

            calendarGrid.addEventListener('touchstart', (e) => {
                touchStartX = e.touches[0].clientX;
                touchStartY = e.touches[0].clientY;
            });

            calendarGrid.addEventListener('touchend', (e) => {
                if (!touchStartX || !touchStartY) return;

                const touchEndX = e.changedTouches[0].clientX;
                const touchEndY = e.changedTouches[0].clientY;

                const deltaX = touchStartX - touchEndX;
                const deltaY = touchStartY - touchEndY;

                // Swipe detection (horizontal swipes for month navigation)
                if (Math.abs(deltaX) > Math.abs(deltaY) && Math.abs(deltaX) > 50) {
                    if (deltaX > 0) {
                        this.nextMonth(); // Swipe left = next month
                    } else {
                        this.previousMonth(); // Swipe right = previous month
                    }
                    e.preventDefault();
                }

                touchStartX = null;
                touchStartY = null;
            });
        }
    }

    handleKeyboardNavigation(e) {
        const modal = document.getElementById('runCalendarModal');
        if (!modal || !modal.classList.contains('show')) return;

        const activeElement = document.activeElement;
        const isInputFocused = activeElement && ['INPUT', 'TEXTAREA', 'SELECT'].includes(activeElement.tagName);

        if (isInputFocused) return; // Don't intercept if user is typing

        switch (e.key) {
            case 'ArrowLeft':
                e.preventDefault();
                this.previousMonth();
                this.announceToScreenReader('Previous month');
                break;
            case 'ArrowRight':
                e.preventDefault();
                this.nextMonth();
                this.announceToScreenReader('Next month');
                break;
            case 'Home':
                e.preventDefault();
                this.goToToday();
                this.announceToScreenReader('Current month');
                break;
            case 'r':
            case 'R':
                if (e.ctrlKey || e.metaKey) {
                    e.preventDefault();
                    this.loadCalendar(true);
                    this.announceToScreenReader('Calendar refreshed');
                }
                break;
            case 'Escape':
                this.hideTooltip();
                break;
            case 'f':
            case 'F':
                if (e.ctrlKey || e.metaKey) {
                    e.preventDefault();
                    this.focusSearchFilter();
                }
                break;
        }
    }

    handleResize() {
        if (this.isCalendarVisible()) {
            this.adjustResponsiveLayout();
            this.repositionTooltip();
        }
    }

    setupAutoRefresh() {
        this.autoRefreshInterval = setInterval(() => {
            if (this.isCalendarVisible() && !this.isLoading && !document.hidden) {
                this.loadCalendar(false, false); // Silent refresh
            }
        }, this.config.refreshInterval);
    }

    pauseAutoRefresh() {
        if (this.autoRefreshInterval) {
            clearInterval(this.autoRefreshInterval);
            this.autoRefreshInterval = null;
        }
    }

    resumeAutoRefresh() {
        if (!this.autoRefreshInterval) {
            this.setupAutoRefresh();
        }
    }

    initializeAccessibility() {
        // Add ARIA labels and live regions
        const calendarGrid = document.getElementById('calendarGrid');
        if (calendarGrid) {
            calendarGrid.setAttribute('role', 'grid');
            calendarGrid.setAttribute('aria-label', 'Basketball runs calendar');
        }

        // Create live region for announcements
        if (!document.getElementById('calendar-live-region')) {
            const liveRegion = document.createElement('div');
            liveRegion.id = 'calendar-live-region';
            liveRegion.setAttribute('aria-live', 'polite');
            liveRegion.setAttribute('aria-atomic', 'true');
            liveRegion.style.position = 'absolute';
            liveRegion.style.left = '-10000px';
            liveRegion.style.width = '1px';
            liveRegion.style.height = '1px';
            liveRegion.style.overflow = 'hidden';
            document.body.appendChild(liveRegion);
        }
    }

    announceToScreenReader(message) {
        const liveRegion = document.getElementById('calendar-live-region');
        if (liveRegion) {
            liveRegion.textContent = message;
        }
    }

    isCalendarVisible() {
        const modal = document.getElementById('runCalendarModal');
        return modal && modal.classList.contains('show');
    }

    async loadCalendar(forceRefresh = false, showLoading = true) {
        if (this.isLoading && !forceRefresh) {
            console.log('📅 Calendar already loading, skipping request');
            return;
        }

        this.isLoading = true;

        try {
            if (showLoading) {
                this.showLoading();
            }

            // Check cache first
            const cacheKey = `${this.currentYear}-${this.currentMonth}`;
            if (!forceRefresh && this.cache.has(cacheKey)) {
                const cachedData = this.cache.get(cacheKey);
                if (Date.now() - cachedData.timestamp < this.config.cacheTimeout) {
                    console.log('📅 Using cached calendar data');
                    this.runs = cachedData.runs;
                    this.renderCalendar();
                    this.updateStats();
                    this.isLoading = false;
                    return;
                }
            }

            await this.fetchRuns(forceRefresh);
            this.renderCalendar();
            this.updateStats();

            // Cache the data
            this.cache.set(cacheKey, {
                runs: [...this.runs],
                timestamp: Date.now()
            });

            if (!showLoading) {
                console.log('🔄 Calendar silently refreshed');
            } else {
                console.log('✅ Calendar loaded successfully');
            }
        } catch (error) {
            console.error('❌ Error loading calendar:', error);
            this.showError(`Failed to load calendar data: ${error.message}`);
        } finally {
            this.isLoading = false;
            if (showLoading) {
                this.hideLoading();
            }
        }
    }

    async fetchRuns(forceRefresh = false) {
        const startDate = new Date(this.currentYear, this.currentMonth - this.config.dateRangeMonths, 1);
        const endDate = new Date(this.currentYear, this.currentMonth + this.config.dateRangeMonths + 1, 0);

        try {
            console.log(`📡 Fetching runs from ${startDate.toISOString()} to ${endDate.toISOString()}`);

            const response = await this.fetchRunsFromAPI(startDate, endDate);
            this.runs = response;

            console.log(`📋 Loaded ${this.runs.length} runs from API`);
        } catch (error) {
            console.warn('⚠️ API fetch failed, using mock data:', error.message);

            // Show user-friendly error message
            if (error.message.includes('fetch')) {
                this.showError('Unable to connect to server. Showing sample data.');
            }

            // Fallback to mock data for demonstration
            await this.generateMockDataWithDelay();
        }
    }

    async fetchRunsFromAPI(startDate, endDate) {
        const url = new URL('/Run/GetRunsForCalendar', window.location.origin);
        url.searchParams.append('startDate', startDate.toISOString());
        url.searchParams.append('endDate', endDate.toISOString());

        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), this.config.fetchTimeout);

        try {
            const response = await fetch(url.toString(), {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': this.getAntiForgeryToken()
                },
                signal: controller.signal
            });

            clearTimeout(timeoutId);

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const data = await response.json();

            if (!data.success) {
                throw new Error(data.message || 'API returned error status');
            }

            // Transform API data to calendar format
            return this.transformAPIData(data.runs || []);
        } catch (error) {
            clearTimeout(timeoutId);

            if (error.name === 'AbortError') {
                throw new Error('Request timed out');
            }

            throw error;
        }
    }

    transformAPIData(apiRuns) {
        return apiRuns.map(run => {
            // Parse the date properly
            let runDate;
            if (run.runDate) {
                runDate = new Date(run.runDate);
                // If we have start time, try to combine it
                if (run.startTime && run.startTime !== '12:00 AM') {
                    const timeParts = this.parseTime(run.startTime);
                    if (timeParts) {
                        runDate.setHours(timeParts.hours, timeParts.minutes, 0, 0);
                    }
                }
            } else {
                runDate = new Date(); // Fallback to today
            }

            return {
                id: run.runId || `run-${Date.now()}-${Math.random()}`,
                name: run.name || 'Basketball Run',
                type: (run.type || 'Pickup').toLowerCase(),
                date: runDate,
                startTime: run.startTime || '12:00 PM',
                endTime: run.endTime || '2:00 PM',
                location: run.location || this.buildLocationString(run),
                skillLevel: run.skillLevel || 'All Levels',
                participants: parseInt(run.playerCount) || 0,
                capacity: parseInt(run.playerLimit) || 10,
                description: run.description || '',
                status: run.status || 'Active',
                isPublic: run.isPublic !== false,
                address: run.address || '',
                city: run.city || '',
                state: run.state || '',
                profileId: run.profileId || ''
            };
        }).filter(run => run.date && !isNaN(run.date.getTime())); // Filter out invalid dates
    }

    buildLocationString(run) {
        const parts = [];
        if (run.address) parts.push(run.address);
        if (run.city) parts.push(run.city);
        if (run.state && parts.length > 0) parts.push(run.state);

        return parts.length > 0 ? parts.join(', ') : 'Location TBD';
    }

    parseTime(timeString) {
        // Parse times like "2:30 PM", "14:30", "2:30:00 PM"
        const timeRegex = /(\d{1,2}):(\d{2})(?::(\d{2}))?\s*(AM|PM)?/i;
        const match = timeString.match(timeRegex);

        if (!match) return null;

        let hours = parseInt(match[1]);
        const minutes = parseInt(match[2]);
        const period = match[4];

        if (period) {
            // 12-hour format
            if (period.toUpperCase() === 'PM' && hours !== 12) {
                hours += 12;
            } else if (period.toUpperCase() === 'AM' && hours === 12) {
                hours = 0;
            }
        }

        return { hours, minutes };
    }

    async generateMockDataWithDelay() {
        // Add a small delay to simulate API call
        await new Promise(resolve => setTimeout(resolve, 600));
        this.runs = this.generateMockRuns();
    }

    // FIXED: Variable declaration conflict resolved
    generateMockRuns() {
        const runs = [];
        const startDate = new Date(this.currentYear, this.currentMonth - this.config.dateRangeMonths, 1);
        const endDate = new Date(this.currentYear, this.currentMonth + this.config.dateRangeMonths + 1, 0);

        const runTypes = ['pickup', 'training', 'tournament', 'youth', 'women'];
        const skillLevels = ['Beginner', 'Intermediate', 'Advanced', 'All Levels'];
        const locations = [
            'Main Basketball Court',
            'Practice Gym',
            'Community Recreation Center',
            'Youth Sports Complex',
            'Westside Basketball Courts',
            'Downtown Sports Center',
            'Local High School Gym',
            'University Recreation Center'
        ];

        // Generate 25-45 random runs for the period
        const runCount = Math.floor(Math.random() * 21) + 25;

        for (let i = 0; i < runCount; i++) {
            // Create random date within range
            const randomDate = new Date(startDate.getTime() + Math.random() * (endDate.getTime() - startDate.getTime()));

            // Set random time (6 AM to 10 PM)
            const randomHour = Math.floor(Math.random() * 16) + 6;
            const randomMinute = Math.floor(Math.random() * 4) * 15; // 0, 15, 30, 45
            randomDate.setHours(randomHour, randomMinute, 0, 0);

            const type = runTypes[Math.floor(Math.random() * runTypes.length)];
            const capacity = Math.floor(Math.random() * 20) + 5;
            const participants = Math.floor(Math.random() * (capacity + 1));

            const startTime = this.formatTime(randomDate);

            // FIXED: Renamed from 'endDate' to 'runEndDate' to avoid variable conflict
            const runEndDate = new Date(randomDate.getTime() + (1 + Math.random() * 2) * 60 * 60 * 1000); // 1-3 hours
            const endTime = this.formatTime(runEndDate);

            runs.push({
                id: `mock-run-${i + 1}`,
                name: `${this.capitalize(type)} ${this.getRandomGameName()}`,
                type: type,
                date: randomDate,
                startTime: startTime,
                endTime: endTime,
                location: locations[Math.floor(Math.random() * locations.length)],
                skillLevel: skillLevels[Math.floor(Math.random() * skillLevels.length)],
                participants: participants,
                capacity: capacity,
                description: this.generateRunDescription(type, skillLevels[Math.floor(Math.random() * skillLevels.length)]),
                status: Math.random() > 0.05 ? 'Active' : 'Cancelled', // 5% chance of cancelled
                isPublic: Math.random() > 0.15, // 85% public
                profileId: `user-${Math.floor(Math.random() * 50) + 1}`
            });
        }

        return runs.sort((a, b) => a.date - b.date);
    }

    getRandomGameName() {
        const names = [
            'Session', 'Game', 'Scrimmage', 'Workout', 'Practice', 'Meetup',
            'Challenge', 'Tournament', 'League Game', 'Open Run'
        ];
        return names[Math.floor(Math.random() * names.length)];
    }

    generateRunDescription(type, skillLevel) {
        const templates = {
            pickup: [
                `Open pickup game for ${skillLevel.toLowerCase()} players. Bring your A-game!`,
                `Casual basketball run. ${skillLevel} level preferred. First come, first served.`,
                `Good vibes and competitive play. Looking for ${skillLevel.toLowerCase()} players.`
            ],
            training: [
                `${skillLevel} training session focused on fundamentals and conditioning.`,
                `Skill development workout for ${skillLevel.toLowerCase()} players.`,
                `Intensive training session. ${skillLevel} level recommended.`
            ],
            tournament: [
                `${skillLevel} tournament bracket. Prizes for winners!`,
                `Competitive tournament play. ${skillLevel} level required.`,
                `3v3 tournament format. ${skillLevel} players welcome.`
            ],
            youth: [
                `Youth development program for ${skillLevel.toLowerCase()} young players.`,
                `Kids basketball session. ${skillLevel} level appropriate.`,
                `Fun and educational youth program. ${skillLevel} welcome.`
            ],
            women: [
                `Women's basketball session. ${skillLevel} level.`,
                `Ladies only run. ${skillLevel} players encouraged to join.`,
                `Supportive environment for women players. ${skillLevel} level.`
            ]
        };

        const typeTemplates = templates[type] || templates.pickup;
        return typeTemplates[Math.floor(Math.random() * typeTemplates.length)];
    }

    renderCalendar() {
        const grid = document.getElementById('calendarGrid');
        const title = document.getElementById('calendarTitle');

        if (!grid || !title) {
            console.error('❌ Calendar grid or title element not found');
            return;
        }

        // Update title with enhanced information
        const monthNames = [
            'January', 'February', 'March', 'April', 'May', 'June',
            'July', 'August', 'September', 'October', 'November', 'December'
        ];

        const currentMonthRuns = this.getRunsForMonth(this.currentMonth, this.currentYear);
        const monthTitle = `${monthNames[this.currentMonth]} ${this.currentYear}`;
        title.textContent = monthTitle;
        title.setAttribute('aria-label', `${monthTitle}, ${currentMonthRuns.length} runs this month`);

        // Clear existing calendar (keep headers)
        const headers = grid.querySelectorAll('.calendar-day-header');
        grid.innerHTML = '';
        headers.forEach(header => grid.appendChild(header));

        // Calculate calendar dates
        const firstDay = new Date(this.currentYear, this.currentMonth, 1);
        const startDate = new Date(firstDay);
        startDate.setDate(startDate.getDate() - firstDay.getDay()); // Start from Sunday

        const today = new Date();
        today.setHours(0, 0, 0, 0);

        // Create document fragment for better performance
        const fragment = document.createDocumentFragment();

        // Render 42 days (6 weeks)
        for (let i = 0; i < 42; i++) {
            const currentDate = new Date(startDate);
            currentDate.setDate(startDate.getDate() + i);

            const dayElement = this.createDayElement(currentDate, today);
            fragment.appendChild(dayElement);
        }

        grid.appendChild(fragment);

        // Add fade in animation
        if (this.config.enableAnimations) {
            grid.classList.remove('show');
            grid.classList.add('calendar-fade');

            // Use requestAnimationFrame for smooth animation
            requestAnimationFrame(() => {
                requestAnimationFrame(() => {
                    grid.classList.add('show');
                });
            });
        }

        // Adjust responsive layout
        this.adjustResponsiveLayout();

        // Announce to screen reader
        this.announceToScreenReader(`Calendar updated. ${currentMonthRuns.length} runs in ${monthTitle}`);
    }

    createDayElement(date, today) {
        const dayDiv = document.createElement('div');
        dayDiv.className = 'calendar-day';
        dayDiv.setAttribute('data-date', date.toISOString().split('T')[0]);

        // Add classes for different states
        if (date.getMonth() !== this.currentMonth) {
            dayDiv.classList.add('other-month');
        }

        if (date.toDateString() === today.toDateString()) {
            dayDiv.classList.add('today');
            const indicator = document.createElement('div');
            indicator.className = 'today-indicator';
            indicator.setAttribute('aria-label', 'Today');
            dayDiv.appendChild(indicator);
        }

        // Add weekend class
        if (date.getDay() === 0 || date.getDay() === 6) {
            dayDiv.classList.add('weekend');
        }

        // Day number
        const dayNumber = document.createElement('div');
        dayNumber.className = 'day-number';
        dayNumber.textContent = date.getDate();
        dayDiv.appendChild(dayNumber);

        // Runs for this day
        const dayRuns = this.getRunsForDate(date);
        if (dayRuns.length > 0) {
            const runsContainer = this.createRunsContainer(dayRuns, date);
            dayDiv.appendChild(runsContainer);
        }

        // Enhanced click handler for day selection
        dayDiv.addEventListener('click', (e) => {
            if (!e.target.closest('.run-item') && !e.target.closest('.more-runs')) {
                this.selectDate(date, dayRuns);
            }
        });

        // Enhanced accessibility
        dayDiv.setAttribute('role', 'gridcell');
        dayDiv.setAttribute('tabindex', date.getMonth() === this.currentMonth ? '0' : '-1');

        const runsText = dayRuns.length === 1 ? '1 run' : `${dayRuns.length} runs`;
        dayDiv.setAttribute('aria-label', `${date.toLocaleDateString()}, ${runsText}`);

        // Add focus handling
        dayDiv.addEventListener('focus', () => {
            this.selectedDate = date;
        });

        return dayDiv;
    }

    createRunsContainer(dayRuns, date) {
        const runsContainer = document.createElement('div');
        runsContainer.className = 'day-runs';

        // Sort runs by start time
        const sortedRuns = dayRuns.sort((a, b) => {
            const timeA = this.parseTime(a.startTime);
            const timeB = this.parseTime(b.startTime);
            if (!timeA || !timeB) return 0;
            return (timeA.hours * 60 + timeA.minutes) - (timeB.hours * 60 + timeB.minutes);
        });

        // Show up to configured max runs, then "X more"
        const visibleRuns = sortedRuns.slice(0, this.config.maxRunsPerDay);

        visibleRuns.forEach(run => {
            const runElement = this.createRunElement(run);
            runsContainer.appendChild(runElement);
        });

        if (dayRuns.length > this.config.maxRunsPerDay) {
            const moreElement = this.createMoreRunsElement(dayRuns.length - this.config.maxRunsPerDay, date, dayRuns);
            runsContainer.appendChild(moreElement);
        }

        return runsContainer;
    }

    createRunElement(run) {
        const runDiv = document.createElement('div');
        runDiv.className = `run-item ${run.type}`;
        runDiv.setAttribute('data-run-id', run.id);

        // Status indicators
        if (run.status === 'Cancelled') {
            runDiv.classList.add('cancelled');
        }

        if (!run.isPublic) {
            runDiv.classList.add('private');
        }

        // Capacity indicators
        if (run.capacity > 0) {
            const fillPercentage = (run.participants / run.capacity) * 100;
            if (fillPercentage >= 85) {
                runDiv.classList.add('almost-full');
            } else if (fillPercentage >= 100) {
                runDiv.classList.add('full');
            }
        }

        // Create run content with enhanced layout
        const timeSpan = document.createElement('span');
        timeSpan.className = 'run-time';
        timeSpan.textContent = this.formatTimeShort(run.startTime);

        const nameSpan = document.createElement('span');
        nameSpan.className = 'run-name';
        nameSpan.textContent = this.truncateText(run.name, 20);

        // Add capacity indicator for small display
        if (run.capacity > 0) {
            const capacitySpan = document.createElement('span');
            capacitySpan.className = 'run-capacity';
            capacitySpan.textContent = `${run.participants}/${run.capacity}`;
            runDiv.appendChild(capacitySpan);
        }

        runDiv.appendChild(timeSpan);
        runDiv.appendChild(nameSpan);

        // Event handlers
        this.attachRunElementEvents(runDiv, run);

        return runDiv;
    }

    attachRunElementEvents(runDiv, run) {
        // Enhanced tooltip events
        if (this.config.enableTooltips) {
            runDiv.addEventListener('mouseenter', (e) => {
                this.showTooltip(e, run);
            });

            runDiv.addEventListener('mouseleave', () => {
                this.hideTooltip();
            });

            runDiv.addEventListener('mousemove', (e) => {
                this.updateTooltipPosition(e);
            });
        }

        // Enhanced click handler with loading state
        runDiv.addEventListener('click', (e) => {
            e.stopPropagation();
            runDiv.classList.add('loading');
            this.openRunDetails(run).finally(() => {
                runDiv.classList.remove('loading');
            });
        });

        // Enhanced keyboard accessibility
        runDiv.setAttribute('tabindex', '0');
        runDiv.setAttribute('role', 'button');

        const capacityText = run.capacity > 0 ? `, ${run.participants} of ${run.capacity} participants` : '';
        runDiv.setAttribute('aria-label', `${run.name} at ${run.startTime}${capacityText}`);

        runDiv.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                this.openRunDetails(run);
            }
        });
    }

    createMoreRunsElement(count, date, allRuns) {
        const moreElement = document.createElement('div');
        moreElement.className = 'more-runs';
        moreElement.textContent = `+${count} more`;
        moreElement.setAttribute('tabindex', '0');
        moreElement.setAttribute('role', 'button');
        moreElement.setAttribute('aria-label', `Show ${count} more runs for ${date.toLocaleDateString()}`);

        const clickHandler = (e) => {
            e.stopPropagation();
            this.showAllRunsForDate(date, allRuns);
        };

        moreElement.addEventListener('click', clickHandler);

        moreElement.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                clickHandler(e);
            }
        });

        return moreElement;
    }

    getRunsForDate(date) {
        const dateString = date.toDateString();
        return this.runs.filter(run => {
            return run.date && run.date.toDateString() === dateString;
        });
    }

    getRunsForMonth(month, year) {
        return this.runs.filter(run =>
            run.date &&
            run.date.getMonth() === month &&
            run.date.getFullYear() === year
        );
    }

    selectDate(date, runs) {
        // Remove previous selection
        document.querySelectorAll('.calendar-day.selected').forEach(day => {
            day.classList.remove('selected');
        });

        // Add selection to current day
        const dayElement = document.querySelector(`[data-date="${date.toISOString().split('T')[0]}"]`);
        if (dayElement) {
            dayElement.classList.add('selected');
            dayElement.focus();
        }

        this.selectedDate = date;

        console.log(`📅 Selected ${date.toLocaleDateString()} with ${runs.length} runs`);

        // Announce to screen reader
        const runsText = runs.length === 1 ? '1 run' : `${runs.length} runs`;
        this.announceToScreenReader(`Selected ${date.toLocaleDateString()}, ${runsText}`);

        // Emit custom event for other components
        document.dispatchEvent(new CustomEvent('calendarDateSelected', {
            detail: { date, runs }
        }));
    }

    showAllRunsForDate(date, runs) {
        // Create a more user-friendly modal instead of alert
        const modal = this.createRunListModal(date, runs);
        document.body.appendChild(modal);

        const bootstrapModal = new bootstrap.Modal(modal);
        bootstrapModal.show();

        // Clean up modal after hiding
        modal.addEventListener('hidden.bs.modal', () => {
            document.body.removeChild(modal);
        });
    }

    createRunListModal(date, runs) {
        const modal = document.createElement('div');
        modal.className = 'modal fade';
        modal.tabIndex = -1;
        modal.setAttribute('aria-labelledby', 'runListModalLabel');
        modal.setAttribute('aria-hidden', 'true');

        const sortedRuns = runs.sort((a, b) => {
            const timeA = this.parseTime(a.startTime);
            const timeB = this.parseTime(b.startTime);
            if (!timeA || !timeB) return 0;
            return (timeA.hours * 60 + timeA.minutes) - (timeB.hours * 60 + timeB.minutes);
        });

        const runsList = sortedRuns.map(run => {
            const capacity = run.capacity > 0 ? ` (${run.participants}/${run.capacity})` : '';
            const status = run.status !== 'Active' ? ` [${run.status}]` : '';
            const privateIndicator = !run.isPublic ? ' 🔒' : '';

            return `
                <div class="list-group-item list-group-item-action run-list-item" data-run-id="${run.id}">
                    <div class="d-flex w-100 justify-content-between">
                        <h6 class="mb-1">${privateIndicator}${run.name}${status}</h6>
                        <small class="text-muted">${run.startTime}</small>
                    </div>
                    <p class="mb-1">${run.location}</p>
                    <small class="text-muted">
                        ${run.skillLevel}${capacity}
                        <span class="badge bg-${this.getTypeColor(run.type)} ms-2">${this.capitalize(run.type)}</span>
                    </small>
                </div>
            `;
        }).join('');

        modal.innerHTML = `
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="runListModalLabel">
                            Runs for ${date.toLocaleDateString()}
                        </h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <div class="list-group">
                            ${runsList}
                        </div>
                    </div>
                </div>
            </div>
        `;

        // Add click handlers to run items
        modal.addEventListener('click', (e) => {
            const runItem = e.target.closest('.run-list-item');
            if (runItem) {
                const runId = runItem.getAttribute('data-run-id');
                const run = runs.find(r => r.id === runId);
                if (run) {
                    bootstrap.Modal.getInstance(modal).hide();
                    this.openRunDetails(run);
                }
            }
        });

        return modal;
    }

    getTypeColor(type) {
        const colors = {
            pickup: 'success',
            training: 'primary',
            tournament: 'danger',
            youth: 'warning',
            women: 'purple'
        };
        return colors[type] || 'secondary';
    }

    showTooltip(event, run) {
        if (!this.tooltip) return;

        const capacityPercent = run.capacity > 0 ? Math.round((run.participants / run.capacity) * 100) : 0;
        const isAlmostFull = capacityPercent >= 85;
        const isFull = capacityPercent >= 100;
        const statusIcon = this.getStatusIcon(run.status);
        const privateIcon = !run.isPublic ? '🔒 ' : '';

        this.tooltip.innerHTML = `
            <div class="tooltip-header">
                <strong>${privateIcon}${run.name}</strong>
                <span class="status-icon">${statusIcon}</span>
            </div>
            <div class="tooltip-content">
                <div class="tooltip-row">
                    <i class="bi bi-clock me-1"></i>
                    <span>${run.startTime} - ${run.endTime}</span>
                </div>
                <div class="tooltip-row">
                    <i class="bi bi-geo-alt me-1"></i>
                    <span>${run.location}</span>
                </div>
                <div class="tooltip-row">
                    <i class="bi bi-people me-1"></i>
                    <span>${run.participants}/${run.capacity} participants</span>
                    ${isFull ? '<span class="badge bg-danger ms-1">Full</span>' :
                isAlmostFull ? '<span class="badge bg-warning ms-1">Almost Full</span>' : ''}
                </div>
                <div class="tooltip-row">
                    <i class="bi bi-star me-1"></i>
                    <span>${run.skillLevel}</span>
                </div>
                <div class="tooltip-row">
                    <i class="bi bi-tag me-1"></i>
                    <span class="badge bg-${this.getTypeColor(run.type)}">${this.capitalize(run.type)}</span>
                </div>
                ${run.status !== 'Active' ? `<div class="tooltip-row text-warning"><i class="bi bi-info-circle me-1"></i>Status: ${run.status}</div>` : ''}
            </div>
            ${run.description ? `<div class="tooltip-description">${this.truncateText(run.description, 100)}</div>` : ''}
            <div class="tooltip-footer">
                <small>Click to view details</small>
            </div>
        `;

        this.updateTooltipPosition(event);
        this.tooltip.classList.add('show');
    }

    getStatusIcon(status) {
        const icons = {
            'Active': '✅',
            'Cancelled': '❌',
            'Completed': '🏁',
            'Postponed': '⏸️'
        };
        return icons[status] || '⚠️';
    }

    updateTooltipPosition(event) {
        if (!this.tooltip) return;

        const rect = event.target.getBoundingClientRect();
        const tooltipRect = this.tooltip.getBoundingClientRect();
        const viewportWidth = window.innerWidth;
        const viewportHeight = window.innerHeight;

        let left = rect.left + (rect.width / 2) - (tooltipRect.width / 2);
        let top = rect.bottom + 8;

        // Adjust horizontal position if tooltip would go off-screen
        if (left < 8) {
            left = 8;
        } else if (left + tooltipRect.width > viewportWidth - 8) {
            left = viewportWidth - tooltipRect.width - 8;
        }

        // Adjust vertical position if tooltip would go off-screen
        if (top + tooltipRect.height > viewportHeight - 8) {
            top = rect.top - tooltipRect.height - 8;
        }

        this.tooltip.style.left = left + 'px';
        this.tooltip.style.top = top + 'px';
    }

    repositionTooltip() {
        if (this.tooltip && this.tooltip.classList.contains('show')) {
            // Force recalculation of tooltip position
            const rect = this.tooltip.getBoundingClientRect();
            if (rect.right > window.innerWidth || rect.bottom > window.innerHeight) {
                this.hideTooltip();
            }
        }
    }

    hideTooltip() {
        if (this.tooltip) {
            this.tooltip.classList.remove('show');
        }
    }

    async openRunDetails(run) {
        console.log('🔍 Opening run details for:', run.name);

        // First, try to integrate with existing run management modal
        if (await this.tryOpenExistingRunModal(run)) {
            return;
        }

        // Fallback: Create a custom modal
        this.showRunDetailsModal(run);
    }

    async tryOpenExistingRunModal(run) {
        // Check if the existing run edit modal and functions exist
        const editModal = document.getElementById('editRunModal');

        if (editModal && typeof window.loadRunDataEnhanced === 'function') {
            try {
                // Close calendar modal first
                const calendarModal = bootstrap.Modal.getInstance(document.getElementById('runCalendarModal'));
                if (calendarModal) {
                    calendarModal.hide();
                }

                // Wait for calendar modal to close
                await new Promise(resolve => setTimeout(resolve, 300));

                // Open edit modal
                const runEditModal = new bootstrap.Modal(editModal);
                runEditModal.show();

                // Load the run data
                window.loadRunDataEnhanced(run.id);

                return true;
            } catch (error) {
                console.warn('⚠️ Could not open existing run modal:', error);
                return false;
            }
        }

        return false;
    }

    showRunDetailsModal(run) {
        const capacityPercent = run.capacity > 0 ? Math.round((run.participants / run.capacity) * 100) : 0;
        const statusEmoji = this.getStatusIcon(run.status);

        const details = `
🏀 ${run.name}

📅 Date: ${run.date.toLocaleDateString()}
🕐 Time: ${run.startTime} - ${run.endTime}
📍 Location: ${run.location}
🎯 Skill Level: ${run.skillLevel}
👥 Participants: ${run.participants}/${run.capacity} (${capacityPercent}%)
${statusEmoji} Status: ${run.status}
🏷️ Type: ${this.capitalize(run.type)}
${!run.isPublic ? '🔒 Private Run' : '🌐 Public Run'}

${run.description ? `📝 Description:\n${run.description}` : ''}
        `.trim();

        // Use a proper modal instead of alert for better UX
        const modal = this.createRunDetailsModal(run);
        document.body.appendChild(modal);

        const bootstrapModal = new bootstrap.Modal(modal);
        bootstrapModal.show();

        // Clean up modal after hiding
        modal.addEventListener('hidden.bs.modal', () => {
            document.body.removeChild(modal);
        });
    }

    createRunDetailsModal(run) {
        const modal = document.createElement('div');
        modal.className = 'modal fade';
        modal.tabIndex = -1;

        const capacityPercent = run.capacity > 0 ? Math.round((run.participants / run.capacity) * 100) : 0;
        const progressBarClass = capacityPercent >= 85 ? 'bg-warning' : 'bg-primary';

        modal.innerHTML = `
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">
                            ${!run.isPublic ? '🔒 ' : ''}${run.name}
                        </h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <div class="row">
                            <div class="col-md-6">
                                <p><i class="bi bi-calendar me-2"></i><strong>Date:</strong> ${run.date.toLocaleDateString()}</p>
                                <p><i class="bi bi-clock me-2"></i><strong>Time:</strong> ${run.startTime} - ${run.endTime}</p>
                                <p><i class="bi bi-geo-alt me-2"></i><strong>Location:</strong> ${run.location}</p>
                            </div>
                            <div class="col-md-6">
                                <p><i class="bi bi-star me-2"></i><strong>Skill Level:</strong> ${run.skillLevel}</p>
                                <p><i class="bi bi-tag me-2"></i><strong>Type:</strong> <span class="badge bg-${this.getTypeColor(run.type)}">${this.capitalize(run.type)}</span></p>
                                <p><i class="bi bi-info-circle me-2"></i><strong>Status:</strong> <span class="badge bg-${run.status === 'Active' ? 'success' : 'warning'}">${run.status}</span></p>
                            </div>
                        </div>
                        
                        <div class="mt-3">
                            <p><strong>Participants (${run.participants}/${run.capacity}):</strong></p>
                            <div class="progress">
                                <div class="progress-bar ${progressBarClass}" role="progressbar" 
                                     style="width: ${Math.min(capacityPercent, 100)}%" 
                                     aria-valuenow="${capacityPercent}" aria-valuemin="0" aria-valuemax="100">
                                    ${capacityPercent}%
                                </div>
                            </div>
                        </div>
                        
                        ${run.description ? `
                            <div class="mt-3">
                                <p><strong>Description:</strong></p>
                                <p class="text-muted">${run.description}</p>
                            </div>
                        ` : ''}
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                        ${run.status === 'Active' && run.participants < run.capacity ?
                '<button type="button" class="btn btn-primary">Join Run</button>' : ''}
                    </div>
                </div>
            </div>
        `;

        return modal;
    }

    updateStats() {
        const currentMonthRuns = this.getRunsForMonth(this.currentMonth, this.currentYear);
        const activeRuns = currentMonthRuns.filter(run => run.status === 'Active');
        const totalParticipants = activeRuns.reduce((sum, run) => sum + (run.participants || 0), 0);
        const totalCapacity = activeRuns.reduce((sum, run) => sum + (run.capacity || 0), 0);
        const averageCapacity = totalCapacity > 0 ? Math.round((totalParticipants / totalCapacity) * 100) : 0;

        // Animate the number changes
        this.animateNumber('totalRuns', this.runs.length);
        this.animateNumber('upcomingRuns', currentMonthRuns.length);
        this.animateNumber('totalParticipants', totalParticipants);
        this.animateNumber('averageCapacity', averageCapacity, '%');
    }

    animateNumber(elementId, targetValue, suffix = '') {
        const element = document.getElementById(elementId);
        if (!element) return;

        const startValue = parseInt(element.textContent.replace(/[^\d]/g, '')) || 0;
        const duration = this.config.animationDuration;
        const startTime = performance.now();

        const animate = (currentTime) => {
            const elapsed = currentTime - startTime;
            const progress = Math.min(elapsed / duration, 1);

            // Use easeOutQuart easing function
            const easeProgress = 1 - Math.pow(1 - progress, 4);

            const currentValue = Math.round(startValue + (targetValue - startValue) * easeProgress);
            element.textContent = currentValue + suffix;

            if (progress < 1) {
                requestAnimationFrame(animate);
            }
        };

        requestAnimationFrame(animate);
    }

    previousMonth() {
        this.currentMonth--;
        if (this.currentMonth < 0) {
            this.currentMonth = 11;
            this.currentYear--;
        }
        console.log(`📅 Navigated to ${this.currentMonth + 1}/${this.currentYear}`);
        this.loadCalendar();
    }

    nextMonth() {
        this.currentMonth++;
        if (this.currentMonth > 11) {
            this.currentMonth = 0;
            this.currentYear++;
        }
        console.log(`📅 Navigated to ${this.currentMonth + 1}/${this.currentYear}`);
        this.loadCalendar();
    }

    goToToday() {
        const today = new Date();
        const wasCurrentMonth = this.currentMonth === today.getMonth() && this.currentYear === today.getFullYear();

        this.currentMonth = today.getMonth();
        this.currentYear = today.getFullYear();

        console.log('📅 Navigated to current month');

        if (wasCurrentMonth) {
            // Just refresh if we're already on current month
            this.loadCalendar(true);
        } else {
            this.loadCalendar();
        }
    }

    adjustResponsiveLayout() {
        const calendarGrid = document.getElementById('calendarGrid');
        if (!calendarGrid) return;

        const viewportWidth = window.innerWidth;
        const isMobile = viewportWidth < this.config.responsive.mobile;
        const isTablet = viewportWidth < this.config.responsive.tablet;

        // Adjust calendar for different screen sizes
        if (isMobile) {
            calendarGrid.style.fontSize = '0.75rem';
            this.config.maxRunsPerDay = 2;
        } else if (isTablet) {
            calendarGrid.style.fontSize = '0.85rem';
            this.config.maxRunsPerDay = 2;
        } else {
            calendarGrid.style.fontSize = '';
            this.config.maxRunsPerDay = 3;
        }

        // Re-render if runs are visible to apply new limits
        if (this.runs.length > 0) {
            const currentRuns = this.runs;
            this.runs = currentRuns; // Trigger re-render with new layout
        }
    }

    showLoading() {
        const grid = document.getElementById('calendarGrid');
        if (!grid) return;

        const headers = grid.querySelectorAll('.calendar-day-header');
        grid.innerHTML = '';
        headers.forEach(header => grid.appendChild(header));

        const loading = document.createElement('div');
        loading.className = 'calendar-loading';
        loading.style.gridColumn = '1 / -1';
        loading.innerHTML = `
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading calendar...</span>
            </div>
            <div class="text-muted mt-2">Loading runs...</div>
        `;
        grid.appendChild(loading);
    }

    hideLoading() {
        // Loading is hidden when calendar renders
    }

    showError(message) {
        const grid = document.getElementById('calendarGrid');
        if (!grid) return;

        const headers = grid.querySelectorAll('.calendar-day-header');
        grid.innerHTML = '';
        headers.forEach(header => grid.appendChild(header));

        const error = document.createElement('div');
        error.className = 'calendar-empty';
        error.style.gridColumn = '1 / -1';
        error.innerHTML = `
            <i class="bi bi-exclamation-triangle text-danger" style="font-size: 3rem; margin-bottom: 1rem;"></i>
            <h5 class="text-danger">Error Loading Calendar</h5>
            <p class="text-muted">${message}</p>
            <button class="btn btn-primary" onclick="window.runCalendar.loadCalendar(true)">
                <i class="bi bi-arrow-clockwise me-2"></i>Try Again
            </button>
        `;
        grid.appendChild(error);
    }

    // Enhanced utility methods
    formatTime(date) {
        return date.toLocaleTimeString('en-US', {
            hour: 'numeric',
            minute: '2-digit',
            hour12: true
        });
    }

    formatTimeShort(timeString) {
        // Convert "2:30 PM" to "2:30p" for compact display
        return timeString.replace(/\s?(AM|PM)/, m => m.trim().toLowerCase().charAt(0));
    }

    capitalize(str) {
        return str.charAt(0).toUpperCase() + str.slice(1);
    }

    truncateText(text, maxLength) {
        if (!text || text.length <= maxLength) return text;
        return text.substring(0, maxLength - 3) + '...';
    }

    getAntiForgeryToken() {
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        return token ? token.value : '';
    }

    clearCache() {
        this.cache.clear();
        console.log('📅 Calendar cache cleared');
    }

    focusSearchFilter() {
        // Focus on search/filter if available
        const searchInput = document.querySelector('#runCalendarModal .form-control[type="search"]');
        if (searchInput) {
            searchInput.focus();
        }
    }

    // Public API methods
    refresh() {
        return this.loadCalendar(true);
    }

    goToDate(date) {
        if (!(date instanceof Date)) {
            date = new Date(date);
        }

        this.currentMonth = date.getMonth();
        this.currentYear = date.getFullYear();
        return this.loadCalendar();
    }

    getRuns() {
        return [...this.runs]; // Return copy to prevent external modification
    }

    getRunsForDateRange(startDate, endDate) {
        return this.runs.filter(run =>
            run.date &&
            run.date >= startDate &&
            run.date <= endDate
        );
    }

    // Cleanup method
    destroy() {
        // Remove event listeners
        document.removeEventListener('keydown', this.handleKeyboardNavigation);
        window.removeEventListener('resize', this.handleResize);

        // Clear intervals
        if (this.autoRefreshInterval) {
            clearInterval(this.autoRefreshInterval);
        }

        // Clear cache
        this.clearCache();

        // Remove live region
        const liveRegion = document.getElementById('calendar-live-region');
        if (liveRegion) {
            document.body.removeChild(liveRegion);
        }

        console.log('🗓️ Enhanced Run Calendar destroyed');
    }
}

// Initialize calendar when DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
    // Only initialize if we're on a page with the calendar modal
    if (document.getElementById('runCalendarModal')) {
        try {
            window.runCalendar = new EnhancedRunCalendar();
            console.log('🗓️ Enhanced Run Calendar initialized and ready');

            // Make calendar available globally for debugging
            if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
                window.calendarDebug = {
                    instance: window.runCalendar,
                    goToDate: (date) => window.runCalendar.goToDate(date),
                    refresh: () => window.runCalendar.refresh(),
                    getRuns: () => window.runCalendar.getRuns(),
                    clearCache: () => window.runCalendar.clearCache(),
                    goToToday: () => window.runCalendar.goToToday()
                };
                console.log('🐛 Enhanced Calendar debug tools available at window.calendarDebug');
            }
        } catch (error) {
            console.error('❌ Failed to initialize Enhanced Run Calendar:', error);
        }
    }

    // Cleanup on page unload
    window.addEventListener('beforeunload', () => {
        if (window.runCalendar && typeof window.runCalendar.destroy === 'function') {
            window.runCalendar.destroy();
        }
    });
});