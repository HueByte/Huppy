using Discord;
using Discord.WebSocket;
using Huppy.Core.Common.Constants;
using Huppy.Core.Common.HuppyMessages;
using Huppy.Core.IRepositories;
using Huppy.Core.Models;
using Huppy.Core.Services.HuppyCacheService;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.ServerInteractionService
{
    public class ServerInteractionService : IServerInteractionService
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _serviceFactory;
        private readonly CacheService _cacheService;
        public ServerInteractionService(ILogger<ServerInteractionService> logger, IServiceScopeFactory serviceScopeFactory, CacheService cacheService)
        {
            _logger = logger;
            _serviceFactory = serviceScopeFactory;
            _cacheService = cacheService;
        }

        public async Task HuppyJoined(SocketGuild guild)
        {
            var embed = new EmbedBuilder().WithTitle("✨ Hello I'm Huppy! ✨")
                                          .WithColor(Color.Teal)
                                          .WithDescription(HuppyBasicMessages.AboutMe)
                                          .WithThumbnailUrl(Icons.Huppy1)
                                          .WithCurrentTimestamp();

            if (!_cacheService.RegisteredGuildsIds.Contains(guild.Id))
            {
                using var scope = _serviceFactory.CreateAsyncScope();
                var serverRepository = scope.ServiceProvider.GetRequiredService<IServerRepository>();

                Server server = new()
                {
                    Id = guild.Id,
                    GreetMessage = "Welcome {username}!",
                    Rooms = new()
                    {
                        OutputRoom = guild.DefaultChannel.Id,
                        GreetingRoom = 0
                    },
                    ServerName = guild.Name,
                    RoleID = 0,
                    UseGreet = false,
                };

                await serverRepository.AddAsync(server);
                await serverRepository.SaveChangesAsync();
            }

            await guild.DefaultChannel.SendMessageAsync(null, false, embed.Build());
        }

        public async Task OnUserJoined(SocketGuildUser user)
        {
            using var scope = _serviceFactory.CreateAsyncScope();
            var serverRepository = scope.ServiceProvider.GetRequiredService<IServerRepository>();

            _logger.LogInformation("New user joined [{Username}] at [{ServerName}]", user.Username, user.Guild.Name);

            var server = await serverRepository.GetAsync(user.Guild.Id);
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

                        await serverRepository.UpdateAsync(server);
                        await serverRepository.SaveChangesAsync();
                        return;
                    }

                    await (user as IGuildUser).AddRoleAsync(server.RoleID);
                }
            }

            _logger.LogWarning("Didn't welcome user because server returned null");
        }
    }
}