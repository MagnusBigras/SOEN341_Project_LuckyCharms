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

        // --- Return all events ---
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            return await _dbContext.Events
                .Include(e => e.Prices)
                .Include(e => e.Tickets)
                .Include(e => e.Organizer)
                .ThenInclude(o => o.Account)
                .ToListAsync();
        }

      [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<Event>>> GetMyEvents()
    {
        try
        {
            var currentUser = Globals.Globals.SessionManager.CurrentLoggedInUser;

            if (currentUser == null)
            {
                currentUser = await _dbContext.UserAccounts.FirstOrDefaultAsync();
                if (currentUser == null)
                    return Ok(new List<Event>());
            }

            var organizer = await _dbContext.EventOrganizers
                .Include(o => o.Events)
                    .ThenInclude(e => e.Prices)
                .Include(o => o.Events)
                    .ThenInclude(e => e.Tickets)
                .Include(o => o.Account)
                .FirstOrDefaultAsync(o => o.UserAccountId == currentUser.Id);

            if (organizer == null)
            {
                var fallbackEvents = await _dbContext.Events
                    .Include(e => e.Prices)
                    .Include(e => e.Organizer)
                    .ThenInclude(o => o.Account)
                    .ToListAsync();
                return Ok(fallbackEvents);
            }

            return Ok(organizer.Events);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading events: {ex.Message}");
            return Ok(new List<Event>());
        }
    }


        // --- Get event by ID ---
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEventById(int id)
        {
            var temp_event = await _dbContext.Events
                .Include(e => e.Tickets)
                .Include(e => e.Prices)
                .Include(e => e.Metric)
                .Include(e => e.Organizer)
                .ThenInclude(o => o.Account)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (temp_event == null) return NotFound();
            return temp_event;
        }

        // --- Create new event ---
       [HttpPost("create")]
        public async Task<ActionResult<Event>> CreateEvent([FromBody] Event newEvent)
        {
            var currentUser = Globals.Globals.SessionManager.CurrentLoggedInUser;
            if (currentUser == null)
                return BadRequest(new { message = "Error! User not logged in!" });

            if (currentUser.IsBanned)
                return BadRequest(new { message = "Your account is banned. You cannot create events." });

            if (currentUser.SuspensionEndUtc != null && currentUser.SuspensionEndUtc > DateTime.UtcNow)
                return BadRequest(new { message = $"Your account is suspended until {currentUser.SuspensionEndUtc.Value:u}. You cannot create events until suspension ends." });

            var organizer = await _dbContext.EventOrganizers
                .FirstOrDefaultAsync(o => o.UserAccountId == currentUser.Id);

            if (organizer == null)
                return BadRequest(new { message = "Current user is not an organizer!" });

            // Link event to organizer
            newEvent.EventOrganizerId = organizer.Id;
            newEvent.IsActive = true;
            newEvent.CreatedAt = DateTime.Now;

            _dbContext.Events.Add(newEvent);
            await _dbContext.SaveChangesAsync();

            // Create metric
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
            await _dbContext.SaveChangesAsync();

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

            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Event and tickets created successfully", eventId = newEvent.Id });
        }


        // --- Delete event ---
        [HttpPost("delete")]
        public async Task<ActionResult> DeleteEvent([FromBody] int id)
        {
            var deleted_event = await _dbContext.Events.FindAsync(id);
            if (deleted_event == null) return BadRequest("Event not found.");

            _dbContext.Events.Remove(deleted_event);
            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Event deleted successfully." });
        }

       // --- Update event ---
        [HttpPost("update")]
        public async Task<IActionResult> UpdateEvent([FromBody] Event updatedEvent)
        {
            var existingEvent = await _dbContext.Events
                .Include(e => e.Prices)
                .Include(e => e.Tickets)
                .FirstOrDefaultAsync(e => e.Id == updatedEvent.Id);

            if (existingEvent == null) return NotFound("Event not found.");

            // Update basic event info
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
            existingEvent.IsActive = updatedEvent.IsActive;
            existingEvent.UpdatedAt = DateTime.Now;
            existingEvent.Category = updatedEvent.Category;

            // Update or add price tiers
            foreach (var updatedPrice in updatedEvent.Prices)
            {
                var existingPrice = existingEvent.Prices
                    .FirstOrDefault(p => p.TicketType == updatedPrice.TicketType && p.Label == updatedPrice.Label);

                if (existingPrice != null)
                {
                    // Update price tier info
                    existingPrice.Price = updatedPrice.Price;
                    existingPrice.MaxQuantity = updatedPrice.MaxQuantity;
                    existingPrice.isAvailable = updatedPrice.isAvailable;
                }
                else
                {
                    // New price tier
                    existingEvent.Prices.Add(new PriceTier
                    {
                        Price = updatedPrice.Price,
                        TicketType = updatedPrice.TicketType,
                        Label = updatedPrice.Label ?? "Default",
                        MaxQuantity = updatedPrice.MaxQuantity,
                        isAvailable = updatedPrice.isAvailable
                    });
                }
            }

            await _dbContext.SaveChangesAsync();

            // Handle tickets: only add new ones if needed
            foreach (var tier in existingEvent.Prices)
            {
                // Count existing tickets for this tier
                int currentCount = existingEvent.Tickets.Count(t => t.TicketType == tier.TicketType);

                // Add tickets if updated MaxQuantity is greater
                int ticketsToAdd = tier.MaxQuantity - currentCount;
                for (int i = 0; i < ticketsToAdd; i++)
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

            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Event updated and tickets adjusted successfully" });
        }


        // --- Update visibility ---
        [HttpPost("update-visibility")]
        public async Task<IActionResult> UpdateVisibility([FromBody] EventVisibilityUpdate request)
        {
            var ev = await _dbContext.Events.FindAsync(request.Id);
            if (ev == null) return NotFound();

            ev.IsActive = request.IsActive;
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
