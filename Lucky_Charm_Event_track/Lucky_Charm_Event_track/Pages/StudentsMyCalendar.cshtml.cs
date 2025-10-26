using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lucky_Charm_Event_track.Pages
{
    public class StudentsMyCalendarModel : PageModel
    {
        public List<CalendarEvent> Events { get; set; } = new();

        public void OnGet()
        {
            Events = new List<CalendarEvent>
            {
                new CalendarEvent { id = 1, title = "Hackathon", start = "2025-10-17T09:00:00", end = "2025-10-17T17:00:00", status = "active", description = "24-hour coding challenge", location = "Main Hall", organizer = "Tech Club", category = "Competition" },
                new CalendarEvent { id = 2, title = "Club Meeting", start = "2025-11-21T14:00:00", status = "active", description = "Monthly student club meeting", location = "Room 101", organizer = "Student Union", category = "Meeting" },
                new CalendarEvent { id = 3, title = "Midterm Study Session", start = "2025-10-10T18:00:00", status = "active", description = "Study session for midterms", location = "Library", organizer = "Academic Support", category = "Study" },
                new CalendarEvent { id = 4, title = "Career Fair", start = "2025-09-30T10:00:00", end = "2025-09-30T16:00:00", status = "cancelled", description = "Networking and recruitment fair", location = "Conference Hall", organizer = "Co-op Office", category = "Career" }
            };
        }

        public string GetEventsJson() => JsonSerializer.Serialize(Events);
    }

    public class CalendarEvent
    {
        public int id { get; set; }
        public string title { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public string status { get; set; } // "active" or "cancelled"
        public string description { get; set; }
        public string location { get; set; }
        public string organizer { get; set; }
        public string category { get; set; }
    }
}
