using System;
using Lucky_Charm_Event_track.Models;
using Xunit;

namespace Lucky_Charm_Event_track.Tests
{
    public class EventTest
    {
        // Test that increasing the event capacity updates the value correctly
        [Fact]
        public void IncreaseCapacity_ShouldUpdateCapacityCorrectly()
        {
            var ev = new Event { Capacity = 0 };
            ev.Capacity += 25;
            Assert.Equal(25, ev.Capacity);
        }

        // Test that the IsInterested flag can be toggled
        [Fact]
        public void Event_InterestFlag_TogglesCorrectly()
        {
            var ev = new Event { IsInterested = false };
            ev.IsInterested = true;
            Assert.True(ev.IsInterested);
        }

        // Test that the organizer ID is correctly assigned
        [Fact]
        public void Event_OrganizerId_AssignedCorrectly()
        {
            var ev = new Event { EventOrganizerId = 5 };
            Assert.Equal(5, ev.EventOrganizerId);
        }

        // Test that the event name is stored properly
        [Fact]
        public void Event_HasValidName_WhenSet()
        {
            var ev = new Event { EventName = "Music Festival" };
            Assert.Equal("Music Festival", ev.EventName);
        }
    }
}
