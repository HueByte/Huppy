using HuppyService.Service.Configuration;

namespace HuppyService.Service
{
    public static class AppExtensions
    {
        public static async Task UseAppConfigurator(this WebApplication app)
        {
            var appConfigurator = app.Services.GetRequiredService<AppConfigurator>();

            appConfigurator.SetApp(app);
            appConfigurator.MapGrpcServices();

            await appConfigurator.UseDatabaseMigrator();            
        }
    }
}
