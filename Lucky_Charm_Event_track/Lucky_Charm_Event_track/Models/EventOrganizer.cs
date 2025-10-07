using System;
using System.Collections.Generic;

namespace Lucky_Charm_Event_track.Models
{
    public class EventOrganizer
    {
        public int Id { get; set; }
        public List<Event> Events { get; set; }
        public int UserAccountId { get; set; }
        public UserAccount Account { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
