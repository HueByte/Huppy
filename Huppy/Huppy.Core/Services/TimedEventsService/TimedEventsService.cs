using System.Runtime.CompilerServices;
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
        private Timer _newsTimer;
        public TimedEventsService(ILogger<TimedEventsService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public Task StartTimers()
        {
            NewsEvent();
            return Task.CompletedTask;
        }

        // TODO add timepsan to news config
        private Task NewsEvent()
        {
            var looper = TimeSpan.FromMinutes(180);

            _newsTimer = new Timer(async (e) =>
            {
                using var scope = _scopeFactory.CreateAsyncScope();
                var _newsService = scope.ServiceProvider.GetRequiredService<INewsApiService>();
                await _newsService.PostNews();
            }, null, new TimeSpan(0), looper);

            return Task.CompletedTask;
        }
    }
}