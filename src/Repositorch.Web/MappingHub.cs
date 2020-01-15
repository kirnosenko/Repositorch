using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Repositorch.Web
{
    public interface IMappingWatcher
    {
        Task Progress(int current, int total);
    }

    public class MappingHub : Hub<IMappingWatcher>
	{
        public async Task StartWatching(string projectName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, projectName);
        }
    }
}
