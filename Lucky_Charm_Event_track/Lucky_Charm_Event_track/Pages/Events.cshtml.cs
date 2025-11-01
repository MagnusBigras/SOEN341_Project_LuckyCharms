using Microsoft.AspNetCore.Mvc.RazorPages;
using Lucky_Charm_Event_track.Models;
using System.Linq;
using System.Security.Claims;

namespace Lucky_Charm_Event_track.Pages
{
    public class EventsModel : PageModel
    {
        private readonly WebAppDBContext _dbContext;

        public string FirstName { get; set; } = "Guest";

        public EventsModel(WebAppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void OnGet()
        {
            // Get logged-in user ID from claims
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
    }
}
