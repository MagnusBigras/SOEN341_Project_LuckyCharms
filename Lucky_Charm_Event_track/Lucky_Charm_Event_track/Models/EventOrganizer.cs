using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lucky_Charm_Event_track.Models
{
    public class EventOrganizer
    {
        public int Id { get; set; }

        public List<Event> Events { get; set; } = new();
        public int UserAccountId { get; set; }
        public UserAccount Account { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public List<Organization> Organizations { get; set; } = new();

        // NOT stored in DB — derived from Account
        [NotMapped]
        public string OrganizerName => $"{Account?.FirstName} {Account?.LastName}".Trim();
    }
}
