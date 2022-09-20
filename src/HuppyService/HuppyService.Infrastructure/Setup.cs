using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HuppyService.Infrastructure;

public static class Setup
{
    public static void AddDbContext(this IServiceCollection services, string ConnectionString)
    {
        services.AddDbContext<HuppyDbContext>(
            options => options.UseSqlite(ConnectionString, x => x.MigrationsAssembly("HuppyService.Infrastructure"))
        );
    }
}