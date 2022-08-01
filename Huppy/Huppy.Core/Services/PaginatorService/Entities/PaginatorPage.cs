using Discord;

namespace Huppy.Core.Services.PaginatorService.Entities
{
    public class PaginatorPage
    {
        public string? Name { get; set; }
        public ushort Page { get; set; }
        public EmbedBuilder? Embed { get; set; }
    }
}