using Huppy.Core.Entities;

namespace Huppy.Core.Services.AiStabilizerService
{
    public interface IAiStabilizerService
    {
        Task<Dictionary<ulong, AiUser>> GetStatistics();
        Task LogUsageAsync(string Prompt, string Username, ulong UserId, string Response);
    }
}