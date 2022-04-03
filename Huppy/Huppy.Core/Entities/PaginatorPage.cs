using Discord;

namespace Huppy.Core.Entities
{
    public class PaginatorPage
    {
        public string Name { get; set; } = null!;
        public Embed? Embed { get; set; }
        public ushort PageNumber { get; set; }

        public PaginatorPage() { }

        public PaginatorPage(string name, Embed embed, ushort pageNumber)
        {
            Name = name;
            Embed = embed;
            PageNumber = pageNumber;
        }
    }
}