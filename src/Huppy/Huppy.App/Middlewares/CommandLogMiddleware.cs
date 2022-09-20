using System.Diagnostics;
using Discord.Interactions;
using Huppy.Core.Interfaces;
using Huppy.Core.Interfaces.IRepositories;
using Huppy.Kernel;
using Microsoft.Extensions.Logging;

namespace Huppy.App.Middlewares
{
    public class CommandLogMiddleware : IMiddleware
    {
        private readonly ICommandLogRepository _commandRepository;
        private readonly ILogger _logger;
        private readonly Stopwatch watch = new();
        public CommandLogMiddleware(ICommandLogRepository commandLogRepository, ILogger<CommandLogMiddleware> logger)
        {
            _commandRepository = commandLogRepository;
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

            CommandLog log = new()
            {
                CommandName = commandInfo.ToString(),
                Date = DateTime.UtcNow,
                IsSuccess = result.IsSuccess,
                UserId = context.User.Id,
                ExecutionTimeMs = watch.ElapsedMilliseconds,
                ChannelId = context.Channel.Id,
                ErrorMessage = result.ErrorReason,
                GuildId = context.Guild.Id
            };

            await _commandRepository.AddAsync(log);
            await _commandRepository.SaveChangesAsync();

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