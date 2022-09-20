using HuppyCore.Protos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Design;

namespace Gateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Home : ControllerBase
    {
        private readonly Resource.ResourceClient _resourceService;
        public Home(Resource.ResourceClient resourceService) { _resourceService = resourceService; }

        [HttpGet("test")]
        public async Task<IActionResult> GetCpuUsage()
        {
            var test = await _resourceService.GetCpuUsageAsyncAsync(new Google.Protobuf.WellKnownTypes.Empty());

            return Ok(test);
        }
    }
}
