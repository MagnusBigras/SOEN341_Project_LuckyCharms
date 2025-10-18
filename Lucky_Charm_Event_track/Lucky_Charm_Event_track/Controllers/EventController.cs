using Lucky_Charm_Event_track.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.Extensions.Logging;

namespace Lucky_Charm_Event_track.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventController : ControllerBase
    {
        private readonly WebAppDBContext _dbContext;
        public EventController(WebAppDBContext context)
        {
            _dbContext = context;
        }
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            return await _dbContext.Events.ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEventById(int id)
        {
            var temp_event = await _dbContext.Events
                .Include(e => e.Metric)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (temp_event == null)
            {
                return NotFound();
            }
            return temp_event;
        }
        [HttpGet("organizer")]
        public async Task<ActionResult<IEnumerable<Event>>> GetEventsbyOrganizer(EventOrganizer eventOrganizer)
        {
            var organizer_events = _dbContext.Events.Where(e => e.EventOrganizerId == eventOrganizer.Id).ToList();
            return organizer_events;
        }
        [HttpGet("city")]
        public async Task<ActionResult<IEnumerable<Event>>> GetEventByCity(string city) 
        {
            var nearby_events = _dbContext.Events.Where(e => e.City == city).ToList();
            return nearby_events;
        }
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<Event>>> GetActiveEvents() 
        {
            var active_events = _dbContext.Events.Where(e => e.isActive).ToList();
            return active_events;
        }
        [HttpPost("create")]
        public  ActionResult<Event> CreateEvent([FromBody] Event newEvent)
        {
            Console.WriteLine($"Received: {newEvent?.EventName}, {newEvent?.StartTime}");
            if (newEvent == null) 
            {
                return BadRequest();
            }   
           _dbContext.Events.Add(newEvent);
            _dbContext.SaveChanges();
            // Initialize associated Metric
            var metric = new Metric
            {
                EventId = newEvent.Id,
                TotalRevenue = 0,
                LastMonthRevenue = 0,
                NewAttendees = 0,
                LastMonthAttendees = 0,
                TotalCapacity = newEvent.Capacity,
                UsedCapacity = 0,
                LastRemaining = newEvent.Capacity,
                RevenueByMonth = new System.Collections.Generic.List<double>(),
                AttendanceByMonth = new System.Collections.Generic.List<int>()
            };
            _dbContext.Metrics.Add(metric);
            _dbContext.SaveChanges();

            return Ok(new { message = "Event created successfully", eventId = newEvent.Id });
        }

         [HttpGet("organizer/{organizerId}/metrics")]
        public IActionResult GetMetricsForOrganizer(int organizerId)
        {
            var organizer = _dbContext.EventOrganizers.FirstOrDefault(o => o.Id == organizerId);
            if (organizer == null)
            {
                return NotFound(new { error = "Organizer not found." });
            }
            var events = _dbContext.Events.Where(e => e.EventOrganizerId == organizerId).ToList();
            if (events == null || events.Count == 0)
            {
                return NotFound(new { error = "No events found for this organizer." });
            }
            var metrics = _dbContext.Metrics
                .Where(m => events.Select(ev => ev.Id).Contains(m.EventId))
                .ToList();
            return Ok(metrics);
        }


        [HttpPost("delete")]
        public ActionResult<Event> DeleteEvent(int id)
        {
            var deleted_event = _dbContext.Events.Find(id);
            if (deleted_event == null)
            {
                return BadRequest();
            }
            _dbContext.Events.Remove(deleted_event);
            _dbContext.SaveChanges();
            return Ok();
        }
        [HttpPost("update")]
        public ActionResult<Event> UpdateEvent(int id, Event updated_event) 
        {
            var event_to_be_updated = _dbContext.Events.Find(id);
            if(event_to_be_updated == null) 
            {
                return BadRequest();
            }
            _dbContext.Events.Update(updated_event);
            _dbContext.SaveChanges();
            return Ok();

        }

    }
}
