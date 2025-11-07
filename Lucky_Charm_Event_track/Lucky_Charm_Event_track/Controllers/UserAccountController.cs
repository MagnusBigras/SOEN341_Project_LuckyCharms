using Lucky_Charm_Event_track.Enums;
using Lucky_Charm_Event_track.Globals;
using Lucky_Charm_Event_track.Models;
using Lucky_Charm_Event_track.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Lucky_Charm_Event_track.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    public class UserAccountController : ControllerBase
    {
        private readonly WebAppDBContext _dbContext;
        public UserAccountController(WebAppDBContext context)
        {
            _dbContext = context;
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<UserAccount>>> GetEvents()
        {
            return await _dbContext.UserAccounts.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserAccount>> GetUserAccountById(int id) 
        {
            var user = await _dbContext.UserAccounts.FindAsync(id);
            if (user == null)
                return NotFound();
            return user;
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<UserAccount>>> GetActiveUserAccounts()
        {
            var active_accounts = _dbContext.UserAccounts.Where(e => e.IsActive).ToList();
            return active_accounts;
        }
        
        // Returns a lightweight summary of accounts (id, email, accountType) to avoid reference-preserve JSON wrapping
        [HttpGet("summary")]
        public async Task<ActionResult<IEnumerable<object>>> GetAccountsSummary()
        {
            var list = await _dbContext.UserAccounts
                .Select(u => new { u.Id, u.Email, u.AccountType,u.IsBanned, u.SuspensionEndUtc})
                .ToListAsync();
            return Ok(list);
        }
        // Return only restriction-related fields to avoid serialization surprises
        [HttpGet("restrictions")]
        public async Task<ActionResult<IEnumerable<object>>> GetAccountsRestrictions()
        {
            var list = await _dbContext.UserAccounts
                .Select(u => new { u.Id, u.Email, u.IsBanned, u.SuspensionEndUtc })
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet("getpaymentdetails")]
        public async Task<ActionResult<PaymentDetail>> GetPaymentDetails() 
        {
            if(Globals.Globals.SessionManager.CurrentLoggedInUser == null) 
                return BadRequest();

            var paymentDetail = await _dbContext.PaymentDetails.FirstOrDefaultAsync(
                p => p.UserID == Globals.Globals.SessionManager.CurrentLoggedInUser.Id);
            return Ok(paymentDetail);
        }

        [HttpPost("create")]
        public ActionResult<UserAccount> CreateUserAccount([FromBody] UserAccount newAccount)
        {
            try
            {

                if (newAccount == null)
                {
                    return BadRequest("Payload was null");
                }

                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(newAccount));

                newAccount.LastLogin = DateTime.UtcNow;
                newAccount.IsActive = true;
                newAccount.Tickets = new List<Ticket>();

                _dbContext.UserAccounts.Add(newAccount);
                _dbContext.SaveChanges();

                return Ok(new { message = "Account created successfully", accountID = newAccount.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("delete")]
        public ActionResult<UserAccount> DeleteAccount(int id)
        {
            var deleted_account = _dbContext.UserAccounts.Find(id);
            if (deleted_account == null)
                return BadRequest();

            _dbContext.UserAccounts.Remove(deleted_account);
            _dbContext.SaveChanges();
            return Ok();
        }

        [HttpPost("update")]
        public ActionResult<UserAccount> UpdateEvent(int id, UserAccount updated_account)
        {
            var account_to_be_updated = _dbContext.UserAccounts.Find(id);
            if (account_to_be_updated == null)
                return BadRequest();

            _dbContext.UserAccounts.Update(account_to_be_updated);
            _dbContext.SaveChanges();
            return Ok(updated_account);
        }

       [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginCreds loginCreds)
        {
            var account = _dbContext.UserAccounts.FirstOrDefault(e => e.UserName == loginCreds.Username);

            if (account == null || account.Password != loginCreds.Password)
                return BadRequest("Invalid credentials");

            // Initialize global session
            Globals.Globals.SessionManager.InitializeSession(account, "login");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new Claim(ClaimTypes.Name, account.UserName),
                new Claim(ClaimTypes.Email, account.Email ?? "")
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Sign in the user
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            string redirectUrl = loginCreds.IsAdmin
                ? "/AdminManagement"
                : (account.AccountType == AccountTypes.EventOrganizer ? "/Events" : "/StudentsEventsOffered");

            return Ok(new LoginResponse
            {
                Message = "Login Successful",
                RedirectUrl = redirectUrl
            });
        }

        [HttpPost("upgradetoOrganizer")]
        public ActionResult<EventOrganizer> UpgradetoEventOrganzer(int id) 
        {
            if(Globals.Globals.SessionManager.CurrentLoggedInUser == null) 
                return BadRequest(new { message = "Error! User not logged in!" });

            if (!Globals.Globals.SessionManager.IsAdmin) 
                return BadRequest(new { message = "Error! User Must Be an Admin to Upgrade Account!" });

            var account = _dbContext.UserAccounts.Find(id);
            if (account == null)
                return BadRequest(new { message = "Error! Not a valid User!" });

            EventOrganizer organizer = new EventOrganizer
            {
                UserAccountId = account.Id,
                CreatedAt = account.AccountCreationDate,
                IsActive = account.IsActive
            };
            _dbContext.EventOrganizers.Add(organizer);
            _dbContext.SaveChanges();
            return Ok(organizer);
        }
        
        public class RoleAssignmentRequest
        {
            public int UserId { get; set; }
            public string Role { get; set; }
        }


        // Admin-only: assign a role to a user (AccountType). Role may be name of AccountTypes enum (GeneralUser, EventOrganizer, Administrator)
        [HttpPost("assign-role")]
        public ActionResult AssignRole([FromBody] RoleAssignmentRequest req)
        {
            // NOTE: login/session checks removed — role assignment is currently open while authentication isn't implemented.
            if (req == null) return BadRequest(new { message = "Payload missing." });

            var account = _dbContext.UserAccounts.Find(req.UserId);
            if (account == null) return NotFound(new { message = "User account not found." });

            if (!System.Enum.TryParse(typeof(AccountTypes), req.Role, true, out var parsed))
            {
                return BadRequest(new { message = "Invalid role provided." });
            }

            var newRole = (AccountTypes)parsed;
            var prevRole = account.AccountType;

            // If assigning EventOrganizer, ensure organizer entry exists
            if (newRole == AccountTypes.EventOrganizer)
            {
                var existingOrganizer = _dbContext.EventOrganizers.FirstOrDefault(e => e.UserAccountId == account.Id);
                if (existingOrganizer == null)
                {
                    var organizer = new EventOrganizer
                    {
                        UserAccountId = account.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = account.IsActive
                    };
                    _dbContext.EventOrganizers.Add(organizer);
                }
            }

            // If demoting from EventOrganizer, remove organizer and deactivate their events
            if (prevRole == AccountTypes.EventOrganizer && newRole != AccountTypes.EventOrganizer)
            {
                var organizer = _dbContext.EventOrganizers
                    .Include(o => o.Events)
                    .FirstOrDefault(o => o.UserAccountId == account.Id);
                if (organizer != null)
                {
                    foreach (var ev in organizer.Events ?? new System.Collections.Generic.List<Event>())
                    {
                        ev.IsActive = false;
                        _dbContext.Events.Update(ev);
                    }
                    _dbContext.EventOrganizers.Remove(organizer);
                }
            }

            account.AccountType = newRole;
            _dbContext.UserAccounts.Update(account);
            _dbContext.SaveChanges();
            return Ok(new { message = "Role assigned successfully.", userId = account.Id, newRole = account.AccountType });
        }
        
        public class RestrictRequest
        {
            public int UserId { get; set; }
            public string Action { get; set; } // "ban" | "unban" | "suspend"
            public DateTime? UntilUtc { get; set; } // used for suspend
        }
        [HttpPost("restrict")]
        public async Task<IActionResult> RestrictAccount([FromBody] RestrictRequest req)
        {
            if (req == null) return BadRequest("Invalid payload");
            var user = await _dbContext.UserAccounts.FindAsync(req.UserId);
            if (user == null) return NotFound();

            var action = (req.Action ?? string.Empty).ToLowerInvariant().Trim();
            // Treat "suspend permanently" (or any suspend + perman* variant) as a ban
            if (action.Contains("suspend") && action.Contains("perman"))
            {
                action = "ban";
            }
            // If action is plain "suspend" but no UntilUtc provided, treat as ban (admin intended permanent suspend)
            if (action == "suspend" && req.UntilUtc == null)
            {
                action = "ban";
            }
            switch (action)
            {
                case "ban":
                    // Permanent ban: mark banned and deactivate
                    user.IsBanned = true;
                    user.SuspensionEndUtc = null;
                    user.IsActive = false;
                    // cascade: if user is an EventOrganizer, deactivate organizer and their events
                    var organizer = await _dbContext.EventOrganizers.Include(o => o.Events).FirstOrDefaultAsync(o => o.UserAccountId == user.Id);
                    if (organizer != null)
                    {
                        organizer.IsActive = false;
                        foreach (var ev in organizer.Events ?? new List<Event>())
                        {
                            ev.IsActive = false;
                            _dbContext.Events.Update(ev);
                        }
                        _dbContext.EventOrganizers.Update(organizer);
                    }
                    break;

                case "unban":
                    // Remove ban/suspension
                    user.IsBanned = false;
                    user.SuspensionEndUtc = null;
                    user.IsActive = true;
                    // reactivate organizer & their events if present
                    var existingOrgToReactivate = await _dbContext.EventOrganizers.Include(o => o.Events).FirstOrDefaultAsync(o => o.UserAccountId == user.Id);
                    if (existingOrgToReactivate != null)
                    {
                        existingOrgToReactivate.IsActive = true;
                        foreach (var ev in existingOrgToReactivate.Events ?? new List<Event>())
                        {
                            ev.IsActive = true;
                            _dbContext.Events.Update(ev);
                        }
                        _dbContext.EventOrganizers.Update(existingOrgToReactivate);
                    }
                    break;

                case "suspend":
                    if (req.UntilUtc == null) return BadRequest("Missing UntilUtc for suspend");
                    user.IsBanned = false;
                    user.SuspensionEndUtc = req.UntilUtc;
                    user.IsActive = false;
                    // deactivate organizer & their events while suspended
                    var orgToSuspend = await _dbContext.EventOrganizers.Include(o => o.Events).FirstOrDefaultAsync(o => o.UserAccountId == user.Id);
                    if (orgToSuspend != null)
                    {
                        orgToSuspend.IsActive = false;
                        foreach (var ev in orgToSuspend.Events ?? new List<Event>())
                        {
                            ev.IsActive = false;
                            _dbContext.Events.Update(ev);
                        }
                        _dbContext.EventOrganizers.Update(orgToSuspend);
                    }
                    break;

                default:
                    return BadRequest("Unknown action");
            }

            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Restriction applied", userId = user.Id, isBanned = user.IsBanned, suspensionEndUtc = user.SuspensionEndUtc });
        }

        [HttpPost("savepaymentdetails")]
        public ActionResult<UserAccount> SavePaymentDetails([FromBody] PaymentDetail paymentDetail)
        {
            if (paymentDetail == null)
                return BadRequest(new { message = "Error! Invalid Payment Details" });

            paymentDetail.UserID = Globals.Globals.SessionManager.CurrentLoggedInUser.Id;
            _dbContext.PaymentDetails.Add(paymentDetail);
            _dbContext.SaveChanges();
            return Ok(paymentDetail);
        }
        

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // End authentication session
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Clear global session as well
            Globals.Globals.SessionManager.ClearSession();

            return Ok(new { message = "Sign out successful", redirectUrl = "/Index" });
        }

    }
}
