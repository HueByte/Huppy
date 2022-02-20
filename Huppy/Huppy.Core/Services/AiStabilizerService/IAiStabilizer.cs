using Huppy.Core.Entities;

namespace Huppy.Core.Services.AiStabilizerService
{
    public interface IAiStabilizerService
    {
        Task<Dictionary<ulong, AiUser>> GetAiStatistics();
        Task LogUsageAsync(string Prompt, string Username, ulong UserId, string Response);
    }
}