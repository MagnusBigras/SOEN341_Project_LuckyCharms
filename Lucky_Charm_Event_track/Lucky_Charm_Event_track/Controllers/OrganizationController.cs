using Lucky_Charm_Event_track.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lucky_Charm_Event_track.Controllers
{
    [ApiController]
    [Route("api/organization")]
    public class OrganizationController : ControllerBase
    {
        private readonly WebAppDBContext _db;
        public OrganizationController(WebAppDBContext db)
        {
            _db = db;
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Organization>>> GetAll()
        {
            var list = await _db.Organizations
                .Include(o => o.Organizer)
                .ToListAsync();
            return Ok(list);
        }

        [HttpPost("create")]
        public ActionResult Create([FromBody] Organization org)
        {
            if (org == null) return BadRequest();
            org.CreatedAt = System.DateTime.UtcNow;
            _db.Organizations.Add(org);
            _db.SaveChanges();
            return Ok(new { id = org.Id });
        }

        public class OrganizationUpdateDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public bool? IsActive { get; set; }
        }

        [HttpPost("update")]
        public async Task<ActionResult> Update([FromBody] OrganizationUpdateDto dto)
        {
            if (dto == null || dto.Id == 0) return BadRequest("Invalid payload");
            if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name is required");

            var org = await _db.Organizations.FindAsync(dto.Id);
            if (org == null) return NotFound();

            // prevent duplicate organization names (global uniqueness)
            var duplicate = await _db.Organizations.AnyAsync(o => o.Id != dto.Id && o.Name == dto.Name);
            if (duplicate) return Conflict("An organization with that name already exists.");

            org.Name = dto.Name;
            org.Description = dto.Description ?? org.Description;
            if (dto.IsActive.HasValue) org.IsActive = dto.IsActive.Value;

            _db.Organizations.Update(org);
            await _db.SaveChangesAsync();

            var updated = await _db.Organizations.Include(o => o.Organizer).FirstOrDefaultAsync(o => o.Id == org.Id);
            return Ok(updated);
        }

        [HttpPost("delete")]
        public ActionResult Delete([FromBody] int id)
        {
            var org = _db.Organizations.Find(id);
            if (org == null) return NotFound();
            _db.Organizations.Remove(org);
            _db.SaveChanges();
            return Ok();
        }
    }
}
