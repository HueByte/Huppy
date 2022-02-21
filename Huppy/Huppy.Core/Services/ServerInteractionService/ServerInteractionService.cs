using System.Text;
using Discord;
using Discord.WebSocket;
using Huppy.Core.Common.Constants;
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
            StringBuilder sb = new();

            sb.AppendLine("> I am a bot who is always looking to help others. I'm willing to lend a hand, and I try to be friendly and welcoming. I am looking to make new friends, and I love spending time with those that I am close to.");
            sb.AppendLine("> I use powerful AI engine to be myself and have a little conversation with you!\n");
            sb.AppendLine("> Also currently I'm mentally stuck at 2019");

            var embed = new EmbedBuilder().WithTitle("✨ Hello I'm Huppy! ✨")
                                          .WithColor(Color.Teal)
                                          .WithDescription(sb.ToString())
                                          .WithThumbnailUrl(Icons.Huppy1)
                                          .WithCurrentTimestamp();


            await guild.DefaultChannel.SendMessageAsync(null, false, embed.Build());
        }

        public async Task WelcomeUser(SocketGuildUser user)
        {
            var server = await _serverRepository.GetOneAsync(user.Guild.Id);
            if (server is not null && server.UseGreet)
            {
                var embed = new EmbedBuilder().WithColor(Color.Teal)
                                              .WithCurrentTimestamp()
                                              .WithDescription(server?.GreetMessage!.Replace("{username}", $"**{user.Username}**"))
                                              .WithTitle("Hello!")
                                              .WithThumbnailUrl(user.GetAvatarUrl());

                await user.SendMessageAsync(null, false, embed.Build());
            }
        }
    }
}