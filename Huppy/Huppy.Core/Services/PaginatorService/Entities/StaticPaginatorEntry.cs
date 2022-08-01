using Discord;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Huppy.Core.Services.PaginatorService.Entities
{
    public class StaticPaginatorEntry : IPaginatorEntry
    {
        public ulong MessageId { get; set; }
        public string Name { get; set; }
        public ushort CurrentPage { get; set; }
        public List<string> Pages { get; set; } = new();
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public StaticPaginatorEntry(IServiceScopeFactory scopeFactory)
        {
            _serviceScopeFactory = scopeFactory;
        }

        public Task<EmbedBuilder?>? GetPageContent(int page)
        {
            if (page < 0 || page >= Pages.Count)
                return Task.FromResult<EmbedBuilder?>(default);

            if (string.IsNullOrEmpty(Pages[page]))
                throw new Exception("Page selected doesn't exist");

            // Fetch page content from registered static embeds in PaginatorService
            var scope = _serviceScopeFactory.CreateAsyncScope();
            var paginatorService = scope.ServiceProvider.GetService<IPaginatorService>();
            var staticEmbeds = paginatorService?.GetStaticEmbeds(Name);

            if (staticEmbeds is null)
                throw new Exception($"Couldn't find static embed named {Name}");

            var result = staticEmbeds.FirstOrDefault(embed => embed.Name == Pages[page]);

            if (result is null)
                throw new Exception($"Couldn't find static page named {Pages[page]}");

            return Task.FromResult(result.Embed)!;
        }

        public int GetPageCount() => Pages.Count;
    }
}