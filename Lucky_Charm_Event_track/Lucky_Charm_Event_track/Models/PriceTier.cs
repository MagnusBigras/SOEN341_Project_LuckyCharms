using Lucky_Charm_Event_track.Enums;

namespace Lucky_Charm_Event_track.Models
{
    public class PriceTier
    {
        public int Id { get; set; }
        public TicketTypes TicketType { get; set; }
        public double Price { get; set; }
        public int EventId { get; set; }
        public Event Event { get; set; }
        public string Label { get; set; }
        public int MaxQuantity { get; set; }
        public bool isAvailable { get; set; }

    }
}
