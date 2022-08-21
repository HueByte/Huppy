using System.ComponentModel.DataAnnotations.Schema;

namespace Huppy.Core.Models
{
    public class Reminder : DbModel<int>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int Id { get; set; }
        public DateTime RemindDate { get; set; }
        public string Message { get; set; } = null!;

        [ForeignKey("UserId")]
        public ulong UserId { get; set; }
        public virtual User? User { get; set; } = null!;
    }
}