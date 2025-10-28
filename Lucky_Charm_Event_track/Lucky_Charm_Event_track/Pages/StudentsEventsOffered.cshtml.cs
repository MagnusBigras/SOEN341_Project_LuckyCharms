using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Lucky_Charm_Event_track.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lucky_Charm_Event_track.Pages
{
    public class StudentsEventsOfferedModel : PageModel
    {
        private readonly WebAppDBContext _context;

        public StudentsEventsOfferedModel(WebAppDBContext context)
        {
            _context = context;
        }

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
            // Fetch all active events with related data
            var events = _context.Events
                .Include(e => e.Prices)
                .Include(e => e.Tickets)
                .Include(e => e.Organizer)
                .Include(e => e.Metric)
                .Where(e => e.isActive)
                .AsEnumerable()
                .Select(e =>
                {
                    int claimedTickets = e.Tickets?.Count(t => t.UserAccountId != null) ?? 0;

                    return new EventItem
                    {
                        Id = e.Id,
                        Name = e.EventName,
                        Date = e.StartTime.ToString("yyyy-MM-dd"),
                        Location = e.Address + ", " + e.City,
                        Price = e.Prices.Any() ? e.Prices.Min(p => p.Price) : 0,
                        Description = e.EventDescription,
                        startTime = e.StartTime.ToString("HH:mm"),
                        endTime = "",
                        TicketsLeft = e.Tickets?.Count(t => t.UserAccountId == null) ?? 0,
                        RemainingCapacity = e.Capacity - claimedTickets,
                        Category = e.Category,
                        Organization = e.Organizer != null && e.Organizer.Account != null
                            ? $"{e.Organizer.Account.FirstName} {e.Organizer.Account.LastName}"
                            : "Unknown",
                        Popularity = e.Tickets?.Count ?? 0,
                        isActive = e.isActive
                    };
                }).ToList();

            // Populate filters
            AllOrganizations = events.Select(e => e.Organization).Distinct().ToList();
            AllLocations = events.Select(e => e.Location).Distinct().ToList();

            // Apply filtering
            var filtered = events.AsQueryable();

            if (!string.IsNullOrEmpty(SearchQuery))
                filtered = filtered.Where(e => e.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(Category))
                filtered = filtered.Where(e => e.Category.Equals(Category, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(Organization))
                filtered = filtered.Where(e => e.Organization.Equals(Organization, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(Location))
                filtered = filtered.Where(e => e.Location.Equals(Location, StringComparison.OrdinalIgnoreCase));

            if (MinPrice.HasValue)
                filtered = filtered.Where(e => e.Price >= MinPrice.Value);

            if (MaxPrice.HasValue)
                filtered = filtered.Where(e => e.Price <= MaxPrice.Value);

            // Apply sorting
            if (!string.IsNullOrEmpty(SortByDate))
                filtered = SortByDate == "asc" ? filtered.OrderBy(e => e.Date) : filtered.OrderByDescending(e => e.Date);

            if (!string.IsNullOrEmpty(SortByPrice))
                filtered = SortByPrice == "asc" ? filtered.OrderBy(e => e.Price) : filtered.OrderByDescending(e => e.Price);

            if (!string.IsNullOrEmpty(SortByPopularity))
                filtered = SortByPopularity == "asc" ? filtered.OrderBy(e => e.Popularity) : filtered.OrderByDescending(e => e.Popularity);

            FilteredEvents = filtered.ToList();
        }

        // Purchase free ticket
        public IActionResult OnPostPurchaseTicket(int eventId)
        {
            int userId = 1; // mock user

            var ticket = _context.Tickets.FirstOrDefault(t => t.EventId == eventId && t.UserAccountId == null);

            if (ticket == null)
            {
                TempData["ErrorMessage"] = "No tickets available for this event.";
                return RedirectToPage();
            }

            ticket.UserAccountId = userId;
            ticket.PurchaseDate = DateTime.Now;
            ticket.QRCodeText = Guid.NewGuid().ToString();
            _context.SaveChanges();

            UpdateMetric(eventId, ticket.Price);

            TempData["SuccessMessage"] = "Ticket claimed successfully!";
            return RedirectToPage();
        }

        // Mock payment for paid tickets
        public IActionResult OnPostMockPay(int eventId)
        {
            int userId = 1; // mock user

            var ticket = _context.Tickets.FirstOrDefault(t => t.EventId == eventId && t.UserAccountId == null);

            if (ticket == null)
            {
                TempData["ErrorMessage"] = "No tickets available for this event.";
                return RedirectToPage();
            }

            ticket.UserAccountId = userId;
            ticket.PurchaseDate = DateTime.Now;
            ticket.QRCodeText = Guid.NewGuid().ToString();
            _context.SaveChanges();

            UpdateMetric(eventId, ticket.Price);

            TempData["SuccessMessage"] = "Mock payment successful!";
            return RedirectToPage();
        }

        // Enhanced metric update 
        private void UpdateMetric(int eventId, double ticketPrice)
        {
            var metric = _context.Metrics.FirstOrDefault(m => m.EventId == eventId);
            var eventEntity = _context.Events
                .Include(e => e.Tickets)
                .FirstOrDefault(e => e.Id == eventId);

            if (metric == null)
            {
                metric = new Metric
                {
                    EventId = eventId,
                    TotalRevenue = 0,
                    NewAttendees = 0,
                    TotalCapacity = eventEntity?.Capacity ?? 0,
                    UsedCapacity = 0,
                    LastRemaining = eventEntity?.Capacity ?? 0
                };
                _context.Metrics.Add(metric);
            }

            metric.TotalRevenue += ticketPrice;
            metric.NewAttendees += 1;

            // Dynamically update remaining and used capacity
            int used = eventEntity?.Tickets.Count(t => t.UserAccountId != null) ?? 0;
            metric.UsedCapacity = used;
            metric.LastRemaining = (eventEntity?.Capacity ?? 0) - used;

            _context.SaveChanges();
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
            public int RemainingCapacity { get; set; }
            public string Category { get; set; }
            public string Organization { get; set; }
            public int Popularity { get; set; }
            public bool isActive { get; set; }
        }
    }
}
