using Discord.WebSocket;
using Huppy.Core.Entities;

namespace Huppy.Core.Services.PaginatedEmbedService
{
    public interface IPaginatorEmbedService
    {
        Task Initialize();
        List<PaginatorEntry> GetStaticPaginatorEntries();
        Task SendStaticPaginatedMessage(SocketInteraction interaction, PaginatorEntry paginatedEntry, int page = 0);
        Task SendStaticPaginatedMessage(SocketInteraction interaction, string paginatedMessageName, int page = 0);
        Task UpdatePaginatedMessage(SocketInteraction interaction, PaginatorEntry paginatedEntry, int page = 0);
        Task AddStaticPaginatorEntry(PaginatorEntry entry);
    }
}