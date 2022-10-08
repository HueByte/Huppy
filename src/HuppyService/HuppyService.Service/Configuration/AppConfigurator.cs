using HuppyService.Infrastructure;
using HuppyService.Service.Services;
using Microsoft.EntityFrameworkCore;

namespace HuppyService.Service.Configuration
{
    public class AppConfigurator
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger _logger;
        private WebApplication? _app;

        public AppConfigurator(IServiceScopeFactory scopeFactory, ILogger<AppConfigurator> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public void SetApp(WebApplication app) => _app = app;

        public async Task UseDatabaseMigrator()
        {
            using var scope = _scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<HuppyDbContext>();

            _logger.LogInformation("Creating database");

            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "save")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "save"));

            await dbContext.Database.MigrateAsync();
        }

        public void MapGrpcServices()
        {
            if (_app is not null)
            {
                _app.MapGrpcService<GreeterService>();
                _app.MapGrpcService<GPTService>();
                _app.MapGrpcService<CommandLogService>();
                _app.MapGrpcService<ServerService>();
                _app.MapGrpcService<ReminderService>();
            }
        }
    }
}
