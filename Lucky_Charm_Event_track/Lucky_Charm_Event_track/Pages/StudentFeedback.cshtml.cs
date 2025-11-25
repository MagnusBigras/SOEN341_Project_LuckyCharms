using Lucky_Charm_Event_track.Models;
using Lucky_Charm_Event_track.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;


namespace Lucky_Charm_Event_track.Pages
{
    public class StudentFeedbackModel : PageModel
    {
        private readonly WebAppDBContext _dbContext;

        public int UserId { get; set; }
        public string FirstName { get; set; } = "Guest";

        public List<Event> UserEvents { get; set; } = new();

        [BindProperty]
        public Review Review { get; set; }

        public StudentFeedbackModel(WebAppDBContext context)
        {
            _dbContext = context;
        }

        // Load user data when page is accessed
        public async Task OnGetAsync()
        {
            await LoadUserDataAsync();
        }

        // Handle review submission
        public async Task<IActionResult> OnPostAsync()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return Page();

            UserId = int.Parse(userIdClaim);

            await LoadUserDataAsync();

            if (!ModelState.IsValid) return Page();

            Review.UserAccountID = UserId;

            _dbContext.Reviews.Add(Review);
            await _dbContext.SaveChangesAsync();

            Review = new Review(); // Reset review for next submission

            return Page();
        }

        // Load user information and events they attended
        private async Task LoadUserDataAsync()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return;

            UserId = int.Parse(userIdClaim);

            var user = await _dbContext.UserAccounts.FirstOrDefaultAsync(u => u.Id == UserId);
            if (user != null) FirstName = user.FirstName;

            // Get tickets for the user and include related event and organizer info
            var tickets = await _dbContext.Tickets
                .Where(t => t.UserAccountId == UserId)
                .Include(t => t.Event)
                .ThenInclude(e => e.Organizer)
                .ThenInclude(o => o.Account)
                .ToListAsync();

            // Filter events that have already started and remove duplicates
      

            UserEvents = tickets
                .Select(t => t.Event)
                .Where(e => e != null && e.StartTime <= DateTime.Now)
                .GroupBy(e => e.Id)
                .Select(g => g.First())
                .Where(e => !_dbContext.Reviews
                .Any(r => r.UserAccountID == UserId && r.EventID == e.Id)).ToList();


            Review ??= new Review();
        }

        // Handle review submission with redirect to confirmation page
         public async Task<IActionResult> OnPostSubmitReviewAsync(){
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return Page();

            UserId = int.Parse(userIdClaim);

            await LoadUserDataAsync(); 

            if (!ModelState.IsValid) return Page();

            Review.UserAccountID = UserId;

            _dbContext.Reviews.Add(Review);
            await _dbContext.SaveChangesAsync();

            return RedirectToPage("/StudentFeedbackConfirmation");
        }

    }
}
