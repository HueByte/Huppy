using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HuppyService.Core.Abstraction;

namespace HuppyService.Core.Models
{
    public class ServerRooms : DbModel<ulong>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public override ulong Id { get; set; }
        public ulong OutputRoom { get; set; }
        public ulong GreetingRoom { get; set; }

        [ForeignKey("ServerId")]
        public ulong ServerId { get; set; }
        public virtual Server? Server { get; set; } = null!;
    }
}