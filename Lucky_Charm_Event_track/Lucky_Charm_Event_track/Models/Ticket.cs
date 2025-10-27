using Lucky_Charm_Event_track.Enums;
using System;

namespace Lucky_Charm_Event_track.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public Event Event { get; set; }
        public int? UserAccountId { get; set; }  // nullable
        public UserAccount Account { get; set; }
        public TicketTypes TicketType { get; set; }
        public double Price { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string QRCodeText { get; set; }
        public bool CheckedIn { get; set; }

        public bool IsHiddenInCalendar { get; set; } = false;
        public bool Paid { get; set; } = false; 



    }
}
