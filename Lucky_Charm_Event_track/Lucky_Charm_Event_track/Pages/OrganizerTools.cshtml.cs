using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Text;

namespace Lucky_Charm_Event_track.Pages
{
    public class OrganizerToolsModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int EventId { get; set; }

        public string EventName { get; set; } = "Sample Event";

        private readonly List<Attendee> Attendees = new()
        {
            new Attendee { Name = "Alice Smith", Email = "alice@example.com" },
            new Attendee { Name = "Bob Johnson", Email = "bob@example.com" }
        };

        public IActionResult OnGetExportCsv(int eventId)
        {
            // Create CSV
            var csv = new StringBuilder();
            csv.AppendLine("Name,Email");
            foreach (var a in Attendees)
                csv.AppendLine($"{a.Name},{a.Email}");

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"Attendees_Event_{eventId}.csv");
        }

        public class Attendee
        {
            public string Name { get; set; }
            public string Email { get; set; }
        }
    }
}
