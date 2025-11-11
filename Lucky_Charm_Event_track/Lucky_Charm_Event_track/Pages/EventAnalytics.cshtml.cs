using Lucky_Charm_Event_track.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Lucky_Charm_Event_track.Pages
{
    public class EventAnalyticsModel : PageModel
    {
        private readonly WebAppDBContext _dbContext;

        public EventAnalyticsModel(WebAppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Logged-in user's first name
        public string FirstName { get; set; } = "Guest";

        // Metric for a single event
        public Metric EventMetric { get; set; }

        // Metrics for all events by an organizer
        public List<Metric> OrganizerMetrics { get; set; }

        public IActionResult OnGet(int? eventId, int? organizerId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdClaim))
            {
                int userId = int.Parse(userIdClaim);
                var user = _dbContext.UserAccounts.FirstOrDefault(u => u.Id == userId);
                if (user != null)
                {
                    FirstName = user.FirstName;
                }
            }

            if (eventId.HasValue)
            {
                EventMetric = _dbContext.Metrics
                    .Include(m => m.Event)
                    .ThenInclude(e => e.Tickets) 
                    .FirstOrDefault(m => m.EventId == eventId.Value);

                if (EventMetric == null)
                    return NotFound($"No metrics found for event ID {eventId.Value}");

                // Compute used capacity and remaining capacity
                EventMetric.UsedCapacity = EventMetric.Event.Tickets.Count(t => t.UserAccountId != null);
                EventMetric.LastRemaining = EventMetric.Event.Capacity - EventMetric.UsedCapacity;
            }

            if (organizerId.HasValue)
            {
                var eventIds = _dbContext.Events
                    .Where(e => e.EventOrganizerId == organizerId.Value)
                    .Select(e => e.Id)
                    .ToList();

                OrganizerMetrics = _dbContext.Metrics
                    .Include(m => m.Event)
                    .ThenInclude(e => e.Tickets)
                    .Where(m => eventIds.Contains(m.EventId))
                    .ToList();

                if (OrganizerMetrics != null)
                {
                    foreach (var metric in OrganizerMetrics)
                    {
                        metric.UsedCapacity = metric.Event.Tickets.Count(t => t.UserAccountId != null);
                        metric.LastRemaining = metric.Event.Capacity - metric.UsedCapacity;
                    }
                }
            }

            return Page();
        }
    }
}
