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
            // Fetch events from database and include related data
            var events = _context.Events
                .Include(e => e.Prices)
                .Include(e => e.Tickets)
                .Include(e => e.Organizer)
                .Where(e => e.isActive)
                .AsEnumerable()
                .Select(e => new EventItem
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
                    Category = e.Category,
                    Organization = e.Organizer != null && e.Organizer.Account != null
                        ? $"{e.Organizer.Account.FirstName} {e.Organizer.Account.LastName}"
                        : "Unknown",
                    Popularity = e.Tickets?.Count ?? 0,
                    isActive = e.isActive
                })
                .ToList();

            // Get all unique organizations and locations
            AllOrganizations = events.Select(e => e.Organization).Distinct().ToList();
            AllLocations = events.Select(e => e.Location).Distinct().ToList();

            // Apply search and filters
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

            // Remove duplicates
            FilteredEvents = filtered
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

        //  POST: Purchase Ticket
        public IActionResult OnPostPurchaseTicket(int eventId)
        {
            int userId = 1; // temporary test value

            // Find available ticket for event
            var ticket = _context.Tickets
                .FirstOrDefault(t => t.EventId == eventId && t.UserAccountId == null);

            if (ticket == null)
            {
                TempData["ErrorMessage"] = "No tickets available for this event.";
                return RedirectToPage();
            }

            // Assign ticket to user
            ticket.UserAccountId = userId;
            ticket.PurchaseDate = DateTime.Now;
            ticket.QRCodeText = Guid.NewGuid().ToString();
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Ticket successfully claimed!";
            return RedirectToPage();
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
