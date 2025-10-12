if (document.getElementById('eventsContainer')) {

  const container = document.getElementById('eventsContainer');

    events.forEach(event => {
        const card = document.createElement('div');
        card.classList.add('event-card');

        card.innerHTML = `
          <div class="event-info">
            <h2 class="event-name">${event.name}</h2>
            <p class="event-date">Date: ${event.date}</p>
            <p class="event-time">Time: ${event.time}</p>
            <p class="event-location">Location: ${event.location}</p>
          </div>
          <div class="event-actions">
            <button class="analytics-btn">Analytics</button>
            <button class="tools-btn">Tools</button>
          </div>
        `;

        card.querySelector('.analytics-btn').addEventListener('click', () => {
            window.location.href = '/analytics_page'; // no query string needed
        });

        container.appendChild(card);
    });
}


function getQueryParam(param) {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get(param);
}


// Dynamically fetch and update analytics values for a specific event using data attribute

const eventId = document.body.getAttribute('data-event-id');

if (document.getElementById('totalRevenue') && eventId && eventId !== '0') {
    console.log('Fetching metrics for eventId:', eventId);
    fetch(`/api/Event/${eventId}`)
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

