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
        private readonly List<PaginatorEntry> _staticPaginatorEntries = new();
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
            _staticPaginatorEntries.Add(BuildHelp());
            return Task.CompletedTask;
        }

        public List<PaginatorEntry> GetStaticPaginatorEntries() => _staticPaginatorEntries;

        public async Task AddStaticPaginatorEntry(PaginatorEntry entry)
        {
            if (_staticPaginatorEntries.Any(en => en.Name == entry.Name))
            {
                _logger.LogError("Tried to add paginator entry but entry with that name already existed");
                return;
            }

            await AddStaticEntry(entry);
        }

        private Task AddStaticEntry(PaginatorEntry entry)
        {
            _staticPaginatorEntries.Add(entry);

            return Task.CompletedTask;
        }

        public async Task SendPaginatedMessage(SocketInteraction interaction, string paginatedMessageName, int page = 0)
        {
            // check if PaginatedEntry exists with this name
            var paginatedEntry = _staticPaginatorEntries.FirstOrDefault(en => en.Name == paginatedMessageName);
            if (paginatedEntry is null)
            {
                _logger.LogError("Couldn't find paginated message with {name} name", paginatedMessageName);
                return;
            }

            var result = await ExecutePaginatedMessage(interaction, paginatedEntry, page);
            if (result > 0)
                await _cacheService.AddPaginatedMessage(result, new PaginatedMessage(result, 0, paginatedEntry.Name));
        }

        public async Task SendPaginatedMessage(SocketInteraction interaction, PaginatorEntry paginatedEntry, int page = 0)
        {
            // if PaginatedEntry is provided add it to collection
            if (!_staticPaginatorEntries.Any(e => e.Name == paginatedEntry.Name))
                await AddStaticEntry(paginatedEntry);

            var result = await ExecutePaginatedMessage(interaction, paginatedEntry, page);
            if (result > 0)
                await _cacheService.AddPaginatedMessage(result, new PaginatedMessage(result, 0, paginatedEntry.Name));
        }

        public async Task UpdatePaginatedMessage(SocketInteraction interaction, PaginatorEntry paginatedEntry, int page = 0)
        {
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

        private PaginatorEntry BuildHelp()
        {
            PaginatorEntry entry = new()
            {
                Name = PaginatorEntriesNames.Help,
                Pages = new()
            };

            var commandGroups = _interactionService.Modules.OrderBy(e => e.SlashCommands.Count)
                                                           .ToList();

            int pageNumber = 0;
            foreach (var group in commandGroups)
            {
                if (group.SlashCommands.Count == 0)
                    continue;

                var embed = new EmbedBuilder().WithTitle(group.SlashGroupName)
                                              .WithColor(Color.Teal)
                                              .WithThumbnailUrl(Icons.Huppy1)
                                              .WithFooter($"Page {pageNumber + 1}/{commandGroups.Where(e => e.SlashCommands.Count > 0).Count()}");

                foreach (var command in group.SlashCommands)
                {
                    embed.AddField($"🔰 {command.Name}", command.Description);
                }

                PaginatorPage page = new()
                {
                    Name = group.Name,
                    PageNumber = (byte)pageNumber,
                    Embed = embed.Build()
                };

                entry.Pages.Add(page);
                pageNumber++;
            }

            return entry;
        }
    }
}