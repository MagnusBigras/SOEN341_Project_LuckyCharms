using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Lucky_Charm_Event_track.Models;

namespace Lucky_Charm_Event_track.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly WebAppDBContext _db;
        public AdminController(WebAppDBContext db) { _db = db; }

        [HttpGet("organizers/{id:int}")]
        public async Task<ActionResult<object>> GetOrganizer(int id)
        {
            var org = await _db.EventOrganizers
                .Include(o => o.Account)
                .Include(o => o.Events)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (org == null) return NotFound();

            var dto = new
            {
                org.Id,
                org.IsActive,
                Account = org.Account == null ? null : new
                {
                    org.UserAccountId,
                    org.Account.FirstName,
                    org.Account.LastName
                },
                Events = org.Events?.Select(e => new
                {
                    e.Id,
                    e.EventName,
                    e.StartTime
                })
            };

            return Ok(dto);
        }

        public class ReasonBody { public string Reason { get; set; } }

        [HttpPost("organizers/{id:int}/approve")]
        public async Task<IActionResult> ApproveOrganizer(int id)
        {
            var org = await _db.EventOrganizers.FirstOrDefaultAsync(o => o.Id == id);
            if (org == null) return NotFound();
            if (!org.IsActive)
            {
                org.IsActive = true;
                await _db.SaveChangesAsync();
            }
            return NoContent();
        }

        [HttpPost("organizers/{id:int}/reject")]
        public async Task<IActionResult> RejectOrganizer(int id, [FromBody] ReasonBody body)
        {
            var org = await _db.EventOrganizers.FirstOrDefaultAsync(o => o.Id == id);
            if (org == null) return NotFound();
            if (org.IsActive)
            {
                org.IsActive = false;
                await _db.SaveChangesAsync();
            }
            return Ok(new { message = "Organizer rejected", id, reason = body?.Reason });
        }

        [HttpGet("events/pending")]
        public async Task<IActionResult> GetPendingEvents()
        {
            var rows = await _db.Events
                .AsNoTracking()
                .Where(e => !e.IsActive)
                .OrderBy(e => e.CreatedAt)
                .Select(e => new { e.Id, e.EventName, e.StartTime, e.Category, e.City, e.Region, e.Country, e.EventOrganizerId })
                .ToListAsync();

            return Ok(rows);
        }

        [HttpPost("events/{eventId:int}/approve")]
        public async Task<IActionResult> ApproveEvent(int eventId)
        {
            var ev = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
            if (ev == null) return NotFound();
            if (!ev.IsActive)
            {
                ev.IsActive = true;
                ev.UpdatedAt = System.DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
            return NoContent();
        }

        [HttpPost("events/{eventId:int}/reject")]
        public async Task<IActionResult> RejectEvent(int eventId, [FromBody] ReasonBody body)
        {
            var ev = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
            if (ev == null) return NotFound();
            if (ev.IsActive)
            {
                ev.IsActive = false;
                ev.UpdatedAt = System.DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
            return Ok(new { message = "Event rejected", eventId, reason = body?.Reason });
        }

        [HttpDelete("events/{eventId:int}")]
        public async Task<IActionResult> RemoveEvent(int eventId)
        {
            var ev = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
            if (ev == null) return NotFound();
            _db.Events.Remove(ev);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Event removed", eventId });
        }
    }
}
