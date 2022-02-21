using System.Text;
using Discord;
using Discord.WebSocket;
using Huppy.Core.Common.Constants;
using Huppy.Core.Common.HuppyMessages;
using Huppy.Core.IRepositories;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.ServerInteractionService
{
    public class ServerInteractionService : IServerInteractionService
    {
        private readonly ILogger _logger;
        private readonly IServerRepository _serverRepository;
        public ServerInteractionService(ILogger<ServerInteractionService> logger, IServerRepository serverRepository)
        {
            _logger = logger;
            _serverRepository = serverRepository;
        }

        public async Task HuppyJoined(SocketGuild guild)
        {
            var embed = new EmbedBuilder().WithTitle("✨ Hello I'm Huppy! ✨")
                                          .WithColor(Color.Teal)
                                          .WithDescription(HuppyBasicMessages.AboutMe)
                                          .WithThumbnailUrl(Icons.Huppy1)
                                          .WithCurrentTimestamp();


            await guild.DefaultChannel.SendMessageAsync(null, false, embed.Build());
        }

        public async Task OnUserJoined(SocketGuildUser user)
        {
            _logger.LogInformation("New user joined [{Username}] at [{ServerName}", user.Username, user.Guild.Name);

            var server = await _serverRepository.GetOneAsync(user.Guild.Id);
            if (server is not null && server.UseGreet)
            {
                var embed = new EmbedBuilder().WithColor(Color.Teal)
                                              .WithCurrentTimestamp()
                                              .WithDescription(server?.GreetMessage!.Replace("{username}", $"**{user.Username}**"))
                                              .WithTitle("Hello!")
                                              .WithThumbnailUrl(user.GetAvatarUrl());

                ISocketMessageChannel? channel = null;
                if (server!.OutputRoom != 0)
                    channel = user.Guild.GetChannel(server.OutputRoom) as ISocketMessageChannel;

                else
                    channel = user.Guild.DefaultChannel as ISocketMessageChannel;

                await channel!.SendMessageAsync(null, false, embed.Build());
                // await user.SendMessageAsync(null, false, embed.Build());
            }
        }
    }
}