using Lucky_Charm_Event_track.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Lucky_Charm_Event_track.Helpers
{
    public class EventVerificationHelper
    {
        public static  bool PerformEventVerification(WebAppDBContext webAppDBContext, Event newevent) 
        {
            //get name, description, catagory, address, city, region, postal code, country
            var stringproperties = newevent.GetType().GetProperties().Where(p => p.PropertyType == typeof(string));
            foreach (var property in stringproperties)
            {
                string propertyvalue = property.GetValue(newevent) as string;
                if (string.IsNullOrWhiteSpace(propertyvalue))
                {
                   return false;
                }
            }
            //check start time
            if(newevent.StartTime < System.DateTime.Now) 
            {
                return false;
            }
            //check event organizer
            var organizer = webAppDBContext.EventOrganizers.Find(newevent.EventOrganizerId);
            if(organizer == null) 
            {
                return false;
            }
            return true;
        }
    }
}
