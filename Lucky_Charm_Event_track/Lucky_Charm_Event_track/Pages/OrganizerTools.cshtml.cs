using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Lucky_Charm_Event_track.Models;
using Lucky_Charm_Event_track.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Linq;
using System.Security.Claims;

namespace Lucky_Charm_Event_track.Pages
{
    public class OrganizerToolsModel : PageModel
    {
        private readonly WebAppDBContext _dbContext;

        public OrganizerToolsModel(WebAppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        [BindProperty(SupportsGet = true)]
        public int EventId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string EventName { get; set; } = "";

        // Logged-in user's first name
        public string FirstName { get; set; } = "Guest";

        public void OnGet()
        {
            // Get logged-in user's first name
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
        }

        // Export CSV from Tickets + UserAccount
        public IActionResult OnGetExportCsv(int eventId)
        {
            var tickets = _dbContext.Tickets
                .Include(t => t.Account)
                .Where(t => t.EventId == eventId)
                .ToList();

            if (!tickets.Any())
                return NotFound(new { success = false, error = "No tickets found for this event." });

            var csv = new StringBuilder();
            csv.AppendLine("Name,Email,TicketType,Price,CheckedIn");

            foreach (var t in tickets)
            {
                var name = $"{t.Account?.FirstName ?? "Unknown"} {t.Account?.LastName ?? ""}".Trim();
                var email = t.Account?.Email ?? "Unknown";
                csv.AppendLine($"{name},{email},{t.TicketType},{t.Price},{t.CheckedIn}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"Attendees_Event_{eventId}.csv");
        }
    }
}
