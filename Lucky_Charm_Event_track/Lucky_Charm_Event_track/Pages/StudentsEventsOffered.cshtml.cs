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
        public string Category { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Organization { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Location { get; set; }

        [BindProperty(SupportsGet = true)]
        public double? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public double? MaxPrice { get; set; }

        public List<string> AllOrganizations { get; set; } = new();
        public List<string> AllLocations { get; set; } = new();

        public List<EventItem> AllEvents { get; set; } = new();
        public List<EventItem> FilteredEvents { get; set; } = new();

        public void OnGet()
        {
            // Sample events
            AllEvents = new List<EventItem> {
                new EventItem { Id = 7, Name = "Tech Expo 2025", Date = "2025-10-20", Location = "Main Hall", Price = 10, Description = "Tech showcase.", startTime = "09:00", endTime = "17:00", TicketsLeft = 50, Category = "workshop", Organization = "TechOrg", Popularity = 80, isActive = true },
                new EventItem { Id = 7,  Name = "Career Fair", Date = "2025-10-22", Location = "Conference Room A", Price = 0, Description = "Meet top employers.", startTime = "10:00", endTime = "16:00", TicketsLeft = 100, Category = "conference", Organization = "CareerCenter", Popularity = 120, isActive = true },
                new EventItem { Id = 7, Name = "AI Workshop", Date = "2025-10-25", Location = "Lab 3", Price = 25, Description = "Learn AI hands-on.", startTime = "13:00", endTime = "16:00", TicketsLeft = 20, Category = "workshop", Organization = "AI Club", Popularity = 60, isActive = true },
                new EventItem { Id = 7, Name = "Music Night", Date = "2025-11-01", Location = "Auditorium", Price = 15, Description = "Live performances.", startTime = "19:00", endTime = "22:00", TicketsLeft = 200, Category = "social", Organization = "MusicSociety", Popularity = 200, isActive = true }
            };

            // Get all unique organizations and locations
            AllOrganizations = AllEvents.Select(e => e.Organization).Distinct().ToList();
            AllLocations = AllEvents.Select(e => e.Location).Distinct().ToList();

            // Filter active events
            var events = AllEvents.Where(e => e.isActive);

            // Apply search
            if (!string.IsNullOrEmpty(SearchQuery))
                events = events.Where(e => e.Name.ToLower().Contains(SearchQuery.ToLower()));

            // Filter by Category
            if (!string.IsNullOrEmpty(Category))
                events = events.Where(e => e.Category == Category);

            // Filter by Organization
            if (!string.IsNullOrEmpty(Organization))
                events = events.Where(e => e.Organization == Organization);

            // Filter by Location
            if (!string.IsNullOrEmpty(Location))
                events = events.Where(e => e.Location == Location);

            // Filter by Price Range
            if (MinPrice.HasValue)
                events = events.Where(e => e.Price >= MinPrice.Value);
            if (MaxPrice.HasValue)
                events = events.Where(e => e.Price <= MaxPrice.Value);

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
        }

        public class EventItem
        {
            public int Id { get; set; }
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
