using Discord;
using Huppy.Core.Interfaces;
using Huppy.Core.Interfaces.IServices;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Huppy.Core.Services.PaginatorService.Entities
{
    public class StaticPaginatorEntry : IPaginatorEntry
    {
        public ulong MessageId { get; set; }
        public string Name { get; set; }
        public ushort CurrentPage { get; set; }
        public List<string> PageNames { get; set; } = new();
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public StaticPaginatorEntry(IServiceScopeFactory scopeFactory)
        {
            _serviceScopeFactory = scopeFactory;
        }

        public Task<EmbedBuilder?>? GetPageContent(int page)
        {
            if (page < 0 || page >= PageNames.Count)
                return Task.FromResult<EmbedBuilder?>(default);

            if (string.IsNullOrEmpty(PageNames[page]))
                throw new Exception("Page selected doesn't exist");

            // Fetch page content from registered static embeds in PaginatorService
            var scope = _serviceScopeFactory.CreateAsyncScope();
            var paginatorService = scope.ServiceProvider.GetService<IPaginatorService>();

            // fetch list of static embeds by key name
            var staticEmbeds = paginatorService?.GetStaticEmbeds(Name);

            if (staticEmbeds is null)
                throw new Exception($"Couldn't find static embed named {Name}");

            // Get static embed page from store by name
            var result = staticEmbeds.FirstOrDefault(embed => embed.Name == PageNames[page]);

            if (result is null)
                throw new Exception($"Couldn't find static page named {PageNames[page]}");

            return Task.FromResult(result.Embed)!;
        }

        public int GetPageCount() => PageNames.Count;
    }
}