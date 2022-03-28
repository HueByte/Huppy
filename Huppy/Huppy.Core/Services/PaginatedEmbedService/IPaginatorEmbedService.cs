using Discord.WebSocket;
using Huppy.Core.Entities;

namespace Huppy.Core.Services.PaginatedEmbedService
{
    public interface IPaginatorEmbedService
    {
        Task Initialize();
        List<PaginatorEntry> GetStaticPaginatorEntries();
        Task SendPaginatedMessage(SocketInteraction interaction, PaginatorEntry paginatedEntry, int page = 0);
        Task SendPaginatedMessage(SocketInteraction interaction, string paginatedMessageName, int page = 0);
        Task UpdatePaginatedMessage(SocketInteraction interaction, PaginatorEntry paginatedEntry, int page = 0);
        Task AddStaticPaginatorEntry(PaginatorEntry entry);
    }
}