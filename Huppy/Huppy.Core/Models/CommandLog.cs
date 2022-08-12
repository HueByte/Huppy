using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Huppy.Core.Models
{
    public class CommandLog
    {
        [Key]
        public int Id { get; set; }
        public string? CommandName { get; set; }
        public DateTime? Date { get; set; }
        public bool IsSuccess { get; set; }
        public long ExecutionTimeMs { get; set; }

        [ForeignKey("UserId")]
        public ulong UserId { get; set; }
        public virtual User? User { get; set; }
    }
}