using Huppy.Kernel;
using System.ComponentModel.DataAnnotations;

namespace Huppy.Core.Models
{
    public class User : DbModel<ulong>
    {
        [Key]
        public override ulong Id { get; set; }
        public string? Username { get; set; }
        public DateTime? JoinDate { get; set; }
    }
}