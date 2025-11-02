using System;
using System.Collections.Generic;

namespace Lucky_Charm_Event_track.Models
{
    public class Organization
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        // Link to the EventOrganizer that manages this organization
        public int EventOrganizerId { get; set; }
        public EventOrganizer Organizer { get; set; }
        // Current number of users (kept for convenience; can be derived)
        public int CurrentUserCount { get; set; }

        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
