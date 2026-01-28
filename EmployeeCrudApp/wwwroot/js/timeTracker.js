class TimeTracker {
    constructor() {
        this.status = 'stopped'; // running, paused, stopped
        this.startTime = null;
        this.pausedTime = 0;
        this.elapsedTime = 0;
        this.taskName = '';
        this.dailyTotal = 0;
        this.interval = null;

        this.loadState();
        this.initUI();
        this.startUIUpdater();
    }

    loadState() {
        const state = JSON.parse(localStorage.getItem('tt_state') || '{}');
        const today = new Date().toDateString();
        const savedDate = localStorage.getItem('tt_date');

        if (savedDate !== today) {
            this.dailyTotal = 0;
            localStorage.setItem('tt_date', today);
            localStorage.setItem('tt_daily_total', '0');
        } else {
            this.dailyTotal = parseInt(localStorage.getItem('tt_daily_total') || '0');
        }

        this.status = state.status || 'stopped';
        this.startTime = state.startTime ? new Date(state.startTime) : null;
        this.pausedTime = state.pausedTime || 0;
        this.taskName = state.taskName || '';

        if (this.status === 'running' && this.startTime) {
            this.elapsedTime = Math.floor((new Date() - this.startTime) / 1000);
        } else {
            this.elapsedTime = this.pausedTime;
        }
    }

    saveState() {
        const state = {
            status: this.status,
            startTime: this.startTime,
            pausedTime: this.pausedTime,
            taskName: this.taskName
        };
        localStorage.setItem('tt_state', JSON.stringify(state));
        localStorage.setItem('tt_daily_total', this.dailyTotal.toString());
    }

    initUI() {
        document.addEventListener('DOMContentLoaded', () => {
            const taskInput = document.getElementById('tt-task-name');
            if (taskInput) {
                taskInput.value = this.taskName;
                taskInput.addEventListener('input', (e) => {
                    this.taskName = e.target.value;
                    this.saveState();
                });
            }
            this.updateUIVisibility();
            this.updateDisplay();
        });
    }

    startUIUpdater() {
        this.interval = setInterval(() => {
            if (this.status === 'running') {
                this.elapsedTime = Math.floor((new Date() - this.startTime) / 1000);
                this.updateDisplay();
            }
        }, 1000);
    }

    start() {
        const taskInput = document.getElementById('tt-task-name');
        this.taskName = taskInput ? taskInput.value : '';

        if (this.status === 'paused') {
            this.startTime = new Date(new Date() - this.pausedTime * 1000);
        } else {
            this.startTime = new Date();
            this.pausedTime = 0;
        }

        this.status = 'running';
        this.saveState();
        this.updateUIVisibility();
    }

    pause() {
        if (this.status === 'running') {
            this.status = 'paused';
            this.pausedTime = this.elapsedTime;
            this.saveState();
            this.updateUIVisibility();
        }
    }

    stop() {
        if (this.status !== 'stopped') {
            this.dailyTotal += this.elapsedTime;
            this.status = 'stopped';
            this.elapsedTime = 0;
            this.pausedTime = 0;
            this.startTime = null;
            this.saveState();
            this.updateUIVisibility();
            this.updateDisplay();
        }
    }

    updateUIVisibility() {
        const startBtn = document.getElementById('tt-start-btn');
        const pauseBtn = document.getElementById('tt-pause-btn');
        const stopBtn = document.getElementById('tt-stop-btn');
        const badge = document.getElementById('timer-badge');

        if (!startBtn) return;

        if (this.status === 'running') {
            startBtn.classList.add('d-none');
            pauseBtn.classList.remove('d-none');
            stopBtn.classList.remove('d-none');
            badge.classList.remove('d-none');
        } else if (this.status === 'paused') {
            startBtn.classList.remove('d-none');
            startBtn.innerHTML = '<i class="bi bi-play-fill me-1"></i> Resume';
            pauseBtn.classList.add('d-none');
            stopBtn.classList.remove('d-none');
            badge.classList.remove('d-none');
        } else {
            startBtn.classList.remove('d-none');
            startBtn.innerHTML = '<i class="bi bi-play-fill me-1"></i> Start';
            pauseBtn.classList.add('d-none');
            stopBtn.classList.add('d-none');
            badge.classList.add('d-none');
        }
    }

    updateDisplay() {
        const display = document.getElementById('tt-display');
        const dailyTotalDisplay = document.getElementById('tt-daily-total');
        if (display) display.innerText = this.formatTime(this.elapsedTime);
        if (dailyTotalDisplay) dailyTotalDisplay.innerText = this.formatTime(this.dailyTotal + (this.status === 'running' ? this.elapsedTime : (this.status === 'paused' ? this.pausedTime : 0)));
    }

    formatTime(seconds) {
        const h = Math.floor(seconds / 3600);
        const m = Math.floor((seconds % 3600) / 60);
        const s = seconds % 60;
        return [h, m, s].map(v => v < 10 ? '0' + v : v).join(':');
    }
}

window.timeTracker = new TimeTracker();
