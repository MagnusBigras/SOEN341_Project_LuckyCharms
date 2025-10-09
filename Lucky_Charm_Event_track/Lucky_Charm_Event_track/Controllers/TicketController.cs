using Lucky_Charm_Event_track.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lucky_Charm_Event_track.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : Controller
    {
        private readonly WebAppDBContext _dbContext;
        public TicketController(WebAppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTickets() 
        {
            return await _dbContext.Tickets.ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Ticket>> GetTicketById(int id) 
        {
            var ticket = await _dbContext.Tickets.FindAsync(id);
            if(User == null) 
            {
                return NotFound();
            }
            return ticket;
        }
        [HttpPost("create")]
        public ActionResult<Ticket> CreateTicket(Ticket new_ticket)
        {
            if (new_ticket == null)
            {
                return BadRequest();
            }
            _dbContext.Tickets.Add(new_ticket);
            _dbContext.SaveChanges();
            return Ok();
        }
        [HttpPost("delete")]
        public ActionResult<Ticket> DeleteAccount(int id)
        {
            var deleted_ticket = _dbContext.Tickets.Find(id);
            if (deleted_ticket == null)
            {
                return BadRequest();
            }
            _dbContext.Tickets.Remove(deleted_ticket);
            _dbContext.SaveChanges();
            return Ok();
        }
        [HttpPost("update")]
        public ActionResult<Ticket> UpdateEvent(int id, Ticket updated_ticket)
        {
            var ticket_to_be_updated = _dbContext.Tickets.Find(id);
            if( ticket_to_be_updated == null)
            {
                return BadRequest();
            }
            _dbContext.Tickets.Update(ticket_to_be_updated);
            _dbContext.SaveChanges();
            return Ok(updated_ticket);

        }

    }
}
