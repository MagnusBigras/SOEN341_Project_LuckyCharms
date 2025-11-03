using Lucky_Charm_Event_track.Enums;
using Microsoft.VisualBasic;
using System;

namespace Lucky_Charm_Event_track.Models
{
    public class Reminder
    {
        public int ID { get; set; }
        public string ReminderName { get; set; }
        public int EventID { get; set; }    
        public Event Event { get; set; }   
        public bool isSent { get; set; }
        public DateTime ReminderDate {  get; set; }
        public ReminderFrequency Frequency { get; set; }
        public string Message { get; set; }
        public bool isActive { get; set; }
    }
}
