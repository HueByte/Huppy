using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NetherNet.Core.Models;
using System.Reflection;

namespace NetherNet.Infrastructure;

public class NetherContext : IdentityDbContext<User>
{
    public NetherContext() { }
    public NetherContext(DbContextOptions<NetherContext> options) : base(options) { }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(AppContext.BaseDirectory + "save.sqlite", 
            x => x.MigrationsAssembly(typeof(NetherContext).Assembly.GetName().Name));
    }

    public DbSet<DiscordAccount>? DiscordUsers { get; set; }
}
