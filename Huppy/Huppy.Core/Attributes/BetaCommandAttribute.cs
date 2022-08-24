using Discord.Interactions;

namespace Huppy.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    [DontAutoRegister]
    public class BetaCommandAttribute : Attribute
    {

    }
}