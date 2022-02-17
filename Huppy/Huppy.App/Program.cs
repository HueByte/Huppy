using Discord;
using Huppy.App;
using Huppy.App.Configuration;
using Huppy.Core.Entities;
using Huppy.Core.Lib;
using Serilog;
using Serilog.Events;

Logo.Print();

var appSettings = AppSettings.IsCreated
    ? AppSettings.Load()
    : AppSettings.Create();

LogEventLevel logLevel = SerilogConfigurator.GetLogEventLevel(appSettings);
RollingInterval logInterval = SerilogConfigurator.GetRollingInterval(appSettings);

if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, @"logs")))
    Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, @"logs"));

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", logLevel)
    .WriteTo.Async(e => e.Console())
    .WriteTo.Async(e => e.File(Path.Combine(AppContext.BaseDirectory, "logs/log.txt"), rollingInterval: logInterval))
    .CreateLogger();


IServiceProvider _serviceProvider = new ModuleConfigurator().AddAppSettings(appSettings)
                                                            .AddDiscord()
                                                            .Build();


var bot = new Creator(_serviceProvider);

await bot.ConfigureCommandsAsync();

await bot.CreateBot();

await Task.Delay(-1);