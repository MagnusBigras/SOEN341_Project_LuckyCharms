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
    public class UserAccountControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public UserAccountControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task GetAllUserAccounts_ReturnsAllAccounts()
        {
            var response = await _client.GetAsync("/api/accounts/all");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var accounts = await response.Content.ReadFromJsonAsync<List<UserAccount>>();
            Assert.NotNull(accounts);
            Assert.True(accounts.Count > 0);
        }
        
        [Fact]
        public async Task GetUserAccountById_ReturnsOk_WhenIdExists()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                var existingUserAccount = await db.UserAccounts.FirstOrDefaultAsync();
                Assert.NotNull(existingUserAccount); // Ensures we have data

                var response = await _client.GetAsync($"/api/accounts/{existingUserAccount.Id}");
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var userAccountData = await response.Content.ReadFromJsonAsync<UserAccount>();
                Assert.NotNull(userAccountData);
                Assert.Equal(existingUserAccount.Id, userAccountData.Id);
            }
        }

        [Fact]
        public async Task GetActiveUserAccounts_ReturnsActiveAccounts()
        {
            var response = await _client.GetAsync("/api/accounts/active");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var accounts = await response.Content.ReadFromJsonAsync<List<UserAccount>>();
            Assert.NotNull(accounts);
            Assert.True(accounts.Count > 0);
        }

        [Fact]
        public async Task CreateUserAccount_ReturnsSuccess_WhenValid()
        {
            var newAccount = new UserAccount
            {
                FirstName = "Test",
                LastName = "User",
                UserName = "testuser" + Guid.NewGuid().ToString().Substring(0, 8),
                Email = "testuser@example.com",
                Password = "hashedpassword123",
                PasswordSalt = "salt123",
                PhoneNumber = "1234567890",
                DateOfBirth = new DateTime(1995, 5, 15),
                AccountCreationDate = DateTime.UtcNow,
                AccountType = Enums.AccountTypes.GeneralUser
            };

            var response = await _client.PostAsJsonAsync("/api/accounts/create", newAccount);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Verify the account was created
            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            Assert.NotNull(result);
        }

        [Fact]
        public async Task DeleteUserAccount_ReturnsSuccess_WhenValid()
        {
            int userIdToDelete;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();

                var testUser = new UserAccount
                {
                    FirstName = "Delete",
                    LastName = "Test",
                    UserName = "deleteuser" + Guid.NewGuid().ToString().Substring(0, 8),
                    Email = "deleteuser@example.com",
                    Password = "hashedpassword",
                    PasswordSalt = "salt",
                    PhoneNumber = "9999999999",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    AccountCreationDate = DateTime.UtcNow,
                    AccountType = Enums.AccountTypes.GeneralUser,
                    LastLogin = DateTime.UtcNow,
                    IsActive = true
                };

                db.UserAccounts.Add(testUser);
                await db.SaveChangesAsync();
                userIdToDelete = testUser.Id;
            }

            var response = await _client.PostAsync($"/api/accounts/delete?id={userIdToDelete}", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Verify deletion
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                var deletedUser = await db.UserAccounts.FindAsync(userIdToDelete);
                Assert.Null(deletedUser);
            }
        }
  
        [Fact]
        public async Task Login_ReturnsSuccess_WhenCredentialsValid()
        {
            var loginCreds = new
            {
                Username = "defaultuser",
                Password = "hashedpassword",
                IsAdmin = false 
            };

            var response = await _client.PostAsJsonAsync("/api/accounts/login", loginCreds);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // The controller returns LoginResponse
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            Assert.NotNull(loginResponse);
            Assert.Equal("Login Successful", loginResponse.Message);
            Assert.Equal("/Events", loginResponse.RedirectUrl); 
        } 

        [Fact]
        public async Task UpgradeToOrganizer_ReturnsSuccess_WhenUserIsAdmin()
        {
            int userIdToUpgrade;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();

                var regularUser = new UserAccount
                {
                    FirstName = "test",
                    LastName = "User",
                    UserName = "regularuser" + Guid.NewGuid().ToString().Substring(0, 8),
                    Email = "regular@example.com",
                    Password = "hashedpassword",
                    PasswordSalt = "salt",
                    PhoneNumber = "6666666666",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    AccountCreationDate = DateTime.UtcNow,
                    AccountType = Enums.AccountTypes.GeneralUser,
                    LastLogin = DateTime.UtcNow,
                    IsActive = true
                };

                db.UserAccounts.Add(regularUser);
                await db.SaveChangesAsync();
                userIdToUpgrade = regularUser.Id;

                // Set the current logged in user as admin
                var adminUser = await db.UserAccounts.FirstOrDefaultAsync(u => u.UserName == "defaultuser");
                Assert.NotNull(adminUser);
                Globals.Globals.SessionManager.InitializeSession(adminUser, "login");
                Globals.Globals.SessionManager.IsAdmin = true;
            }

            var response = await _client.PostAsync($"/api/accounts/upgradetoOrganizer?id={userIdToUpgrade}", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();
                var organizer = await db.EventOrganizers
                    .FirstOrDefaultAsync(o => o.UserAccountId == userIdToUpgrade);
                
                Assert.NotNull(organizer);
                Assert.Equal(userIdToUpgrade, organizer.UserAccountId);
            }
        }
    }
}