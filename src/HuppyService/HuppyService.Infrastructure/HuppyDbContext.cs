using HuppyService.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace HuppyService.Infrastructure
{
    public class HuppyDbContext : DbContext
    {
        public HuppyDbContext() { }
        public HuppyDbContext(DbContextOptions<HuppyDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Server>()
                   .HasOne(e => e.Rooms)
                   .WithOne(e => e.Server)
                   .HasForeignKey<ServerRooms>(k => k.Id)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Server>()
                .HasMany(e => e.CommangLogs)
                .WithOne(e => e.Guild)
                .OnDelete(DeleteBehavior.Cascade);
        }

        //public DbSet<User> Users { get; set; }
        public DbSet<CommandLog> CommandLogs { get; set; }
        public DbSet<Server> Servers { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
    }
}