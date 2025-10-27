using Lucky_Charm_Event_track.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lucky_Charm_Event_track.Pages
{
    public class StudentsMyCalendarModel : PageModel
    {
        private readonly WebAppDBContext _dbContext;

        public List<CalendarEvent> Events { get; set; } = new();

        public StudentsMyCalendarModel(WebAppDBContext context)
        {
            _dbContext = context;
        }

public async Task OnGetAsync()
{
    int currentUserId = 1;

    // Fetch tickets for this user where the event is not hidden
    var userTickets = await _dbContext.Tickets
        .Where(t => t.UserAccountId == currentUserId && !t.IsHiddenInCalendar)
        .Include(t => t.Event)
        .ThenInclude(e => e.Organizer)
        .ThenInclude(o => o.Account)
        .ToListAsync();

    // Group by EventId so we only display one event per event
    var uniqueEvents = userTickets
        .GroupBy(t => t.EventId)
        .Select(g => g.First()) 
        .ToList();

    Events = uniqueEvents.Select(t => new CalendarEvent
    {
        id = t.Id,
        title = t.Event.EventName,
        start = t.Event.StartTime.ToString("yyyy-MM-ddTHH:mm:ss"),
        end = null,
        status = t.Event.isActive ? "active" : "cancelled",
        description = t.Event.EventDescription,
        location = $"{t.Event.Address}, {t.Event.City}",
        organizer = t.Event.Organizer != null && t.Event.Organizer.Account != null
            ? $"{t.Event.Organizer.Account.FirstName} {t.Event.Organizer.Account.LastName}"
            : "Unknown",
        category = t.Event.Category,
        eventId = t.Event.Id
    }).ToList();
}

        public string GetEventsJson() => JsonSerializer.Serialize(Events);
    }

    public class CalendarEvent
    {
        public int id { get; set; }       
        public string title { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public string status { get; set; }
        public string description { get; set; }
        public string location { get; set; }
        public string organizer { get; set; }
        public string category { get; set; }
        public int eventId { get; set; }  // used for hiding all tickets of an event
    }
}
