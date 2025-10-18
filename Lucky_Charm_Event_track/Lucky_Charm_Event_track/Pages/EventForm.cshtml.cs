using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lucky_Charm_Event_track.Pages
{
    public class EventFormModel : PageModel
    {
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
