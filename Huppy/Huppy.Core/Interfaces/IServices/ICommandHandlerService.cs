using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Huppy.Core.Interfaces.IServices
{
    public interface ICommandHandlerService
    {
        Task InitializeAsync();
        Task HandleCommandAsync(SocketInteraction command);
        Task SlashCommandExecuted(SlashCommandInfo arg1, IInteractionContext arg2, IResult arg3);
        Task ComponentExecuted(ComponentCommandInfo commandInfo, IInteractionContext context, IResult result);
    }
}