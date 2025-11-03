using Lucky_Charm_Event_track.Models;
using Lucky_Charm_Event_track.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Lucky_Charm_Event_track.Services
{
    public static class DatabaseSeeder
    {
        public static void Seed(WebAppDBContext db)
        {
            // Only seed if no events exist
            if (db.UserAccounts.Any(u => u.UserName == "seeduser"))
            {
                Console.WriteLine("[DatabaseSeeder] Seed data already present (seeduser). Skipping.");
                return;
            }
            var rnd = new Random();

            var user = new UserAccount
            {
                FirstName = "Seed",
                LastName = "User",
                UserName = "seeduser",
                Email = "seeduser@example.com",
                Password = "password",
                PasswordSalt = "salt",
                PhoneNumber = "1234567890",
                DateOfBirth = DateTime.Now.AddYears(-30),
                AccountCreationDate = DateTime.Now.AddYears(-1),
                AccountType = AccountTypes.EventOrganizer,
                LastLogin = DateTime.Now,
                IsActive = true,
                SuspensionEndUtc = null,
                IsBanned = false
            };
            db.UserAccounts.Add(user);
            db.SaveChanges();

            var organizer = new EventOrganizer
            {
                UserAccountId = user.Id,
                CreatedAt = DateTime.Now.AddMonths(-9),
                IsActive = true
            };
            db.EventOrganizers.Add(organizer);
            db.SaveChanges();

            var newEvent = new Event
            {
                EventName = "Simulated Event",
                EventDescription = "This event was created by DatabaseSeeder.",
                City = "Montreal",
                Capacity = 100,
                EventOrganizerId = organizer.Id,
                isActive = true,
                StartTime = DateTime.Now.AddDays(17),
                Address = "123 Main St",
                Region = "QC",
                PostalCode = "H3A 1A1",
                Country = "Canada",
                TicketType = TicketTypes.GeneralAddmission,
                CreatedAt = DateTime.Now.AddDays(5),
                UpdatedAt = DateTime.Now.AddDays(6),
            };
            db.Events.Add(newEvent);
            db.SaveChanges();

            var newEvent2 = new Event
            {
                EventName = "Simulated Event 2",
                EventDescription = "SECOND EVENT FOR TEST.",
                City = "quebec",
                Capacity = 1600,
                EventOrganizerId = organizer.Id,
                isActive = true,
                StartTime = DateTime.Now.AddDays(-29),
                Address = "blv st laurier",
                Region = "maisonneuve",
                PostalCode = "H21A 2A2",
                Country = "france",
                TicketType = TicketTypes.GeneralAddmission,
                CreatedAt = DateTime.Now.AddDays(-31),
                UpdatedAt = DateTime.Now.AddDays(-30),
            };
            db.Events.Add(newEvent2);
            db.SaveChanges();
            var newEvent3 = new Event
            {
                EventName = "Simulated Event 3",
                EventDescription = "Third event for testing.",
                City = "st jean",
                Capacity = 110,
                EventOrganizerId = organizer.Id,
                isActive = true,
                StartTime = DateTime.Now.AddDays(-10),
                Address = "rue des fleurs",
                Region = "chicoutimi",
                PostalCode = "H3H 3A3",
                Country = "cuba",
                TicketType = TicketTypes.GeneralAddmission,
                CreatedAt = DateTime.Now.AddDays(-21),
                UpdatedAt = DateTime.Now.AddDays(-20),
            };
            db.Events.Add(newEvent3);
            db.SaveChanges();

            // generate random values for metric 1
            var totalRevenue1 = Math.Round(rnd.NextDouble() * 5000 + 50, 2);
            var usedCapacity1 = rnd.Next(0, newEvent.Capacity + 1);
            var totalAttendees1 = usedCapacity1;
            var metric = new Metric
            {
                EventId = newEvent.Id,
                TotalRevenue = totalRevenue1,
                LastMonthRevenue = Math.Round(totalRevenue1 * (0.2 + rnd.NextDouble() * 0.8), 2),
                NewAttendees = rnd.Next(0, newEvent.Capacity + 1),
                LastMonthAttendees = rnd.Next(0, newEvent.Capacity + 1),
                TotalCapacity = newEvent.Capacity,
                UsedCapacity = 54,
                LastRemaining = newEvent.Capacity-54,
                RevenueByMonth = new System.Collections.Generic.List<double>(),
                AttendanceByMonth = new System.Collections.Generic.List<int>()
            };
            db.Metrics.Add(metric);
            db.SaveChanges();

            // metric 2 random values
            var totalRevenue2 = Math.Round(rnd.NextDouble() * 20000 + 200, 2);
            var usedCapacity2 = rnd.Next(0, newEvent2.Capacity + 1);
            var totalAttendees2 = usedCapacity2;
            var metric2 = new Metric
            {
                EventId = newEvent2.Id,
                TotalRevenue = totalRevenue2,
                LastMonthRevenue = Math.Round(totalRevenue2 * (0.1 + rnd.NextDouble() * 0.9), 2),
                NewAttendees = rnd.Next(0, newEvent2.Capacity + 1),
                LastMonthAttendees = rnd.Next(0, newEvent2.Capacity + 1),
                TotalCapacity = newEvent2.Capacity,
                UsedCapacity = usedCapacity2,
                LastRemaining = Math.Max(0, newEvent2.Capacity - usedCapacity2),
                RevenueByMonth = GenerateMonthlyRevenue(rnd, totalRevenue2),
                AttendanceByMonth = GenerateMonthlyAttendance(rnd, totalAttendees2, newEvent2.Capacity)
            };
            db.Metrics.Add(metric2);
            db.SaveChanges();
            
            // metric 3 random values
            var totalRevenue3 = Math.Round(rnd.NextDouble() * 10000 + 100, 2);
            var usedCapacity3 = rnd.Next(0, newEvent3.Capacity + 1);
            var totalAttendees3 = usedCapacity3;
            var metric3 = new Metric
            {
                EventId = newEvent3.Id,
                TotalRevenue = totalRevenue3,
                LastMonthRevenue = Math.Round(totalRevenue3 * (0.05 + rnd.NextDouble() * 0.95), 2),
                NewAttendees = rnd.Next(0, newEvent3.Capacity + 1),
                LastMonthAttendees = rnd.Next(0, newEvent3.Capacity + 1),
                TotalCapacity = newEvent3.Capacity,
                UsedCapacity = usedCapacity3,
                LastRemaining = Math.Max(0, newEvent3.Capacity - usedCapacity3),
                RevenueByMonth = GenerateMonthlyRevenue(rnd, totalRevenue3),
                AttendanceByMonth = GenerateMonthlyAttendance(rnd, totalAttendees3, newEvent3.Capacity)
            };
            db.Metrics.Add(metric3);
            db.SaveChanges();
        }

        // helper: generate 12 monthly revenue amounts that sum roughly to totalRevenue (cents rounded)
        private static System.Collections.Generic.List<double> GenerateMonthlyRevenue(Random rnd, double totalRevenue)
        {
            var weights = new double[12];
            double sum = 0;
            for (int i = 0; i < 12; i++) { weights[i] = 0.5 + rnd.NextDouble(); sum += weights[i]; }
            var list = new System.Collections.Generic.List<double>(12);
            double running = 0;
            for (int i = 0; i < 11; i++)
            {
                var v = Math.Round(totalRevenue * (weights[i] / sum), 2);
                list.Add(v);
                running += v;
            }
            // last month = remainder to make the sum equal totalRevenue (adjust for rounding)
            list.Add(Math.Round(Math.Max(0, totalRevenue - running), 2));
            return list;
        }

        // helper: distribute integer attendees across 12 months (sum equals totalAttendees)
        private static System.Collections.Generic.List<int> GenerateMonthlyAttendance(Random rnd, int totalAttendees, int capacity)
        {
            var weights = new double[12];
            double sum = 0;
            for (int i = 0; i < 12; i++) { weights[i] = 0.5 + rnd.NextDouble(); sum += weights[i]; }
            var list = new System.Collections.Generic.List<int>(12);
            int distributed = 0;
            for (int i = 0; i < 11; i++)
            {
                var val = (int)Math.Round(totalAttendees * (weights[i] / sum));
                if (val > capacity) val = capacity;
                list.Add(val);
                distributed += val;
            }
            list.Add(Math.Max(0, Math.Min(capacity, totalAttendees - distributed)));
            return list;
        }
    }
}
