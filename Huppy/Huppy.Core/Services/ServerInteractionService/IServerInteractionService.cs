using Discord.WebSocket;

namespace Huppy.Core.Services.ServerInteractionService
{
    public interface IServerInteractionService
    {
        Task HuppyJoined(SocketGuild guild);
        Task WelcomeUser(SocketGuildUser user);
    }
}