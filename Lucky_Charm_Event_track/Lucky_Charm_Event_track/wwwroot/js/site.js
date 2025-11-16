
// Dynamically fetch and update analytics values for a specific event using data attribute
// Prevent the file from executing twice (avoid redeclaration errors when site.js is included more than once)
if (window.__siteJsLoaded) {
    console.debug('site.js already initialized; skipping duplicate execution');
} else {
    window.__siteJsLoaded = true;

document.addEventListener('DOMContentLoaded', () => {
    const container = document.getElementById('eventsContainer');
    if (!container) return;

    // Fetch all events (basic info)
    async function fetchEvents() {
        try {
            const res = await fetch('/api/events/my');
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
            const fullEvent = event.prices ? event : await fetchEventDetails(event.id);
            if (!fullEvent) continue; 

            const { eventPrice, eventType } = getEventPriceAndType(fullEvent);

            const card = document.createElement('div');
            card.classList.add('event-card');

            const startDate = new Date(fullEvent.startTime);
            const today = new Date();
            today.setHours(0, 0, 0, 0); 

            // --- Auto-hide finished events ---
            if (startDate < today) {
                fullEvent.isActive = false;
                try {
                    await fetch('/api/events/update-visibility', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ id: fullEvent.id, isActive: false })
                    });
                } catch (err) {
                    console.error('Failed to auto-hide finished event:', err);
                }
            }

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

            // --- Visibility button ---
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

            container.appendChild(card);
        }
    }

    // Initial fetch from backend
    fetchEvents();
});

}