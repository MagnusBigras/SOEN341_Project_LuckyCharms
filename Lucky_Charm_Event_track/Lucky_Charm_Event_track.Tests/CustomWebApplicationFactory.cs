using Lucky_Charm_Event_track;
using Lucky_Charm_Event_track.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;

namespace Lucky_Charm_Event_track.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Startup>
    {
        private readonly string _dbPath;

        public CustomWebApplicationFactory()
        {
            // Define the test DB path
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            _dbPath = Path.Combine(path, "eventtracker_test.db");

            // Ensure the file exists (EF Core will create it on migration if needed)
            if (!File.Exists(_dbPath))
            {
                using var fs = File.Create(_dbPath);
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

                // Add DbContext pointing to test DB
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

                    // Apply migrations (creates tables if they don't exist)
                    db.Database.Migrate();

                    // Seed default data using your Startup logic
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
