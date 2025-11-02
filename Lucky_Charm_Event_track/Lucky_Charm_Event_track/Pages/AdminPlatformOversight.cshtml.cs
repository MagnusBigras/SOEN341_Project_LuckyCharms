using Microsoft.AspNetCore.Mvc.RazorPages;
using Lucky_Charm_Event_track.Models;
using System.Security.Claims;
using System.Linq;

namespace Lucky_Charm_Event_track.Pages
{
    public class AdminPlatformOversightModel : PageModel
    {
        private readonly WebAppDBContext _dbContext;

        public AdminPlatformOversightModel(WebAppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Logged-in admin first name
        public string FirstName { get; set; } = "Admin";

        public void OnGet()
        {
            // Get logged-in admin first name
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
