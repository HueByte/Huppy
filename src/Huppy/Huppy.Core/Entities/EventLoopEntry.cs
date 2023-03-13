namespace Huppy.Core.Entities
{
    public class EventLoopEntry
    {
        public object? Data { get; set; }
        public string Name { get; set; } = null!;
        public Func<object?, Task> Task { get; set; } = null!;
    }
}