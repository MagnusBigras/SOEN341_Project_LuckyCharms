using Lucky_Charm_Event_track.Enums;
using System;
using System.Collections.Generic;

namespace Lucky_Charm_Event_track.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string EventName { get; set; }
        public string EventDescription { get; set; }

        public string Category { get; set; }

        public DateTime StartTime { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public int Capacity { get; set; }
        public int EventOrganizerId { get; set; }
        public EventOrganizer Organizer { get; set; }
        public TicketTypes TicketType { get; set; }
        public List<PriceTier> Prices { get; set; }
        public bool isActive { get; set; }
        public bool IsActive { get => isActive; set => isActive = value; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set;}
        public List<Ticket> Tickets { get; set; }
        public Metric Metric { get; set; }
        

    }
}
