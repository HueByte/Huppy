using Discord;
namespace Huppy.Core.Services.PaginatorService.Entities
{
    public interface IPaginatorEntry
    {
        ulong MessageId { get; set; }
        string Name { get; set; }
        ushort CurrentPage { get; set; }
        Task<EmbedBuilder?>? GetPageContent(int page);
        int GetPageCount();
    }
}