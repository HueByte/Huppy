# Examples
## Static embed
Static embed needs to be registered, perfectly at the initialization 
```cs
    Dictionary<string, List<PaginatorPage>> staticEmbeds = new();
    List<PaginatorPage> pages = new();

    PaginatorPage page1 = new()
    {
        Name = $"Help0",
        Page = (byte)0,
        Embed = new EmbedBuilder().WithTitle("Help page 1");
    }
    // Add more pages...

    pages.Add(page1);
    staticEmbeds.TryAdd("HelpKey", pages);

    _paginatorService.RegisterStaticEmbeds(staticEmbeds);
```

And this how you can use it: 
```cs
var pageNames = _paginatorService.GetStaticEmbedsNames("HelpKey");

StaticPaginatorEntry help = new(_serviceScopeFactory)
{
    MessageId = 0,
    CurrentPage = 0,
    Name = "HelpKey" // important, must be the same as the key of staticEmbeds entry that contains the pages
    Pages = pageNames // page name must be the same as the name of PaginatorPage you want to get
};

await _paginatorService.SendPaginatedMessage(Context.Interaction, help);
```

### How works?
Method `#StaticPaginatorEntry.GetPageContent(page)` creates local service scope, gets `IPaginatorService` and from there gets staticEmbeds (`List<PaginatorPage>?`) by key which is `StaticPaginatorEntry.Name`, 
```cs
var scope = _serviceScopeFactory.CreateAsyncScope();
var paginatorService = scope.ServiceProvider.GetService<IPaginatorService>();
var staticEmbeds = paginatorService?.GetStaticEmbeds(this.Name);
```

once it gets matched list of `PaginatorPage`, it tries to match `Pages` property with `PaginatorPage.Name` by name
```cs
var result = staticEmbeds.FirstOrDefault(embed => embed.Name == Pages[page]);
```

## Dynamic embed
Usage example:
```cs
DynamicPaginatorEntry entry = new(_serviceScopeFactory)
{
    MessageId = 0,
    CurrentPage = 0,
    Name = "Server Info"
    Pages = new()
};

Func<AsyncServiceScope, Task<PaginatorPage>> page1 = async (scope) => {
    var someRepositoryService = scope.ServiceProvider.GetRequiredService<ISomeRepositoryService>();
    var dataString = await someRepositoryService.GetDataAsync();

    var embed = new EmbedBuilder().WithDescription(dataString);

    return new PaginatorPage() 
    {
        Embed = embed,
        Page = 0
    };
};
// Add more pages...

entry.Pages.Add(page1);
await _paginatorservice.SendPaginatedMessage(Context.Interaction, entry);
```

### How works?
The same as in static paginated entry, async service scope is created in `DynamicPaginatorEntry.GetPageContent(page)` and then passed to the `Func<AsyncServiceScope, Task<PaginatorPage>>` delegate. The delegate gets invoked and the resulting `EmbedBuilder` is extracted from `PaginatorPage`   
```cs
var asyncScope = _serviceScopeFactory.CreateAsyncScope();
EmbedBuilder? embed = (await Pages[page]?.Invoke(asyncScope)!).Embed;
```
