using Microsoft.Extensions.DependencyInjection;

namespace Huppy.Core.Entities
{
    public class TimedJob
    {
        public Guid JobId { get; set; }
        public string? Name { get; set; }
        public Func<AsyncServiceScope, object?, Task?> Function { get; set; } = null!;
        public object? Data { get; set; }
        public TimeSpan DueTime { get; set; }
        public TimeSpan Period { get; set; }
    }
}