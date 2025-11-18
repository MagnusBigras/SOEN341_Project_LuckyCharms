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
    public class AdminControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public AdminControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task GetOrganizer_ReturnsOk_WhenOrganizerExists()
        {
            int organizerId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                var organizer = await db.EventOrganizers.FirstOrDefaultAsync();
                Assert.NotNull(organizer);
                organizerId = organizer.Id;
            }

            var response = await _client.GetAsync($"/api/admin/organizers/{organizerId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var organizerData = await response.Content.ReadFromJsonAsync<dynamic>();
            Assert.NotNull(organizerData);
        }

        [Fact]
        public async Task GetOrganizer_ReturnsNotFound_WhenOrganizerDoesNotExist()
        {
            var response = await _client.GetAsync("/api/admin/organizers/99999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ApproveOrganizer_ReturnsNoContent_WhenOrganizerExists()
        {
            int organizerId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();

                // Create a test organizer that is inactive
                var user = new UserAccount
                {
                    FirstName = "Test",
                    LastName = "Organizer",
                    UserName = "testorg" + Guid.NewGuid().ToString().Substring(0, 8),
                    Email = "testorg@example.com",
                    Password = "hashedpassword",
                    PasswordSalt = "salt",
                    PhoneNumber = "1111111111",
                    DateOfBirth = new DateTime(1985, 1, 1),
                    AccountCreationDate = DateTime.UtcNow,
                    AccountType = Enums.AccountTypes.EventOrganizer,
                    IsActive = true
                };

                db.UserAccounts.Add(user);
                await db.SaveChangesAsync();

                var organizer = new EventOrganizer
                {
                    UserAccountId = user.Id,
                    IsActive = false
                };

                db.EventOrganizers.Add(organizer);
                await db.SaveChangesAsync();
                organizerId = organizer.Id;
            }

            var response = await _client.PostAsync($"/api/admin/organizers/{organizerId}/approve", null);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify organizer is now active
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                var organizer = await db.EventOrganizers.FindAsync(organizerId);
                Assert.NotNull(organizer);
                Assert.True(organizer.IsActive);
            }
        }

        [Fact]
        public async Task ApproveOrganizer_ReturnsNotFound_WhenOrganizerDoesNotExist()
        {
            var response = await _client.PostAsync("/api/admin/organizers/99999/approve", null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task RejectOrganizer_ReturnsOk_WhenOrganizerExists()
        {
            int organizerId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();

                // Create a test organizer that is active
                var user = new UserAccount
                {
                    FirstName = "Test",
                    LastName = "Organizer",
                    UserName = "testorg" + Guid.NewGuid().ToString().Substring(0, 8),
                    Email = "testorg2@example.com",
                    Password = "hashedpassword",
                    PasswordSalt = "salt",
                    PhoneNumber = "2222222222",
                    DateOfBirth = new DateTime(1985, 1, 1),
                    AccountCreationDate = DateTime.UtcNow,
                    AccountType = Enums.AccountTypes.EventOrganizer,
                    IsActive = true
                };

                db.UserAccounts.Add(user);
                await db.SaveChangesAsync();

                var organizer = new EventOrganizer
                {
                    UserAccountId = user.Id,
                    IsActive = true
                };

                db.EventOrganizers.Add(organizer);
                await db.SaveChangesAsync();
                organizerId = organizer.Id;
            }

            var requestBody = new { Reason = "Test rejection reason" };
            var response = await _client.PostAsJsonAsync($"/api/admin/organizers/{organizerId}/reject", requestBody);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            Assert.NotNull(result);

            // Verify organizer is now inactive
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                var organizer = await db.EventOrganizers.FindAsync(organizerId);
                Assert.NotNull(organizer);
                Assert.False(organizer.IsActive);
            }
        }

        [Fact]
        public async Task RejectOrganizer_ReturnsNotFound_WhenOrganizerDoesNotExist()
        {
            var requestBody = new { Reason = "Test rejection reason" };
            var response = await _client.PostAsJsonAsync("/api/admin/organizers/99999/reject", requestBody);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetPendingEvents_ReturnsOk_AndReturnsPendingEvents()
        {
            var response = await _client.GetAsync("/api/admin/events/pending");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var events = await response.Content.ReadFromJsonAsync<List<dynamic>>();
            Assert.NotNull(events);
        }

        [Fact]
        public async Task ApproveEvent_ReturnsNoContent_WhenEventExists()
        {
            int eventId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();

                // Get an organizer to associate the event with
                var organizer = await db.EventOrganizers.FirstOrDefaultAsync();
                Assert.NotNull(organizer);

                // Create a test event that is inactive (pending)
                var testEvent = new Event
                {
                    EventName = "Pending Event",
                    EventDescription = "Test pending event",
                    StartTime = DateTime.UtcNow.AddDays(30),
                    Address = "123 Test St",
                    City = "TestCity",
                    Region = "TestRegion",
                    PostalCode = "12345",
                    Country = "TestCountry",
                    Capacity = 100,
                    EventOrganizerId = organizer.Id,
                    TicketType = Enums.TicketTypes.Free,
                    IsActive = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Category = "Test"
                };

                db.Events.Add(testEvent);
                await db.SaveChangesAsync();
                eventId = testEvent.Id;
            }

            var response = await _client.PostAsync($"/api/admin/events/{eventId}/approve", null);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify event is now active
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                var ev = await db.Events.FindAsync(eventId);
                Assert.NotNull(ev);
                Assert.True(ev.IsActive);
            }
        }

        [Fact]
        public async Task ApproveEvent_ReturnsNotFound_WhenEventDoesNotExist()
        {
            var response = await _client.PostAsync("/api/admin/events/99999/approve", null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task RejectEvent_ReturnsOk_WhenEventExists()
        {
            int eventId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();

                // Get an organizer to associate the event with
                var organizer = await db.EventOrganizers.FirstOrDefaultAsync();
                Assert.NotNull(organizer);

                // Create a test event that is active
                var testEvent = new Event
                {
                    EventName = "Active Event to Reject",
                    EventDescription = "Test active event",
                    StartTime = DateTime.UtcNow.AddDays(30),
                    Address = "456 Test St",
                    City = "TestCity",
                    Region = "TestRegion",
                    PostalCode = "54321",
                    Country = "TestCountry",
                    Capacity = 100,
                    EventOrganizerId = organizer.Id,
                    TicketType = Enums.TicketTypes.Free,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Category = "Test"
                };

                db.Events.Add(testEvent);
                await db.SaveChangesAsync();
                eventId = testEvent.Id;
            }

            var requestBody = new { Reason = "Event does not meet guidelines" };
            var response = await _client.PostAsJsonAsync($"/api/admin/events/{eventId}/reject", requestBody);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            Assert.NotNull(result);

            // Verify event is now inactive
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                var ev = await db.Events.FindAsync(eventId);
                Assert.NotNull(ev);
                Assert.False(ev.IsActive);
            }
        }

        [Fact]
        public async Task RejectEvent_ReturnsNotFound_WhenEventDoesNotExist()
        {
            var requestBody = new { Reason = "Event does not meet guidelines" };
            var response = await _client.PostAsJsonAsync("/api/admin/events/99999/reject", requestBody);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task RemoveEvent_ReturnsOk_WhenEventExists()
        {
            int eventId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();

                // Get an organizer to associate the event with
                var organizer = await db.EventOrganizers.FirstOrDefaultAsync();
                Assert.NotNull(organizer);

                // Create a test event to delete
                var testEvent = new Event
                {
                    EventName = "Event to Delete",
                    EventDescription = "Test event for deletion",
                    StartTime = DateTime.UtcNow.AddDays(30),
                    Address = "789 Test St",
                    City = "TestCity",
                    Region = "TestRegion",
                    PostalCode = "67890",
                    Country = "TestCountry",
                    Capacity = 100,
                    EventOrganizerId = organizer.Id,
                    TicketType = Enums.TicketTypes.Free,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Category = "Test"
                };

                db.Events.Add(testEvent);
                await db.SaveChangesAsync();
                eventId = testEvent.Id;
            }

            var response = await _client.DeleteAsync($"/api/admin/events/{eventId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            Assert.NotNull(result);

            // Verify event is deleted
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                var ev = await db.Events.FindAsync(eventId);
                Assert.Null(ev);
            }
        }

        [Fact]
        public async Task RemoveEvent_ReturnsNotFound_WhenEventDoesNotExist()
        {
            var response = await _client.DeleteAsync("/api/admin/events/99999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}