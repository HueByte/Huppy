using System.ComponentModel.DataAnnotations;

namespace Huppy.Core.Models
{
    public class User
    {
        [Key]
        public ulong Id { get; set; }
        public string? Username { get; set; }
        public DateTime? JoinDate { get; set; }
    }
}