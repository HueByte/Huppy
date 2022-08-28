using Huppy.Core.Entities;

namespace Huppy.Core.Interfaces.IServices
{
    public interface IUrbanService
    {
        Task<UrbanResponse> GetDefinition(string term);
    }
}