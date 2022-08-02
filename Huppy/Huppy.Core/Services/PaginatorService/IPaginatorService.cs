using Discord;
using Discord.WebSocket;
using Huppy.Core.Services.PaginatorService.Entities;

namespace Huppy.Core.Services.PaginatorService
{
    public interface IPaginatorService
    {
        List<PaginatorPage>? GetStaticEmbeds(string name);
        List<string?>? GetStaticEmbedsNames(string name);
        Task RegisterStaticEmbeds(Dictionary<string, List<PaginatorPage>> embeds);
        Task SendPaginatedMessage(SocketInteraction interaction, IPaginatorEntry paginatorEntry);
        Task UpdatePaginatedMessage(SocketInteraction interaction, IPaginatorEntry paginatorEntry, int page);
    }
}