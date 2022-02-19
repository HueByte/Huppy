namespace Huppy.Core.Services.GPTService
{
    public interface IGPTService
    {
        Task GetEngines();
        Task<string> DavinciCompletion(string prompt);
    }
}