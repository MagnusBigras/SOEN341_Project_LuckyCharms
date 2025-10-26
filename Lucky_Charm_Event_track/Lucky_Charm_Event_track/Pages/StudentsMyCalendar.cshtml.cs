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
            // Get the currently logged-in user's ID from the claims 
            var currentUserIdString = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (!int.TryParse(currentUserIdString, out int currentUserId))
            {
                Events = new List<CalendarEvent>();
                return;
            }

            // Fetch tickets that belong to this user, include Event details
            var userTickets = await _dbContext.Tickets
                .Where(t => t.UserAccountId == currentUserId)
                .Include(t => t.Event)
                .ThenInclude(e => e.Organizer)
                .ThenInclude(o => o.Account)
                .ToListAsync();

            // Map to calendar events
            Events = userTickets.Select(t => new CalendarEvent
            {
                id = t.Event.Id,
                title = t.Event.EventName,
                start = t.Event.StartTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                end = null,
                status = t.Event.isActive ? "active" : "cancelled",
                description = t.Event.EventDescription,
                location = $"{t.Event.Address}, {t.Event.City}",
                organizer = t.Event.Organizer != null && t.Event.Organizer.Account != null
                    ? $"{t.Event.Organizer.Account.FirstName} {t.Event.Organizer.Account.LastName}"
                    : "Unknown",
                category = t.Event.TicketType.ToString()
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
    }
}
