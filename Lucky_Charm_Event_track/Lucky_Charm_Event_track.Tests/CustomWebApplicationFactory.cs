using Lucky_Charm_Event_track;
using Lucky_Charm_Event_track.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;

namespace Lucky_Charm_Event_track.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Startup>
    {
        private readonly string _dbPath;

        public CustomWebApplicationFactory()
        {
            // Create a unique test database file per test
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            //used uniqueID to avoid race conditions when running multiple tests in parallel
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            _dbPath = Path.Combine(path, $"eventtracker_test_{uniqueId}.db");

            // Delete this specific test DB only (not all test DBs)
            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
            }
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<WebAppDBContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                
                builder.UseEnvironment("Test");

                // Add DbContext pointing to this test's unique DB
                services.AddDbContext<WebAppDBContext>(options =>
                {
                    options.UseSqlite($"Data Source={_dbPath}");
                });

                // Configure controllers JSON to handle cycles
                services.AddControllers().AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });

                // Build the service provider
                var sp = services.BuildServiceProvider();

                using (var scope = sp.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<WebAppDBContext>();

                    // Apply migrations
                    db.Database.Migrate();

                    // Seed default data we already have in our startup.cs 
                    var startup = new Startup(new ConfigurationBuilder().Build());
                    startup.seedDefaultUser(db);
                    var seededUser = db.UserAccounts.FirstOrDefault(u => u.UserName == "defaultuser");
                    if (seededUser != null)
                    {
                        Globals.Globals.SessionManager.CurrentLoggedInUser = seededUser;
                    }
                }
            });

            return base.CreateHost(builder);
        }
    }
}