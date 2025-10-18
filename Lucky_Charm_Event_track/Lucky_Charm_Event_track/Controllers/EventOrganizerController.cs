using Lucky_Charm_Event_track.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lucky_Charm_Event_track.Controllers
{
    [ApiController]
    [Route("api/event_organizer")]
    public class EventOrganizerController : Controller
    {
        private readonly WebAppDBContext _dbContext;
        public EventOrganizerController(WebAppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<EventOrganizer>>> GetEventOrganizers()
        {
            return await _dbContext.EventOrganizers.ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<EventOrganizer>> GetEventOrganinzerById(int id)
        {
            var event_organizer = await _dbContext.EventOrganizers.FindAsync(id);
            if (event_organizer == null)
            {
                return NotFound();
            }
            return event_organizer;
        }
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<EventOrganizer>>> GetActiveUserAccounts()
        {
            var active_event_organizers = _dbContext.EventOrganizers.Where(e => e.IsActive).ToList();
            return active_event_organizers;
        }
        [HttpPost("create")]
        public ActionResult<UserAccount> CreateEventOrganizer(EventOrganizer eventOrganizer)
        {
            if (eventOrganizer == null)
            {
                return BadRequest();
            }
            _dbContext.EventOrganizers.Add(eventOrganizer);
            _dbContext.SaveChanges();
            return Ok();
        }
        [HttpPost("delete")]
        public ActionResult<EventOrganizer> DeleteEventOrganizer(int id)
        {
            var deleted_event_organizer = _dbContext.EventOrganizers.Find(id);
            if (deleted_event_organizer == null)
            {
                return BadRequest();
            }
            _dbContext.EventOrganizers.Remove(deleted_event_organizer);
            _dbContext.SaveChanges();
            return Ok();
        }
        [HttpPost("update")]
        public ActionResult<EventOrganizer> UpdateEventOrganizer(int id, EventOrganizer updated_event_organizer)
        {
            var event_organizer_to_be_updated = _dbContext.EventOrganizers.Find(id);
            if (event_organizer_to_be_updated == null)
            {
                return BadRequest();
            }
            _dbContext.EventOrganizers.Update(event_organizer_to_be_updated);
            _dbContext.SaveChanges();
            return Ok(event_organizer_to_be_updated);

        }
    }

}
