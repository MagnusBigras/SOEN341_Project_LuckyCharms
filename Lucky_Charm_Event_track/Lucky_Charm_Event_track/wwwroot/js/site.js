/ ---------------------- Events Page Script ---------------------- //
document.addEventListener('DOMContentLoaded', () => {
    const eventsContainer = document.getElementById('eventsContainer');
    if (!eventsContainer) return;

    // Fetch all active events from controller
    async function fetchEvents() {
        try {
            const res = await fetch('/api/event/active'); 
            const visibleEvents = await res.json();
            renderEvents(visibleEvents);
        } catch (err) {
            console.error('Error fetching events:', err);
            eventsContainer.innerHTML = '<p>Unable to load events.</p>';
        }
    }

    function renderEvents(visibleEvents) {
        eventsContainer.innerHTML = '';

        if (!visibleEvents || visibleEvents.length === 0) {
            eventsContainer.innerHTML = `<div class="no-events-message">
                <p>There are no events currently.</p>
            </div>`;
            return;
        }

        visibleEvents.forEach(event => {
            const card = document.createElement('div');
            card.classList.add('event-card');

            card.innerHTML = `
                <div class="event-header">
                    <h2 class="event-name">${event.name}</h2>
                    <button class="edit-btn" title="Edit Event">
                        <img src="~/Images/7965593328e6cb27ffb10553f680443da25fad1a.png" alt="Edit" class="edit-icon">
                    </button>
                </div>
                <div class="event-info">
                    <p class="event-date"> Date: ${event.date}</p>
                    <p class="event-time"> Time: ${event.time}</p>
                    <p class="event-location"> Location: ${event.location}</p>
                    <p class="event-type"> Type: ${event.type}</p>
                    <p class="event-description">Description: ${event.description}</p>
                </div>
                <div class="event-actions">
                    <button class="analytics-btn">Analytics</button>
                    <button class="tools-btn">Tools</button>
                    <button class="visibility-btn">
                        <span class="visibility-text">${event.isActive ? 'Visible' : 'Hidden'}</span>
                    </button>
                    <button class="cancel-event-btn">Cancel</button>
                </div>
            `;

            // Edit button navigates to Razor Page EditEvent
            card.querySelector('.edit-btn').addEventListener('click', () => {
                window.location.href = `/EditEvent?eventId=${event.id}`;
            });

            // Analytics button navigates to Razor Page EventAnalytics
            card.querySelector('.analytics-btn').addEventListener('click', () => {
                window.location.href = `/EventAnalytics?eventId=${event.id}&eventName=${encodeURIComponent(event.name)}`;
            });

            // Cancel event
            card.querySelector('.cancel-event-btn').addEventListener('click', async () => {
                if (!confirm(`Are you sure you want to cancel "${event.name}"?`)) return;

                try {
                    await fetch('/api/event/delete', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(event.id)
                    });
                    fetchEvents(); // refresh list after deletion
                } catch (err) {
                    console.error('Error cancelling event:', err);
                }
            });

            // Toggle visibility
            const visibilityBtn = card.querySelector('.visibility-btn');
            const visibilityText = visibilityBtn.querySelector('.visibility-text');

            visibilityBtn.addEventListener('click', async () => {
                event.isActive = !event.isActive;
                visibilityText.textContent = event.isActive ? 'Visible' : 'Hidden';

                try {
                    await fetch('/api/event/update', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(event)
                    });
                } catch (err) {
                    console.error('Error updating visibility:', err);
                }
            });

            eventsContainer.appendChild(card);
        });
    }

    // Initial fetch
    fetchEvents();
});





// ---------------------- Analytics Page Script ---------------------- //
document.addEventListener('DOMContentLoaded', () => {
    const eventNameEl = document.getElementById('eventName');
    const revenueChartEl = document.getElementById('revenueChart');
    const attendanceChartEl = document.getElementById('attendanceChart');
    const analyticsDateEl = document.getElementById('analyticsDate');

    if (!eventNameEl || !revenueChartEl || !attendanceChartEl || !analyticsDateEl) return;

    const urlParams = new URLSearchParams(window.location.search);
    const eventId = urlParams.get('eventId');
    const eventName = urlParams.get('eventName') || "Unknown Event";
    eventNameEl.textContent = eventName;

    let revenueChartInstance = null;
    let attendanceChartInstance = null;

    function fetchAndRender(date) {
        const selectedDate = new Date(date);
        const month = selectedDate.getMonth() + 1; // Month for top metrics
        const year = selectedDate.getFullYear();    // Year for charts

        fetch(`/api/analytics?eventId=${eventId}&month=${month}&year=${year}`)
            .then(res => res.json())
            .then(data => {
                // Top Metrics (Monthly)
                const revenueChange = data.totalRevenue - data.lastMonthRevenue;
                const revenuePercent = ((revenueChange / data.lastMonthRevenue) * 100).toFixed(1);
                document.getElementById('totalRevenue').textContent = "$" + data.totalRevenue;
                document.getElementById('revenueChange').textContent = revenueChange >= 0 
                    ? `▲ ${revenuePercent}% from last month` 
                    : `▼ ${Math.abs(revenuePercent)}% from last month`;

                const attendeesChange = data.newAttendees - data.lastMonthAttendees;
                const attendeesPercent = ((attendeesChange / data.lastMonthAttendees) * 100).toFixed(1);
                document.getElementById('newAttendees').textContent = data.newAttendees;
                document.getElementById('attendeesChange').textContent = attendeesChange >= 0 
                    ? `▲ ${attendeesPercent}% from last month` 
                    : `▼ ${Math.abs(attendeesPercent)}% from last month`;

                const remaining = data.totalCapacity - data.usedCapacity;
                const lastRemaining = data.lastRemaining || remaining;
                const capacityChange = remaining - lastRemaining;
                const capacityPercent = ((capacityChange / lastRemaining) * 100).toFixed(1);
                document.getElementById('remCapacity').textContent = remaining;
                document.getElementById('capacityChange').textContent = capacityChange >= 0
                    ? `▲ ${capacityPercent}% since last month`
                    : `▼ ${Math.abs(capacityPercent)}% since last month`;

                // Yearly Charts
                if (revenueChartInstance) revenueChartInstance.destroy();
                if (attendanceChartInstance) attendanceChartInstance.destroy();

                revenueChartInstance = new Chart(revenueChartEl.getContext('2d'), {
                    type: 'bar',
                    data: {
                        labels: ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'],
                        datasets: [{ label: 'Revenue', data: data.revenueByMonth, backgroundColor: '#912338' }]
                    },
                    options: { responsive: true, plugins: { legend: { display: false } } }
                });

                attendanceChartInstance = new Chart(attendanceChartEl.getContext('2d'), {
                    type: 'line',
                    data: {
                        labels: ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'],
                        datasets: [{
                            label: 'Attendance',
                            data: data.attendanceByMonth,
                            borderColor: '#912338',
                            backgroundColor: 'rgba(145, 35, 56, 0.2)',
                            fill: true
                        }]
                    },
                    options: { responsive: true, plugins: { legend: { display: false } } }
                });
            })
            .catch(err => console.error("Error fetching analytics:", err));
    }

    fetchAndRender(analyticsDateEl.value);

    analyticsDateEl.addEventListener('change', () => {
        fetchAndRender(analyticsDateEl.value);
    });
});
