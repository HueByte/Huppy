using Huppy.App.Configuration;
using Huppy.Core.Services.Huppy;
using Huppy.Core.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

Logo.Print();

// Get config
var appSettings = AppSettings.IsCreated
    ? AppSettings.Load()
    : AppSettings.Create();

Log.Logger = SerilogConfigurator.ConfigureLogger(appSettings);

await Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(host =>
    {
        host.AddUserSecrets<Program>();
    })
    .ConfigureServices(services =>
    {
        _ = new ModuleConfigurator(services)
        .AddAppSettings(appSettings)
        .AddGRPCServices()
        .AddLogger(Log.Logger)
        .AddDiscord()
        .AddServices()
        .AddDatabase()
        .AddHttpClients()
        .AddMiddlewares();

        services.AddHostedService<HuppyHostedService>();
    })
    .ConfigureLogging(ctx => ctx.ClearProviders())
    .UseSerilog(Log.Logger)
    .Build()
    .RunAsync();