using Huppy.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Huppy.Infrastructure
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
                   .HasForeignKey<ServerRooms>(k => k.ServerRoomsID)
                   .OnDelete(DeleteBehavior.Cascade);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<CommandLog> CommandLogs { get; set; }
        public DbSet<Server> Servers { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
    }
}