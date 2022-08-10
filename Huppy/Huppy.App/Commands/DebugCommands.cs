using System.Diagnostics;
using System.Text;
using Discord;
using Discord.Interactions;
using Huppy.Core.Services.HuppyCacheService;
using Microsoft.Extensions.Logging;

namespace Huppy.App.Commands
{
    [Group("debug", "debug commands")]
    public class DebugCommands : InteractionModuleBase<ShardedInteractionContext>
    {
        private readonly ILogger<DebugCommands> _logger;
        private readonly CacheService _cacheService;
        public DebugCommands(ILogger<DebugCommands> logger, CacheService cacheService)
        {
            _logger = logger;
            _cacheService = cacheService;
        }

        [SlashCommand("getmemory", "Gets memory usage of Huppy")]
        [RequireOwner]
        public async Task GetMemoryUsage()
        {
            StringBuilder description = new();
            Process currentProcess = Process.GetCurrentProcess();
            long totalBytesOfMemoryUsed = currentProcess.WorkingSet64;
            // var objects = GetObjects();
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
            objects.Add("Cache service user basic data size", _cacheService.UserBasicData);

            return objects;
        }

        private static string ToEmbedNumber(double number)
        {
            var result = Math.Truncate(number) / 1000;
            return string.Format("~`{0:N2} KB`", result);
        }
    }
}