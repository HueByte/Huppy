using Huppy.Core.Entities;

namespace Huppy.Core.Services.AiStabilizerService
{
    public interface IAiStabilizerService
    {
        Task<Dictionary<ulong, AiUser>> GetAiStatistics();
        Task<int> GetCurrentMessageCount();
        Task<AiUser> GetUserUsage(ulong UserId);
        Task LogUsageAsync(string Prompt, string Username, ulong UserId, string Response);
    }
}