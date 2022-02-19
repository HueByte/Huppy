using System.Runtime.InteropServices;
using Discord;
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
    }
}