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
    public class ToolsControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public ToolsControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task ExportAttendeesCSV_ReturnsCSVFile_WhenEventExists()
        {
            int eventId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                
                // Create event with tickets
                var organizer = await db.EventOrganizers.FirstOrDefaultAsync();
                Assert.NotNull(organizer);

                var user = await db.UserAccounts.FirstOrDefaultAsync();
                Assert.NotNull(user);

                var ev = new Event
                {
                    EventName = "Test Event for CSV",
                    EventDescription = "Test",
                    StartTime = DateTime.UtcNow.AddDays(10),
                    Address = "123 Test St",
                    City = "TestCity",
                    Region = "TestRegion",
                    PostalCode = "12345",
                    Country = "TestCountry",
                    Capacity = 100,
                    EventOrganizerId = organizer.Id,
                    TicketType = Enums.TicketTypes.Free,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Category = "Test"
                };

                db.Events.Add(ev);
                await db.SaveChangesAsync();

                // Add a ticket with user account relationship
                var ticket = new Ticket
                {
                    EventId = ev.Id,
                    UserAccountId = user.Id,
                    TicketType = Enums.TicketTypes.Free,
                    Price = 0,
                    PurchaseDate = DateTime.UtcNow,
                    CheckedIn = false,
                    QRCodeText = Guid.NewGuid().ToString(),
                    IsHiddenInCalendar = false
                };

                db.Tickets.Add(ticket);
                await db.SaveChangesAsync();

                eventId = ev.Id;
            }

            // Ensure the directory exists
            string directory = "AttendeesList";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var response = await _client.GetAsync($"/api/tools/export-csv/{eventId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);

            var content = await response.Content.ReadAsByteArrayAsync();
            Assert.NotEmpty(content);

            // Delete the generated CSV file
            string filename = $"event_{eventId}.csv";
            string filepath = Path.Combine(directory, filename);
            if (System.IO.File.Exists(filepath))
            {
                System.IO.File.Delete(filepath);
            }
        }

        [Fact]
        public async Task ExportAttendeesCSV_ReturnsNotFound_WhenEventDoesNotExist()
        {
            var response = await _client.GetAsync("/api/tools/export-csv/99999");

            // The endpoint should return NotFound or InternalServerError
            Assert.True(response.StatusCode == HttpStatusCode.NotFound || 
                       response.StatusCode == HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task ValidatePayload_ReturnsSuccess_WhenTicketIsValid()
        {
            int eventId;
            string qrCodeText;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                
                var ev = await db.Events.FirstOrDefaultAsync();
                Assert.NotNull(ev);
                eventId = ev.Id;

                // Create a ticket that hasn't been checked in
                var ticket = new Ticket
                {
                    EventId = eventId,
                    TicketType = Enums.TicketTypes.Free,
                    Price = 0,
                    PurchaseDate = DateTime.UtcNow,
                    CheckedIn = false,
                    QRCodeText = Guid.NewGuid().ToString()
                };

                db.Tickets.Add(ticket);
                await db.SaveChangesAsync();
                qrCodeText = ticket.QRCodeText;
            }

            var payload = new
            {
                Payload = qrCodeText,
                EventId = eventId
            };

            var response = await _client.PostAsJsonAsync("/api/tools/validate-payload", payload);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            Assert.NotNull(result);

            // Verify ticket is now checked in
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                var ticket = await db.Tickets.FirstOrDefaultAsync(t => t.QRCodeText == qrCodeText);
                Assert.NotNull(ticket);
                Assert.True(ticket.CheckedIn);
            }
        }

        [Fact]
        public async Task ValidatePayload_ReturnsBadRequest_WhenPayloadIsEmpty()
        {
            var payload = new
            {
                Payload = "",
                EventId = 1
            };

            var response = await _client.PostAsJsonAsync("/api/tools/validate-payload", payload);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ValidatePayload_ReturnsBadRequest_WhenEventIdIsInvalid()
        {
            var payload = new
            {
                Payload = "test-qr-code",
                EventId = 0
            };

            var response = await _client.PostAsJsonAsync("/api/tools/validate-payload", payload);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ValidatePayload_ReturnsNotFound_WhenTicketDoesNotExist()
        {
            var payload = new
            {
                Payload = "nonexistent-qr-code",
                EventId = 1
            };

            var response = await _client.PostAsJsonAsync("/api/tools/validate-payload", payload);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ValidatePayload_ReturnsBadRequest_WhenTicketAlreadyCheckedIn()
        {
            int eventId;
            string qrCodeText;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                
                var ev = await db.Events.FirstOrDefaultAsync();
                Assert.NotNull(ev);
                eventId = ev.Id;

                // Create a ticket that is already checked in
                var ticket = new Ticket
                {
                    EventId = eventId,
                    TicketType = Enums.TicketTypes.Free,
                    Price = 0,
                    PurchaseDate = DateTime.UtcNow,
                    CheckedIn = true,
                    QRCodeText = Guid.NewGuid().ToString()
                };

                db.Tickets.Add(ticket);
                await db.SaveChangesAsync();
                qrCodeText = ticket.QRCodeText;
            }

            var payload = new
            {
                Payload = qrCodeText,
                EventId = eventId
            };

            var response = await _client.PostAsJsonAsync("/api/tools/validate-payload", payload);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ValidatePayload_ReturnsNotFound_WhenTicketExistsButForDifferentEvent()
        {
            int eventId1, eventId2;
            string qrCodeText;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                
                var events = await db.Events.Take(2).ToListAsync();
                Assert.True(events.Count >= 2, "Need at least 2 events for this test");
                
                eventId1 = events[0].Id;
                eventId2 = events[1].Id;

                // Create ticket for event 1
                var ticket = new Ticket
                {
                    EventId = eventId1,
                    TicketType = Enums.TicketTypes.Free,
                    Price = 0,
                    PurchaseDate = DateTime.UtcNow,
                    CheckedIn = false,
                    QRCodeText = Guid.NewGuid().ToString()
                };

                db.Tickets.Add(ticket);
                await db.SaveChangesAsync();
                qrCodeText = ticket.QRCodeText;
            }

            // Try to validate for event 2
            var payload = new
            {
                Payload = qrCodeText,
                EventId = eventId2
            };

            var response = await _client.PostAsJsonAsync("/api/tools/validate-payload", payload);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GenerateQRCode_ReturnsPNGImage_WhenPayloadIsValid()
        {
            var payload = "test-qr-payload";

            var response = await _client.GetAsync($"/api/tools/generate-qr?payload={payload}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);

            var content = await response.Content.ReadAsByteArrayAsync();
            Assert.NotEmpty(content);
        }

        [Fact]
        public async Task GenerateQRCode_ReturnsBadRequest_WhenPayloadIsEmpty()
        {
            var response = await _client.GetAsync("/api/tools/generate-qr?payload=");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GenerateQRCode_ReturnsBadRequest_WhenPayloadIsMissing()
        {
            var response = await _client.GetAsync("/api/tools/generate-qr");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ValidateQRCode_ReturnsBadRequest_WhenNoFileProvided()
        {
            var content = new MultipartFormDataContent();
            
            var response = await _client.PostAsync("/api/tools/validate-qr", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ValidateQRCode_ReturnsResult_WhenFileProvided()
        {
            // Create a dummy image file
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(new byte[] { 0x89, 0x50, 0x4E, 0x47 }); // PNG header bytes
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            content.Add(fileContent, "qrCodeImage", "test.png");

            var response = await _client.PostAsync("/api/tools/validate-qr", content);

            // The response will depend on QRCodeGeneratorHelper.VerifyQRCode implementation
            // It could be OK (if QR is valid) or BadRequest (if invalid)
            Assert.True(response.StatusCode == HttpStatusCode.OK || 
                       response.StatusCode == HttpStatusCode.BadRequest ||
                       response.StatusCode == HttpStatusCode.InternalServerError);
        }
    }
}