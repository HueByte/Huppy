using Discord;
using Discord.Interactions;
using Huppy.Core.Attributes;
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

            var commandGroups = _interactionService.Modules
                .OrderByDescending(e => e.SlashCommands.Count)
                .Where(e => !e.Attributes.Any(e => e is DebugGroupAttribute))
                .ToList();

            int pageNumber = 0;
            foreach (var group in commandGroups)
            {
                if (group.SlashCommands.Count == 0)
                    continue;

                string title = string.IsNullOrEmpty(group.SlashGroupName) ? "⚗ General Commands" : $"⚗ /{group.SlashGroupName}";
                string description = string.IsNullOrEmpty(group.Description) ? "" : $"*{group.Description}*";

                var embed = new EmbedBuilder()
                    .WithTitle(title)
                    .WithDescription(description)
                    .WithColor(Color.LightOrange)
                    .WithThumbnailUrl(Icons.Huppy1);

                foreach (var command in group.SlashCommands)
                {
                    embed.AddField($"`{command.Name}`", command.Description, false);
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