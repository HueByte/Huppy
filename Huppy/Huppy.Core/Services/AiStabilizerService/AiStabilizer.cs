using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;
using Huppy.Core.Entities;
using Huppy.Core.IRepositories;
using Huppy.Core.Models;
using Microsoft.Extensions.Logging;

namespace Huppy.Core.Services.AiStabilizerService
{
    // TODO implement white list and limiting for AI
    public class AiStabilizerService : IAiStabilizerService
    {
        private readonly ILogger _logger;

        public AiStabilizerService(ILogger<AiStabilizerService> logger)
        {
            _logger = logger;
        }
    }
}