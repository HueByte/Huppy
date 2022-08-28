using Discord.Interactions;
using Discord.WebSocket;
using Huppy.Core.Interfaces.IServices;
using Huppy.Core.Services.HuppyCacheService;
using Microsoft.Extensions.Logging;

namespace Huppy.App.Commands.ComponentInteractions;

public class PaginatorInteractions : InteractionModuleBase<ExtendedShardedInteractionContext>
{
    private readonly ILogger _logger;
    private readonly CacheService _cacheService;
    private readonly IPaginatorService _paginatorService;
    public PaginatorInteractions(ILogger<PaginatorInteractions> logger, CacheService cacheService, IPaginatorService paginatorService)
    {
        _logger = logger;
        _cacheService = cacheService;
        _paginatorService = paginatorService;

    }

    [ComponentInteraction("page-left")]
    public async Task PageLeft()
    {
        await MovePage(-1);
    }

    [ComponentInteraction("page-right")]
    public async Task PageRight()
    {
        await MovePage(1);
    }

    private async Task MovePage(short value)
    {
        var msg = Context.Interaction as SocketMessageComponent;

        var cachedPaginatedEntry = await _cacheService.GetPaginatorEntry(msg!.Message.Id);

        if (cachedPaginatedEntry is null)
            return;

        await _paginatorService.UpdatePaginatedMessage(Context.Interaction, cachedPaginatedEntry, cachedPaginatedEntry.CurrentPage + value);
    }
}
