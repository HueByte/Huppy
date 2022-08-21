using System.Net.Http.Json;
using System.Text;
using Discord;
using Discord.WebSocket;
using Huppy.Core.Common.Constants;
using Huppy.Core.Dto;
using Huppy.Core.IRepositories;
using Huppy.Core.Models;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.NewsService
{
    /// <summary>
    /// No Longer supported
    /// </summary>
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
            var fromTime = DateTime.UtcNow.AddMinutes(-180).ToString("o");
            var toTime = DateTime.UtcNow.ToString("o");

            var client = _clientFactory.CreateClient("News");
            var response = await client.GetAsync($"everything?q=\"war\"+\"US\"+\"Russia\"+\"ukraine\"&SortBy=popularity&from={fromTime}&to={toTime}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content!.ReadFromJsonAsync<NewsResponse>();
                return result!;
            }
            else
            {
                var failedResponse = await response.Content.ReadAsStringAsync();
                _logger.LogError(failedResponse);

                throw new Exception("News request wasn't successful");
            }
        }

        // TODO better handler for errors
        public async Task PostNews()
        {
            try
            {
                // var servers = (await _serverRepository.GetAll()).Where(en => en.AreNewsEnabled);
                List<Server> servers = new();
                if (servers.Any())
                {
                    var news = (await GetNews()).Articles!.Take(5);

                    StringBuilder sb = new();
                    int count = 1;
                    foreach (var article in news)
                    {
                        sb.AppendLine($"**{count}. {article.Title?.Replace("*", String.Empty)}**\n");
                        sb.AppendLine($"> {article.Description?.Replace("*", String.Empty)}\n");
                        sb.AppendLine($"*{article.Author?.Replace("*", String.Empty)} - {article.Source!.Name?.Replace("*", String.Empty)}*");
                        sb.AppendLine($"{article.Url}\n");
                        count++;
                    }

                    var embed = new EmbedBuilder().WithTitle("✨ Most recent news ✨")
                                      .WithColor(Color.Teal)
                                      .WithDescription(sb.ToString())
                                      .WithThumbnailUrl(Icons.Huppy1)
                                      .WithCurrentTimestamp()
                                      .Build();

                    foreach (var server in servers)
                    {
                        var guild = _client.GetGuild(server.Id);

                        if (guild is null)
                        {
                            _logger.LogWarning("Didn't find server with ID {ServerID}, no news sent", server.Id);

                            server.UseGreet = false;

                            // TODO: consider fire and forget
                            await _serverRepository.UpdateAsync(server);
                            await _serverRepository.SaveChangesAsync();
                            continue;
                        }

                        ISocketMessageChannel? channel = default;
                        if (server!.Rooms is not null && server!.Rooms.GreetingRoom > 0)
                            channel = guild.GetChannel(server.Rooms.GreetingRoom) as ISocketMessageChannel;

                        channel ??= guild.DefaultChannel;

                        await channel.SendMessageAsync(null, false, embed);
                    }
                }
                else
                {
                    _logger.LogWarning("No servers use News API");
                }
            }
            catch (Exception e)
            {
                _logger.LogError("News Error {message}\n{stack}", e.Message, e.StackTrace);
            }
        }
    }
}