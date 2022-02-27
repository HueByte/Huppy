using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.LoggerService
{
    public class LoggingService
    {
        private readonly ILogger _logger;
        private readonly DiscordShardedClient _client;
        public LoggingService(ILogger<LoggingService> logger, DiscordShardedClient client)
        {
            _logger = logger;
            _client = client;
        }

        public Task OnReadyAsync(DiscordSocketClient socketClient)
        {
            _logger.LogInformation("Connected as [ {Username} ]", _client.CurrentUser.Username);
            _logger.LogInformation("Used by [ {Count} ] servers", _client.Guilds.Count);

            return Task.CompletedTask;
        }

        public Task OnLogAsync(LogMessage msg)
        {
            string logMessage = $"{msg.Exception?.ToString() ?? msg.Message}";

            switch (msg.Severity.ToString())
            {
                case "Critical":
                    _logger.LogCritical(logMessage);
                    break;

                case "Warning":
                    _logger.LogWarning(logMessage);
                    break;

                case "Info":
                    _logger.LogInformation(logMessage);
                    break;

                case "Verbose":
                    _logger.LogInformation(logMessage);
                    break;

                case "Debug":
                    _logger.LogDebug(logMessage);
                    break;

                case "Error":
                    _logger.LogError(logMessage);
                    break;
            }

            return Task.CompletedTask;
        }
    }
}