using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Huppy.Core.Services.HuppyCacheService;
using Huppy.Core.Services.PaginatorService.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.PaginatorService
{
    public class PaginatorService : IPaginatorService
    {
        private readonly CacheService _cacheService;
        private readonly ILogger<PaginatorService> _logger;
        private Dictionary<string, List<PaginatorPage>> _staticEmbeds;

        public PaginatorService(ILogger<PaginatorService> logger, CacheService cacheService, InteractionService interactionService)
        {
            _logger = logger;
            _cacheService = cacheService;
            _staticEmbeds = new();
        }

        public void AddStaticEmbed(string key, List<PaginatorPage> pages)
        {
            _staticEmbeds.TryAdd(key, pages);
        }

        public Task RegisterStaticEmbeds(Dictionary<string, List<PaginatorPage>> embeds)
        {
            _staticEmbeds = new(embeds);
            foreach ((var key, var pages) in _staticEmbeds)
            {
                _logger.LogInformation("Static Embed Registered: [{key}] - [{pages}]", key, string.Join(", ", pages.Select(e => e.Name)));
            }

            return Task.CompletedTask;
        }

        public List<PaginatorPage>? GetStaticEmbeds(string keyName)
        {
            _staticEmbeds.TryGetValue(keyName, out List<PaginatorPage>? result);
            return result;
        }

        public List<string?>? GetStaticEmbedsNames(string keyName)
        {
            _staticEmbeds.TryGetValue(keyName, out var result);
            return result?.Select(e => e.Name).ToList();
        }

        public async Task SendPaginatedMessage(SocketInteraction interaction, IPaginatorEntry paginatorEntry)
        {
            var messageId = await ExecutePaginator(interaction, paginatorEntry, 0);
            if (messageId == 0)
                return;

            paginatorEntry.MessageId = messageId;
            await _cacheService.AddPaginatedEntry(messageId, paginatorEntry);
        }

        public async Task UpdatePaginatedMessage(SocketInteraction interaction, IPaginatorEntry paginatorEntry, int page)
        {
            var messageId = await ExecutePaginator(interaction, paginatorEntry, page);
            if (messageId == 0)
                return;

            paginatorEntry.CurrentPage = (ushort)page;
            await _cacheService.UpdatePaginatorEntry(messageId, paginatorEntry);
        }

        private async Task<ulong> ExecutePaginator(SocketInteraction interaction, IPaginatorEntry entry, int page)
        {
            var embed = await entry?.GetPageContent(page)!;
            if (embed is null)
                return 0;

            var components = new ComponentBuilder()
                .WithButton("◀", "page-left")
                .WithButton("▶", "page-right");

            var result = await interaction.ModifyOriginalResponseAsync((msg) =>
            {
                msg.Embed = embed.WithFooter($"{page + 1}/{entry.GetPageCount()}").Build();
                msg.Components = components.Build();
            });

            return result.Id;
        }
    }
}