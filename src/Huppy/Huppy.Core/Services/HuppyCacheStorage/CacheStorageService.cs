using System.Collections.Specialized;
using Google.Protobuf.WellKnownTypes;
//using Huppy.Core.Entities;
using Huppy.Core.Interfaces.IRepositories;
using Huppy.Core.Interfaces.IServices;
using Huppy.Kernel;
using HuppyService.Service.Protos;
using Microsoft.Extensions.DependencyInjection;

namespace Huppy.Core.Services.HuppyCacheStorage;

public partial class CacheStorageService
{
    private const int MessageCacheSize = 1000;
    private readonly IServiceScopeFactory _serviceFactory;

    public IReadOnlyDictionary<ulong, string?> CacheUsers => _cacheUsers;
    private Dictionary<ulong, string?> _cacheUsers = null!;

    public IReadOnlyDictionary<ulong, int> UserAiUsage => (IReadOnlyDictionary<ulong, int>)_userAiUsage;
    private IDictionary<ulong, int> _userAiUsage = null!;

    // Switch to IReadOnlySet<ulong>
    public HashSet<ulong> RegisteredGuildsIds => _registeredGuildsIds;
    private HashSet<ulong> _registeredGuildsIds = null!;

    public IReadOnlySet<ulong> DeveloperIds => _developerIds;
    private HashSet<ulong> _developerIds = null!;

    public OrderedDictionary PaginatorEntries { get; private set; } = null!;

    public CacheStorageService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceFactory = serviceScopeFactory;
    }

    public async Task Initialize()
    {
        using var scope = _serviceFactory.CreateAsyncScope();
        var commandLogService = scope.ServiceProvider.GetRequiredService<ICommandLogService>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var serverService = scope.ServiceProvider.GetRequiredService<IServerService>();
        var appSettings = scope.ServiceProvider.GetRequiredService<AppSettings>();

        var aiUsage = await commandLogService.GetAiUsage();

        _cacheUsers = new(await userRepository.GetUsersForCacheAsync());
        _userAiUsage = aiUsage;
        _registeredGuildsIds = (await serverService.GetAllAsync(new Empty())).ServerModels.Select(guild => guild.Id).ToHashSet();
        _developerIds = appSettings.Developers!.Split(';').Where(sId => !string.IsNullOrEmpty(sId)).Select(sId => ulong.Parse(sId)).ToHashSet();
        PaginatorEntries = new();

        // TODO: configurable in appsettings
        SetMaxMessageCacheSize(MessageCacheSize);
    }
}
