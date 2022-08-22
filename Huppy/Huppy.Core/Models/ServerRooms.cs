using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;

namespace Huppy.Core.Models
{
    public class ServerRooms : DbModel<ulong>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public override ulong Id { get; set; }
        public ulong OutputRoom { get; set; }
        public ulong GreetingRoom { get; set; }

        [ForeignKey("ServerId")]
        public ulong? ServerID { get; set; }
        public virtual Server? Server { get; set; } = null!;
    }
}