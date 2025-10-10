using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lucky_Charm_Event_track.Pages
{
    public class EventFormModel : PageModel
    {
        // Add properties to bind form data
        [BindProperty]
        public string EventName { get; set; }
        [BindProperty]
        public string EventDate { get; set; }
        [BindProperty]
        public string EventTime { get; set; }
        [BindProperty]
        public string EventLocation { get; set; }
        [BindProperty]
        public string EventDescription { get; set; }
        [BindProperty]
        public string EventType { get; set; }

        public void OnGet()
        {
            // Get the initial data if needed
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
