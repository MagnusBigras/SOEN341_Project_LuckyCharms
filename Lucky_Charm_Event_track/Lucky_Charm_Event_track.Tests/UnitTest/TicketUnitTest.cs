using System;
using Lucky_Charm_Event_track.Models;
using Xunit;

namespace Lucky_Charm_Event_track.Tests
{
    public class TicketUnitTest
    {
        // Test that a ticket can be assigned to a user after creation
        [Fact]
        public void Ticket_CanBeAssignedToUser()
        {
            var ticket = new Ticket { UserAccountId = null };
            ticket.UserAccountId = 12;
            Assert.Equal(12, ticket.UserAccountId);
        }

        // Test that new tickets default to unpaid
        [Fact]
        public void Ticket_IsPaid_DefaultsToFalse()
        {
            var ticket = new Ticket();
            Assert.False(ticket.Paid);
        }

        // Test that the CheckedIn flag can be toggled correctly
        [Fact]
        public void Ticket_CheckIn_TogglesCorrectly()
        {
            var ticket = new Ticket { CheckedIn = false };
            ticket.CheckedIn = true;
            Assert.True(ticket.CheckedIn);
        }

        // Test that the purchase date is stored correctly when set
        [Fact]
        public void Ticket_PurchaseDate_SetsCorrectly()
        {
            var now = DateTime.UtcNow;
            var ticket = new Ticket { PurchaseDate = now };
            Assert.Equal(now, ticket.PurchaseDate);
        }
    }
}
