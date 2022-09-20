using HuppyService.Core.Abstraction;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HuppyService.Core.Models
{
    public class Ticket : DbModel<string>
    {
        [Key]
        public override string Id { get; set; } = null!;
        public string Topic { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool IsClosed { get; set; }
        public string? TicketAnswer { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ClosedDate { get; set; }

        [ForeignKey("UserId")]
        public ulong UserId { get; set; }
        public virtual User User { get; set; } = null!;
    }
}