using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Collections.Generic;

namespace Lucky_Charm_Event_track.Models
{
    public class Metric
    {
        public int Id { get; set; }
        public double TotalRevenue { get; set; }
        public double LastMonthRevenue { get; set; }
        public int NewAttendees { get; set; }
        public int LastMonthAttendees { get; set; }
        public int TotalCapacity { get; set; }
        public int UsedCapacity { get; set; }
        public int LastRemaining { get; set; }
        public List<double> RevenueByMonth { get; set; }
        public List<int> AttendanceByMonth { get; set; }
        public Event Event { get; set; }
        public int EventId { get; set; }

    }
}
