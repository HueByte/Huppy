using Discord.Interactions;

namespace Huppy.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    [DontAutoRegister]
    public class DebugCommandGroupAttribute : Attribute
    {
        public DebugCommandGroupAttribute() { }
    }
}