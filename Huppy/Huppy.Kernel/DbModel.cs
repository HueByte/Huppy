using System.ComponentModel.DataAnnotations;

namespace Huppy.Kernel
{

    public class DbModel<TKey> where TKey : IConvertible
    {
        public virtual TKey Id { get; set; }
    }
}