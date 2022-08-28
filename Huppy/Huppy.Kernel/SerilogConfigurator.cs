using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Huppy.Kernel
{
    public static class SerilogConfigurator
    {
        public static ILogger ConfigureLogger(AppSettings appSettings)
        {
            if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, @"logs")))
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, @"logs"));

            return new LoggerConfiguration().MinimumLevel.Is(GetLogEventLevel(appSettings.Logger!.LogLevel!))
                .Enrich.FromLogContext()
                .WriteTo.Async(e => e.Console(theme: AnsiConsoleTheme.Code, applyThemeToRedirectedOutput: true))
                .WriteTo.Async(e => e.File(Path.Combine(AppContext.BaseDirectory, "logs/log.txt"), rollingInterval: GetRollingInterval(appSettings.Logger!.TimeInterval!)))
                .CreateLogger();
        }

        public static LogEventLevel GetLogEventLevel(string loglevel)
        {
            return loglevel.ToLower() switch
            {
                SerilogConstants.LogLevels.Verbose => LogEventLevel.Verbose,
                SerilogConstants.LogLevels.Debug => LogEventLevel.Debug,
                SerilogConstants.LogLevels.Information => LogEventLevel.Information,
                SerilogConstants.LogLevels.Warning => LogEventLevel.Warning,
                SerilogConstants.LogLevels.Error => LogEventLevel.Error,
                SerilogConstants.LogLevels.Fatal => LogEventLevel.Fatal,
                _ => LogEventLevel.Warning
            };
        }

        public static RollingInterval GetRollingInterval(string logInterval)
        {
            return logInterval.ToLower() switch
            {
                SerilogConstants.TimeIntervals.Minute => RollingInterval.Minute,
                SerilogConstants.TimeIntervals.Hour => RollingInterval.Hour,
                SerilogConstants.TimeIntervals.Day => RollingInterval.Day,
                SerilogConstants.TimeIntervals.Month => RollingInterval.Month,
                SerilogConstants.TimeIntervals.Year => RollingInterval.Year,
                SerilogConstants.TimeIntervals.Infinite => RollingInterval.Infinite,
                _ => RollingInterval.Hour
            };
        }
    }
}