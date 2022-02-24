using System.Net.Http.Json;
using System.Text;
using System.Threading.Channels;
using Discord;
using Discord.WebSocket;
using Huppy.Core.Common.Constants;
using Huppy.Core.Dto;
using Huppy.Core.IRepositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.NewsService
{
    public class NewsApiService : INewsApiService
    {
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IServerRepository _serverRepository;
        private readonly DiscordShardedClient _client;
        public NewsApiService(ILogger<NewsApiService> logger, IHttpClientFactory clientFactory, IServerRepository serverRepository, DiscordShardedClient client)
        {
            _logger = logger;
            _clientFactory = clientFactory;
            _serverRepository = serverRepository;
            _client = client;
        }

        public async Task<NewsResponse> GetNews()
        {
            var fromTime = DateTime.UtcNow.AddMinutes(-30).ToString("o");
            var toTime = DateTime.UtcNow.ToString("o");

            var client = _clientFactory.CreateClient("News");
            var response = await client.GetAsync($"everything?q=\"war\"+\"US\"+\"Russia\"+\"ukraine\"&SortBy=relevancy&from={fromTime}&to={toTime}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content!.ReadFromJsonAsync<NewsResponse>();
                return result!;
            }
            else
            {
                var failedResponse = await response.Content.ReadAsStringAsync();
                _logger.LogError(failedResponse);

                throw new Exception("Urban request wasn't successful");
            }
        }

        public async Task PostNews()
        {
            var servers = (await _serverRepository.GetAll()).Where(en => en.AreNewsEnabled);
            if (servers.Any())
            {
                var news = (await GetNews()).Articles!.Take(5);

                StringBuilder sb = new();
                int count = 1;
                foreach (var article in news)
                {
                    sb.AppendLine($"**{count}. {article.Title}**\n");
                    sb.AppendLine($"> {article.Description}\n");
                    sb.AppendLine($"*{article.Author} - {article.Source!.Name}*\n");
                    count++;
                }

                var embed = new EmbedBuilder().WithTitle("✨ Hello I'm Huppy! ✨")
                                  .WithColor(Color.Teal)
                                  .WithDescription(sb.ToString())
                                  .WithThumbnailUrl(Icons.Huppy1)
                                  .WithCurrentTimestamp()
                                  .Build();

                foreach (var server in servers)
                {
                    var guild = _client.GetGuild(server.ID);

                    if (guild is null)
                        throw new Exception($"Didn't find server with ID {server.ID}");

                    var room = server.NewsOutputRoom > 0
                        ? guild.GetTextChannel(server.NewsOutputRoom)
                        : guild.DefaultChannel;

                    if (room is null)
                    {
                        throw new Exception($"Could find a room with [{server.NewsOutputRoom}] ID");
                    }

                    _logger.LogInformation("Sending news to [{servername}] to room [{room}]", guild.Name, room.Name);

                    await room.SendMessageAsync(null, false, embed);
                }
            }
            else
            {
                _logger.LogWarning("No servers use News API");
            }
        }
    }
}