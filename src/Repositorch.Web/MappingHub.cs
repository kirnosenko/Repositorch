using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Repositorch.Web
{
	public interface IMappingWatcher
	{
		Task Progress(string project, string progress, string error, bool working);
	}

	public class MappingHub : Hub<IMappingWatcher>
	{
		private readonly IMappingNotifier mappingNotifier;
		private readonly ConcurrentDictionary<string, string> connections =
			new ConcurrentDictionary<string, string>();

		public MappingHub(IMappingNotifier mappingNotifier)
		{
			this.mappingNotifier = mappingNotifier;
		}

		public override async Task OnConnectedAsync()
		{
			await base.OnConnectedAsync();
			await mappingNotifier.NotifyOnConnect(Context.ConnectionId);
		}
	}
}
