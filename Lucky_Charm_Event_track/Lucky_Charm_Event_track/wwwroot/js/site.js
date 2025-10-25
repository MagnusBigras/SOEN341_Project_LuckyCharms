// Place this at the top level, before renderEvents
/*function renderEventMetrics(event) {
    if (!event.metric) return '';
    // Calculate analytics
    const totalCapacity = event.metric.totalCapacity ?? 0;
    const usedCapacity = event.metric.usedCapacity ?? 0;
    const attendanceRate = totalCapacity > 0 ? ((usedCapacity / totalCapacity) * 100).toFixed(1) : 0;
    const ticketsIssued = usedCapacity;
    const remainingCapacity = totalCapacity - usedCapacity;
    const newAttendees = event.metric.newAttendees ?? 0;
    return `
        <div class="event-metrics">
            <p>Total Revenue: $${event.metric.totalRevenue ?? 0}</p>
            <p>New Attendees: ${newAttendees}</p>
            <p>Tickets Issued: ${ticketsIssued}</p>
            <p>Remaining Capacity: ${remainingCapacity}</p>
            <p>Attendance Rate: <span class="attendance-rate-label" style="font-size:0.9em;color:#666;">${attendanceRate}%</span></p>
        </div>
    `;
}
function getQueryParam(param) {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get(param);
}
*/




