using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Huppy.Core.Interfaces.IServices;
using Huppy.Core.Services.HuppyCacheStorage;
using Huppy.Kernel;
using Huppy.Kernel.Constants;

namespace Huppy.App.Commands.SlashCommands;

[Group("ai", "Enjoy AI commands!")]
public class AiCommands : InteractionModuleBase<ExtendedShardedInteractionContext>
{
    private readonly IGPTService _aiService;
    private readonly CacheStorageService _cacheService;
    private readonly DiscordShardedClient _client;
    public AiCommands(IGPTService aiService, CacheStorageService cacheService, DiscordShardedClient client)
    {
        _aiService = aiService;
        _cacheService = cacheService;
        _client = client;
    }

    [SlashCommand("chat", "☄ Talk with Huppy!")]
    public async Task GptCompletion(string message)
    {
        // var messageCount = await _stabilizerService.GetCurrentMessageCount();
        // if (messageCount > _appSettings.GPT!.FreeMessageQuota)
        // {
        //     await ModifyOriginalResponseAsync((msg) => msg.Content = "There's no free quota available now");
        //     return;
        // }


        var result = await _aiService.DavinciCompletion(message);

        var embed = new EmbedBuilder().WithCurrentTimestamp()
                                      .WithDescription(result)
                                      .WithTitle(message)
                                      .WithColor(Color.Teal)
                                      .WithAuthor(Context.User)
                                      .WithThumbnailUrl(Icons.Huppy1);

        await ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
        await _cacheService.LogUsageAsync(Context.User.Username, Context.User.Id);
    }

    [SlashCommand("explanation", "Explanation how Huppy works")]
    public async Task HowItWorks()
    {
        var embed = new EmbedBuilder().WithDescription(HuppyBasicMessages.HowAiWorks)
                                      .WithTitle("How does Huppy work?")
                                      .WithColor(Color.Teal)
                                      .WithAuthor(Context.User)
                                      .WithThumbnailUrl(Icons.Huppy1);

        await ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
    }

    [SlashCommand("stats", "Get chat statistics of Huppy")]
    public async Task GetAiStats()
    {
        var stats = await _cacheService.GetAiStatistics();
        var topStats = stats.OrderByDescending(x => x.Value).Take(5);

        StringBuilder sb = new();
        sb.AppendLine("✨ Top Huppy friends ✨\n");

        //TODO: user username!
        foreach (var item in topStats)
        {
            sb.AppendLine($"> **{_client.GetUser(item.Key).Username}** : `{item.Value}`\n");
        }

        var embed = new EmbedBuilder().WithCurrentTimestamp()
                                      .WithTitle("Statistics for AI service usage")
                                      .WithThumbnailUrl(Icons.Huppy1)
                                      .WithColor(Color.Magenta)
                                      .WithDescription(sb.ToString());

        embed.AddField("Conversations", stats.Sum(x => x.Value), true);
        embed.AddField("Huppy friend count", stats.Keys.Count, true);

        await ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
    }
}
