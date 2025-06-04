/**
 * Complete Run Calendar Implementation
 * Enhanced calendar view for displaying basketball runs like appointments
 * Integrates with existing run management system
 */

class RunCalendar {
    constructor() {
        this.currentDate = new Date();
        this.currentMonth = this.currentDate.getMonth();
        this.currentYear = this.currentDate.getFullYear();
        this.selectedDate = null;
        this.runs = [];
        this.tooltip = document.getElementById('runTooltip');
        this.isLoading = false;

        // Calendar configuration
        this.config = {
            maxRunsPerDay: 3,
            dateRangeMonths: 2,
            animationDuration: 500,
            fetchTimeout: 30000,
            refreshInterval: 5 * 60 * 1000 // 5 minutes
        };

        this.init();
    }

    init() {
        this.bindEvents();
        this.setupAutoRefresh();
        console.log('🗓️ Run Calendar initialized successfully');
    }

    bindEvents() {
        // Navigation buttons
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

        // Modal events
        const modal = document.getElementById('runCalendarModal');
        if (modal) {
            modal.addEventListener('shown.bs.modal', () => {
                this.loadCalendar();
            });

            modal.addEventListener('hidden.bs.modal', () => {
                this.hideTooltip();
            });
        }

        // Keyboard navigation
        document.addEventListener('keydown', (e) => {
            const modal = document.getElementById('runCalendarModal');
            if (modal && modal.classList.contains('show')) {
                this.handleKeyboardNavigation(e);
            }
        });

        // Window resize handler for responsive adjustments
        let resizeTimeout;
        window.addEventListener('resize', () => {
            clearTimeout(resizeTimeout);
            resizeTimeout = setTimeout(() => {
                if (this.isCalendarVisible()) {
                    this.adjustResponsiveLayout();
                }
            }, 250);
        });
    }

    handleKeyboardNavigation(e) {
        switch (e.key) {
            case 'ArrowLeft':
                e.preventDefault();
                this.previousMonth();
                break;
            case 'ArrowRight':
                e.preventDefault();
                this.nextMonth();
                break;
            case 'Home':
                e.preventDefault();
                this.goToToday();
                break;
            case 'r':
            case 'R':
                if (e.ctrlKey || e.metaKey) {
                    e.preventDefault();
                    this.loadCalendar(true);
                }
                break;
            case 'Escape':
                this.hideTooltip();
                break;
        }
    }

    setupAutoRefresh() {
        // Auto-refresh calendar data every 5 minutes if modal is open
        setInterval(() => {
            if (this.isCalendarVisible() && !this.isLoading) {
                this.loadCalendar(false, false); // Silent refresh
            }
        }, this.config.refreshInterval);
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

        if (showLoading) {
            this.showLoading();
        }

        try {
            await this.fetchRuns(forceRefresh);
            this.renderCalendar();
            this.updateStats();

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
        }
    }

    async fetchRuns(forceRefresh = false) {
        // Calculate date range
        const startDate = new Date(this.currentYear, this.currentMonth - this.config.dateRangeMonths, 1);
        const endDate = new Date(this.currentYear, this.currentMonth + this.config.dateRangeMonths + 1, 0);

        try {
            console.log(`📡 Fetching runs from ${startDate.toISOString()} to ${endDate.toISOString()}`);

            const response = await this.fetchRunsFromAPI(startDate, endDate);
            this.runs = response;

            console.log(`📋 Loaded ${this.runs.length} runs from API`);
        } catch (error) {
            console.warn('⚠️ API fetch failed, using mock data:', error.message);

            // Fallback to mock data for demonstration/testing
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
                state: run.state || ''
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
        await new Promise(resolve => setTimeout(resolve, 800));
        this.runs = this.generateMockRuns();
    }

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
            'Downtown Sports Center'
        ];

        // Generate 20-35 random runs for the period
        const runCount = Math.floor(Math.random() * 16) + 20;

        for (let i = 0; i < runCount; i++) {
            // Create random date within range
            const randomDate = new Date(startDate.getTime() + Math.random() * (endDate.getTime() - startDate.getTime()));

            // Set random time (8 AM to 9 PM)
            const randomHour = Math.floor(Math.random() * 13) + 8;
            const randomMinute = Math.floor(Math.random() * 4) * 15; // 0, 15, 30, 45
            randomDate.setHours(randomHour, randomMinute, 0, 0);

            const type = runTypes[Math.floor(Math.random() * runTypes.length)];
            const capacity = Math.floor(Math.random() * 20) + 5;
            const participants = Math.floor(Math.random() * (capacity + 1));

            const startTime = this.formatTime(randomDate);
            const endDate = new Date(randomDate.getTime() + (1.5 + Math.random()) * 60 * 60 * 1000); // 1.5-2.5 hours
            const endTime = this.formatTime(endDate);

            runs.push({
                id: `mock-run-${i + 1}`,
                name: `${this.capitalize(type)} Session ${i + 1}`,
                type: type,
                date: randomDate,
                startTime: startTime,
                endTime: endTime,
                location: locations[Math.floor(Math.random() * locations.length)],
                skillLevel: skillLevels[Math.floor(Math.random() * skillLevels.length)],
                participants: participants,
                capacity: capacity,
                description: `${this.capitalize(type)} basketball session for ${skillLevels[Math.floor(Math.random() * skillLevels.length)]} level players. Come ready to play!`,
                status: Math.random() > 0.1 ? 'Active' : 'Cancelled', // 10% chance of cancelled
                isPublic: Math.random() > 0.2 // 80% public
            });
        }

