using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Lucky_Charm_Event_track.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Lucky_Charm_Event_track.Tests.IntegrationTests
{
    public class EventControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public EventControllerTests(CustomWebApplicationFactory factory)
        {
            // This HttpClient acts like a fake web browser hitting your API
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAllEvents_ReturnsSuccessStatusCode()
        {
            // Act — Call your GET endpoint (adjust the route to match your actual controller)
            var response = await _client.GetAsync("/api/events/all");

            // Assert — check that the API returned HTTP 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Optionally check that you got back data
            var events = await response.Content.ReadFromJsonAsync<List<Event>>();
            Assert.NotNull(events);
            Assert.True(events.Count > 0); // Should return at least the seeded one
        }

        [Fact]
        public async Task GetEventById_ReturnsNotFound_WhenIdDoesNotExist()
        {
            var response = await _client.GetAsync("/api/events/99999");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var events = await response.Content.ReadFromJsonAsync<List<Event>>();
            Assert.NotNull(events);
            Assert.True(events.Count > 0);
        }

        [Fact]
        public async Task GetActiveEvent_ReturnsNotFound_WhenIdDoesNotExist()
        {
            var response = await _client.GetAsync("/api/events/active");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var events = await response.Content.ReadFromJsonAsync<List<Event>>();
            Assert.NotNull(events);
            Assert.True(events.Count > 0);
        }

        [Fact]
        public async Task CreateEvent_ReturnsSuccess_WhenValid()
        {
            var newEvent = new Event
            {
                EventName = "Integration Test Event",
                EventDescription = "Created via test"
            };

            var response = await _client.PostAsJsonAsync("/api/events/create", newEvent);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task DeleteEvent_ReturnsSuccess_WhenValid()
        {
            var newEvent = new Event
            {
                EventName = "Test Event to Delete",
                EventDescription = "For Deletetion test"
            };

            var response = await _client.PostAsJsonAsync("/api/events/delete", newEvent);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task UpdateEvent_ReturnsSuccess_WhenValid()
        {
            var newEvent = new Event
            {
                EventName = "Integration Test Event",
                EventDescription = "Created via test"
            };

            var response = await _client.PostAsJsonAsync("/api/events/update", newEvent);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        
        [Fact]
        public async Task UpdateEventVisibility_ReturnsSuccess_WhenValid()
        {
            var newEvent = new Event
            {
                EventName = "Integration Test Event",
                EventDescription = "Created via test"
            };

            var response = await _client.PostAsJsonAsync("/api/events/update-visibility", newEvent);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
