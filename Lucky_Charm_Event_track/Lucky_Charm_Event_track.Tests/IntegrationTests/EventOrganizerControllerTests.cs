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
    public class EventOrganizerControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public EventOrganizerControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task GetAllEventOrganizers_ReturnsAllEvents()
        {
            var response = await _client.GetAsync("/api/event_organizer/all");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var eventOrganizers = await response.Content.ReadFromJsonAsync<List<EventOrganizer>>();
            Assert.NotNull(eventOrganizers);
            Assert.True(eventOrganizers.Count > 0);
        }
        //ID
        [Fact]
        public async Task GetEventOrganizerById_ReturnsOk_WhenIdExists()
        {
            var testEventId = 1;
            var response = await _client.GetAsync($"/api/event_organizer/{testEventId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var eventOrganizerData = await response.Content.ReadFromJsonAsync<EventOrganizer>();
            Assert.NotNull(eventOrganizerData);
            Assert.Equal(testEventId, eventOrganizerData.Id);
        }
        [Fact]
        public async Task GetActiveEventOrganizer_ReturnsActiveEventOrganizer()
        {
            var response = await _client.GetAsync("/api/event_organizer/active");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var eventOrganizer = await response.Content.ReadFromJsonAsync<List<EventOrganizer>>();
            Assert.NotNull(eventOrganizer);
            Assert.True(eventOrganizer.Count > 0);
        }

        [Fact]
public async Task CreateEventOrganizer_ReturnsSuccess_WhenValid()
{
    int userId;
    using (var scope = _factory.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
        
        var newUser = new UserAccount
        {
            FirstName = "Test",
            LastName = "Organizer",
            UserName = "testorganizer",
            Email = "testorg@events.com",
            Password = "hashedpassword",
            PasswordSalt = "salt",
            PhoneNumber = "9876543210",
            DateOfBirth = new DateTime(1985, 5, 15),
            AccountCreationDate = DateTime.UtcNow,
            AccountType = Enums.AccountTypes.EventOrganizer,
            LastLogin = DateTime.UtcNow,
            IsActive = true
        };
        
        db.UserAccounts.Add(newUser);
        await db.SaveChangesAsync();
        userId = newUser.Id;
    }

    var newOrganizer = new EventOrganizer
    {
        UserAccountId = userId,
        CreatedAt = DateTime.UtcNow,
        IsActive = true
    };

    var response = await _client.PostAsJsonAsync("/api/event_organizer/create", newOrganizer);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    using (var scope = _factory.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
        var createdOrganizer = await db.EventOrganizers
            .FirstOrDefaultAsync(o => o.UserAccountId == userId);
        
        Assert.NotNull(createdOrganizer);
        Assert.Equal(userId, createdOrganizer.UserAccountId);
        Assert.True(createdOrganizer.IsActive);
    }
}
        
        //delete

        //update
    }
}