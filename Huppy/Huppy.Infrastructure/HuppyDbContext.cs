using Huppy.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Huppy.Infrastructure
{
    public class HuppyDbContext : DbContext
    {
        public HuppyDbContext() { }
        public HuppyDbContext(DbContextOptions<HuppyDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

        protected override void OnModelCreating(ModelBuilder builder) { }

        public DbSet<AiUsage>? AiUsages { get; set; }
    }
}