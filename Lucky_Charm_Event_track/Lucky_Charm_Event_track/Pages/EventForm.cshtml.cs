using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Lucky_Charm_Event_track.Models;
using System.Security.Claims;
using System.Linq;

namespace Lucky_Charm_Event_track.Pages
{
    public class EventFormModel : PageModel
    {
        private readonly WebAppDBContext _context;

        public EventFormModel(WebAppDBContext context)
        {
            _context = context;
        }

        public string FirstName { get; set; } = "Guest"; 

        [BindProperty]
        public string EventName { get; set; }

        [BindProperty]
        public string EventDate { get; set; }

        [BindProperty]
        public string EventTime { get; set; }

        [BindProperty]
        public string EventLocation { get; set; }

        [BindProperty]
        public string Address { get; set; }

        [BindProperty]
        public string City { get; set; }

        [BindProperty]
        public string Region { get; set; }

        [BindProperty]
        public string PostalCode { get; set; }

        [BindProperty]
        public string Country { get; set; }

        [BindProperty]
        public int Capacity { get; set; }

        [BindProperty]
        public string EventType { get; set; } // "paid" or "free"

        [BindProperty]
        public decimal Price { get; set; }

        [BindProperty]
        public string Category { get; set; } 

        [BindProperty]
        public string EventDescription { get; set; }

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

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Handle the form submission logic, e.g., save event data
            // Redirect or show a success message
            return RedirectToPage("/EventConfirmation");
        }
    }
}
