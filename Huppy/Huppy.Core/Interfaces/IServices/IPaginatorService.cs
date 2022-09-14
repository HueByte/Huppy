using Discord;
using Discord.WebSocket;
using Huppy.Core.Services.Paginator.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Huppy.Core.Interfaces.IServices
{
    public interface IPaginatorService
    {
        List<PaginatorPage>? GetStaticEmbeds(string name);
        List<string?>? GetStaticEmbedsNames(string name);
        Task RegisterStaticEmbeds(Dictionary<string, List<PaginatorPage>> embeds);
        Task SendPaginatedMessage(SocketInteraction interaction, IPaginatorEntry paginatorEntry);
        Task UpdatePaginatedMessage(SocketInteraction interaction, IPaginatorEntry paginatorEntry, int page);
        Task<DynamicPaginatorEntry> GeneratePaginatorEntry(IInteractionContext context, int elementsCount, int pageSize, Func<int, AsyncServiceScope, object?, Task<PaginatorPage?>> generatePageCallback);
    }
}