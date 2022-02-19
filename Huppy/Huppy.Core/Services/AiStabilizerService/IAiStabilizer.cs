namespace Huppy.Core.Services.AiStabilizerService
{
    public interface IAiStabilizerService
    {
        Task<Dictionary<ulong, int>> GetStatistics();
        Task LogUsageAsync(string Prompt, string Username, ulong UserId, string Response);
    }
}