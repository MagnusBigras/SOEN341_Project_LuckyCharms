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
    public class TicketControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public TicketControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task GetTickets_ReturnsAllTickets()
        {
            var response = await _client.GetAsync("/api/tickets/all");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var tickets = await response.Content.ReadFromJsonAsync<List<Ticket>>();
            Assert.NotNull(tickets);
        }

        [Fact]
        public async Task GetTicketById_ReturnsOk_WhenTicketExists()
        {
            int ticketId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                var ticket = await db.Tickets.FirstOrDefaultAsync();
                Assert.NotNull(ticket);
                ticketId = ticket.Id;
            }

            var response = await _client.GetAsync($"/api/tickets/{ticketId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var ticketData = await response.Content.ReadFromJsonAsync<Ticket>();
            Assert.NotNull(ticketData);
            Assert.Equal(ticketId, ticketData.Id);
        }

        [Fact]
        public async Task GetTicketById_ReturnsNotFound_WhenTicketDoesNotExist()
        {
            var response = await _client.GetAsync("/api/tickets/99999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateTicket_ReturnsSuccess_WhenValid()
        {
            int eventId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                var ev = await db.Events.FirstOrDefaultAsync();
                Assert.NotNull(ev);
                eventId = ev.Id;
            }

            var newTicket = new Ticket
            {
                EventId = eventId,
                TicketType = Enums.TicketTypes.Free,
                Price = 0,
                PurchaseDate = DateTime.UtcNow,
                CheckedIn = false,
                QRCodeText = Guid.NewGuid().ToString(),
                IsHiddenInCalendar = false
            };

            var response = await _client.PostAsJsonAsync("/api/tickets/create", newTicket);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Verify ticket was created
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                var created = await db.Tickets
                    .FirstOrDefaultAsync(t => t.QRCodeText == newTicket.QRCodeText);
                Assert.NotNull(created);
            }
        }

        [Fact]
        public async Task CreateTicket_ReturnsBadRequest_WhenTicketIsNull()
        {
            var response = await _client.PostAsJsonAsync("/api/tickets/create", (Ticket)null!);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        
        [Fact]
        public async Task DeleteTicket_ReturnsSuccess_WhenTicketExists()
        {
            int ticketId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                
                var ev = await db.Events.FirstOrDefaultAsync();
                Assert.NotNull(ev);

                var testTicket = new Ticket
                {
                    EventId = ev.Id,
                    TicketType = Enums.TicketTypes.Free,
                    Price = 0,
                    PurchaseDate = DateTime.UtcNow,
                    CheckedIn = false,
                    QRCodeText = Guid.NewGuid().ToString(),
                    IsHiddenInCalendar = false
                };

                db.Tickets.Add(testTicket);
                await db.SaveChangesAsync();
                ticketId = testTicket.Id;
            }

            // Send as query parameter, not JSON body
            var response = await _client.PostAsync($"/api/tickets/delete?id={ticketId}", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Verify ticket was deleted
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                var deleted = await db.Tickets.FindAsync(ticketId);
                Assert.Null(deleted);
            }
        }


        [Fact]
        public async Task DeleteTicket_ReturnsBadRequest_WhenTicketDoesNotExist()
        {
            var response = await _client.PostAsJsonAsync("/api/tickets/delete", 99999);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateTicket_ReturnsSuccess_WhenValid()
        {
            int ticketId, userId, eventId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                
                var ev = await db.Events.FirstOrDefaultAsync();
                Assert.NotNull(ev);
                eventId = ev.Id;

                var user = await db.UserAccounts.FirstOrDefaultAsync();
                Assert.NotNull(user);
                userId = user.Id;

                var testTicket = new Ticket
                {
                    EventId = eventId,
                    TicketType = Enums.TicketTypes.Free,
                    Price = 0,
                    PurchaseDate = DateTime.UtcNow,
                    CheckedIn = false,
                    QRCodeText = Guid.NewGuid().ToString(),
                    IsHiddenInCalendar = false
                };

                db.Tickets.Add(testTicket);
                await db.SaveChangesAsync();
                ticketId = testTicket.Id;
            }

            var updatedTicket = new Ticket
            {
                EventId = eventId,
                UserAccountId = userId,
                IsHiddenInCalendar = true
            };

            var response = await _client.PostAsJsonAsync($"/api/tickets/update?id={ticketId}", updatedTicket);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<Ticket>();
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserAccountId);
            Assert.True(result.IsHiddenInCalendar);

            // Verify ticket was updated
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                var updated = await db.Tickets.FindAsync(ticketId);
                Assert.NotNull(updated);
                Assert.Equal(userId, updated.UserAccountId);
                Assert.True(updated.IsHiddenInCalendar);
            }
        }

        [Fact]
        public async Task UpdateTicket_ReturnsBadRequest_WhenTicketDoesNotExist()
        {
            var updatedTicket = new Ticket
            {
                EventId = 1,
                UserAccountId = 1,
                IsHiddenInCalendar = true
            };

            var response = await _client.PostAsJsonAsync("/api/tickets/update?id=99999", updatedTicket);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task HideTicketEvent_ReturnsSuccess_WhenTicketExists()
        {
            int ticketId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                
                var ev = await db.Events.FirstOrDefaultAsync();
                Assert.NotNull(ev);

                var testTicket = new Ticket
                {
                    EventId = ev.Id,
                    TicketType = Enums.TicketTypes.Free,
                    Price = 0,
                    PurchaseDate = DateTime.UtcNow,
                    CheckedIn = false,
                    QRCodeText = Guid.NewGuid().ToString(),
                    IsHiddenInCalendar = false
                };

                db.Tickets.Add(testTicket);
                await db.SaveChangesAsync();
                ticketId = testTicket.Id;
            }

            var response = await _client.PostAsync($"/api/tickets/hide?ticketId={ticketId}", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            Assert.NotNull(result);

            // Verify ticket is hidden
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                var ticket = await db.Tickets.FindAsync(ticketId);
                Assert.NotNull(ticket);
                Assert.True(ticket.IsHiddenInCalendar);
            }
        }

        [Fact]
        public async Task ClaimTicket_ReturnsBadRequest_WhenEventDoesNotExist()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                var user = await db.UserAccounts.FirstOrDefaultAsync();
                Assert.NotNull(user);
                Globals.Globals.SessionManager.InitializeSession(user, "login");
            }

            var response = await _client.PostAsync("/api/tickets/claim?eventid=99999", null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UnhideEventTickets_ReturnsSuccess_WhenTicketsExist()
        {
            int eventId, userId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                
                var ev = await db.Events.FirstOrDefaultAsync();
                Assert.NotNull(ev);
                eventId = ev.Id;

                var user = await db.UserAccounts.FirstOrDefaultAsync();
                Assert.NotNull(user);
                userId = user.Id;

                // Create hidden tickets for this user and event
                var ticket1 = new Ticket
                {
                    EventId = eventId,
                    UserAccountId = userId,
                    TicketType = Enums.TicketTypes.Free,
                    Price = 0,
                    PurchaseDate = DateTime.UtcNow,
                    CheckedIn = false,
                    QRCodeText = Guid.NewGuid().ToString(),
                    IsHiddenInCalendar = true
                };

                var ticket2 = new Ticket
                {
                    EventId = eventId,
                    UserAccountId = userId,
                    TicketType = Enums.TicketTypes.Free,
                    Price = 0,
                    PurchaseDate = DateTime.UtcNow,
                    CheckedIn = false,
                    QRCodeText = Guid.NewGuid().ToString(),
                    IsHiddenInCalendar = true
                };

                db.Tickets.Add(ticket1);
                db.Tickets.Add(ticket2);
                await db.SaveChangesAsync();
            }

            var response = await _client.PostAsync($"/api/tickets/unhide-by-event?eventId={eventId}&userId={userId}", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            Assert.NotNull(result);

            // Verify tickets are unhidden
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                var tickets = await db.Tickets
                    .Where(t => t.EventId == eventId && t.UserAccountId == userId)
                    .ToListAsync();
                
                Assert.NotEmpty(tickets);
                Assert.All(tickets, t => Assert.False(t.IsHiddenInCalendar));
            }
        }
    }
}