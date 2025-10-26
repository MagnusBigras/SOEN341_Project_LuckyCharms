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
                    endTime = "", // Can add EndTime if exists
                    TicketsLeft = e.Tickets?.Count(t => t.UserAccountId == null) ?? 0,
                    Category = e.Category,
                    Organization = e.Organizer != null && e.Organizer.Account != null
                        ? $"{e.Organizer.Account.FirstName} {e.Organizer.Account.LastName}"
                        : "Unknown",
                    Popularity = e.Tickets?.Count ?? 0,
                    isActive = e.isActive,
                    IsMockPaid = e.Tickets?.Any(t => t.UserAccountId == 1) ?? false // mock user 1
                }).ToList();

            AllOrganizations = events.Select(e => e.Organization).Distinct().ToList();
            AllLocations = events.Select(e => e.Location).Distinct().ToList();

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

            TempData["SuccessMessage"] = "Mock payment successful!";
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
            public bool IsMockPaid { get; set; }
        }
    }
}
