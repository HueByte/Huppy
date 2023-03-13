using HuppyService.Service;
using HuppyService.Service.Configuration;
using HuppyService.Service.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureServices(services =>
{
    _ = new ModuleConfiguration(builder.Configuration, services)
        .AddAppConfigurator()
        .ConfigureMappings()
        .AddGrpc()
        .AddHttpClients()
        .AddDatabase()
        .AddRepositories();
});

var app = builder.Build();

await app.UseAppConfigurator();

app.UseHttpLogging();

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();
