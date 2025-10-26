// Place this at the top level, before renderEvents
function renderEventMetrics(event) {
    if (!event.metric) return '';
    return `
        <div class="event-metrics">
            <p>Total Revenue: $${event.metric.totalRevenue ?? 0}</p>
            <p>New Attendees: ${event.metric.newAttendees ?? 0}</p>
            <p>Last Month Revenue: $${event.metric.lastMonthRevenue ?? 0}</p>
            <p>Last Month Attendees: ${event.metric.lastMonthAttendees ?? 0}</p>
            <p>Remaining Capacity: ${event.metric.lastRemaining ?? 0}</p>
        </div>
    `;
}
function getQueryParam(param) {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get(param);
}





// Dynamically fetch and update analytics values for a specific event using data attribute

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

                document.getElementById('totalRevenue').textContent = `$${totalRevenue}`;
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

                document.getElementById('newAttendees').textContent = newAttendees;
                const attendeesChangeElement = document.getElementById('attendeesChange');
                if (attendeesChangeElement) {
                    if (attendeesChange >= 0) {
                        attendeesChangeElement.textContent = `▲ ${attendeesPercent}% from last month`;
                    } else {
                        attendeesChangeElement.textContent = `▼ ${Math.abs(attendeesPercent)}% from last month`;
                    }
                }

                // Remaining capacity (numEvents card)
                const numEvents = event.metric.lastRemaining ?? 0;
                document.getElementById('numEvents').textContent = numEvents;
                // Optionally update eventsChange if you have lastMonthEvents
            } else {
                console.warn('No metric found in API response:', event);
            }
        })
        .catch(error => {
            console.error('Error fetching event metrics:', error);
        });
}

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
}





