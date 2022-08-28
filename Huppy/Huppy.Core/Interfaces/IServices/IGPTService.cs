namespace Huppy.Core.Interfaces.IServices
{
    public interface IGPTService
    {
        Task GetEngines();
        Task<string> DavinciCompletion(string prompt);
    }
}