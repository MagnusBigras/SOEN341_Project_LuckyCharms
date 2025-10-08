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

if (document.getElementById('totalRevenue')) {
    const totalRevenue = 12345;
    const lastMonthRevenue = 11738;
    const revenueChange = totalRevenue - lastMonthRevenue;
    const revenuePercent = ((revenueChange / lastMonthRevenue) * 100).toFixed(1);

    document.getElementById('totalRevenue').textContent = `$${totalRevenue}`;
    const revenueChangeElement = document.getElementById('revenueChange');

    if (revenueChange >= 0) {
        revenueChangeElement.textContent = `▲ ${revenuePercent}% from last month`;
    }
    else {
        revenueChangeElement.textContent = `▼ ${Math.abs(revenuePercent)}% from last month`;
    }

    const newAttendees = 250;
    const lastMonthAttendees = 230;
    const attendeesChange = newAttendees - lastMonthAttendees;
    const attendeesPercent = ((attendeesChange / lastMonthAttendees) * 100).toFixed(1);

    document.getElementById('newAttendees').textContent = newAttendees;
    const attendeesChangeElement = document.getElementById('attendeesChange');

    if (attendeesChange >= 0) {
        attendeesChangeElement.textContent = `▲ ${attendeesPercent}% from last month`;
    } 
    else {
        attendeesChangeElement.textContent = `▼ ${Math.abs(attendeesPercent)}% from last month`;
    }

    const numEvents = 15;
    const lastMonthEvents = 12;
    const eventsChange = numEvents - lastMonthEvents;
    const eventsPercent = ((eventsChange / lastMonthEvents) * 100).toFixed(1);

    document.getElementById('numEvents').textContent = numEvents;
    const eventsChangeElement = document.getElementById('eventsChange');

    if (eventsChange >= 0) {
        eventsChangeElement.textContent = `▲ ${eventsPercent}% from last month`;
    } 
    else {
        eventsChangeElement.textContent = `▼ ${Math.abs(eventsPercent)}% from last month`;
    }
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

