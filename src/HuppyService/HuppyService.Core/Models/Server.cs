using HuppyService.Core.Abstraction;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HuppyService.Core.Models
{
    public class Server : DbModel<ulong>
    {
        [Key]
        public override ulong Id { get; set; }
        public string? ServerName { get; set; }
        public bool UseGreet { get; set; }
        public string? GreetMessage { get; set; }
        public ulong RoleID { get; set; }

        [ForeignKey("ServerRoomsId")]
        public ulong ServerRoomsId { get; set; }
        public virtual ServerRooms? Rooms { get; set; }
        public virtual IList<CommandLog>? CommangLogs { get; set; }
    }
}