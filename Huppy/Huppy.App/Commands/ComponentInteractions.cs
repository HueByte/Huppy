using System.Runtime.CompilerServices;
using Discord.Interactions;
using Discord.WebSocket;
using Huppy.Core.Services.HuppyCacheService;
using Huppy.Core.Services.PaginatedEmbedService;
using Microsoft.Extensions.Logging;

namespace Huppy.App.Commands
{
    public class ComponentInteractions : InteractionModuleBase<ShardedInteractionContext>
    {
        private readonly ILogger _logger;
        private readonly CacheService _cacheService;
        private readonly IPaginatorEmbedService _paginatorService;
        public ComponentInteractions(ILogger<ComponentInteractions> logger, CacheService cacheService, IPaginatorEmbedService paginatorService)
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

            var cacheMessage = await _cacheService.GetPaginatedMessage(msg!.Message.Id);

            if (cacheMessage is null)
                return;

            await _paginatorService.UpdatePaginatedMessage(Context.Interaction, cacheMessage, cacheMessage.CurrentPage + value);
        }
    }
}