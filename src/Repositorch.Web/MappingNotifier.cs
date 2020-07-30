using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Repositorch.Web
{
	public interface IMappingNotifier
	{
		Task Notify(string project, string progress, IEnumerable<string> errors, bool working);
		Task NotifyOnConnect(string connectionId);
	}

	public class MappingNotifier : IMappingNotifier
	{
		private readonly IHubContext<MappingHub, IMappingWatcher> mappingHub;
		private readonly ConcurrentDictionary<string, (string progress,IEnumerable<string> errors,bool working)> notifications;

		public MappingNotifier(IHubContext<MappingHub, IMappingWatcher> mappingHub)
		{
			this.mappingHub = mappingHub;
			this.notifications = new ConcurrentDictionary<string, (string, IEnumerable<string>, bool)>();
		}

		public async Task Notify(string project, string progress, IEnumerable<string> errors, bool working)
		{
			notifications.AddOrUpdate(
				project,
				(progress, errors, working),
				(k,v) => (progress, errors, working));
			await mappingHub.Clients.All
				.Progress(project, progress, errors, working);
		}

		public async Task NotifyOnConnect(string connectionId)
		{
			foreach (var n in notifications)
			{
				await mappingHub.Clients.Client(connectionId)
					.Progress(n.Key, n.Value.progress, n.Value.errors, n.Value.working);
			}
		}
	}
}
