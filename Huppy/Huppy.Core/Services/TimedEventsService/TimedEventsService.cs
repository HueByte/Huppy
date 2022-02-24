using Huppy.Core.Services.NewsService;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.TimedEventsService
{
    public class TimedEventsService : ITimedEventsService
    {
        private readonly ILogger _logger;
        private readonly INewsApiService _newsService;
        private System.Threading.Timer _newsTimer;
        public TimedEventsService(ILogger<TimedEventsService> logger, INewsApiService newsService)
        {
            _logger = logger;
            _newsService = newsService;
        }

        public Task StartTimers()
        {
            NewsEvent();
            return Task.CompletedTask;
        }

        private Task NewsEvent()
        {
            var looper = TimeSpan.FromMinutes(30);

            _newsTimer = new Timer(async (e) =>
            {
                await _newsService.PostNews();

            }, null, new TimeSpan(0), looper);

            return Task.CompletedTask;
        }
    }
}