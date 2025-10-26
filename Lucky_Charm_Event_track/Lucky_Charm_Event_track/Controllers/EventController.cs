using Lucky_Charm_Event_track.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;

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

        public class EventVisibilityUpdate
        {
            public int Id { get; set; }
            public bool IsActive { get; set; }
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
                .Include(e => e.Tickets)  
                .Include(e => e.Prices)
                .Include(e => e.Metric)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (temp_event == null) return NotFound();
            return temp_event;
        }


        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<Event>>> GetActiveEvents()
        {
            var active_events = _dbContext.Events.Where(e => e.isActive).ToList();
            return active_events;
        }

        [HttpPost("create")]
        public ActionResult<Event> CreateEvent([FromBody] Event newEvent)
        {
            if (newEvent == null) 
                return BadRequest("Event cannot be null");

            // Add the event to the database
            _dbContext.Events.Add(newEvent);
            _dbContext.SaveChanges(); 

            // Create metric entry
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
                RevenueByMonth = new List<double>(),
                AttendanceByMonth = new List<int>()
            };
            _dbContext.Metrics.Add(metric);
            _dbContext.SaveChanges();

            // Generate tickets
            newEvent.Tickets = new List<Ticket>();

            if (newEvent.Prices != null && newEvent.Prices.Count > 0)
            {
                // Use PriceTiers to generate tickets
                foreach (var tier in newEvent.Prices)
                {
                    for (int i = 0; i < tier.MaxQuantity; i++)
                    {
                        var ticket = new Ticket
                        {
                            EventId = newEvent.Id,
                            TicketType = tier.TicketType,
                            Price = tier.Price,
                            PurchaseDate = DateTime.MinValue, 
                            CheckedIn = false,
                            QRCodeText = Guid.NewGuid().ToString()
                        };
                        _dbContext.Tickets.Add(ticket);
                        newEvent.Tickets.Add(ticket);
                    }
                }
            }
            else
            {
                // No PriceTiers: generate default tickets according to capacity
                for (int i = 0; i < newEvent.Capacity; i++)
                {
                    var ticket = new Ticket
                    {
                        EventId = newEvent.Id,
                        TicketType = newEvent.TicketType,
                        Price = 0, 
                        PurchaseDate = DateTime.MinValue,
                        CheckedIn = false,
                        QRCodeText = Guid.NewGuid().ToString()
                    };
                    _dbContext.Tickets.Add(ticket);
                    newEvent.Tickets.Add(ticket);
                }
            }

            _dbContext.SaveChanges();

            return Ok(new { message = "Event and tickets created successfully", eventId = newEvent.Id });
        }


        [HttpPost("delete")]
        public ActionResult DeleteEvent([FromBody] int id)
        {
            var deleted_event = _dbContext.Events.Find(id);
            if (deleted_event == null) return BadRequest("Event not found.");

            _dbContext.Events.Remove(deleted_event);
            _dbContext.SaveChanges();
            return Ok(new { message = "Event deleted successfully." });
        }


        [HttpPost("update")]
        public async Task<IActionResult> UpdateEvent([FromBody] Event updatedEvent)
        {
            // Load the existing event with prices
            var existingEvent = await _dbContext.Events
                .Include(e => e.Prices)
                .FirstOrDefaultAsync(e => e.Id == updatedEvent.Id);

            if (existingEvent == null) return NotFound("Event not found.");

            // Update basic event fields
            existingEvent.EventName = updatedEvent.EventName;
            existingEvent.EventDescription = updatedEvent.EventDescription;
            existingEvent.StartTime = updatedEvent.StartTime;
            existingEvent.Address = updatedEvent.Address;
            existingEvent.City = updatedEvent.City;
            existingEvent.Region = updatedEvent.Region;
            existingEvent.PostalCode = updatedEvent.PostalCode;
            existingEvent.Country = updatedEvent.Country;
            existingEvent.Capacity = updatedEvent.Capacity;
            existingEvent.TicketType = updatedEvent.TicketType;
            existingEvent.isActive = updatedEvent.isActive;
            existingEvent.UpdatedAt = updatedEvent.UpdatedAt;
            existingEvent.Category = updatedEvent.Category;


            // Replace price tiers
            existingEvent.Prices.Clear();
            if (updatedEvent.Prices != null && updatedEvent.Prices.Count > 0)
            {
                foreach (var price in updatedEvent.Prices)
                {
                    existingEvent.Prices.Add(new PriceTier
                    {
                        Price = price.Price,
                        TicketType = price.TicketType,
                        Label = price.Label ?? "Default",
                        MaxQuantity = price.MaxQuantity,
                        isAvailable = price.isAvailable
                    });
                }
            }

            await _dbContext.SaveChangesAsync();

            // Delete old tickets
            var oldTickets = await _dbContext.Tickets
                .Where(t => t.EventId == updatedEvent.Id)
                .ToListAsync();
            _dbContext.Tickets.RemoveRange(oldTickets);
            await _dbContext.SaveChangesAsync();

            // Generate new tickets
            if (updatedEvent.Prices != null && updatedEvent.Prices.Count > 0)
            {
                foreach (var tier in updatedEvent.Prices)
                {
                    for (int i = 0; i < tier.MaxQuantity; i++)
                    {
                        var ticket = new Ticket
                        {
                            EventId = existingEvent.Id,
                            UserAccountId = null,
                            TicketType = tier.TicketType,
                            Price = tier.Price,
                            PurchaseDate = DateTime.MinValue,
                            QRCodeText = Guid.NewGuid().ToString(),
                            CheckedIn = false
                        };
                        _dbContext.Tickets.Add(ticket);
                    }
                }
            }

            await _dbContext.SaveChangesAsync();

        return Ok(new { message = "Event and tickets updated successfully" });
        }


        [HttpPost("update-visibility")]
        public IActionResult UpdateVisibility([FromBody] EventVisibilityUpdate request)
        {
            var ev = _dbContext.Events.Find(request.Id);
            if (ev == null) return NotFound();

            ev.isActive = request.IsActive;
            _dbContext.SaveChanges();
            return Ok();
        }
    }
}
