namespace Lucky_Charm_Event_track.Models
{
    public class UserSpecificReminder : Reminder
    {
        public int UserAccountId { get; set; }
        public UserAccount UserAccount {  get; set; }
    }
}
