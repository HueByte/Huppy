using Discord;
using Microsoft.Extensions.DependencyInjection;

namespace Huppy.Core.Entities
{
    public class PaginatorDynamicPage
    {
        public string? Name { get; set; }
        public Func<IServiceScopeFactory, Task<Embed>>? Embed { get; set; }
        public ushort PageNumber { get; set; }
        public IServiceScopeFactory _scopeFactory;

        public PaginatorDynamicPage() { }
        public PaginatorDynamicPage(string name, Func<IServiceScopeFactory, Task<Embed>> embedCallback, ushort pageNumber, IServiceScopeFactory scopeFactory)
        {
            Name = name;
            Embed = embedCallback;
            PageNumber = pageNumber;
            _scopeFactory = scopeFactory;
        }
    }
}