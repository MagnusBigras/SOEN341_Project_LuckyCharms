using Lucky_Charm_Event_track;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using Lucky_Charm_Event_track.Models;

namespace Lucky_Charm_Event_track.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            // Add a custom service configuration for testing
            builder.ConfigureServices(services =>
            {
                // Remove the app’s real DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<WebAppDBContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add a new in-memory database for testing
                services.AddDbContext<WebAppDBContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });

                // Build the service provider
                var sp = services.BuildServiceProvider();

                // Create a scope to get the DB context and seed data
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<WebAppDBContext>();

                    // Ensure the database is created
                    db.Database.EnsureCreated();

                    // Seed data for tests, still need to add the other fields
                    db.Events.Add(new Event { EventName = "Test Event", EventDescription = "Seeded event for testing",EventOrganizerId=12345});
                    db.SaveChanges();
                }
            });

            return base.CreateHost(builder);
        }
    }
}
