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
            var temp_event = await _dbContext.Events.FindAsync(id);
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
            return Ok(new { message = "Event created successfully", eventId = newEvent.Id });
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
