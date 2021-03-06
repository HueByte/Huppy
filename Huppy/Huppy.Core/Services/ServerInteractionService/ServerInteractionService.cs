using Discord;
using Discord.WebSocket;
using Huppy.Core.Common.Constants;
using Huppy.Core.Common.HuppyMessages;
using Huppy.Core.IRepositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.ServerInteractionService
{
    public class ServerInteractionService : IServerInteractionService
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _serviceFactory;
        public ServerInteractionService(ILogger<ServerInteractionService> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceFactory = serviceScopeFactory;
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
            using var scope = _serviceFactory.CreateAsyncScope();
            var _serverRepository = scope.ServiceProvider.GetRequiredService<IServerRepository>();

            _logger.LogInformation("New user joined [{Username}] at [{ServerName}]", user.Username, user.Guild.Name);

            var server = await _serverRepository.GetOneAsync(user.Guild.Id);
            if (server is not null)
            {
                if (server.UseGreet)
                {
                    var embed = new EmbedBuilder().WithColor(Color.Teal)
                                                  .WithCurrentTimestamp()
                                                  .WithDescription(server?.GreetMessage!.Replace("{username}", $"**{user.Username}**").Replace("\\n", "\n"))
                                                  .WithTitle("Hello!")
                                                  .WithThumbnailUrl(user.GetAvatarUrl());

                    ISocketMessageChannel? channel = default;
                    if (server!.Rooms is not null && server!.Rooms.GreetingRoom > 0)
                        channel = user.Guild.GetChannel(server.Rooms.GreetingRoom) as ISocketMessageChannel;
                    
                    channel ??= user.Guild.DefaultChannel;

                    await channel.SendMessageAsync(null, false, embed.Build());
                }

                if (server.RoleID > 0)
                {
                    var role = user.Guild.GetRole(server.RoleID);
                    if (role is null)
                    {
                        _logger.LogWarning("Role with [{RoleID}] ID on [{ServerName}] is not found. Updating default role to none", server.RoleID, user.Guild.Name);
                        server.RoleID = default;
                        
                        await _serverRepository.UpdateOne(server);
                        return;
                    }

                    await (user as IGuildUser).AddRoleAsync(server.RoleID);
                }
            }
        }
    }
}