using System.Runtime.CompilerServices;
using Huppy.Core.Entities;
using Huppy.Core.Services.NewsService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.TimedEventsService
{
    // TODO to remove
    public class TimedEventsService : ITimedEventsService
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly List<Timer> _timers;
        private readonly AppSettings _settings;
        public TimedEventsService(ILogger<TimedEventsService> logger, IServiceScopeFactory scopeFactory, AppSettings settings)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _settings = settings;
            _timers = new();
        }

        public Task StartTimers()
        {
            _logger.LogInformation("Starting Timed Events");
            NewsEvent();
            return Task.CompletedTask;
        }

        private Task NewsEvent()
        {
            if (_settings.NewsAPI!.IsEnabled)
            {
                double frequency = _settings.NewsAPI!.Frequency ?? 180;
                var looper = TimeSpan.FromMinutes(frequency);

                _timers.Add(new Timer(async (e) =>
                {
                    using var scope = _scopeFactory.CreateAsyncScope();
                    var _newsService = scope.ServiceProvider.GetRequiredService<INewsApiService>();
                    await _newsService.PostNews();
                }, null, new TimeSpan(0), looper));

            }

            return Task.CompletedTask;
        }
    }
}