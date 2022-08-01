using Discord;
using Discord.Interactions;
using Huppy.Core.Common.Constants;
using Huppy.Core.Services.PaginatorService.Entities;

namespace Huppy.Core.Services.PaginatorService
{
    public enum StaticEmbeds
    {
        Help
    }

    public class BuildStaticEmbeds
    {
        private readonly InteractionService _interactionService;
        private readonly Dictionary<string, List<PaginatorPage>> pages = new();
        public BuildStaticEmbeds(InteractionService interactionService)
        {
            _interactionService = interactionService;
        }

        public Dictionary<string, List<PaginatorPage>> GetStaticEmbeds()
        {
            // add static embeds builders here
            pages.TryAdd(StaticEmbeds.Help.ToString(), BuildHelp());

            return pages;
        }

        private List<PaginatorPage> BuildHelp()
        {
            List<PaginatorPage> helpEmbeds = new();

            var commandGroups = _interactionService.Modules.OrderBy(e => e.SlashCommands.Count)
                                                           .ToList();

            int pageNumber = 0;
            foreach (var group in commandGroups)
            {
                if (group.SlashCommands.Count == 0)
                    continue;

                var embed = new EmbedBuilder().WithTitle(group.SlashGroupName)
                                              .WithColor(Color.Teal)
                                              .WithThumbnailUrl(Icons.Huppy1);

                foreach (var command in group.SlashCommands)
                {
                    embed.AddField($"🔰 {command.Name}", command.Description);
                }

                PaginatorPage page = new()
                {
                    Name = $"Help{pageNumber}",
                    Page = (byte)pageNumber,
                    Embed = embed
                };

                helpEmbeds.Add(page);
                pageNumber++;
            }

            return helpEmbeds;
        }
    }
}