using Lucky_Charm_Event_track.Helpers;
using Lucky_Charm_Event_track.Models;
using Lucky_Charm_Event_track.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lucky_Charm_Event_track.Controllers
{
    [ApiController]
    [Route("api/tickets")]
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
            if (ticket == null)
                return NotFound();
            return ticket;
        }

        [HttpPost("create")]
        public ActionResult<Ticket> CreateTicket(Ticket new_ticket)
        {
            if (new_ticket == null)
                return BadRequest();

            _dbContext.Tickets.Add(new_ticket);
            _dbContext.SaveChanges();
            return Ok();
        }

        [HttpPost("delete")]
        public ActionResult<Ticket> DeleteTicket(int id)
        {
            var deleted_ticket = _dbContext.Tickets.Find(id);
            if (deleted_ticket == null)
                return BadRequest();

            _dbContext.Tickets.Remove(deleted_ticket);
            _dbContext.SaveChanges();
            return Ok();
        }

        [HttpPost("update")]
        public ActionResult<Ticket> UpdateTicket(int id, Ticket updated_ticket)
        {
            var ticket_to_update = _dbContext.Tickets.Find(id);
            if (ticket_to_update == null)
                return BadRequest();

            ticket_to_update.EventId = updated_ticket.EventId;
            ticket_to_update.UserAccountId = updated_ticket.UserAccountId;
            ticket_to_update.IsHiddenInCalendar = updated_ticket.IsHiddenInCalendar;

            _dbContext.Tickets.Update(ticket_to_update);
            _dbContext.SaveChanges();
            return Ok(ticket_to_update);
        }

        [HttpPost("hide")]
        public async Task<IActionResult> HideTicketEvent(int ticketId)
        {
            var ticket = await _dbContext.Tickets.FindAsync(ticketId);
            if (ticket == null)
                return NotFound(new { success = false, error = "Ticket not found." });

            ticket.IsHiddenInCalendar = true;
            await _dbContext.SaveChangesAsync();

            return Ok(new { success = true, message = "Event hidden from calendar." });
        }
        [HttpPost("claim")]
        public ActionResult<Ticket> UpdateEvent(int eventid)
        {
            var selectedevent = _dbContext.Events.Find(eventid);
            string qr_payload = "test";
            if (selectedevent == null || Globals.Globals.SessionManager.CurrentLoggedInUser == null) 
            {
                return BadRequest();
            }
            Ticket ticket = new Ticket
            {
                EventId = eventid,
                UserAccountId = Globals.Globals.SessionManager.CurrentLoggedInUser.Id,
                TicketType = Enums.TicketTypes.Free,
                QRCodeText = qr_payload,
                QRCode = QRCodeGeneratorHelper.GenerateQRCode(qr_payload),
                CheckedIn = false,
                Price = 0,
                PurchaseDate = System.DateTime.Now

            };
            _dbContext.Tickets.Add(ticket);
            _dbContext.SaveChanges();
            selectedevent.Tickets.Add(ticket);
            return Ok(ticket);

        // Hides all tickets for a given event and user
        [HttpPost("hide-by-event")]
        public async Task<IActionResult> HideEventTickets([FromQuery] int eventId, [FromQuery] int userId)
        {
            var tickets = await _dbContext.Tickets
                .Where(t => t.EventId == eventId && t.UserAccountId == userId)
                .ToListAsync();

            if (!tickets.Any())
                return NotFound(new { success = false, error = "No tickets found for this event." });

            tickets.ForEach(t => t.IsHiddenInCalendar = true);
            await _dbContext.SaveChangesAsync();

            return Ok(new { success = true, message = "All tickets for this event hidden." });
        }


        [HttpPost("unhide-by-event")]
        public async Task<IActionResult> UnhideEventTickets([FromQuery] int eventId, [FromQuery] int userId)
        {
            var tickets = await _dbContext.Tickets
                .Where(t => t.EventId == eventId && t.UserAccountId == userId)
                .ToListAsync();

            if (!tickets.Any())
                return NotFound(new { success = false, error = "No tickets found for this event." });

            tickets.ForEach(t => t.IsHiddenInCalendar = false);
            await _dbContext.SaveChangesAsync();

            return Ok(new { success = true, message = "All tickets for this event unhidden." });
        }
    }
}
