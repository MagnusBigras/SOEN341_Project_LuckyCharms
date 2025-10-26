using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;

namespace Lucky_Charm_Event_track.Pages
{
    public class AdminEventAnalyticsModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public DateTime SelectedMonth { get; set; } = DateTime.Today;

        public int TotalEvents { get; set; }
        public int LastMonthEvents { get; set; }
        public string EventsPercentChange { get; set; }

        public int TotalTicketsIssued { get; set; }
        public int LastMonthTicketsIssued { get; set; }
        public string TicketsIssuedPercentChange { get; set; }

        public int TotalTicketsRedeemed { get; set; }
        public int LastMonthTicketsRedeemed { get; set; }
        public string TicketsRedeemedPercentChange { get; set; }

        // Tickets per Category
        public List<string> Categories { get; set; }
        public List<int> TicketsPerCategory { get; set; }

        // Attendance Trend per month
        public List<int> AttendancePerMonth { get; set; }


        // Hardcoded monthly data
        private readonly Dictionary<string, (int Events, int TicketsIssued, int TicketsRedeemed)> MonthlyData
            = new()
        {
            { "2025-08", (10, 250, 230) },
            { "2025-09", (12, 300, 280) },
            { "2025-10", (15, 340, 310) }
        };

        // Hardcoded category data per month
        private readonly Dictionary<string, (List<string> Categories, List<int> Tickets)> CategoryData
            = new()
        {
            { "2025-08", (new List<string>{ "Workshop", "Seminar", "Music", "Art" }, new List<int>{ 55, 75, 40, 30 }) },
            { "2025-09", (new List<string>{ "Workshop", "Seminar", "Sports", "Music", "Tech" }, new List<int>{ 65, 90, 50, 60, 25 }) },
            { "2025-10", (new List<string>{ "Workshop", "Seminar", "Sports", "Music" }, new List<int>{ 80, 120, 70, 70 }) },
            { "2025-11", (new List<string>{ "Seminar", "Music", "Tech", "Art" }, new List<int>{ 90, 110, 60, 40 }) },
        };
        
        // Hardcoded attendance trend (12 data points per month)
        private readonly Dictionary<string, List<int>> AttendanceData
            = new()
        {
            { "2025-08", new List<int>{ 50, 60, 55, 70, 65, 80, 75, 60, 90, 85, 70, 100 } },
            { "2025-09", new List<int>{ 55, 65, 60, 75, 70, 85, 80, 65, 95, 90, 75, 105 } },
            { "2025-10", new List<int>{ 60, 70, 65, 80, 75, 90, 85, 70, 100, 95, 80, 110 } }

        };

        public void OnGet()
        {
            string currentKey = SelectedMonth.ToString("yyyy-MM");
            string previousKey = SelectedMonth.AddMonths(-1).ToString("yyyy-MM");

            // Current & previous month metrics
            if (!MonthlyData.TryGetValue(currentKey, out var current)) 
                current = (0, 0, 0);


            if (!MonthlyData.TryGetValue(previousKey, out var previous))
                previous = (0, 0, 0);

            TotalEvents = current.Events;
            TotalTicketsIssued = current.TicketsIssued;
            TotalTicketsRedeemed = current.TicketsRedeemed;


            LastMonthEvents = previous.Events;
            LastMonthTicketsIssued = previous.TicketsIssued;
            LastMonthTicketsRedeemed = previous.TicketsRedeemed;

            EventsPercentChange = GetPercentChange(previous.Events, current.Events);
            TicketsIssuedPercentChange = GetPercentChange(previous.TicketsIssued, current.TicketsIssued);
            TicketsRedeemedPercentChange = GetPercentChange(previous.TicketsRedeemed, current.TicketsRedeemed);


            // Tickets per category
            if (!CategoryData.TryGetValue(currentKey, out var category))
                category = (new List<string>(), new List<int>());

            Categories = category.Categories;
            TicketsPerCategory = category.Tickets;


            // Attendance trend
            if (!AttendanceData.TryGetValue(currentKey, out var attendance))
                attendance = new List<int>(new int[12]);

            AttendancePerMonth = attendance;
        }

        private string GetPercentChange(int previous, int current)
        {
            if (previous == 0) return "N/A";
            double change = ((double)(current - previous) / previous) * 100;
            string arrow = change >= 0 ? "▲" : "▼";
            return $"{arrow} {Math.Abs(change):0.#}% from last month";
        }
    }
}
