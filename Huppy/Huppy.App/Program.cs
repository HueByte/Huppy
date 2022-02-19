using System.Text.Json;
using Huppy.App;
using Huppy.App.Configuration;
using Huppy.Core.Entities;
using Huppy.Core.Lib;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;

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
                                                            .AddHttpClient()
                                                            .Build();

// Start bot
var bot = new Creator(_serviceProvider);

await bot.CreateCommands();

await bot.CreateEvents();

await bot.CreateBot();

await Task.Delay(-1);