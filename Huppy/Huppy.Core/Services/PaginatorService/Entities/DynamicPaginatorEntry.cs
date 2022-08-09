using Discord;
using Microsoft.Extensions.DependencyInjection;

namespace Huppy.Core.Services.PaginatorService.Entities
{
    public class DynamicPaginatorEntry : IPaginatorEntry
    {
        public ulong MessageId { get; set; }
        public string? Name { get; set; }
        public ushort CurrentPage { get; set; }
        public object? Data { get; set; }
        public List<Func<AsyncServiceScope, object?, Task<PaginatorPage>?>> Pages { get; set; } = new();
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public DynamicPaginatorEntry(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<EmbedBuilder?>? GetPageContent(int page)
        {
            if (page < 0 || page >= Pages.Count)
                return null;

            var asyncScope = _serviceScopeFactory.CreateAsyncScope();
            EmbedBuilder? embed = (await Pages[page]?.Invoke(asyncScope, Data)!).Embed;

            if (embed is null)
                throw new Exception("Something went wrong while attepting to get dynamic paginated embed");

            return embed;
        }

        public int GetPageCount() => Pages.Count;
    }
}