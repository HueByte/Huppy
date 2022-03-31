using Discord;
using Discord.Interactions;
using Huppy.Core.Common.Constants;
using Huppy.Core.Entities;

namespace Huppy.Core.Services.PaginatedEmbedService
{
    public class StaticPaginatedEntriesBuilder
    {
        private readonly InteractionService _interactionService;
        public StaticPaginatedEntriesBuilder(InteractionService interactionService)
        {
            _interactionService = interactionService;
        }

        public List<PaginatorEntry> Build()
        {
            List<PaginatorEntry> entries = new();
            entries.Add(BuildHelp());

            return entries;
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