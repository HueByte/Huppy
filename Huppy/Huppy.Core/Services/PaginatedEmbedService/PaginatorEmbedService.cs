using System.Text;
using Discord;
using Discord.Interactions;
using Huppy.Core.Common.Constants;
using Huppy.Core.Entities;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.PaginatedEmbedService
{
    public class PaginatorEmbedService : IPaginatorEmbedService
    {
        private List<PaginatorEntry> PaginatorEntries;
        private readonly ILogger<PaginatorEmbedService> _logger;
        private readonly InteractionService _interactionService;
        public PaginatorEmbedService(ILogger<PaginatorEmbedService> logger, InteractionService interactionService)
        {
            PaginatorEntries = new();
            _logger = logger;
            _interactionService = interactionService;
        }

        // public async Task SendPaginatedMessage()
        // {

        // }

        public Task Initialize()
        {
            PaginatorEntries.Add(BuildHelp());
            return Task.CompletedTask;
        }
        public List<PaginatorEntry> GetPaginatorEntries() => PaginatorEntries;

        private PaginatorEntry BuildHelp()
        {
            PaginatorEntry entry = new()
            {
                Name = PaginatorEntriesNames.Help,
                Pages = new()
            };

            var commandGroups = _interactionService.Modules.OrderBy(e => e.SlashCommands.Count)
                                                           .ToList();

            int count = 0;
            foreach (var group in commandGroups)
            {
                var embed = new EmbedBuilder().WithTitle(group.SlashGroupName)
                                              .WithColor(Color.Teal)
                                              .WithThumbnailUrl(Icons.Huppy1);

                foreach (var command in group.SlashCommands)
                {
                    embed.AddField($"🔰 {command.Name}", command.Description);
                }

                PaginatorPage page = new()
                {
                    Name = group.Name,
                    PageNumber = (byte)count,
                    Embed = embed.Build()
                };

                entry.Pages.Add(page);
                count++;
            }

            return entry;
        }
    }
}