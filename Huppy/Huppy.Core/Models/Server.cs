using System.ComponentModel.DataAnnotations;

namespace Huppy.Core.Models
{
    // TODO add default role
    public class Server
    {
        [Key]
        public ulong ID { get; set; }
        public string? ServerName { get; set; }
        public bool UseGreet { get; set; }
        public string? GreetMessage { get; set; }
        public ulong OutputRoom { get; set; }
        public ulong RoleID { get; set; }
    }
}