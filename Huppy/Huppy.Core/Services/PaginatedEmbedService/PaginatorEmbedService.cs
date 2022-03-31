using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Huppy.Core.Common.Constants;
using Huppy.Core.Entities;
using Huppy.Core.Services.HuppyCacheService;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.PaginatedEmbedService
{
    public class PaginatorEmbedService : IPaginatorEmbedService
    {
        // TODO: remake it to dictionary with static entry name as key as it must be unique
        // TODO: implement dynamic cachable paginated entries
        private List<PaginatorEntry> _staticPaginatorEntries = new();
        private readonly ILogger<PaginatorEmbedService> _logger;
        private readonly InteractionService _interactionService;
        private readonly CacheService _cacheService;
        public PaginatorEmbedService(ILogger<PaginatorEmbedService> logger, InteractionService interactionService, CacheService cacheService)
        {
            _logger = logger;
            _interactionService = interactionService;
            _cacheService = cacheService;
        }

        public Task Initialize()
        {
            StaticPaginatedEntriesBuilder entriesBuilder = new(_interactionService);
            _staticPaginatorEntries = entriesBuilder.Build();

            return Task.CompletedTask;
        }

        public List<PaginatorEntry> GetStaticPaginatorEntries() => _staticPaginatorEntries;

        public Task AddStaticPaginatorEntry(PaginatorEntry entry)
        {
            if (_staticPaginatorEntries.Any(e => e.Name == entry.Name))
                throw new Exception("Static paginator entries must have unique name");

            _staticPaginatorEntries.Add(entry);
            return Task.CompletedTask;
        }

        public async Task SendStaticPaginatedMessage(SocketInteraction interaction, string paginatedMessageName, int page = 0)
        {
            // check if that static paginated entry exists
            var paginatedEntry = _staticPaginatorEntries.FirstOrDefault(en => en.Name == paginatedMessageName);
            if (paginatedEntry is null)
            {
                _logger.LogError("Couldn't find paginated message with {name} name", paginatedMessageName);
                return;
            }

            // execute paginated message and receive the message ID
            var result = await ExecutePaginatedMessage(interaction, paginatedEntry, page);

            // if message was executed successfully add it to cache
            if (result > 0)
                await _cacheService.AddPaginatedMessage(result, new PaginatedMessage(result, 0, paginatedEntry.Name));
        }

        public async Task SendStaticPaginatedMessage(SocketInteraction interaction, PaginatorEntry paginatedEntry, int page = 0)
        {
            // check if that static paginated entry exists
            if (!_staticPaginatorEntries.Any(e => e.Name == paginatedEntry.Name))
            {
                _logger.LogError("Couldn't find paginated message with {name} name", paginatedEntry.Name);
                return;
            }

            // execute paginated message and receive the message ID
            var result = await ExecutePaginatedMessage(interaction, paginatedEntry, page);

            // if message was executed successfully add it to cache
            if (result > 0)
                await _cacheService.AddPaginatedMessage(result, new PaginatedMessage(result, 0, paginatedEntry.Name));
        }

        public async Task UpdatePaginatedMessage(SocketInteraction interaction, string paginatedMessageName, int page = 0)
        {
            // check if that static paginated entry exists
            var paginatedEntry = _staticPaginatorEntries.FirstOrDefault(en => en.Name == paginatedMessageName);
            if (paginatedEntry is null)
            {
                _logger.LogError("Couldn't find paginated message with {name} name", paginatedMessageName);
                return;
            }

            var result = await ExecutePaginatedMessage(interaction, paginatedEntry, page);
            if (result > 0)
                await _cacheService.UpdatePaginatedMessage(result, new PaginatedMessage(result, (ushort)page, paginatedEntry.Name));
        }

        private static async Task<ulong> ExecutePaginatedMessage(SocketInteraction interaction, PaginatorEntry paginatedEntry, int page)
        {
            // check range
            if (page < 0 || page >= paginatedEntry.Pages.Count)
                return 0;

            var component = new ComponentBuilder().WithButton("◀", "page-left")
                                                  .WithButton("▶", "page-right");

            var result = await interaction.ModifyOriginalResponseAsync((msg) =>
            {
                msg.Embed = paginatedEntry.Pages[page].Embed;
                msg.Components = component.Build();
            });

            return result.Id;
        }
    }
}