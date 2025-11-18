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
    public class OrganizationControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public OrganizationControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task GetAll_ReturnsAllOrganizations()
        {
            var response = await _client.GetAsync("/api/organization/all");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var organizations = await response.Content.ReadFromJsonAsync<List<Organization>>();
            Assert.NotNull(organizations);
        }

        [Fact]
        public async Task Create_ReturnsSuccess_WhenValidOrganization()
        {
            int organizerId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                var organizer = await db.EventOrganizers.FirstOrDefaultAsync();
                Assert.NotNull(organizer);
                organizerId = organizer.Id;
            }

            var newOrg = new Organization
            {
                Name = "Test Organization " + Guid.NewGuid().ToString().Substring(0, 8),
                Description = "Test description",
                EventOrganizerId = organizerId,
                IsActive = true
            };

            var response = await _client.PostAsJsonAsync("/api/organization/create", newOrg);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            Assert.NotNull(result);

            // Verify the organization was created
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                var created = await db.Organizations
                    .FirstOrDefaultAsync(o => o.Name == newOrg.Name);
                Assert.NotNull(created);
                Assert.Equal(newOrg.Description, created.Description);
            }
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenOrganizationIsNull()
        {
            var response = await _client.PostAsJsonAsync("/api/organization/create", (Organization)null!);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Update_ReturnsSuccess_WhenValidUpdate()
        {
            int orgId;
            string originalName;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                
                var organizer = await db.EventOrganizers.FirstOrDefaultAsync();
                Assert.NotNull(organizer);

                var testOrg = new Organization
                {
                    Name = "Original Org " + Guid.NewGuid().ToString().Substring(0, 8),
                    Description = "Original description",
                    EventOrganizerId = organizer.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                db.Organizations.Add(testOrg);
                await db.SaveChangesAsync();
                orgId = testOrg.Id;
                originalName = testOrg.Name;
            }

            var updateDto = new
            {
                Id = orgId,
                Name = "Updated Org " + Guid.NewGuid().ToString().Substring(0, 8),
                Description = "Updated description",
                IsActive = false
            };

            var response = await _client.PostAsJsonAsync("/api/organization/update", updateDto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<Organization>();
            Assert.NotNull(result);
            Assert.Equal(updateDto.Name, result.Name);
            Assert.Equal(updateDto.Description, result.Description);
            Assert.False(result.IsActive);

            // Verify the organization was updated in database
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                var updated = await db.Organizations.FindAsync(orgId);
                Assert.NotNull(updated);
                Assert.Equal(updateDto.Name, updated.Name);
                Assert.Equal(updateDto.Description, updated.Description);
                Assert.False(updated.IsActive);
            }
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenIdIsZero()
        {
            var updateDto = new
            {
                Id = 0,
                Name = "Test Org",
                Description = "Test description"
            };

            var response = await _client.PostAsJsonAsync("/api/organization/update", updateDto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenNameIsEmpty()
        {
            int orgId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                var org = await db.Organizations.FirstOrDefaultAsync();
                Assert.NotNull(org);
                orgId = org.Id;
            }

            var updateDto = new
            {
                Id = orgId,
                Name = "",
                Description = "Test description"
            };

            var response = await _client.PostAsJsonAsync("/api/organization/update", updateDto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenOrganizationDoesNotExist()
        {
            var updateDto = new
            {
                Id = 99999,
                Name = "Test Org",
                Description = "Test description"
            };

            var response = await _client.PostAsJsonAsync("/api/organization/update", updateDto);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Update_ReturnsConflict_WhenDuplicateNameExists()
        {
            int orgId1, orgId2;
            string existingName;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                
                var organizer = await db.EventOrganizers.FirstOrDefaultAsync();
                Assert.NotNull(organizer);

                // Create first organization
                var org1 = new Organization
                {
                    Name = "Existing Org " + Guid.NewGuid().ToString().Substring(0, 8),
                    Description = "First org",
                    EventOrganizerId = organizer.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                // Create second organization
                var org2 = new Organization
                {
                    Name = "Second Org " + Guid.NewGuid().ToString().Substring(0, 8),
                    Description = "Second org",
                    EventOrganizerId = organizer.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                db.Organizations.Add(org1);
                db.Organizations.Add(org2);
                await db.SaveChangesAsync();
                
                orgId1 = org1.Id;
                orgId2 = org2.Id;
                existingName = org1.Name;
            }

            // Try to update org2 with first organization's name
            var updateDto = new
            {
                Id = orgId2,
                Name = existingName,
                Description = "Updated description"
            };

            var response = await _client.PostAsJsonAsync("/api/organization/update", updateDto);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Delete_ReturnsSuccess_WhenOrganizationExists()
        {
            int orgId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                
                var organizer = await db.EventOrganizers.FirstOrDefaultAsync();
                Assert.NotNull(organizer);

                var testOrg = new Organization
                {
                    Name = "Delete Me " + Guid.NewGuid().ToString().Substring(0, 8),
                    Description = "To be deleted",
                    EventOrganizerId = organizer.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                db.Organizations.Add(testOrg);
                await db.SaveChangesAsync();
                orgId = testOrg.Id;
            }

            var response = await _client.PostAsJsonAsync("/api/organization/delete", orgId);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Verify the organization was deleted
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                var deleted = await db.Organizations.FindAsync(orgId);
                Assert.Null(deleted);
            }
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenOrganizationDoesNotExist()
        {
            var response = await _client.PostAsJsonAsync("/api/organization/delete", 99999);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}