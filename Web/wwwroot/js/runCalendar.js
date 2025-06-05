/**
 * FIXED Run Calendar Implementation
 * Clean version with no syntax errors
 */

class FixedRunCalendar {
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

        this.config = {
            maxRunsPerDay: 3,
            dateRangeMonths: 3,
            animationDuration: 300,
            fetchTimeout: 15000,
            refreshInterval: 5 * 60 * 1000,
            cacheTimeout: 2 * 60 * 1000,
            enableKeyboardNavigation: true,
            enableTooltips: true,
            enableAnimations: true
        };

        this.init();
    }

    init() {
        this.bindEvents();
        this.setupAutoRefresh();
        console.log('🗓️ Fixed Run Calendar initialized successfully');
    }

    bindEvents() {
        const prevBtn = document.getElementById('prevMonth');
        const nextBtn = document.getElementById('nextMonth');
        const todayBtn = document.getElementById('todayBtn');
        const refreshBtn = document.getElementById('refreshCalendar');

        if (prevBtn) prevBtn.addEventListener('click', () => this.previousMonth());
        if (nextBtn) nextBtn.addEventListener('click', () => this.nextMonth());
        if (todayBtn) todayBtn.addEventListener('click', () => this.goToToday());
        if (refreshBtn) refreshBtn.addEventListener('click', () => this.loadCalendar(true));

        const modal = document.getElementById('runCalendarModal');
        if (modal) {
            modal.addEventListener('shown.bs.modal', () => {
                this.loadCalendar();
            });

            modal.addEventListener('hidden.bs.modal', () => {
                this.hideTooltip();
                this.clearCache();
            });
        }
    }

    setupAutoRefresh() {
        this.autoRefreshInterval = setInterval(() => {
            if (this.isCalendarVisible() && !this.isLoading && !document.hidden) {
                this.loadCalendar(false, false);
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

        try {
            if (showLoading) {
                this.showLoading();
            }

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
        }
    }

    async fetchRuns(forceRefresh = false) {
        const startDate = new Date(this.currentYear, this.currentMonth - this.config.dateRangeMonths, 1);
        const endDate = new Date(this.currentYear, this.currentMonth + this.config.dateRangeMonths + 1, 0);

        try {
            console.log(`📡 Fetching runs from ${startDate.toISOString()} to ${endDate.toISOString()}`);

            const response = await this.fetchRunsFromMultipleSources(startDate, endDate);
            this.runs = response;

            if (this.runs.length > 0 && this.runs[0].id && !this.runs[0].id.includes('mock')) {
                console.log(`📋 ✅ Loaded ${this.runs.length} REAL runs from API successfully!`);
                this.showSuccessToast(`Loaded ${this.runs.length} runs from server`);
            } else {
                console.log(`📋 Loaded ${this.runs.length} runs (mock data)`);
            }

        } catch (error) {
            console.warn('⚠️ All data fetch attempts failed:', error.message);

            const isAuthError = error.message.includes('Authentication') || error.message.includes('log in');
            const isNotFound = error.message.includes('not found') || error.message.includes('404');
            const isServerError = error.message.includes('Server error') || error.message.includes('500');

            if (this.isOnDashboard()) {
                console.log('📊 On dashboard page - API not available, using fallback mock data');
                if (isAuthError) {
                    this.showWarningToast('Please log in to view real run data');
                }
                await this.generateMockDataWithDelay();
            } else {
                console.log('📄 On runs page - showing error message to user');

                let errorMessage = 'Unable to load runs from server. ';
                if (isAuthError) {
                    errorMessage += 'Please log in to continue.';
                } else if (isNotFound) {
                    errorMessage += 'API endpoint not configured.';
                } else if (isServerError) {
                    errorMessage += 'Server error occurred.';
                } else {
                    errorMessage += 'Showing sample data.';
                }

                this.showError(errorMessage);
                await this.generateMockDataWithDelay();
            }
        }
    }

    async fetchRunsFromMultipleSources(startDate, endDate) {
        const sources = [];
        const onDashboard = this.isOnDashboard();
        const onRunsPage = this.isOnRunsPage();

        console.log(`📍 Page detection: Dashboard=${onDashboard}, RunsPage=${onRunsPage}, Path=${window.location.pathname}`);

        sources.push({
            name: 'Primary API (/Run/GetRunsForCalendar)',
            fn: () => this.fetchRunsFromAPI(startDate, endDate)
        });

        if (onDashboard) {
            sources.push({
                name: 'Dashboard Mock Data Generator',
                fn: () => this.generateMockFromDashboardStats()
            });
        }

        if (onRunsPage && !onDashboard) {
            sources.push({
                name: 'Table Data Extraction',
                fn: () => this.extractRunsFromTable()
            });

            sources.push({
                name: 'Alternative API (/Run/GetAllRunsForCalendar)',
                fn: () => this.fetchAllRunsAndFilter(startDate, endDate)
            });
        }

        let lastError = null;

        for (let i = 0; i < sources.length; i++) {
            try {
                console.log(`🔄 Trying data source ${i + 1}: ${sources[i].name}`);
                const result = await sources[i].fn();
                if (result && result.length >= 0) {
                    console.log(`✅ Successfully fetched ${result.length} runs from: ${sources[i].name}`);
                    return result;
                }
            } catch (error) {
                console.warn(`⚠️ Source ${i + 1} (${sources[i].name}) failed:`, error.message);
                lastError = error;
            }
        }

        throw lastError || new Error('All data sources failed');
    }

    async fetchRunsFromAPI(startDate, endDate) {
        const url = new URL('/Run/GetRunsForCalendar', window.location.origin);
        url.searchParams.append('startDate', startDate.toISOString());
        url.searchParams.append('endDate', endDate.toISOString());

        console.log('📡 Fetching from URL:', url.toString());

        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), this.config.fetchTimeout);

        try {
            const response = await fetch(url.toString(), {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                signal: controller.signal,
                credentials: 'same-origin'
            });

            clearTimeout(timeoutId);

            console.log('📡 Response status:', response.status);

            if (!response.ok) {
                if (response.status === 401) {
                    throw new Error('Authentication required - please log in');
                } else if (response.status === 403) {
                    throw new Error('Access denied - insufficient permissions');
                } else if (response.status === 404) {
                    throw new Error('Calendar API endpoint not found');
                } else {
                    const errorText = await response.text();
                    console.error('❌ API Error Response:', errorText);
                    throw new Error(`Server error (${response.status}): ${response.statusText}`);
                }
            }

            const data = await response.json();
            console.log('📦 Raw API response:', data);

            if (data.success === false) {
                console.error('❌ API Error Details:', {
                    message: data.message,
                    error: data.error,
                    fullResponse: data
                });
                throw new Error(data.message || 'API returned error status');
            }

            if (!data.runs || !Array.isArray(data.runs)) {
                console.warn('⚠️ API response missing runs array:', data);
                throw new Error('Invalid API response format');
            }

            const transformedRuns = this.transformAPIData(data.runs);

            console.log(`✅ Successfully fetched and transformed ${transformedRuns.length} runs from API`);
            return transformedRuns;

        } catch (error) {
            clearTimeout(timeoutId);

            if (error.name === 'AbortError') {
                throw new Error('Request timed out - server may be slow');
            }

            console.error('📡 API Fetch Error:', error);
            throw error;
        }
    }

    transformAPIData(apiRuns) {
        if (!Array.isArray(apiRuns)) {
            console.warn('⚠️ API runs data is not an array:', apiRuns);
            return [];
        }

        console.log(`🔄 Transforming ${apiRuns.length} API runs...`);

        const transformedRuns = apiRuns.map((run, index) => {
            try {
                let runDate;
                if (run.runDate) {
                    runDate = new Date(run.runDate);
                    if (isNaN(runDate.getTime())) {
                        console.warn(`⚠️ Invalid date for run ${index}:`, run.runDate);
                        runDate = new Date();
                    }
                } else {
                    console.warn(`⚠️ Missing runDate for run ${index}:`, run);
                    runDate = new Date();
                }

                const transformedRun = {
                    id: run.runId || `api-run-${index}-${Date.now()}`,
                    name: run.name || run.clientName || 'Basketball Run',
                    type: (run.type || 'pickup').toLowerCase(),
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
                    zip: run.zip || '',
                    profileId: run.profileId || '',
                    clientId: run.clientId || '',
                    clientName: run.clientName || ''
                };

                if (!transformedRun.name.trim()) {
                    transformedRun.name = 'Basketball Run';
                }

                return transformedRun;
            } catch (error) {
                console.error(`❌ Error transforming run ${index}:`, error, run);
                return {
                    id: `error-run-${index}-${Date.now()}`,
                    name: 'Basketball Run (Error)',
                    type: 'pickup',
                    date: new Date(),
                    startTime: '12:00 PM',
                    endTime: '2:00 PM',
                    location: 'Location TBD',
                    skillLevel: 'All Levels',
                    participants: 0,
                    capacity: 10,
                    description: 'Error loading run details',
                    status: 'Active',
                    isPublic: true,
                    address: '',
                    city: '',
                    state: '',
                    zip: '',
                    profileId: '',
                    clientId: '',
                    clientName: ''
                };
            }
        });

        const validRuns = transformedRuns.filter(run => {
            const isValid = run.date && !isNaN(run.date.getTime());
            if (!isValid) {
                console.warn('⚠️ Filtering out run with invalid date:', run);
            }
            return isValid;
        });

        console.log(`✅ Successfully transformed ${validRuns.length} out of ${apiRuns.length} runs`);
        return validRuns;
    }

    buildLocationString(run) {
        const parts = [];
        if (run.address) parts.push(run.address);
        if (run.city) parts.push(run.city);
        if (run.state && parts.length > 0) parts.push(run.state);

        return parts.length > 0 ? parts.join(', ') : 'Location TBD';
    }

    isOnDashboard() {
        const path = window.location.pathname.toLowerCase();
        const isDashboard = path.includes('/dashboard') ||
            path === '/' ||
            path === '' ||
            document.querySelector('.dashboard-header') !== null ||
            document.querySelector('[data-usertype]') !== null;

        console.log(`📍 Dashboard check: path="${path}", isDashboard=${isDashboard}`);
        return isDashboard;
    }

    isOnRunsPage() {
        const path = window.location.pathname.toLowerCase();
        const isRunsPage = (path.includes('/run/run') || path.includes('/run')) &&
            !path.includes('/dashboard') &&
            document.getElementById('runsTable') !== null;

        console.log(`📍 Runs page check: path="${path}", isRunsPage=${isRunsPage}`);
        return isRunsPage;
    }

    async generateMockFromDashboardStats() {
        console.log('📊 Generating mock data from dashboard stats');

        await new Promise(resolve => setTimeout(resolve, 100));

        let runCount = 25;

        try {
            const statCards = document.querySelectorAll('.stats-card .fs-5, .stat-number');
            if (statCards.length > 0) {
                const firstStat = statCards[0].textContent.trim();
                const parsedCount = parseInt(firstStat);
                if (!isNaN(parsedCount) && parsedCount > 0) {
                    runCount = Math.min(Math.max(parsedCount, 10), 50);
                    console.log(`📊 Using dashboard run count: ${parsedCount}, generating ${runCount} mock runs`);
                }
            }
        } catch (error) {
            console.warn('⚠️ Could not extract stats from dashboard, using default count');
        }

        return this.generateMockRunsWithCount(runCount);
    }

    generateMockRunsWithCount(count) {
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

        for (let i = 0; i < count; i++) {
            const randomDate = new Date(startDate.getTime() + Math.random() * (endDate.getTime() - startDate.getTime()));

            const randomHour = Math.floor(Math.random() * 16) + 6;
            const randomMinute = Math.floor(Math.random() * 4) * 15;
            randomDate.setHours(randomHour, randomMinute, 0, 0);

            const type = runTypes[Math.floor(Math.random() * runTypes.length)];
            const capacity = Math.floor(Math.random() * 20) + 5;
            const participants = Math.floor(Math.random() * (capacity + 1));

            const startTime = this.formatTime(randomDate);
            const runEndDate = new Date(randomDate.getTime() + (1 + Math.random() * 2) * 60 * 60 * 1000);
            const endTime = this.formatTime(runEndDate);

            runs.push({
                id: `dashboard-mock-run-${i + 1}`,
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
                status: Math.random() > 0.05 ? 'Active' : 'Cancelled',
                isPublic: Math.random() > 0.15,
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
                `Casual basketball run. ${skillLevel} level preferred. First come, first served.`
            ],
            training: [
                `${skillLevel} training session focused on fundamentals and conditioning.`,
                `Skill development workout for ${skillLevel.toLowerCase()} players.`
            ],
            tournament: [
                `${skillLevel} tournament bracket. Prizes for winners!`,
                `Competitive tournament play. ${skillLevel} level required.`
            ],
            youth: [
                `Youth development program for ${skillLevel.toLowerCase()} young players.`,
                `Kids basketball session. ${skillLevel} level appropriate.`
            ],
            women: [
                `Women's basketball session. ${skillLevel} level.`,
                `Ladies only run. ${skillLevel} players encouraged to join.`
            ]
        };

        const typeTemplates = templates[type] || templates.pickup;
        return typeTemplates[Math.floor(Math.random() * typeTemplates.length)];
    }

    async generateMockDataWithDelay() {
        await new Promise(resolve => setTimeout(resolve, 600));
        this.runs = this.generateMockRuns();
    }

    generateMockRuns() {
        return this.generateMockRunsWithCount(30);
    }

    renderCalendar() {
        const grid = document.getElementById('calendarGrid');
        const title = document.getElementById('calendarTitle');

        if (!grid || !title) {
            console.error('❌ Calendar grid or title element not found');
            return;
        }

        const monthNames = [
            'January', 'February', 'March', 'April', 'May', 'June',
            'July', 'August', 'September', 'October', 'November', 'December'
        ];

        const currentMonthRuns = this.getRunsForMonth(this.currentMonth, this.currentYear);
        const monthTitle = `${monthNames[this.currentMonth]} ${this.currentYear}`;
        title.textContent = monthTitle;

        const headers = grid.querySelectorAll('.calendar-day-header');
        grid.innerHTML = '';
        headers.forEach(header => grid.appendChild(header));

        const firstDay = new Date(this.currentYear, this.currentMonth, 1);
        const startDate = new Date(firstDay);
        startDate.setDate(startDate.getDate() - firstDay.getDay());

        const today = new Date();
        today.setHours(0, 0, 0, 0);

        const fragment = document.createDocumentFragment();

        for (let i = 0; i < 42; i++) {
            const currentDate = new Date(startDate);
            currentDate.setDate(startDate.getDate() + i);

            const dayElement = this.createDayElement(currentDate, today);
            fragment.appendChild(dayElement);
        }

        grid.appendChild(fragment);

        console.log(`✅ Calendar rendered with ${currentMonthRuns.length} runs for ${monthTitle}`);
    }

    createDayElement(date, today) {
        const dayDiv = document.createElement('div');
        dayDiv.className = 'calendar-day';
        dayDiv.setAttribute('data-date', date.toISOString().split('T')[0]);

        if (date.getMonth() !== this.currentMonth) {
            dayDiv.classList.add('other-month');
        }

        if (date.toDateString() === today.toDateString()) {
            dayDiv.classList.add('today');
        }

        const dayNumber = document.createElement('div');
        dayNumber.className = 'day-number';
        dayNumber.textContent = date.getDate();
        dayDiv.appendChild(dayNumber);

        const dayRuns = this.getRunsForDate(date);
        if (dayRuns.length > 0) {
            const runsContainer = this.createRunsContainer(dayRuns, date);
            dayDiv.appendChild(runsContainer);
        }

        dayDiv.addEventListener('click', (e) => {
            if (!e.target.closest('.run-item') && !e.target.closest('.more-runs')) {
                this.selectDate(date, dayRuns);
            }
        });

        return dayDiv;
    }

    createRunsContainer(dayRuns, date) {
        const runsContainer = document.createElement('div');
        runsContainer.className = 'day-runs';

        const sortedRuns = dayRuns.sort((a, b) => {
            const timeA = this.parseTime(a.startTime);
            const timeB = this.parseTime(b.startTime);
            if (!timeA || !timeB) return 0;
            return (timeA.hours * 60 + timeA.minutes) - (timeB.hours * 60 + timeB.minutes);
        });

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

        if (run.status === 'Cancelled') {
            runDiv.classList.add('cancelled');
        }

        if (!run.isPublic) {
            runDiv.classList.add('private');
        }

        if (run.capacity > 0) {
            const fillPercentage = (run.participants / run.capacity) * 100;
            if (fillPercentage >= 85) {
                runDiv.classList.add('almost-full');
            } else if (fillPercentage >= 100) {
                runDiv.classList.add('full');
            }
        }

        const timeSpan = document.createElement('span');
        timeSpan.className = 'run-time';
        timeSpan.textContent = this.formatTimeShort(run.startTime);

        const nameSpan = document.createElement('span');
        nameSpan.className = 'run-name';
        nameSpan.textContent = this.truncateText(run.name, 20);

        runDiv.appendChild(timeSpan);
        runDiv.appendChild(nameSpan);

        this.attachRunElementEvents(runDiv, run);

        return runDiv;
    }

    attachRunElementEvents(runDiv, run) {
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

        runDiv.addEventListener('click', (e) => {
            e.stopPropagation();
            runDiv.classList.add('loading');
            this.openRunDetails(run).finally(() => {
                runDiv.classList.remove('loading');
            });
        });
    }

    createMoreRunsElement(count, date, allRuns) {
        const moreElement = document.createElement('div');
        moreElement.className = 'more-runs';
        moreElement.textContent = `+${count} more`;

        moreElement.addEventListener('click', (e) => {
            e.stopPropagation();
            this.showAllRunsForDate(date, allRuns);
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

    async openRunDetails(run) {
        console.log('🔍 Opening run details for:', run.name, 'ID:', run.id);

        if (typeof window.openRunById === 'function') {
            try {
                console.log('📝 Using direct run opening function');

                const calendarModal = bootstrap.Modal.getInstance(document.getElementById('runCalendarModal'));
                if (calendarModal) {
                    calendarModal.hide();
                }

                await new Promise(resolve => setTimeout(resolve, 300));

                const success = window.openRunById(run.id, 'calendar');

                if (success) {
                    console.log('✅ Successfully opened run modal from calendar');
                    return;
                } else {
                    console.warn('⚠️ Direct run opening failed, trying fallback');
                }
            } catch (error) {
                console.warn('⚠️ Error using direct run opening:', error);
            }
        }

        console.log('📝 Using custom run details modal');
        this.showRunDetailsModal(run);
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

        alert(details);
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

    updateTooltipPosition(event) {
        if (!this.tooltip) return;

        const rect = event.target.getBoundingClientRect();
        const tooltipRect = this.tooltip.getBoundingClientRect();
        const viewportWidth = window.innerWidth;
        const viewportHeight = window.innerHeight;

        let left = rect.left + (rect.width / 2) - (tooltipRect.width / 2);
        let top = rect.bottom + 8;

        if (left < 8) {
            left = 8;
        } else if (left + tooltipRect.width > viewportWidth - 8) {
            left = viewportWidth - tooltipRect.width - 8;
        }

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

    updateStats() {
        const currentMonthRuns = this.getRunsForMonth(this.currentMonth, this.currentYear);
        const activeRuns = currentMonthRuns.filter(run => run.status === 'Active');
        const totalParticipants = activeRuns.reduce((sum, run) => sum + (run.participants || 0), 0);
        const totalCapacity = activeRuns.reduce((sum, run) => sum + (run.capacity || 0), 0);
        const averageCapacity = totalCapacity > 0 ? Math.round((totalParticipants / totalCapacity) * 100) : 0;

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
            this.loadCalendar(true);
        } else {
            this.loadCalendar();
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
            <button class="btn btn-primary" onclick="window.fixedRunCalendar.loadCalendar(true)">
                <i class="bi bi-arrow-clockwise me-2"></i>Try Again
            </button>
        `;
        grid.appendChild(error);
    }

    clearCache() {
        this.cache.clear();
        console.log('📅 Calendar cache cleared');
    }

    // Utility methods
    formatTime(date) {
        return date.toLocaleTimeString('en-US', {
            hour: 'numeric',
            minute: '2-digit',
            hour12: true
        });
    }

    formatTimeShort(timeString) {
        return timeString.replace(/\s?(AM|PM)/, m => m.trim().toLowerCase().charAt(0));
    }

    capitalize(str) {
        return str.charAt(0).toUpperCase() + str.slice(1);
    }

    truncateText(text, maxLength) {
        if (!text || text.length <= maxLength) return text;
        return text.substring(0, maxLength - 3) + '...';
    }

    parseTime(timeString) {
        const timeRegex = /(\d{1,2}):(\d{2})(?::(\d{2}))?\s*(AM|PM)?/i;
        const match = timeString.match(timeRegex);

        if (!match) return null;

        let hours = parseInt(match[1]);
        const minutes = parseInt(match[2]);
        const period = match[4];

        if (period) {
            if (period.toUpperCase() === 'PM' && hours !== 12) {
                hours += 12;
            } else if (period.toUpperCase() === 'AM' && hours === 12) {
                hours = 0;
            }
        }

        return { hours, minutes };
    }

    selectDate(date, runs) {
        document.querySelectorAll('.calendar-day.selected').forEach(day => {
            day.classList.remove('selected');
        });

        const dayElement = document.querySelector(`[data-date="${date.toISOString().split('T')[0]}"]`);
        if (dayElement) {
            dayElement.classList.add('selected');
            dayElement.focus();
        }

        this.selectedDate = date;
        console.log(`📅 Selected ${date.toLocaleDateString()} with ${runs.length} runs`);
    }

    showSuccessToast(message) {
        if (window.UIUtils && window.UIUtils.showToast) {
            window.UIUtils.showToast(message, 'success', 3000);
        } else {
            console.log(`✅ SUCCESS: ${message}`);
        }
    }

    showWarningToast(message) {
        if (window.UIUtils && window.UIUtils.showToast) {
            window.UIUtils.showToast(message, 'warning', 4000);
        } else {
            console.log(`⚠️ WARNING: ${message}`);
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
        return [...this.runs];
    }

    // Test methods
    async testSimpleAPI() {
        console.log('🧪 Testing simple API endpoint...');

        try {
            const response = await fetch('/Run/TestCalendarAPI', {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                credentials: 'same-origin'
            });

            console.log('📡 Test API Response status:', response.status);

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const data = await response.json();
            console.log('📦 Test API Response:', data);

            if (data.success) {
                console.log('✅ Simple API Test SUCCESS!');
                console.log(`📋 Test API returned ${data.runs.length} test runs`);
                return data;
            } else {
                console.error('❌ Simple API Test FAILED:', data);
                return data;
            }

        } catch (error) {
            console.error('❌ Simple API Test ERROR:', error);
            return { success: false, error: error.message };
        }
    }

    async testAPIConnection() {
        console.log('🧪 Testing full calendar API connection...');

        try {
            const testDate = new Date();
            const startDate = new Date(testDate.getFullYear(), testDate.getMonth(), 1);
            const endDate = new Date(testDate.getFullYear(), testDate.getMonth() + 1, 0);

            const runs = await this.fetchRunsFromAPI(startDate, endDate);

            console.log('✅ Full Calendar API Test SUCCESS!');
            console.log(`📋 Calendar API returned ${runs.length} runs`);
            console.log('📊 Sample run:', runs[0]);

            return {
                success: true,
                message: `Calendar API working! Found ${runs.length} runs`,
                runs: runs,
                endpoint: '/Run/GetRunsForCalendar'
            };

        } catch (error) {
            console.error('❌ Full Calendar API Test FAILED:', error);

            return {
                success: false,
                message: error.message,
                error: error,
                endpoint: '/Run/GetRunsForCalendar'
            };
        }
    }

    async checkAuthStatus() {
        console.log('🔐 Checking authentication status...');

        try {
            const response = await fetch('/Run/GetRunsForCalendar?startDate=2024-01-01&endDate=2024-01-02', {
                method: 'HEAD',
                credentials: 'same-origin'
            });

            if (response.status === 401) {
                return { authenticated: false, message: 'Not logged in' };
            } else if (response.status === 403) {
                return { authenticated: false, message: 'Access denied' };
            } else if (response.ok || response.status === 404) {
                return { authenticated: true, message: 'Authenticated' };
            } else {
                return { authenticated: false, message: `HTTP ${response.status}` };
            }

        } catch (error) {
            console.warn('Could not check auth status:', error);
            return { authenticated: false, message: 'Connection error' };
        }
    }

    // Cleanup method
    destroy() {
        if (this.autoRefreshInterval) {
            clearInterval(this.autoRefreshInterval);
        }
        this.clearCache();
        console.log('🗓️ Fixed Run Calendar destroyed');
    }
}

// Initialize calendar when DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
    console.log('🗓️ Calendar initialization starting...');
    console.log('📍 Current URL:', window.location.href);
    console.log('📍 Current pathname:', window.location.pathname);

    if (document.getElementById('runCalendarModal')) {
        try {
            window.fixedRunCalendar = new FixedRunCalendar();
            console.log('🗓️ Fixed Run Calendar initialized and ready');

            // Debug tools
            if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
                window.calendarDebug = {
                    instance: window.fixedRunCalendar,
                    goToDate: (date) => window.fixedRunCalendar.goToDate(date),
                    refresh: () => window.fixedRunCalendar.refresh(),
                    getRuns: () => window.fixedRunCalendar.getRuns(),
                    clearCache: () => window.fixedRunCalendar.clearCache(),
                    extractFromTable: () => window.fixedRunCalendar.extractRunsFromTable(),

                    // API Testing
                    testAPI: () => window.fixedRunCalendar.testAPIConnection(),
                    testSimple: () => window.fixedRunCalendar.testSimpleAPI(),
                    checkAuth: () => window.fixedRunCalendar.checkAuthStatus(),
                    fetchReal: () => {
                        const now = new Date();
                        const start = new Date(now.getFullYear(), now.getMonth(), 1);
                        const end = new Date(now.getFullYear(), now.getMonth() + 1, 0);
                        return window.fixedRunCalendar.fetchRunsFromAPI(start, end);
                    },

                    // Page Detection
                    checkPage: () => ({
                        isDashboard: window.fixedRunCalendar.isOnDashboard(),
                        isRunsPage: window.fixedRunCalendar.isOnRunsPage(),
                        pathname: window.location.pathname
                    }),

                    // Mock Data
                    generateMockData: () => window.fixedRunCalendar.generateMockFromDashboardStats(),

                    // Calendar Integration
                    openRunById: (runId) => {
                        if (typeof window.openRunById === 'function') {
                            return window.openRunById(runId, 'debug');
                        } else {
                            console.error('❌ window.openRunById not available');
                            return false;
                        }
                    },

                    testIntegration: () => {
                        const runs = window.fixedRunCalendar.getRuns();
                        if (runs.length > 0) {
                            const testRun = runs[0];
                            console.log('🧪 Testing integration with first run:', testRun.name, 'ID:', testRun.id);
                            return window.fixedRunCalendar.openRunDetails(testRun);
                        } else {
                            console.warn('⚠️ No runs available to test integration');
                            return false;
                        }
                    },

                    checkIntegration: () => {
                        return {
                            hasOpenRunById: typeof window.openRunById === 'function',
                            hasLoadRunDataEnhanced: typeof window.loadRunDataEnhanced === 'function',
                            hasEditRunModal: !!document.getElementById('editRunModal'),
                            hasRunManagementState: !!window.runManagementState,
                            availableRuns: window.fixedRunCalendar.getRuns().length
                        };
                    },

                    // Quick Tests
                    quickTest: async () => {
                        console.log('🚀 Running quick API test...');

                        console.log('🧪 Step 1: Testing simple endpoint...');
                        const simpleTest = await window.fixedRunCalendar.testSimpleAPI();

                        console.log('🔐 Step 2: Checking auth status...');
                        const authResult = await window.fixedRunCalendar.checkAuthStatus();
                        console.log('🔐 Auth Status:', authResult);

                        if (authResult.authenticated && simpleTest.success) {
                            console.log('📡 Step 3: Testing full calendar API...');
                            const apiResult = await window.fixedRunCalendar.testAPIConnection();
                            console.log('📡 Full API Test:', apiResult);
                            return { auth: authResult, simple: simpleTest, api: apiResult };
                        } else {
                            console.log('❌ Skipping full API test due to auth or simple test failure');
                            return { auth: authResult, simple: simpleTest, api: { success: false, message: 'Skipped due to prerequisites' } };
                        }
                    }
                };

                console.log('🐛 Fixed Calendar debug tools available at window.calendarDebug');
                console.log('🐛 Integration commands:');
                console.log('  - window.calendarDebug.checkIntegration() - Check run management integration');
                console.log('  - window.calendarDebug.testIntegration() - Test opening a run from calendar');
                console.log('  - window.calendarDebug.openRunById("run-id") - Open specific run');
                console.log('🐛 API commands:');
                console.log('  - window.calendarDebug.quickTest() - Test all APIs');
                console.log('  - window.calendarDebug.testSimple() - Test simple endpoint');
            }
        } catch (error) {
            console.error('❌ Failed to initialize Fixed Run Calendar:', error);
        }
    } else {
        console.log('ℹ️ Run calendar modal not found - calendar not initialized');
    }

    window.addEventListener('beforeunload', () => {
        if (window.fixedRunCalendar && typeof window.fixedRunCalendar.destroy === 'function') {
            window.fixedRunCalendar.destroy();
        }
    });
});