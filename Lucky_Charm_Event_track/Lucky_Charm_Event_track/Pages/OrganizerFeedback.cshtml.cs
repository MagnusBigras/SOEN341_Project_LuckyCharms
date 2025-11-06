using Lucky_Charm_Event_track.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lucky_Charm_Event_track.Pages
{
    public class OrganizerFeedbackModel : PageModel
    {
        private readonly WebAppDBContext _dbContext;

        public OrganizerFeedbackModel(WebAppDBContext context)
        {
            _dbContext = context;
        }

        public string FirstName { get; set; } = "Organizer";

        public List<Event> MyEvents { get; set; } = new();

        // Key: EventId, Value: List of Reviews for that Event
        public Dictionary<int, List<Review>> EventFeedbacks { get; set; } = new();

        // Filters
        [BindProperty(SupportsGet = true)]
        public string EventNameFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? RatingFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? DateFilter { get; set; }

        public async Task OnGetAsync()
        {
            await LoadOrganizerDataAsync();
        }

        private async Task LoadOrganizerDataAsync()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return;

            int organizerId = int.Parse(userIdClaim);

            var organizer = await _dbContext.UserAccounts.FirstOrDefaultAsync(u => u.Id == organizerId);
            if (organizer != null) FirstName = organizer.FirstName;

            MyEvents = await _dbContext.Events
                .Where(e => e.EventOrganizerId == organizerId)
                .Include(e => e.Reviews)
                    .ThenInclude(r => r.UserAccount)
                .ToListAsync();

            // Apply filters
            if (!string.IsNullOrEmpty(EventNameFilter))
            {
                MyEvents = MyEvents
                    .Where(e => e.EventName.Contains(EventNameFilter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (DateFilter.HasValue)
            {
                MyEvents = MyEvents
                    .Where(e => e.StartTime.Date == DateFilter.Value.Date)
                    .ToList();
            }

            // Build EventFeedbacks dictionary
            EventFeedbacks = MyEvents.ToDictionary(
                e => e.Id,
                e => e.Reviews
                          .Where(r => !RatingFilter.HasValue || r.OverallExperience == RatingFilter.Value)
                          .ToList()
            );
        }

        // Helper method for average rating
        public double GetAverageRating(int eventId)
        {
            if (!EventFeedbacks.ContainsKey(eventId) || EventFeedbacks[eventId].Count == 0)
                return 0;

            return EventFeedbacks[eventId].Average(r => r.OverallExperience);
        }
    }
}
