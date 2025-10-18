using Lucky_Charm_Event_track.Enums;
using Lucky_Charm_Event_track.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

namespace Lucky_Charm_Event_track
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
                services.AddDbContext<WebAppDBContext>(options => options.UseSqlite("Data Source=eventtracker.db"));
                services.AddRazorPages();
                services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, WebAppDBContext db)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            db.Database.Migrate();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers(); 
            });
            seedDefaultUser(db);
        }
        public void seedDefaultUser(WebAppDBContext db) 
        {

            if (!db.UserAccounts.Any())
            {
                var user = new UserAccount
                {
                    FirstName = "Default",
                    LastName = "User",
                    UserName = "defaultuser",
                    Email = "default@events.com",
                    Password = "hashedpassword", // ideally hashed
                    PasswordSalt = "salt",
                    PhoneNumber = "1234567890",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    AccountCreationDate = DateTime.UtcNow,
                    AccountType = AccountTypes.EventOrganizer,
                    LastLogin = DateTime.UtcNow,
                    IsActive = true
                };

                db.UserAccounts.Add(user);
                db.SaveChanges();

                db.EventOrganizers.Add(new EventOrganizer
                {
                    UserAccountId = user.Id,
                    Account = user,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                });

                db.SaveChanges();
            }
        }
    }
}