// Dynamically fetch and update analytics values for a specific event using data attribute
// Prevent the file from executing twice (avoid redeclaration errors when site.js is included more than once)
if (window.__siteJsLoaded) {
    console.debug('site.js already initialized; skipping duplicate execution');
} else {
    window.__siteJsLoaded = true;


const eventId = document.body.getAttribute('data-event-id');

if (document.getElementById('totalRevenue') && eventId && eventId !== '0') {
    console.log('Fetching metrics for eventId:', eventId);
            fetch(`/api/events/${eventId}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(event => {
            console.log('API response:', event);
            if (event && event.metric) {
                // Current and last month revenue
                const totalRevenue = event.metric.totalRevenue ?? 0;
                const lastMonthRevenue = event.metric.lastMonthRevenue ?? 0;
                const revenueChange = totalRevenue - lastMonthRevenue;
                const revenuePercent = lastMonthRevenue !== 0 ? ((revenueChange / lastMonthRevenue) * 100).toFixed(1) : 0;

                const totalRevenueElem = document.getElementById('totalRevenue');
                if (totalRevenueElem) totalRevenueElem.textContent = `$${totalRevenue}`;
                const revenueChangeElement = document.getElementById('revenueChange');
                if (revenueChangeElement) {
                    if (revenueChange >= 0) {
                        revenueChangeElement.textContent = `▲ ${revenuePercent}% from last month`;
                    } else {
                        revenueChangeElement.textContent = `▼ ${Math.abs(revenuePercent)}% from last month`;
                    }
                }

                // New and last month attendees
                const newAttendees = event.metric.newAttendees ?? 0;
                const lastMonthAttendees = event.metric.lastMonthAttendees ?? 0;
                const attendeesChange = newAttendees - lastMonthAttendees;
                const attendeesPercent = lastMonthAttendees !== 0 ? ((attendeesChange / lastMonthAttendees) * 100).toFixed(1) : 0;

                const newAttendeesElem = document.getElementById('newAttendees');
                if (newAttendeesElem) newAttendeesElem.textContent = newAttendees;
                const attendeesChangeElement = document.getElementById('attendeesChange');
                if (attendeesChangeElement) {
                    if (attendeesChange >= 0) {
                        attendeesChangeElement.textContent = `▲ ${attendeesPercent}% from last month`;
                    } else {
                        attendeesChangeElement.textContent = `▼ ${Math.abs(attendeesPercent)}% from last month`;
                    }
                }

                // Remaining capacity (numEvents card)
                const numEvents = event.metric.currentCapacity ?? 0;
                const numEventsElem = document.getElementById('numEvents');
                if (numEventsElem) numEventsElem.textContent = numEvents;

                
            } else {
                console.warn('No metric found in API response:', event);
            }
        })
        .catch(error => {
            console.error('Error fetching event metrics:', error);
        });
}







/*
if (document.getElementById('revenueChart')) {
    const revenueChart = new Chart(document.getElementById('revenueChart').getContext('2d'), {
        type: 'bar',
        data: {
            labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
            datasets: [{
                label: 'Revenue',
                data: [12000, 15000, 14000, 17000, 16000, 18000, 17500, 19000, 20000, 21000, 22000, 23000],
                backgroundColor: '#912338'
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: { display: false }
            }
        }
    });
}

if (document.getElementById('attendanceChart')) {
    const attendanceChart = new Chart(document.getElementById('attendanceChart').getContext('2d'), {
        type: 'line',
        data: {
            labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
            datasets: [{
                label: 'Attendance',
                data: [50, 75, 60, 80, 70, 90, 85, 95, 100, 105, 110, 120],
                borderColor: '#912338',
                backgroundColor: 'rgba(145, 35, 56, 0.2)',
                fill: true
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: { display: false }
            }
        }
    });
}*/// CHARTS COMMENTED OUT FOR THE MOMENT


document.addEventListener('DOMContentLoaded', () => {
    const container = document.getElementById('eventsContainer');
    if (!container) return;

    // Fetch all active events from backend
    async function fetchEvents() {
        try {
                        const res = await fetch('/api/events/active');
            if (!res.ok) throw new Error('Network response was not ok');
            const events = await res.json();
            renderEvents(events);
        } catch (err) {
            console.error('Error fetching events:', err);
            container.innerHTML = '<p>Unable to load events.</p>';
        }
    }

    function renderEvents(events) {
        container.innerHTML = '';
        // Handle $values if present (from ReferenceHandler.Preserve)
        if (events && events.$values) {
            events = events.$values;
        }
        if (!events || events.length === 0) {
            container.innerHTML = `<div class="no-events-message">
                <p>There are no events currently.</p>
            </div>`;
            return;
        }

        events.forEach(event => {
            const card = document.createElement('div');
            card.classList.add('event-card');

            const startDate = new Date(event.startTime);
            const formattedDate = startDate.toLocaleDateString([], { year: 'numeric', month: 'long', day: 'numeric' });
            const formattedTime = startDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

            card.innerHTML = `
                <div class="event-header">
                    <h2 class="event-name">${event.eventName}</h2>
                    <button class="edit-btn" title="Edit Event">
                        <img src="./Images/7965593328e6cb27ffb10553f680443da25fad1a.png" alt="Edit" class="edit-icon">
                    </button>
                </div>
                <div class="event-info">
                    <p class="event-date">Date: ${formattedDate}</p>
                    <p class="event-time">Time: ${formattedTime}</p>
                    <p class="event-location">Location: ${event.address || 'N/A'}</p>
                    <p class="event-type">Type: ${event.eventType || 'N/A'}</p>
                    <p class="event-description">Description: ${event.eventDescription || 'No description provided.'}</p>
                </div>
                ${renderEventMetrics(event)}
                <div class="event-actions">
                    <button class="analytics-btn">Analytics</button>
                    <button class="tools-btn">Tools</button>
                    <button class="visibility-btn">
                        <span class="visibility-text">${event.isActive ? 'Visible' : 'Hidden'}</span>
                    </button>
                    <button class="cancel-event-btn">Cancel</button>
                </div>
            `;

            // Edit button
            card.querySelector('.edit-btn').addEventListener('click', () => {
                window.location.href = `/EditEvent?eventId=${event.id}`;
            });

            // Cancel button
            card.querySelector('.cancel-event-btn').addEventListener('click', async () => {
                if (!confirm(`Are you sure you want to cancel "${event.eventName}"?`)) return;
                try {
                                await fetch('/api/events/delete', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(event.id)
                    });
                    fetchEvents(); // Refresh list after cancellation
                } catch (err) {
                    console.error('Error cancelling event:', err);
                }
            });

            // Visibility toggle
            const visibilityBtn = card.querySelector('.visibility-btn');
            const visibilityText = visibilityBtn.querySelector('.visibility-text');
            visibilityBtn.addEventListener('click', async () => {
                event.isActive = !event.isActive;
                visibilityText.textContent = event.isActive ? 'Visible' : 'Hidden';
                try {
                                await fetch('/api/events/update', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(event)
                    });
                } catch (err) {
                    console.error('Error updating visibility:', err);
                }
            });

            // Analytics button
            card.querySelector('.analytics-btn').addEventListener('click', () => {
                window.location.href = `/EventAnalytics?eventId=${event.id}&eventName=${encodeURIComponent(event.eventName)}`;
            });

            container.appendChild(card);
        });
    }

    // Initial fetch from backend
        fetchEvents();
    });

    } // end guard: only initialize site.js once

