using Discord;
using Discord.Interactions;
using Huppy.Core.Attributes;
using Huppy.Core.Services.PaginatorService.Entities;
using Huppy.Kernel.Constants;

namespace Huppy.Kernel
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
                .Where(e => !e.Attributes.Any(e => e is DebugCommandGroupAttribute || e is BetaCommandGroupAttribute) && e.SlashCommands.Count > 0)
                .ToList();

            // first page summinary
            var summinary = new EmbedBuilder()
                .WithTitle("Huppy summinary")
                .WithDescription("Navigate to appropriate page to get more detailed information about the command module")
                .WithColor(Color.LightOrange)
                .WithThumbnailUrl(Icons.Huppy1);

            for (int i = 0; i < commandGroups.Count; i++)
            {
                string title = string.IsNullOrEmpty(commandGroups[i].SlashGroupName) ? $"⚗ `{i + 2}` `General Commands`" : $"⚗ `{i + 2}` `/{commandGroups[i].SlashGroupName}`";
                string description = string.IsNullOrEmpty(commandGroups[i].Description) ? "*Miscellaneous commands*" : $"*{commandGroups[i].Description}*";

                summinary.AddField(title, description, true);
            }

            PaginatorPage summinaryPage = new()
            {
                Name = "Help0",
                Page = (byte)0,
                Embed = summinary
            };

            helpEmbeds.Add(summinaryPage);

            int pageNumber = 1;
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
                    embed.AddField($"`{command.Name}`", $"> {command.Description}", false);
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