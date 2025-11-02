using Lucky_Charm_Event_track.Enums;
using Lucky_Charm_Event_track.Controllers;
using Lucky_Charm_Event_track.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.Cookies;
using System;
using System.Linq;
using Lucky_Charm_Event_track.Services;

namespace Lucky_Charm_Event_track
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<WebAppDBContext>(options => options.UseSqlite("Data Source=eventtracker.db"));
            services.AddRazorPages();
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
            });

            // Added cookie authentication
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(options =>
                    {
                        options.LoginPath = "/Login";
                        options.Cookie.Name = "LuckyCharmAuth";
                        options.ExpireTimeSpan = TimeSpan.FromHours(1);
                    });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, WebAppDBContext db)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            db.Database.Migrate();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });


            DatabaseSeeder.Seed(db);
            seedDefaultUser(db);   
        }

        public void seedDefaultUser(WebAppDBContext db)
        {
            if (db.UserAccounts.Any(u => u.UserName == "defaultuser"))
            {
                Console.WriteLine("[Startup] seedDefaultUser: defaultuser already exists - skipping.");
                return;
            }
            
            var user = new UserAccount
            {
                FirstName = "Default",
                LastName = "User",
                UserName = "defaultuser",
                Email = "default@events.com",
                Password = "hashedpassword",
                PasswordSalt = "salt",
                PhoneNumber = "1234567890",
                DateOfBirth = new DateTime(1990, 1, 1),
                AccountCreationDate = DateTime.UtcNow,
                AccountType = AccountTypes.EventOrganizer,
                LastLogin = DateTime.UtcNow,
                IsActive = true,
                SuspensionEndUtc = null,
                IsBanned = false
            };

            db.UserAccounts.Add(user);
            db.SaveChanges();

            var eventorg = new EventOrganizer
            {
                UserAccountId = user.Id,
                Account = user,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
            };
            db.EventOrganizers.Add(eventorg);
            db.SaveChanges();

            var testevent = new Event
            {
                EventName = "test",
                EventDescription = "test",
                StartTime = DateTime.UtcNow,
                Address = "test_address",
                City = "test",
                Region = "test",
                PostalCode = "test",
                Country = "test",
                Capacity = 15,
                EventOrganizerId = eventorg.Id,
                CreatedAt = DateTime.Now,
                isActive = true,
                UpdatedAt = DateTime.Now
            };

            db.Events.Add(testevent);
            db.SaveChanges();

            db.Tickets.Add(new Ticket
            {
                EventId = testevent.Id,
                Event = testevent,
                UserAccountId = user.Id,
                Account = user,
                TicketType = TicketTypes.Free,
                Price = 10,
                PurchaseDate = DateTime.UtcNow,
                QRCodeText = "test",
                CheckedIn = false
            });

            db.SaveChanges();
            db.Organizations.Add(new Organization
            {
                Name = "Default Organization",
                EventOrganizerId = eventorg.Id,
                Organizer = eventorg,
                CurrentUserCount = 10,
                IsActive = true
            });
            db.SaveChanges();            
        }
    }
}
