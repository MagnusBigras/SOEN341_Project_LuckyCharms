using Microsoft.EntityFrameworkCore;
using System;

namespace Lucky_Charm_Event_track.Models

{

    public class WebAppDBContext : DbContext
    {
        public DbSet<Event> Events { get; set; }
        public DbSet<EventOrganizer> EventOrganizers { get; set; }
        public DbSet<PriceTier> PriceTiers { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<UserAccount> UserAccounts { get; set; }
        public string DbPath { get; set; }

        public WebAppDBContext(DbContextOptions<WebAppDBContext> options): base(options)
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = System.IO.Path.Join(path, "eventtracker.db");
        }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                options.UseSqlite($"Data Source={DbPath}");
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
                // EventOrganizer → UserAccount (One-to-One, one-way)
                modelBuilder.Entity<EventOrganizer>()
                    .HasOne(eo => eo.Account)
                    .WithOne()
                    .HasForeignKey<EventOrganizer>(eo => eo.UserAccountId);

                // EventOrganizer → Events (One-to-Many)
                modelBuilder.Entity<EventOrganizer>()
                    .HasMany(eo => eo.Events)
                    .WithOne(e => e.Organizer)
                    .HasForeignKey(e => e.EventOrganizerId);

                // Event → Tickets (One-to-Many)
                modelBuilder.Entity<Event>()
                    .HasMany(e => e.Tickets)
                    .WithOne(t => t.Event)
                    .HasForeignKey(t => t.EventId);

                // Event → PriceTiers (One-to-Many)
                modelBuilder.Entity<Event>()
                    .HasMany(e => e.Prices)
                    .WithOne(pt => pt.Event)
                    .HasForeignKey(pt => pt.EventId);

                // UserAccount → Tickets (One-to-Many)
                modelBuilder.Entity<UserAccount>()
                    .HasMany(u => u.Tickets)
                    .WithOne(t => t.Account)
                    .HasForeignKey(t => t.UserAccountId);
            
        }
    }
}
