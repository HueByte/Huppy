using System.Diagnostics;

namespace Huppy.Core.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    public class DebugGroupAttribute : Attribute
    {
        public DebugGroupAttribute() { }
    }
}