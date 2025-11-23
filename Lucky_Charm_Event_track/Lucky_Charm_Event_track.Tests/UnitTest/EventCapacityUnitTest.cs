using Lucky_Charm_Event_track.Models;
using Xunit;

using Lucky_Charm_Event_track.Models;
using Xunit;

using Lucky_Charm_Event_track.Models;
using Xunit;

namespace Lucky_Charm_Event_track.Tests.UnitTest
{
    public class EventCapacityUnitTest
    {
        // Test: Capacity can be increased ONLY when event is sold out
        [Fact]
        public void IncreaseCapacity_OnlyAllowedWhenSoldOut()
        {
            // Arrange
            var ev = new Event
            {
                Capacity = 2,
                TicketsSold = 2 // Event is sold out
            };

            // Act
            bool canIncrease = ev.TicketsSold >= ev.Capacity;
            if (canIncrease)
            {
                ev.Capacity += 3; // Increase capacity from 2 → 5
            }

            // Assert
            Assert.True(canIncrease);         // Increasing should be allowed
            Assert.Equal(5, ev.Capacity);     // New capacity should be correct
        }

        // Test: Increasing capacity should FAIL when event is NOT sold out
        [Fact]
        public void IncreaseCapacity_ShouldFail_WhenNotSoldOut()
        {
            // Arrange
            var ev = new Event
            {
                Capacity = 2,
                TicketsSold = 1 // Not sold out
            };

            // Act
            bool canIncrease = ev.TicketsSold >= ev.Capacity;

            // Assert
            Assert.False(canIncrease);        // Should NOT allow increasing
            Assert.Equal(2, ev.Capacity);     // Capacity stays the same
        }

        /**
         * Test that available tickets update correctly after capacity increases
         */
        [Fact]
        public void AvailableTickets_UpdateAfterCapacityIncrease()
        {
            // Arrange
            var ev = new Event
            {
                Capacity = 3,
                TicketsSold = 3 // Sold out
            };
            // Act
            bool canIncrease = ev.TicketsSold >= ev.Capacity;
            if (canIncrease)
            {
                ev.Capacity += 4; // Increase capacity from 3 → 7
            }
            int availableTickets = ev.Capacity - ev.TicketsSold;
            // Assert
            Assert.True(canIncrease);             // Increasing should be allowed
            Assert.Equal(7, ev.Capacity);         // New capacity should be correct
            Assert.Equal(4, availableTickets);    // Available tickets should reflect new capacity
        }
    }
}