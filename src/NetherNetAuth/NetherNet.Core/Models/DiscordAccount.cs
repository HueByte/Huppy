using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetherNet.Core.Models
{
    public class DiscordAccount
    {
        [Key]
        public ulong Id { get; set; }
        public string? Username { get; set; }
    }
}
