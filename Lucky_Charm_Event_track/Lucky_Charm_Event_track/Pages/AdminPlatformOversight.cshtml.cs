using Lucky_Charm_Event_track.Enums;
using Lucky_Charm_Event_track.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;



namespace Lucky_Charm_Event_track.Pages
{
    public class AdminPlatformOversightModel : PageModel
    {
        // Assuming your enum looks like:
        // public enum AccountTypes { User = 1, Organizer = 2, Admin = 3 }

        private const AccountTypes OrganizerAccountTypeDefault = AccountTypes.EventOrganizer;

        private readonly WebAppDBContext _dbContext;

        public AdminPlatformOversightModel(WebAppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public string FirstName { get; private set; } = "Admin";
        public List<EventDto> Events { get; private set; } = new();
        public List<OrganizerDto> Organizers { get; private set; } = new();

        // Optional query param lets you override the filter
        public async Task OnGetAsync(AccountTypes? organizerAccountType)
        {


            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdClaim))
            {
                if (int.TryParse(userIdClaim, out int userId))
                {
                    var user = await _dbContext.UserAccounts.FirstOrDefaultAsync(u => u.Id == userId);
                    if (user != null)
                    {
                        FirstName = user.FirstName;
                    }
                }
            }



            var organizerType = organizerAccountType ?? OrganizerAccountTypeDefault;

            // EVENTS
            Events = await
                (from e in _dbContext.Events
                join eo in _dbContext.EventOrganizers on e.EventOrganizerId equals eo.Id into org
                from eo in org.DefaultIfEmpty()
                join u in _dbContext.UserAccounts on eo.UserAccountId equals u.Id into users
                from u in users.DefaultIfEmpty()
                orderby e.StartTime descending
                select new EventDto
                {
                    Id = e.Id,
                    EventName = e.EventName,
                    StartTime = e.StartTime,
                    IsActive = e.IsActive,
                    OrganizerName = u != null ? $"{u.FirstName} {u.LastName}" : "N/A",
                    Address = e.Address,
                    City = e.City,
                    Region = e.Region,
                    PostalCode = e.PostalCode,
                    Country = e.Country
                })
                .ToListAsync();


            // ORGANIZER ACCOUNTS
            var rawOrganizers = await _dbContext.UserAccounts
                .Where(u => u.AccountType == organizerType)
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.PhoneNumber,
                    u.AccountType,   // enum
                    u.IsActive,      // int 0/1
                    u.IsBanned       // int 0/1
                })
                .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
                .ToListAsync();

            Organizers = rawOrganizers.Select(u => new OrganizerDto
            {
                Id = u.Id,
                FullName = $"{u.FirstName ?? ""} {u.LastName ?? ""}".Trim(),
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                AccountType = (int)u.AccountType,   // store as int in the DTO if you prefer
                IsActive = u.IsActive,
                IsBanned = u.IsBanned
            }).ToList();
        }

        public class EventDto
        {
            public int Id { get; set; }
            public string? EventName { get; set; }
            public DateTime? StartTime { get; set; }
            public bool IsActive { get; set; }
            public string? Description { get; set; }
            public string? OrganizerName { get; set; }
            public string Location => string.Join(", ", new[] { Address, City, Region, PostalCode, Country }.Where(s => !string.IsNullOrWhiteSpace(s)));
            public string? Address { get; set; }
            public string? City { get; set; }
            public string? Region { get; set; }
            public string? PostalCode { get; set; }
            public string? Country { get; set; }
                }

        public class OrganizerDto
        {
            public int Id { get; set; }
            public string FullName { get; set; } = "";
            public string? Email { get; set; }
            public string? PhoneNumber { get; set; }
            public int AccountType { get; set; }     
            public bool IsActive { get; set; }
            public bool IsBanned { get; set; }
        }

        public async Task<IActionResult> OnPostApproveEventAsync(int eventId)
        {
            var ev = await _dbContext.Events.FindAsync(eventId);
            if (ev == null) return NotFound();
            ev.IsActive = true;
            await _dbContext.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDisableEventAsync(int eventId)
        {
            var ev = await _dbContext.Events.FindAsync(eventId);
            if (ev == null) return NotFound();
            ev.IsActive = false;
            await _dbContext.SaveChangesAsync();
            return RedirectToPage();
        }
        public async Task<IActionResult> OnPostApproveOrganizerAsync(int userId)
        {
            var u = await _dbContext.UserAccounts.FindAsync(userId);
            if (u == null) return NotFound();
            u.IsActive = true;   // or 1
            await _dbContext.SaveChangesAsync();
            return RedirectToPage();
        }


        public async Task<IActionResult> OnPostUnapproveOrganizerAsync(int userId)
        {
            var u = await _dbContext.UserAccounts.FindAsync(userId);
            if (u == null) return NotFound();
            u.IsActive = false;  
            await _dbContext.SaveChangesAsync();
            return RedirectToPage();
        }


        public async Task<IActionResult> OnPostBanOrganizerAsync(int userId)
        {
            var u = await _dbContext.UserAccounts.FindAsync(userId);
            if (u == null) return NotFound();
            u.IsBanned = true;  
            await _dbContext.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUnbanOrganizerAsync(int userId)
        {
            var u = await _dbContext.UserAccounts.FindAsync(userId);
            if (u == null) return NotFound();
            u.IsBanned = false;
            await _dbContext.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}
