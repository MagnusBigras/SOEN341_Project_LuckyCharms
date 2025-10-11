using Lucky_Charm_Event_track.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lucky_Charm_Event_track.Helpers
{
    public class CSVCreationHelper
    {
        public static void CreateAttendeeListCSV(WebAppDBContext webAppDBContext, int event_id) 
        {
            List<Ticket> tickets =  webAppDBContext.Tickets.Where(e=>e.EventId == event_id).Include(e=> e.Account).ToList();
            string directory = "AttendeesList";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            string filename = $"event_{event_id}.csv";
            string filepath = Path.Combine(directory,filename);
            StreamWriter writer = new StreamWriter(filepath);
            writer.WriteLine("TicketID,First Name, Last Name");
            foreach (Ticket ticket in tickets)
            {
                UserAccount current_account = ticket.Account;
                string row = string.Join(",", ticket.Id, current_account.FirstName, current_account.LastName);
                writer.WriteLine(row);
            }

        }
    }
}
