using Huppy.Core.Dto;

namespace Huppy.Core.Services.UrbanService
{
    public interface IUrbanService
    {
        Task<UrbanResponse> GetDefinition(string term);
    }
}