        return runs.sort((a, b) => a.date - b.date);
    }

    renderCalendar() {
        const grid = document.getElementById('calendarGrid');
        const title = document.getElementById('calendarTitle');

        if (!grid || !title) {
            console.error('❌ Calendar grid or title element not found');
            return;
        }

        // Update title
        const monthNames = [
            'January', 'February', 'March', 'April', 'May', 'June',
            'July', 'August', 'September', 'October', 'November', 'December'
        ];
        title.textContent = `${monthNames[this.currentMonth]} ${this.currentYear}`;

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
        grid.classList.remove('show');
        grid.classList.add('calendar-fade');

        // Use requestAnimationFrame for smooth animation
        requestAnimationFrame(() => {
            requestAnimationFrame(() => {
                grid.classList.add('show');
            });
        });

        // Adjust responsive layout
        this.adjustResponsiveLayout();
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

        // Click handler for day selection
        dayDiv.addEventListener('click', (e) => {
            if (!e.target.closest('.run-item') && !e.target.closest('.more-runs')) {
                this.selectDate(date, dayRuns);
            }
        });

        // Accessibility
        dayDiv.setAttribute('role', 'gridcell');
        dayDiv.setAttribute('tabindex', date.getMonth() === this.currentMonth ? '0' : '-1');
        dayDiv.setAttribute('aria-label', `${date.toLocaleDateString()}, ${dayRuns.length} runs`);

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

        // Create run content
        const timeSpan = document.createElement('span');
        timeSpan.className = 'run-time';
        timeSpan.textContent = run.startTime;

        const nameSpan = document.createElement('span');
        nameSpan.className = 'run-name';
        nameSpan.textContent = run.name;

        runDiv.appendChild(timeSpan);
        runDiv.appendChild(nameSpan);

        // Event handlers
        this.attachRunElementEvents(runDiv, run);

        return runDiv;
    }

    attachRunElementEvents(runDiv, run) {
        // Tooltip events
        runDiv.addEventListener('mouseenter', (e) => {
            this.showTooltip(e, run);
        });

        runDiv.addEventListener('mouseleave', () => {
            this.hideTooltip();
        });

        runDiv.addEventListener('mousemove', (e) => {
            this.updateTooltipPosition(e);
        });

        // Click handler
        runDiv.addEventListener('click', (e) => {
            e.stopPropagation();
            this.openRunDetails(run);
        });

        // Keyboard accessibility
        runDiv.setAttribute('tabindex', '0');
        runDiv.setAttribute('role', 'button');
        runDiv.setAttribute('aria-label', `${run.name} at ${run.startTime}`);

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
        moreElement.setAttribute('aria-label', `Show ${count} more runs`);

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

    selectDate(date, runs) {
        // Remove previous selection
        document.querySelectorAll('.calendar-day.selected').forEach(day => {
            day.classList.remove('selected');
        });

        // Add selection to current day
        const dayElement = document.querySelector(`[data-date="${date.toISOString().split('T')[0]}"]`);
        if (dayElement) {
            dayElement.classList.add('selected');
        }

        this.selectedDate = date;

        console.log(`📅 Selected ${date.toLocaleDateString()} with ${runs.length} runs`);

        // Could emit custom event here for other components to listen to
        document.dispatchEvent(new CustomEvent('calendarDateSelected', {
            detail: { date, runs }
        }));
    }

    showAllRunsForDate(date, runs) {
        const runsList = runs
            .sort((a, b) => {
                const timeA = this.parseTime(a.startTime);
                const timeB = this.parseTime(b.startTime);
                if (!timeA || !timeB) return 0;
                return (timeA.hours * 60 + timeA.minutes) - (timeB.hours * 60 + timeB.minutes);
            })
            .map(run => {
                const capacity = run.capacity > 0 ? ` (${run.participants}/${run.capacity})` : '';
                const status = run.status !== 'Active' ? ` [${run.status}]` : '';
                return `• ${run.startTime} - ${run.name}${capacity}${status}\n  📍 ${run.location}`;
            })
            .join('\n\n');

        const message = `📅 All runs for ${date.toLocaleDateString()}:\n\n${runsList}`;

        // In a real app, you might want to show this in a modal instead of alert
        alert(message);
    }

    showTooltip(event, run) {
        if (!this.tooltip) return;

        const capacityPercent = run.capacity > 0 ? Math.round((run.participants / run.capacity) * 100) : 0;
        const isAlmostFull = capacityPercent >= 85;
        const statusIcon = run.status === 'Active' ? '✅' : run.status === 'Cancelled' ? '❌' : '⚠️';
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
                    ${isAlmostFull ? '<span class="badge bg-warning ms-1">Almost Full</span>' : ''}
                </div>
                <div class="tooltip-row">
                    <i class="bi bi-star me-1"></i>
                    <span>${run.skillLevel}</span>
                </div>
                ${run.status !== 'Active' ? `<div class="tooltip-row text-warning"><i class="bi bi-info-circle me-1"></i>Status: ${run.status}</div>` : ''}
            </div>
            ${run.description ? `<div class="tooltip-description">${run.description}</div>` : ''}
            <div class="tooltip-footer">
                <small>Click to view details</small>
            </div>
        `;

        this.updateTooltipPosition(event);
        this.tooltip.classList.add('show');
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

    hideTooltip() {
        if (this.tooltip) {
            this.tooltip.classList.remove('show');
        }
    }

    openRunDetails(run) {
        console.log('🔍 Opening run details for:', run.name);

        // First, try to integrate with existing run management modal
        if (this.tryOpenExistingRunModal(run)) {
            return;
        }

        // Fallback: Create a custom modal or alert
        this.showRunDetailsModal(run);
    }

    tryOpenExistingRunModal(run) {
        // Check if the existing run edit modal and functions exist
        const editModal = document.getElementById('editRunModal');

        if (editModal && typeof window.loadRunDataEnhanced === 'function') {
            try {
                // Close calendar modal first
                const calendarModal = bootstrap.Modal.getInstance(document.getElementById('runCalendarModal'));
                if (calendarModal) {
                    calendarModal.hide();
                }

                // Open edit modal after calendar modal is hidden
                setTimeout(() => {
                    const runEditModal = new bootstrap.Modal(editModal);
                    runEditModal.show();

                    // Load the run data
                    window.loadRunDataEnhanced(run.id);
                }, 300);

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
        const statusEmoji = run.status === 'Active' ? '✅' : run.status === 'Cancelled' ? '❌' : '⚠️';

        const details = `
🏀 ${run.name}

📅 Date: ${run.date.toLocaleDateString()}
🕐 Time: ${run.startTime} - ${run.endTime}
📍 Location: ${run.location}
🎯 Skill Level: ${run.skillLevel}
👥 Participants: ${run.participants}/${run.capacity} (${capacityPercent}%)
${statusEmoji} Status: ${run.status}
${!run.isPublic ? '🔒 Private Run' : '🌐 Public Run'}

${run.description ? `📝 Description:\n${run.description}` : ''}
        `.trim();

        alert(details);
    }

    updateStats() {
        const currentMonthRuns = this.runs.filter(run =>
            run.date &&
            run.date.getMonth() === this.currentMonth &&
            run.date.getFullYear() === this.currentYear
        );

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

        const isSmallScreen = window.innerWidth < 768;
        const isMobileScreen = window.innerWidth < 576;

        // Adjust calendar for different screen sizes
        if (isMobileScreen) {
            calendarGrid.style.fontSize = '0.8rem';
        } else if (isSmallScreen) {
            calendarGrid.style.fontSize = '0.9rem';
        } else {
            calendarGrid.style.fontSize = '';
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

    // Utility methods
    formatTime(date) {
        return date.toLocaleTimeString('en-US', {
            hour: 'numeric',
            minute: '2-digit',
            hour12: true
        });
    }

    capitalize(str) {
        return str.charAt(0).toUpperCase() + str.slice(1);
    }

    getAntiForgeryToken() {
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        return token ? token.value : '';
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

    getRunsForMonth(month, year) {
        return this.runs.filter(run =>
            run.date &&
            run.date.getMonth() === month &&
            run.date.getFullYear() === year
        );
    }
}

// Initialize calendar when DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
    // Only initialize if we're on a page with the calendar modal
    if (document.getElementById('runCalendarModal')) {
        try {
            window.runCalendar = new RunCalendar();
            console.log('🗓️ Run Calendar initialized and ready');

            // Make calendar available globally for debugging
            if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
                window.calendarDebug = {
                    instance: window.runCalendar,
                    goToDate: (date) => window.runCalendar.goToDate(date),
                    refresh: () => window.runCalendar.refresh(),
                    getRuns: () => window.runCalendar.getRuns()
                };
                console.log('🐛 Calendar debug tools available at window.calendarDebug');
            }
        } catch (error) {
            console.error('❌ Failed to initialize Run Calendar:', error);
        }
    }
});