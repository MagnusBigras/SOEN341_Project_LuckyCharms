namespace Lucky_Charm_Event_track.Models
{
    public class PaymentDetail
    {
        public int Id { get; set; } 
        public string CardHolderName { get; set; }
        public int CardNumber { get; set; }
        public string ExpiryDate { get; set; }
        public string CVV { get; set; }
        public int UserID { get; set; }
        public UserAccount Account { get; set; }
    }
}
