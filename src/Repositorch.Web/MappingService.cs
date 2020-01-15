using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.SignalR;

namespace Repositorch.Web
{
	public class MappingService : BackgroundService
	{
        private readonly IHubContext<MappingHub, IMappingWatcher> mappingHub;
        private readonly ConcurrentDictionary<string, int> counters;

        public MappingService(IHubContext<MappingHub, IMappingWatcher> mappingHub)
        {
            this.mappingHub = mappingHub;
            this.counters = new ConcurrentDictionary<string, int>();
        }

        public void StartMapping(string projectName)
        {
            this.counters.TryAdd(projectName, 0);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var key in counters.Keys)
                {
                    counters[key] += 1;
                    await mappingHub.Clients.Group(key)
                        .Progress(counters[key], 60);
                }
                await Task.Delay(1000);
            }
        }
    }
}
