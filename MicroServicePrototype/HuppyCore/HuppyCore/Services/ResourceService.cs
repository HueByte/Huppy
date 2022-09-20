using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using HuppyCore.Protos;
using System.Diagnostics;

namespace HuppyCore.Services
{
    public class ResourceService : Resource.ResourceBase
    {
        public ResourceService() { }

        public async override Task<CPUUsage> GetCpuUsageAsync(Empty request, ServerCallContext context)
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;

            await Task.Delay(1000); // gets CPU time between 1000ms

            var endTime = DateTime.UtcNow;
            var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;

            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

            return new CPUUsage()
            {
                Usage = string.Concat(Math.Round(cpuUsageTotal * 100, 2), '%')
            };
        }
    }
}
