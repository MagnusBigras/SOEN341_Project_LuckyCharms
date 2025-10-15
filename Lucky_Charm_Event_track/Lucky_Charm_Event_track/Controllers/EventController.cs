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
    public class EventController : Controller
    {
        private readonly WebAppDBContext _dbContext;
        public EventController(WebAppDBContext context)
        {
            _dbContext = context;
        }
        public IActionResult Index()
        {
            return View();
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

        public static void SimulateCreateEvent(WebAppDBContext dbContext)  //for matei testing purposes
        {
            // 1. Seed a UserAccount
            var user = new Lucky_Charm_Event_track.Models.UserAccount
            {
                FirstName = "Seed",
                LastName = "User",
                UserName = "seeduser",
                Email = "seeduser@example.com",
                Password = "password",
                PasswordSalt = "salt",
                PhoneNumber = "1234567890",
                DateOfBirth = System.DateTime.Now.AddYears(-30),
                AccountCreationDate = System.DateTime.Now.AddYears(-1),
                AccountType = Lucky_Charm_Event_track.Enums.AccountTypes.EventOrganizer,
                LastLogin = System.DateTime.Now,
                IsActive = true
            };
            dbContext.UserAccounts.Add(user);
            dbContext.SaveChanges();

            // 2. Seed an EventOrganizer
            var organizer = new Lucky_Charm_Event_track.Models.EventOrganizer
            {
                UserAccountId = user.Id,
                CreatedAt = System.DateTime.Now.AddMonths(-9),
                IsActive = true
            };
            dbContext.EventOrganizers.Add(organizer);
            dbContext.SaveChanges();

            // 3. Seed an Event
            var newEvent = new Lucky_Charm_Event_track.Models.Event
            {
                EventName = "Simulated Event",
                EventDescription = "This event was created by SimulateCreateEvent().",
                City = "Montreal",
                Capacity = 100,
                EventOrganizerId = organizer.Id,
                isActive = true,
                StartTime = System.DateTime.Now.AddDays(17),
                Address = "123 Main St",
                Region = "QC",
                PostalCode = "H3A 1A1",
                Country = "Canada",
                TicketType = Lucky_Charm_Event_track.Enums.TicketTypes.GeneralAddmission,
                CreatedAt = System.DateTime.Now.AddDays(5),
                UpdatedAt = System.DateTime.Now.AddDays(6),
            };
            dbContext.Events.Add(newEvent);
            dbContext.SaveChanges();
            var newEvent2 = new Lucky_Charm_Event_track.Models.Event
            {
                EventName = "Simulated Event 2",
                EventDescription = "SECOND EVENT FOR TEST.",
                City = "quebec",
                Capacity = 160,
                EventOrganizerId = organizer.Id,
                isActive = true,
                StartTime = System.DateTime.Now.AddDays(-29),
                Address = "blv st laurier",
                Region = "maisonneuve",
                PostalCode = "H21A 2A2",
                Country = "france",
                TicketType = Lucky_Charm_Event_track.Enums.TicketTypes.GeneralAddmission,
                CreatedAt = System.DateTime.Now.AddDays(-31),
                UpdatedAt = System.DateTime.Now.AddDays(-30),

            };
            dbContext.Events.Add(newEvent2);
            dbContext.SaveChanges();

            // 4. Seed a Metric
            var metric = new Lucky_Charm_Event_track.Models.Metric
            {
                EventId = newEvent.Id,
                TotalRevenue = 10,
                LastMonthRevenue = 50,
                NewAttendees = 33,
                LastMonthAttendees = 44,
                TotalCapacity = newEvent.Capacity,
                UsedCapacity = 54,
                LastRemaining = newEvent.Capacity,
                RevenueByMonth = new System.Collections.Generic.List<double>(),
                AttendanceByMonth = new System.Collections.Generic.List<int>()
            };
            dbContext.Metrics.Add(metric);
            dbContext.SaveChanges();
            var metric2 = new Lucky_Charm_Event_track.Models.Metric
            {
                EventId = newEvent2.Id,
                TotalRevenue = 100,
                LastMonthRevenue = 500,
                NewAttendees = 330,
                LastMonthAttendees = 440,
                TotalCapacity = newEvent2.Capacity,
                UsedCapacity = 504,
                LastRemaining = newEvent2.Capacity,
                RevenueByMonth = new System.Collections.Generic.List<double>(),
                AttendanceByMonth = new System.Collections.Generic.List<int>()
            };
            dbContext.Metrics.Add(metric2);
            dbContext.SaveChanges();
        }

    }
}
