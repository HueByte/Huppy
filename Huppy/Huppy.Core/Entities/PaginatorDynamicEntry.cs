namespace Huppy.Core.Entities
{
    public class PaginatorDynamicEntry
    {
        public string Name { get; set; }
        public List<PaginatorDynamicPage>? DynamicPages { get; set; }
    }
}