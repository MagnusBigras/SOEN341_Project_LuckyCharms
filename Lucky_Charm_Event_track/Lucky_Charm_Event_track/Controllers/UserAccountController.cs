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
            if (newAccount == null)
                return BadRequest("Payload was null");

            newAccount.LastLogin = DateTime.UtcNow;
            newAccount.IsActive = true;
            newAccount.Tickets = new List<Ticket>();

            _dbContext.UserAccounts.Add(newAccount);
            _dbContext.SaveChanges();

            return Ok(new { message = "Account created successfully", accountID = newAccount.Id });
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
        public async Task<ActionResult> Login([FromBody]LoginCreds loginCreds) 
        {
            var account = _dbContext.UserAccounts.FirstOrDefault(e => e.UserName == loginCreds.Username);
            if (account == null)
                return BadRequest("Invalid credentials");

            // Initialize global session
            Globals.Globals.SessionManager.InitializeSession(account, "login");

            // Create claims for authentication
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

            string redirectUrl = loginCreds.IsAdmin ? "/AdminPlatformOversight" : "/StudentsEventsOffered";

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

        [HttpPost("savepaymentdetails")]
        public ActionResult<UserAccount> SavePaymentDetails([FromBody]PaymentDetail paymentDetail) 
        {
            if (paymentDetail == null) 
                return BadRequest(new { message = "Error! Invalid Payment Details" });

            paymentDetail.UserID = Globals.Globals.SessionManager.CurrentLoggedInUser.Id;
            _dbContext.PaymentDetails.Add(paymentDetail);
            _dbContext.SaveChanges();
            return Ok(paymentDetail);
        }
    }
}
