namespace Huppy.Core.Services.EventService
{
    public class TimedEvent
    {
        public string Name { get; set; }
        public Func<Task?> Event { get; set; }
    }
}