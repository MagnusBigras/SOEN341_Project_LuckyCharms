using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Lucky_Charm_Event_track.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Linq;

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

        // <-- Make sure SupportsGet is true
        [BindProperty(SupportsGet = true)]
        public string EventName { get; set; } = "";

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