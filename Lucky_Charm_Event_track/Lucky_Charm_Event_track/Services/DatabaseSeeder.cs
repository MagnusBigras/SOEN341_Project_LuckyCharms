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
            if (db.Events.Any()) return;

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
                IsActive = true
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
                Capacity = 160,
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

            var metric = new Metric
            {
                EventId = newEvent.Id,
                TotalRevenue = 10,
                LastMonthRevenue = 50,
                NewAttendees = 33,
                LastMonthAttendees = 44,
                TotalCapacity = newEvent.Capacity,
                UsedCapacity = 54,
                LastRemaining = newEvent.Capacity,
                RevenueByMonth = new System.Collections.Generic.List<double>(),
                AttendanceByMonth = new System.Collections.Generic.List<int>()
            };
            db.Metrics.Add(metric);
            db.SaveChanges();

            var metric2 = new Metric
            {
                EventId = newEvent2.Id,
                TotalRevenue = 100,
                LastMonthRevenue = 500,
                NewAttendees = 330,
                LastMonthAttendees = 440,
                TotalCapacity = newEvent2.Capacity,
                UsedCapacity = 504,
                LastRemaining = newEvent2.Capacity,
                RevenueByMonth = new System.Collections.Generic.List<double>(),
                AttendanceByMonth = new System.Collections.Generic.List<int>()
            };
            db.Metrics.Add(metric2);
            db.SaveChanges();
            
            var metric3 = new Metric
            {
                EventId = newEvent3.Id,
                TotalRevenue = 200,
                LastMonthRevenue = 600,
                NewAttendees = 400,
                LastMonthAttendees = 500,
                TotalCapacity = newEvent3.Capacity,
                UsedCapacity = 604,
                LastRemaining = newEvent3.Capacity,
                RevenueByMonth = new System.Collections.Generic.List<double>(),
                AttendanceByMonth = new System.Collections.Generic.List<int>()
            };
            db.Metrics.Add(metric3);
            db.SaveChanges();
        }
    }
}