document.addEventListener('DOMContentLoaded', () => {
    const container = document.getElementById('eventsContainer');
    if (!container) return;

    // Fetch all events (basic info)
    async function fetchEvents() {
        try {
            const res = await fetch('/api/events/all');
            if (!res.ok) throw new Error('Network response was not ok');
            let events = await res.json();

            // Handle $values if coming from EF serialization
            if (events && events.$values) events = events.$values;

            await renderEvents(events);
        } catch (err) {
            console.error('Error fetching events:', err);
            container.innerHTML = '<p>Unable to load events.</p>';
        }
    }

    // Fetch full details for a single event (including prices)
    async function fetchEventDetails(eventId) {
        try {
            const res = await fetch(`/api/events/${eventId}`);
            if (!res.ok) throw new Error('Failed to fetch event details');
            return await res.json();
        } catch (err) {
            console.error(`Error fetching event ${eventId} details:`, err);
            return null;
        }
    }
function getEventPriceAndType(event) {
    let price = 0;

    if (event.prices && event.prices.$values) {
        event.prices = event.prices.$values;
    }

    if (event.prices && Array.isArray(event.prices) && event.prices.length > 0) {
        const p = event.prices[0];
        price = p.price ?? p.Price ?? 0; 
    }

    // Determine event type based on ticketType enum
    // 0 = Paid, 3 = Free
    const eventType = (event.ticketType === 0) ? 'Paid' : 'Free';

    return {
        eventPrice: price > 0 ? `$${price.toFixed(2)}` : '0.00$',
        eventType
    };
}


    async function renderEvents(events) {
        container.innerHTML = '';

        if (!events || events.length === 0) {
            container.innerHTML = `<div class="no-events-message"><p>There are no events currently.</p></div>`;
            return;
        }

        for (const event of events) {
            // Fetch full event details if prices is null
            const fullEvent = event.prices ? event : await fetchEventDetails(event.id);
            if (!fullEvent) continue; 

            const { eventPrice, eventType } = getEventPriceAndType(fullEvent);

            const card = document.createElement('div');
            card.classList.add('event-card');

            const startDate = new Date(fullEvent.startTime);
            const formattedDate = startDate.toLocaleDateString([], { year: 'numeric', month: 'long', day: 'numeric' });
            const formattedTime = startDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

            card.innerHTML = `
                <div class="event-header">
                    <h2 class="event-name">${fullEvent.eventName}</h2>
                    <button class="edit-btn" title="Edit Event">
                        <img src="./Images/7965593328e6cb27ffb10553f680443da25fad1a.png" alt="Edit" class="edit-icon">
                    </button>
                </div>
                <div class="event-info">
                    <p class="event-date">Date: ${formattedDate}</p>
                    <p class="event-time">Time: ${formattedTime}</p>
                    <p class="event-location">Location: ${fullEvent.address || 'N/A'}</p>
                    <p class="event-type">Type: ${eventType}</p>
                    <p class="event-price">Price: ${eventPrice}</p>
                    <p class="event-category">Category: ${fullEvent.category || 'N/A'}</p>
                    <p class="event-description">Description: ${fullEvent.eventDescription || 'No description provided.'}</p>
                </div>
                <div class="event-actions">
                    <button class="analytics-btn">Analytics</button>
                    <button class="tools-btn">Tools</button>
                    <button class="visibility-btn">
                        <span class="visibility-text">${fullEvent.isActive ? 'Visible' : 'Hidden'}</span>
                    </button>
                    <button class="cancel-event-btn">Cancel</button>
                </div>
            `;

            // --- Edit button ---
            card.querySelector('.edit-btn').addEventListener('click', () => {
                window.location.href = `/EditEventPage?eventId=${fullEvent.id}`;
            });

            // --- Cancel button ---
            card.querySelector('.cancel-event-btn').addEventListener('click', async () => {
                if (!confirm(`Are you sure you want to cancel "${fullEvent.eventName}"?`)) return;
                try {
                    await fetch('/api/events/delete', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(fullEvent.id)
                    });
                    fetchEvents();
                } catch (err) {
                    console.error('Error cancelling event:', err);
                }
            });

        const visibilityBtn = card.querySelector('.visibility-btn');
        const visibilityText = visibilityBtn.querySelector('.visibility-text');

        card.style.opacity = fullEvent.isActive ? 1 : 0.5;
        visibilityBtn.style.backgroundColor = fullEvent.isActive ? '#4dbb4dff' : '#912338';

        visibilityBtn.addEventListener('click', async () => {
            fullEvent.isActive = !fullEvent.isActive;

            visibilityText.textContent = fullEvent.isActive ? 'Visible' : 'Hidden';
            card.style.opacity = fullEvent.isActive ? 1 : 0.5;
            visibilityBtn.style.backgroundColor = fullEvent.isActive ? '#4dbb4dff' : '#912338';

            try {
                const res = await fetch('/api/events/update-visibility', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ id: fullEvent.id, isActive: fullEvent.isActive })
                });
                if (!res.ok) throw new Error('Failed to update visibility');
            } catch (err) {
                console.error('Error updating visibility:', err);

                fullEvent.isActive = !fullEvent.isActive;
                visibilityText.textContent = fullEvent.isActive ? 'Visible' : 'Hidden';
                card.style.opacity = fullEvent.isActive ? 1 : 0.5;
                visibilityBtn.style.backgroundColor = fullEvent.isActive ? '#4dbb4dff' : '#912338';
                alert('Failed to update visibility. Check console for details.');
            }
        });


            // --- Analytics button ---
            card.querySelector('.analytics-btn').addEventListener('click', () => {
                window.location.href = `/EventAnalytics?eventId=${fullEvent.id}&eventName=${encodeURIComponent(fullEvent.eventName)}`;
            });

            // --- Tools button ---
            card.querySelector('.tools-btn').addEventListener('click', () => {
                window.location.href = `/OrganizerTools?eventId=${fullEvent.id}&eventName=${encodeURIComponent(fullEvent.eventName)}`;
            });

            card.querySelector('.tools-btn').addEventListener('click', () => {
                window.location.href = `/OrganizerTools?eventId=${event.id}&eventName=${encodeURIComponent(event.eventName)}`;
            });


            container.appendChild(card);
        }
    }

    // Initial fetch call
    fetchEvents();
});
