using Discord;
using Discord.WebSocket;
using Huppy.Core.Interfaces.IRepositories;
using Huppy.Core.Interfaces.IServices;
using Huppy.Core.Models;
using Huppy.Core.Services.HuppyCacheStorage;
using Huppy.Kernel.Constants;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.ServerInteraction;

public class ServerInteractionService : IServerInteractionService
{
    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _serviceFactory;
    private readonly CacheStorageService _cacheService;
    public ServerInteractionService(ILogger<ServerInteractionService> logger, IServiceScopeFactory serviceScopeFactory, CacheStorageService cacheService)
    {
        _logger = logger;
        _serviceFactory = serviceScopeFactory;
        _cacheService = cacheService;
    }

    public async Task HuppyJoined(SocketGuild guild)
    {
        //var embed = new EmbedBuilder().WithTitle("✨ Hello I'm Huppy! ✨")
        //                              .WithColor(Color.Teal)
        //                              .WithDescription(HuppyBasicMessages.AboutMe)
        //                              .WithThumbnailUrl(Icons.Huppy1)
        //                              .WithCurrentTimestamp();

        //if (!_cacheService.RegisteredGuildsIds.Contains(guild.Id))
        //{
        //    using var scope = _serviceFactory.CreateAsyncScope();
        //    var serverRepository = scope.ServiceProvider.GetRequiredService<IServerRepository>();

        //    Server server = new()
        //    {
        //        Id = guild.Id,
        //        GreetMessage = "Welcome {username}!",
        //        Rooms = new()
        //        {
        //            OutputRoom = guild.DefaultChannel.Id,
        //            GreetingRoom = 0
        //        },
        //        ServerName = guild.Name,
        //        RoleID = 0,
        //        UseGreet = false,
        //    };

        //    await serverRepository.AddAsync(server);
        //    await serverRepository.SaveChangesAsync();
        //}

        //await guild.DefaultChannel.SendMessageAsync(null, false, embed.Build());
    }

    public async Task OnUserJoined(SocketGuildUser user)
    {
        using var scope = _serviceFactory.CreateAsyncScope();
        var serverService = scope.ServiceProvider.GetRequiredService<IServerService>();

        _logger.LogInformation("New user joined [{Username}] at [{ServerName}]", user.Username, user.Guild.Name);

        var server = await serverService.GetAsync(user.Guild.Id);
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

            if (server.RoleId > 0)
            {
                var role = user.Guild.GetRole(server.RoleId);
                if (role is null)
                {
                    _logger.LogWarning("Role with [{RoleID}] ID on [{ServerName}] is not found. Updating default role to none", server.RoleId, user.Guild.Name);
                    server.RoleId = default;

                    await serverService.UpdateAsync(server);
                    return;
                }

                await (user as IGuildUser).AddRoleAsync(server.RoleId);
            }
        }

        _logger.LogWarning("Didn't welcome user because server returned null");
    }
}
