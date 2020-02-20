using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Repositorch.Web
{
    public interface IMappingWatcher
    {
        Task Progress(string progress, string error, bool working);
    }

    public class MappingHub : Hub<IMappingWatcher>
	{
        private readonly ConcurrentDictionary<string, string> connections =
            new ConcurrentDictionary<string, string>();

        public async Task WatchProject(string projectName)
        {
            var id = Context.ConnectionId;
            await Groups.AddToGroupAsync(id, projectName);
            connections.TryAdd(id, projectName);
        }
        
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var id = Context.ConnectionId;
            await Groups.RemoveFromGroupAsync(id, connections[id]);

            await base.OnDisconnectedAsync(exception);
        }
    }
}
