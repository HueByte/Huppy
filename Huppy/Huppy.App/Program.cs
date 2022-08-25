using Huppy.App;
using Huppy.App.Configuration;
using Serilog;

Logo.Print();

// Get config
var appSettings = AppSettings.IsCreated
    ? AppSettings.Load()
    : AppSettings.Create();

// Configure Logger
Log.Logger = SerilogConfigurator.ConfigureLogger(appSettings);

// Configure Service provider
IServiceProvider _serviceProvider = new ModuleConfigurator().AddAppSettings(appSettings)
                                                            .AddLogger(Log.Logger)
                                                            .AddDiscord()
                                                            .AddServices()
                                                            .AddDatabase()
                                                            .AddHttpClients()
                                                            .AddMiddlewares()
                                                            .Build();

// Start bot
Creator bot = new(_serviceProvider);

await bot.CreateDatabase();

await bot.CreateCommands();

await bot.PopulateSingletons();

await bot.CreateEvents();

await bot.CreateBot();

await Task.Delay(-1);