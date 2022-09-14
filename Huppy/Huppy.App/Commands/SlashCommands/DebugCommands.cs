using System.Diagnostics;
using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Huppy.Core.Attributes;
using Huppy.Core.Interfaces.IServices;
using Huppy.Core.Services.HuppyCache;
using Huppy.Kernel;
using Microsoft.Extensions.Logging;

namespace Huppy.App.Commands.SlashCommands;

[Group("debug", "debug commands")]
[DebugCommandGroup]
[DontAutoRegister]
public class DebugCommands : InteractionModuleBase<ExtendedShardedInteractionContext>
{
    private readonly ILogger<DebugCommands> _logger;
    private readonly CacheService _cacheService;
    private readonly IResourcesService _resourceService;
    private readonly DiscordShardedClient _client;
    public DebugCommands(ILogger<DebugCommands> logger, CacheService cacheService, IResourcesService resourceService, DiscordShardedClient client)
    {
        _logger = logger;
        _cacheService = cacheService;
        _resourceService = resourceService;
        _client = client;
    }

    [SlashCommand("info", "Gets current resources statistics of Huppy")]
    [RequireDev]
    public async Task GetStatus()
    {
        var embed = new EmbedBuilder()
            .WithColor(Color.Magenta)
            .WithThumbnailUrl(_client.CurrentUser.GetAvatarUrl())
            .WithDescription("Current state of resources of Huppy")
            .WithTitle("Resource Monitor");

        var cpuUsage = await _resourceService.GetCpuUsageAsync();
        var ramUsage = _resourceService.GetRamUsage();
        var shardCount = _resourceService.GetShardCount();
        var avgExecutionTime = await _resourceService.GetAverageExecutionTimeAsync();

        var upTime = _resourceService.GetUpTime();
        var upTimeFormatted = string.Format(
            @"{0}::{1}::{2}::{3}::{4}",
            upTime.Days,
            upTime.Hours,
            upTime.Minutes,
            upTime.Seconds,
            upTime.Milliseconds);

        embed.AddField("CPU", $"`{cpuUsage}`", true);
        embed.AddField("RAM", $"`{ramUsage}`", true);
        embed.AddField("Shard Count", $"`{shardCount}`", true);
        embed.AddField("Bot Uptime (DD:HH:MM:SS:MS)", $"`{upTimeFormatted}`", true);
        embed.AddField("Average command executon time", $"`{avgExecutionTime} ms`", true);
        embed.AddField("Bot Version", $"`v...`", true);

        await ModifyOriginalResponseAsync((msg) => msg.Embed = embed.Build());
    }

    [SlashCommand("getmemory", "Gets memory usage of Huppy")]
    [RequireDev]
    public async Task GetMemoryUsage()
    {
        StringBuilder description = new();
        Process currentProcess = Process.GetCurrentProcess();
        long totalBytesOfMemoryUsed = currentProcess.WorkingSet64;
        Dictionary<string, string> objectSizes = new();
        double cpuUsage = currentProcess.TotalProcessorTime.TotalMilliseconds / 1000;

        // foreach (var item in objects)
        // {
        //     var name = item.Key;
        //     var size = 0f;

        //     using Stream s = new MemoryStream();
        //     BinaryFormatter formatter = new();
        //     formatter.Serialize(s, item.Value);
        //     size = s.Length;

        //     objectSizes.Add(name, ToEmbedNumber(size));
        // }

        var embed = new EmbedBuilder()
            .WithColor(Color.LightOrange)
            .WithThumbnailUrl(Context.User.GetAvatarUrl())
            .WithDescription(description.ToString())
            .WithTitle("Memory state of Huppy");

        embed.AddField("RAM usage", $"{ToEmbedNumber(totalBytesOfMemoryUsed)}", true);
        embed.AddField("CPU usage", $"`{string.Format("{0:N2}%", cpuUsage)}`", true);

        // foreach (var item in objectSizes)
        // {
        //     embed.AddField($"{item.Key}", item.Value, true);
        // }

        await ModifyOriginalResponseAsync((msg) =>
        {
            msg.Embed = embed.Build();
        });
    }

    private Dictionary<string, object?> GetObjects()
    {
        Dictionary<string, object?> objects = new();

        // objects.Add("Cache service size", _cacheService);
        objects.Add("Cache service paginator size", _cacheService.PaginatorEntries);
        // objects.Add("Cache service user AI usage size", _cacheService.UserAiUsage);
        objects.Add("Cache service user basic data size", _cacheService.CacheUsers);

        return objects;
    }

    private static string ToEmbedNumber(double number)
    {
        var result = Math.Truncate(number) / 1000;
        return string.Format("~`{0:N2} KB`", result);
    }
}
