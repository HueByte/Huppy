using System.Runtime.InteropServices;
using Discord.Interactions;
using Huppy.Core.Common.SlashCommands;
using Huppy.Core.Services.CommandService;
using Serilog;

namespace Huppy.App.Commands
{
    public class TestCommands : InteractionModuleBase<ShardedInteractionContext>
    {
        public InteractionService _commands { get; set; }
        private readonly ICommandHandlerService _commandHandler;
        public TestCommands(ICommandHandlerService commandHandler)
        {
            _commandHandler = commandHandler;
            Log.Information("Test Commands module loaded");
        }

        [SlashCommand(Ping.Name, Ping.Description)]
        public async Task PingCommand()
        {
            // await ModifyOriginalResponseAsync(async (msg) => msg.Content = "Pong");
            await RespondAsync("pong");
            Log.Information("Worked");
        }
    }
}