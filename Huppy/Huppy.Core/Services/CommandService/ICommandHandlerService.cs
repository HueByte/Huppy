using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Huppy.Core.Services.CommandService
{
    public interface ICommandHandlerService
    {
        Task InitializeAsync();
        Task HandleCommandAsync(SocketInteraction command);
        Task SlashCommandExecuted(SlashCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3);
        List<string> GetUserNames();
    }
}