using Discord;
using Discord.WebSocket;
using Huppy.Core.Interfaces.IRepositories;
using Huppy.Core.Interfaces.IServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.ActivityService
{
    public class ActivityControlService : IActivityControlService
    {
        private readonly ITimedEventsService _timedEventsService;
        private readonly ILogger _logger;
        private readonly List<Func<AsyncServiceScope, Task<IActivity>>> _activities = new();
        private readonly DiscordShardedClient _client;
        private int _lastIndex = 0;
        private readonly TimeSpan updateStatusFrequency = new(0, 10, 0);

        public ActivityControlService(ITimedEventsService timedEventsService, ILogger<ActivityControlService> logger, DiscordShardedClient client)
        {
            _timedEventsService = timedEventsService;
            _logger = logger;
            _client = client;
        }

        public Task Initialize()
        {
            _logger.LogInformation("Starting Activity Control Service");

            _timedEventsService.AddJob(
                Guid.NewGuid(),
                null,
                new TimeSpan(0),
                updateStatusFrequency,
                async (scope, data) =>
                {
                    await ChangeActivity(scope);
                }
            );

            AddActivities();

            return Task.CompletedTask;
        }

        public async Task ChangeActivity(AsyncServiceScope scope)
        {
            var activityService = scope.ServiceProvider.GetRequiredService<IActivityControlService>();

            var activity = await activityService.GetActivity(scope);
            activity ??= new Game("Hello World", ActivityType.Playing);

            await _client.SetActivityAsync(activity);
            _logger.LogInformation("Changing activity to [{activityMessage}]", activity.Name);
        }

        public async Task<IActivity?> GetActivity(AsyncServiceScope scope)
        {
            if (_activities.Count < 0) return null;

            _lastIndex = _lastIndex + 1 >= _activities.Count ? 0 : _lastIndex + 1;

            var activityJob = _activities[_lastIndex];

            if (activityJob is null) return null;

            var activity = await activityJob.Invoke(scope);

            return activity;
        }

        private void AddActivities()
        {
            var serverCountActivity = Task<IActivity> (AsyncServiceScope scope) =>
            {
                var client = scope.ServiceProvider.GetRequiredService<DiscordShardedClient>();
                var message = $"I'm currently in {client.Guilds.Count} servers 🍭";
                IActivity activity = new Game(message, ActivityType.Playing);

                return Task.FromResult(activity);
            };

            var helpActivity = Task<IActivity> (AsyncServiceScope scope) =>
            {
                var message = "Use /help command to get list of the commands 💚";
                IActivity activity = new Game(message, ActivityType.Playing);

                return Task.FromResult(activity);
            };

            var usersActivity = async Task<IActivity> (AsyncServiceScope scope) =>
            {
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var userCount = await (await userRepository.GetAllAsync()).CountAsync();
                var message = $"I've been used by over {userCount} users";

                IActivity activity = new Game(message, ActivityType.Playing);
                return activity;
            };

            _activities.Add(serverCountActivity);
            _activities.Add(helpActivity);
            _activities.Add(usersActivity);
        }
    }
}