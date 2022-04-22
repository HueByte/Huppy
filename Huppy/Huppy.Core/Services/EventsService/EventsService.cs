using System.Collections.Concurrent;

namespace Huppy.Core.Services.EventsService
{
    public class EventsService : IEventsService
    {
        public ConcurrentDictionary<DateTime, Action> events = new();
        public EventsService() {}

        public async Task AddEvent(DateTime time, Action event)
        {
            
        }
    }
}