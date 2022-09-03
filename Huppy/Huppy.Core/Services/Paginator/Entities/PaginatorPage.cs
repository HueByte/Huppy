using Discord;

namespace Huppy.Core.Services.Paginator.Entities;

public class PaginatorPage
{
    public string? Name { get; set; }
    public ushort Page { get; set; }
    public EmbedBuilder? Embed { get; set; }
}
