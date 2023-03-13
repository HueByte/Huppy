using System.Reflection;
using Huppy.Core.Interfaces.IServices;

namespace Huppy.Core.Services.App;

public class AppMetadataService : IAppMetadataService
{
    public string Version { get; } = Assembly.GetEntryAssembly()!
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
        .InformationalVersion;
}
