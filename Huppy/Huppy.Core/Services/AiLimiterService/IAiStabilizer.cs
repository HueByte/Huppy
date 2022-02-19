namespace Huppy.Core.Services.AiLimiterService
{
    public interface IAiStabilizer
    {
        Task LogUsageAsync(string Prompt, string Username, ulong UserId, string Response);
    }
}