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

            // Get all unique organizations and locations
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

            TempData["SuccessMessage"] = "Ticket purchased successfully!";
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
