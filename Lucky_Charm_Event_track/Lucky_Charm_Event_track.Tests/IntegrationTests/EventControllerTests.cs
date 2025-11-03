using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Lucky_Charm_Event_track.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace Lucky_Charm_Event_track.Tests.IntegrationTests
{
    public class EventControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public EventControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task GetAllEvents_ReturnsAllEvents()
        {
            var response = await _client.GetAsync("/api/events/all");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var events = await response.Content.ReadFromJsonAsync<List<Event>>();
            Assert.NotNull(events);
            Assert.True(events.Count > 0);
        }

        [Fact]
        public async Task GetEventById_ReturnsOk_WhenIdExists()
        {
            var testEventId = 1;
            var response = await _client.GetAsync($"/api/events/{testEventId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var eventData = await response.Content.ReadFromJsonAsync<Event>();
            Assert.NotNull(eventData);
            Assert.Equal(testEventId, eventData.Id);
        }

        [Fact]
        public async Task GetActiveEvent_ReturnsActiveEvents()
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
            int organizerId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                var user = await db.UserAccounts.FirstOrDefaultAsync(u => u.UserName == "defaultuser");
                Assert.NotNull(user);
                
                var organizer = await db.EventOrganizers.FirstOrDefaultAsync(o => o.UserAccountId == user.Id);
                Assert.NotNull(organizer); 

                organizerId = organizer.Id;
            }

            var newEvent = new Event
            {
                EventName = "Integration Test Event",
                EventDescription = "Created via integration test",
                StartTime = DateTime.UtcNow.AddDays(7),
                Address = "123 Test Street",
                City = "Test City",
                Region = "Test Region",
                PostalCode = "12345",
                Country = "Test Country",
                Capacity = 100,
                EventOrganizerId = organizerId,
                TicketType = Enums.TicketTypes.Free,
                isActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Category = "Test"
            };

            var response = await _client.PostAsJsonAsync("/api/events/create", newEvent);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task DeleteEvent_ReturnsSuccess_WhenValid()
        {
            // event to delete
            int eventIdToDelete;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                var user = await db.UserAccounts.FirstOrDefaultAsync(u => u.UserName == "defaultuser");
                Assert.NotNull(user);
                
                var organizer = await db.EventOrganizers.FirstOrDefaultAsync(o => o.UserAccountId == user.Id);
                Assert.NotNull(organizer);

                var testEvent = new Event
                {
                    EventName = "Event to Delete",
                    EventDescription = "Will be deleted",
                    StartTime = DateTime.UtcNow,
                    Address = "Delete Address",
                    City = "Delete City",
                    Region = "Delete Region",
                    PostalCode = "00000",
                    Country = "Delete Country",
                    Capacity = 50,
                    EventOrganizerId = organizer.Id,
                    isActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                db.Events.Add(testEvent);
                await db.SaveChangesAsync();
                eventIdToDelete = testEvent.Id;
            }

            var response = await _client.PostAsJsonAsync("/api/events/delete", eventIdToDelete);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task UpdateEvent_ReturnsSuccess_WhenValid()
        {
            //event to update
            int eventIdToUpdate;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                var user = await db.UserAccounts.FirstOrDefaultAsync(u => u.UserName == "defaultuser");
                Assert.NotNull(user);
                
                var organizer = await db.EventOrganizers.FirstOrDefaultAsync(o => o.UserAccountId == user.Id);
                Assert.NotNull(organizer);

                var testEvent = new Event
                {
                    EventName = "Original Event Name",
                    EventDescription = "Original Description",
                    StartTime = DateTime.UtcNow,
                    Address = "Original Address",
                    City = "Original City",
                    Region = "Original Region",
                    PostalCode = "11111",
                    Country = "Original Country",
                    Capacity = 50,
                    EventOrganizerId = organizer.Id,
                    TicketType = Enums.TicketTypes.Free,
                    isActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Category = "Original"
                };

                db.Events.Add(testEvent);
                await db.SaveChangesAsync();
                eventIdToUpdate = testEvent.Id;
            }

            var updatedEvent = new Event
            {
                Id = eventIdToUpdate,
                EventName = "Updated Event Name",
                EventDescription = "Updated Description",
                StartTime = DateTime.UtcNow.AddDays(10),
                Address = "Updated Address",
                City = "Updated City",
                Region = "Updated Region",
                PostalCode = "22222",
                Country = "Updated Country",
                Capacity = 75,
                TicketType = Enums.TicketTypes.Free,
                isActive = true,
                UpdatedAt = DateTime.UtcNow,
                Category = "Updated"
            };

            var response = await _client.PostAsJsonAsync("/api/events/update", updatedEvent);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        
        [Fact]
        public async Task UpdateEventVisibility_ReturnsSuccess_WhenValid()
        {
            int eventId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                var user = await db.UserAccounts.FirstOrDefaultAsync(u => u.UserName == "defaultuser");
                Assert.NotNull(user);
                
                var organizer = await db.EventOrganizers.FirstOrDefaultAsync(o => o.UserAccountId == user.Id);
                Assert.NotNull(organizer);

                var testEvent = new Event
                {
                    EventName = "Visibility Test Event",
                    EventDescription = "For visibility test",
                    StartTime = DateTime.UtcNow,
                    Address = "Test Address",
                    City = "Test City",
                    Region = "Test Region",
                    PostalCode = "33333",
                    Country = "Test Country",
                    Capacity = 100,
                    EventOrganizerId = organizer.Id,
                    isActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                db.Events.Add(testEvent);
                await db.SaveChangesAsync();
                eventId = testEvent.Id;
            }

            var visibilityUpdate = new 
            {
                Id = eventId,
                IsActive = false
            };

            var response = await _client.PostAsJsonAsync("/api/events/update-visibility", visibilityUpdate);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                var updatedEvent = await db.Events.FindAsync(eventId);
                Assert.NotNull(updatedEvent);
                Assert.False(updatedEvent.isActive);
            }
        }
    }
}