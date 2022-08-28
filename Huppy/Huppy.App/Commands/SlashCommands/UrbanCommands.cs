using System.Text;
using Discord;
using Discord.Interactions;
using Huppy.Core.Interfaces.IServices;
using Huppy.Kernel;
using Huppy.Kernel.Constants;
using Microsoft.Extensions.Logging;

namespace Huppy.App.Commands.SlashCommands;

[Group("urban", "Urban dicionary commands")]
public class UrbanCommands : InteractionModuleBase<ExtendedShardedInteractionContext>
{
    ILogger<UrbanCommands> _logger;
    IUrbanService _urbanService;
    public UrbanCommands(ILogger<UrbanCommands> logger, IUrbanService urbanService)
    {
        _logger = logger;
        _urbanService = urbanService;
    }

    [SlashCommand("define", "Get a urban dictionary definition")]
    public async Task Define(string term)
    {
        var result = (await _urbanService.GetDefinition(term)).List!.OrderByDescending(e => e.ThumbsUp - e.ThumbsDown).FirstOrDefault();

        StringBuilder sb = new();

        if (result is null)
            throw new Exception($"Didn't find any definition for {term}");

        sb.AppendLine(result.Definition!.Replace('[', '*').Replace(']', '*'));

        sb.AppendLine($"\n>>> {result!.Example!.Replace('[', '*').Replace(']', '*')}");

        var embed = new EmbedBuilder().WithTitle(result.Word)
                                      .WithColor(Color.Teal)
                                      .WithDescription(sb.ToString())
                                      .WithThumbnailUrl(Icons.Huppy1)
                                      .WithCurrentTimestamp();

        embed.AddField("👍 Likes", $"`{result.ThumbsUp}`", true);
        embed.AddField("👎 Dislikes", $"`{result.ThumbsDown}`", true);
        embed.AddField("👾 Author", $"`{result.Author}`", true);

        await ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
    }
}
