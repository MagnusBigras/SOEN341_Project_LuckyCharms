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
            // Hardcoded sample events with unique IDs
            Events = new List<CalendarEvent>
            {
                new CalendarEvent { id = 1, title = "Hackathon", start = "2025-10-17T09:00:00", end = "2025-10-17T17:00:00", status = "active", description = "24-hour coding challenge", location = "Main Hall" },
                new CalendarEvent { id = 2, title = "Club Meeting", start = "2025-11-21T14:00:00", status = "active", description = "Monthly student club meeting", location = "Room 101" },
                new CalendarEvent { id = 3, title = "Midterm Study Session", start = "2025-10-24T18:00:00", status = "active", description = "Study session for midterms", location = "Library" }
            };
        }

        public string GetEventsJson() => JsonSerializer.Serialize(Events);
    }

public class CalendarEvent
{
    public int id { get; set; }             // Unique identifier
    public string title { get; set; }       // Event title
    public string start { get; set; }       // Start date/time
    public string end { get; set; }         // Optional end date/time
    public string status { get; set; }      // "active" or "cancelled"
    public string description { get; set; } // Event details
    public string location { get; set; }    // Event location
}

}
