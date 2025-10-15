using Lucky_Charm_Event_track.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lucky_Charm_Event_track.Pages
{
    public class EventAnalyticsModel : PageModel
    {
        private readonly WebAppDBContext _dbContext;
        public EventAnalyticsModel(WebAppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Metrics for a single event
        public Metric EventMetric { get; set; }
        // Metrics for all events by an organizer
        public List<Metric> OrganizerMetrics { get; set; }

        public IActionResult OnGet(int? eventId, int? organizerId)
        {
            if (eventId.HasValue)
            {
                EventMetric = _dbContext.Metrics
                    .Include(m => m.Event)
                    .FirstOrDefault(m => m.EventId == eventId.Value);
                if (EventMetric == null)
                {
                    return NotFound($"No metrics found for event ID {eventId.Value}");
                }
            }
            if (organizerId.HasValue)
            {
                var eventIds = _dbContext.Events
                    .Where(e => e.EventOrganizerId == organizerId.Value)
                    .Select(e => e.Id)
                    .ToList();
                OrganizerMetrics = _dbContext.Metrics
                    .Include(m => m.Event)
                    .Where(m => eventIds.Contains(m.EventId))
                    .ToList();
                if (OrganizerMetrics == null || OrganizerMetrics.Count == 0)
                {
                    return NotFound($"No metrics found for organizer ID {organizerId.Value}");
                }
            }
            return Page();
        }
    }
}
