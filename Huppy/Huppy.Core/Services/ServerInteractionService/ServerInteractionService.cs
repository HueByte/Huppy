using System.Text;
using Discord;
using Discord.WebSocket;
using Huppy.Core.Common.Constants;
using Huppy.Core.Common.HuppyMessages;
using Huppy.Core.IRepositories;
using Huppy.Core.Models;
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
                                              .WithDescription(server?.GreetMessage!.Replace("{username}", $"**{user.Username}**").Replace("\\n", "\n"))
                                              .WithTitle("Hello!")
                                              .WithThumbnailUrl(user.GetAvatarUrl());

                ISocketMessageChannel? channel;
                if (server!.OutputRoom > 0)
                    channel = user.Guild.GetChannel(server.OutputRoom) as ISocketMessageChannel;

                else
                    channel = user.Guild.DefaultChannel as ISocketMessageChannel;

                await channel!.SendMessageAsync(null, false, embed.Build());
            }
            if (server!.RoleID > 0)
            {
                try
                {
                    var role = user.Guild.GetRole(server.RoleID);
                    if (role is null)
                        throw new Exception("This role doesn't exists");

                    await (user as IGuildUser).AddRoleAsync(server.RoleID);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Role with [{RoleID}] ID on [{ServerName}] is not found. Updating default role to none", server.RoleID, user.Guild.Name);
                    server.RoleID = 0;
                    await _serverRepository.UpdateOne(server);
                }
            }
        }
    }
}