using System.Diagnostics;
using Discord.Interactions;
using Huppy.Core.Interfaces;
using Huppy.Core.Interfaces.IRepositories;
using Huppy.Core.Interfaces.IServices;
using Huppy.Core.Utilities;
using Huppy.Kernel;
using HuppyService.Service.Protos;
using Microsoft.Extensions.Logging;

namespace Huppy.App.Middlewares
{
    public class CommandLogMiddleware : IMiddleware
    {
        private readonly ICommandLogService _commandLogService;
        private readonly ILogger _logger;
        private readonly Stopwatch watch = new();
        public CommandLogMiddleware(ICommandLogService commandLogService, ILogger<CommandLogMiddleware> logger)
        {
            _commandLogService = commandLogService;
            _logger = logger;
        }

        public Task BeforeAsync(ExtendedShardedInteractionContext context)
        {
            watch.Start();
            return Task.CompletedTask;
        }

        public async Task AfterAsync(ExtendedShardedInteractionContext context, ICommandInfo commandInfo, IResult result)
        {
            watch.Stop();

            CommandLogModel log = new()
            {
                CommandName = commandInfo.ToString(),
                UnixTime = Miscellaneous.DateTimeToUnixTimestamp(DateTime.UtcNow),
                IsSuccess = result.IsSuccess,
                UserId = context.User.Id,
                ExecutionTimeMs = watch.ElapsedMilliseconds,
                ChannelId = context.Channel.Id,
                ErrorMessage = !string.IsNullOrEmpty(result.ErrorReason) ? result.ErrorReason : "",
                GuildId = context.Guild.Id
            };

            await _commandLogService.AddCommand(log);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Command [{CommandName}] executed for [{Username}] in [{GuildName}] [{time} ms]", commandInfo.ToString(), context.User.Username, context.Guild.Name, string.Format("{0:n0}", watch.ElapsedMilliseconds));
            }
            else
            {
                _logger.LogError("Command [{CommandName}] resulted in error: [{Error}]", commandInfo.Name, result.ErrorReason);
            }
        }
    }
}