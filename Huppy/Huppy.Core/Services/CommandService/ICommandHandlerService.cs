using Discord.WebSocket;

namespace Huppy.Core.Services.CommandService
{
    public interface ICommandHandlerService
    {
        Task InitializeAsync();
        Task HandleCommandAsync(SocketSlashCommand command);
    }
}