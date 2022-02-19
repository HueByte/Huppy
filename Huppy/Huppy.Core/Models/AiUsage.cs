namespace Huppy.Core.Models
{
    public class AiUsage
    {
        public int ID { get; set; }
        public string? Username { get; set; }
        public ulong UserId { get; set; }
        public string? Prompt { get; set; }
        public string? Response { get; set; }
        public DateTime? Date { get; set; }
    }
}