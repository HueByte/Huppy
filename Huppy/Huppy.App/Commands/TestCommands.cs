using System.Runtime.InteropServices;
using Discord.Interactions;
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
        }

        [SlashCommand("ping", "pings bot")]
        public async Task PingCommand()
        {
            await ModifyOriginalResponseAsync((msg) => msg.Content = "Pong");
        }

        [SlashCommand("say", "Says the input message")]
        public async Task SayCommand(string message)
        {
            await ModifyOriginalResponseAsync((msg) => msg.Content = message);
        }
    }
}