using Discord.WebSocket;

namespace Huppy.Core.Interfaces.IServices
{
    public interface IServerInteractionService
    {
        Task HuppyJoined(SocketGuild guild);
        Task OnUserJoined(SocketGuildUser user);
    }
}