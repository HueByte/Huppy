using System.Diagnostics;
using Discord.Interactions;

namespace Huppy.Core.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    [DontAutoRegister]
    public class DebugGroupAttribute : Attribute
    {
        public DebugGroupAttribute() { }
    }
}