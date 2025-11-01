using Lucky_Charm_Event_track.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace Lucky_Charm_Event_track.Pages
{
    public class StudentsMyTicketsModel : PageModel
    {
        private readonly WebAppDBContext _dbContext;

        public List<TicketView> Tickets { get; set; } = new();
        public string FirstName { get; set; } = "Guest"; 
        public int UserId { get; set; }

        public StudentsMyTicketsModel(WebAppDBContext context)
        {
            _dbContext = context;
        }

        public async Task OnGetAsync()
        {
            // Get the currently logged-in user's ID from claims
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                // User is not logged in
                return;
            }

            UserId = int.Parse(userIdClaim);

            // Fetch the user from the database
            var user = await _dbContext.UserAccounts.FirstOrDefaultAsync(u => u.Id == UserId);
            if (user != null)
            {
                FirstName = user.FirstName; // display first name in navbar
            }

            // Fetch tickets for this user
            var tickets = await _dbContext.Tickets
                .Where(t => t.UserAccountId == UserId)
                .Include(t => t.Event)
                .ToListAsync();

            Tickets = tickets.Select(t => new TicketView
            {
                TicketId = t.Id,
                EventName = t.Event.EventName,
                EventDate = t.Event.StartTime,
                Location = $"{t.Event.Address}, {t.Event.City}",
                QRCodeText = t.QRCodeText,
                IsHiddenInCalendar = t.IsHiddenInCalendar,
                EventId = t.EventId
            }).ToList();
        }
    }

    public class TicketView
    {
        public int TicketId { get; set; }
        public string EventName { get; set; }
        public System.DateTime EventDate { get; set; }
        public string Location { get; set; }
        public string QRCodeText { get; set; }
        public bool IsHiddenInCalendar { get; set; }
        public int EventId { get; set; }
    }
}
