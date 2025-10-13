using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace Lucky_Charm_Event_track.Pages
{
    public class StudentsEventsOfferedModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortByDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortByPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortByPopularity { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortByCategory { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortByOrganization { get; set; }

        public List<EventItem> AllEvents { get; set; } = new();
        public List<EventItem> FilteredEvents { get; set; } = new();

        public void OnGet()
        {
            // Sample events with additional properties
            AllEvents = new List<EventItem> {
                new EventItem { Name = "Tech Expo 2025", Date = "2025-10-20", Location = "Main Hall", Price = 10, Description = "Tech showcase.", startTime = "09:00", endTime = "17:00", TicketsLeft = 50, Category = "Technology", Organization = "TechOrg", Popularity = 80, isActive = true },
                new EventItem { Name = "Career Fair", Date = "2025-10-22", Location = "Conference Room A", Price = 0, Description = "Meet top employers.", startTime = "10:00", endTime = "16:00", TicketsLeft = 100, Category = "Career", Organization = "CareerCenter", Popularity = 120, isActive = true },
                new EventItem { Name = "AI Workshop", Date = "2025-10-25", Location = "Lab 3", Price = 25, Description = "Learn AI hands-on.", startTime = "13:00", endTime = "16:00", TicketsLeft = 20, Category = "Workshop", Organization = "AI Club", Popularity = 60, isActive = true },
                new EventItem { Name = "Music Night", Date = "2025-11-01", Location = "Auditorium", Price = 15, Description = "Live performances.", startTime = "19:00", endTime = "22:00", TicketsLeft = 200, Category = "Entertainment", Organization = "MusicSociety", Popularity = 200, isActive = true }
            };

            // Filter active events
            var events = AllEvents.Where(e => e.isActive);

            // Apply search
            if (!string.IsNullOrEmpty(SearchQuery))
                events = events.Where(e => e.Name.ToLower().Contains(SearchQuery.ToLower()));

            // Remove duplicates
            FilteredEvents = events
                .GroupBy(e => e.Name + e.Date)
                .Select(g => g.First())
                .ToList();

            // Apply sorting
            if (!string.IsNullOrEmpty(SortByDate))
                FilteredEvents = SortByDate == "asc" ? FilteredEvents.OrderBy(e => e.Date).ToList() : FilteredEvents.OrderByDescending(e => e.Date).ToList();

            if (!string.IsNullOrEmpty(SortByPrice))
                FilteredEvents = SortByPrice == "asc" ? FilteredEvents.OrderBy(e => e.Price).ToList() : FilteredEvents.OrderByDescending(e => e.Price).ToList();

            if (!string.IsNullOrEmpty(SortByPopularity))
                FilteredEvents = SortByPopularity == "asc" ? FilteredEvents.OrderBy(e => e.Popularity).ToList() : FilteredEvents.OrderByDescending(e => e.Popularity).ToList();

            if (!string.IsNullOrEmpty(SortByCategory))
                FilteredEvents = SortByCategory == "asc" ? FilteredEvents.OrderBy(e => e.Category).ToList() : FilteredEvents.OrderByDescending(e => e.Category).ToList();

            if (!string.IsNullOrEmpty(SortByOrganization))
                FilteredEvents = SortByOrganization == "asc" ? FilteredEvents.OrderBy(e => e.Organization).ToList() : FilteredEvents.OrderByDescending(e => e.Organization).ToList();
        }

        public class EventItem
        {
            public string Name { get; set; }
            public string Date { get; set; }
            public string Location { get; set; }
            public double Price { get; set; }
            public string Description { get; set; }
            public string startTime { get; set; }
            public string endTime { get; set; }
            public int TicketsLeft { get; set; }
            public string Category { get; set; }
            public string Organization { get; set; }
            public int Popularity { get; set; }  
            public bool isActive { get; set; }
        }
    }
}
