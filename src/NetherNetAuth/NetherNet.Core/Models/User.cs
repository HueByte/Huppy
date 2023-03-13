using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetherNet.Core.Models
{
    public class User : IdentityUser
    {
        public DateTime CreatedDate { get; set; }

        [ForeignKey("DiscordAccountid")]
        public ulong? DiscordAccountId { get; set; }
        public virtual DiscordAccount? DiscordAccount { get; set; }
    }
}
