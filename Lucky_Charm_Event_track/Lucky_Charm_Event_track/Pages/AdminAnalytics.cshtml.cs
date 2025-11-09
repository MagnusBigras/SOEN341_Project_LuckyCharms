using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Lucky_Charm_Event_track.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Lucky_Charm_Event_track.Pages
{
    public class AdminEventAnalyticsModel : PageModel
    {
        private readonly WebAppDBContext _dbContext;
        public AdminEventAnalyticsModel(WebAppDBContext dbContext) => _dbContext = dbContext;

        public string FirstName { get; set; } = "Admin";

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

        public List<string> Categories { get; set; } = new();
        public List<int> TicketsPerCategory { get; set; } = new();
        public List<int> AttendancePerMonth { get; set; } = new();

        private const int MonthsToShow = 12;

        public void OnGet()
        {
            // --- Load logged-in admin first name ---
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdClaim))
            {
                int userId = int.Parse(userIdClaim);
                var user = _dbContext.UserAccounts.FirstOrDefault(u => u.Id == userId);
                if (user != null) FirstName = user.FirstName;
            }

            // --- Metrics for selected and previous month ---
            var startCurrentMonth = new DateTime(SelectedMonth.Year, SelectedMonth.Month, 1);
            var endCurrentMonth = startCurrentMonth.AddMonths(1);
            var startPreviousMonth = startCurrentMonth.AddMonths(-1);
            var endPreviousMonth = startCurrentMonth;

            var currentMonthEvents = _dbContext.Events
                .Where(e => e.StartTime >= startCurrentMonth && e.StartTime < endCurrentMonth)
                .ToList();

            var previousMonthEvents = _dbContext.Events
                .Where(e => e.StartTime >= startPreviousMonth && e.StartTime < endPreviousMonth)
                .ToList();

            TotalEvents = currentMonthEvents.Count;
            LastMonthEvents = previousMonthEvents.Count;

            TotalTicketsIssued = _dbContext.Tickets.Count(t => t.Event.StartTime >= startCurrentMonth && t.Event.StartTime < endCurrentMonth);
            LastMonthTicketsIssued = _dbContext.Tickets.Count(t => t.Event.StartTime >= startPreviousMonth && t.Event.StartTime < endPreviousMonth);

            TotalTicketsRedeemed = _dbContext.Tickets.Count(t => t.CheckedIn && t.Event.StartTime >= startCurrentMonth && t.Event.StartTime < endCurrentMonth);
            LastMonthTicketsRedeemed = _dbContext.Tickets.Count(t => t.CheckedIn && t.Event.StartTime >= startPreviousMonth && t.Event.StartTime < endPreviousMonth);

            EventsPercentChange = GetPercentChange(LastMonthEvents, TotalEvents);
            TicketsIssuedPercentChange = GetPercentChange(LastMonthTicketsIssued, TotalTicketsIssued);
            TicketsRedeemedPercentChange = GetPercentChange(LastMonthTicketsRedeemed, TotalTicketsRedeemed);

            // --- Categories & tickets per category ---
            Categories = currentMonthEvents.Select(e => e.Category).Distinct().ToList();
            TicketsPerCategory = Categories.Select(cat => 
                _dbContext.Tickets.Count(t => t.Event.Category == cat && t.Event.StartTime >= startCurrentMonth && t.Event.StartTime < endCurrentMonth)
            ).ToList();

            // --- Attendance per month (Jan → Dec of selected year) ---
            AttendancePerMonth.Clear();
            for (int m = 1; m <= 12; m++)
            {
                var monthStart = new DateTime(SelectedMonth.Year, m, 1);
                var monthEnd = monthStart.AddMonths(1);
                int attended = _dbContext.Tickets.Count(t => t.CheckedIn && t.Event.StartTime >= monthStart && t.Event.StartTime < monthEnd);
                AttendancePerMonth.Add(attended);
            }
        }

        private string GetPercentChange(int previous, int current)
        {
            if (previous == 0) return current == 0 ? "N/A" : "▲ 100% (new data)";
            double change = ((double)(current - previous) / previous) * 100;
            string arrow = change >= 0 ? "▲" : "▼";
            return $"{arrow} {Math.Abs(change):0.#}% from last month";
        }
    }
}