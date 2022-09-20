using Discord;

namespace Huppy.Core.Entities
{
    public class ReminderInput
    {
        public IUser User { get; set; } = null!;
        public string? Message { get; set; }
    }
}