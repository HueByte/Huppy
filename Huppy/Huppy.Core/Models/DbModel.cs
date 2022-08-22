using System.ComponentModel.DataAnnotations;

namespace Huppy.Core.Models
{

    public class DbModel<TKey> where TKey : IConvertible
    {
        public virtual TKey Id { get; set; }
    }
}