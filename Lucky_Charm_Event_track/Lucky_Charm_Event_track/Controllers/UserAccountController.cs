
﻿using Lucky_Charm_Event_track.Enums;
using Lucky_Charm_Event_track.Globals;
using Lucky_Charm_Event_track.Models;
﻿using Lucky_Charm_Event_track.Models;
using Lucky_Charm_Event_track.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
            {
                return NotFound();
            }
            return user;
        }
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<UserAccount>>> GetActiveUserAccounts()
        {
            var active_accounts = _dbContext.UserAccounts.Where(e => e.IsActive).ToList();
            return active_accounts;
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
            {
                return BadRequest();
            }
            _dbContext.UserAccounts.Remove(deleted_account);
            _dbContext.SaveChanges();
            return Ok();
        }
        [HttpPost("update")]
        public ActionResult<UserAccount> UpdateEvent(int id, UserAccount updated_account)
        {
            var account_to_be_updated = _dbContext.UserAccounts.Find(id);
            if (account_to_be_updated == null)
            {
                return BadRequest();
            }
            _dbContext.UserAccounts.Update(account_to_be_updated);
            _dbContext.SaveChanges();
            return Ok(updated_account);

        }
        [HttpPost("login")]
        public ActionResult<UserAccount> Login([FromBody]LoginCreds loginCreds) 
        {
            var account = _dbContext.UserAccounts.FirstOrDefault(e => e.UserName == loginCreds.Username);
            if (account == null)
            {
                return BadRequest("Invalid credentials");
            }
            Globals.Globals.SessionManager.InitializeSession((UserAccount)account, "login");
            return Ok(account);
        }
        [HttpPost("upgradetoOrganizer")]
        public ActionResult<EventOrganizer> UpgradetoEventOrganzer(int id) 
        {
            //check if logged in user is admin
            if(Globals.Globals.SessionManager.CurrentLoggedInUser == null) 
            {
                return BadRequest(new { message = "Error! User not logged in!" });
            }
            if (!Globals.Globals.SessionManager.IsAdmin) 
            {
                return BadRequest(new { message = "Error! User Must Be an Admin to Upgrade Account!" });
            }

            var account = _dbContext.UserAccounts.Find(id);
            if (account == null)
            {
                return BadRequest(new { message = "Error! Not a valid User!" });
            }
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
    }
}
