namespace Huppy.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class BetaCommandGroupAttribute : Attribute
    {
        public BetaCommandGroupAttribute() { }
    }
}