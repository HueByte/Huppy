namespace Huppy.Core.Entities
{
    public class AiUsage
    {
        public string? Username { get; set; }
        public ulong UserId { get; set; }
        public string? Prompt { get; set; }
        public string? Response { get; set; }
        public DateTime? Date { get; set; }
    }
}