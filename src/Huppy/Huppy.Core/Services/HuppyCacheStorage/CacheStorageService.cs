using System.Collections.Specialized;
using Huppy.Core.Entities;
using Huppy.Core.Interfaces.IRepositories;
using Huppy.Kernel;
using Microsoft.Extensions.DependencyInjection;

namespace Huppy.Core.Services.HuppyCacheStorage;

public partial class CacheStorageService
{
    private const int MessageCacheSize = 1000;
    private readonly IServiceScopeFactory _serviceFactory;

    public IReadOnlyDictionary<ulong, string?> CacheUsers => _cacheUsers;
    private Dictionary<ulong, string?> _cacheUsers = null!;

    public IReadOnlyDictionary<ulong, AiUser> UserAiUsage => _userAiUsage;
    private Dictionary<ulong, AiUser> _userAiUsage = null!;

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
        var commandRepository = scope.ServiceProvider.GetRequiredService<ICommandLogRepository>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var serverRepository = scope.ServiceProvider.GetRequiredService<IServerRepository>();
        var appSettings = scope.ServiceProvider.GetRequiredService<AppSettings>();

        _cacheUsers = new(await userRepository.GetUsersForCacheAsync());
        _userAiUsage = new(await commandRepository.GetAiUsage());
        _registeredGuildsIds = new((await serverRepository.GetAllAsync()).Select(guild => guild.Id).ToHashSet());
        _developerIds = appSettings.Developers!.Split(';').Where(sId => !string.IsNullOrEmpty(sId)).Select(sId => ulong.Parse(sId)).ToHashSet();
        PaginatorEntries = new();

        // TODO: configurable in appsettings
        SetMaxMessageCacheSize(MessageCacheSize);
    }
}
