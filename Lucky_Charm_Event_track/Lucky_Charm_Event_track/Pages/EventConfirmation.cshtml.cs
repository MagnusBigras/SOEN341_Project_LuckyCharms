using Microsoft.AspNetCore.Mvc.RazorPages;
using Lucky_Charm_Event_track.Models;
using System.Security.Claims;
using System.Linq;

namespace Lucky_Charm_Event_track.Pages
{
    public class EventConfirmationModel : PageModel
    {
        private readonly WebAppDBContext _context;

        public EventConfirmationModel(WebAppDBContext context)
        {
            _context = context;
        }

        public string FirstName { get; set; } = "Guest"; 

        public void OnGet()
        {
            // Get logged-in user's first name
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdClaim))
            {
                int userId = int.Parse(userIdClaim);
                var user = _context.UserAccounts.FirstOrDefault(u => u.Id == userId);
                if (user != null)
                {
                    FirstName = user.FirstName;
                }
            }
        }
    }
}
