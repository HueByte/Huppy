using HuppyService.Core.Abstraction;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HuppyService.Core.Models;

public class CommandLog : DbModel<int>
{
    [Key]
    public override int Id { get; set; }
    public string? CommandName { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? Date { get; set; }
    public bool IsSuccess { get; set; }
    public long ExecutionTimeMs { get; set; }
    public ulong ChannelId { get; set; }

    //[ForeignKey("UserId")]
    public ulong UserId { get; set; }
    //public virtual User? User { get; set; }

    [ForeignKey("GuildId")]
    public ulong? GuildId { get; set; }
    public virtual Server? Guild { get; set; }
}