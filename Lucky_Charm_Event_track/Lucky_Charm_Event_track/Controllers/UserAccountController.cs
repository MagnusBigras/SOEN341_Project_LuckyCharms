using Lucky_Charm_Event_track.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lucky_Charm_Event_track.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserAccountController : Controller
    {
        private readonly WebAppDBContext _dbContext;
        public UserAccountController(WebAppDBContext context)
        {
            _dbContext = context;
        }
        public IActionResult Index()
        {
            return View();
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
        public ActionResult<UserAccount> CreateUserAccount(UserAccount newAccount)
        {
            if (newAccount == null)
            {
                return BadRequest();
            }
            _dbContext.UserAccounts.Add(newAccount);
            _dbContext.SaveChanges();
            return Ok();
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
    }
}
