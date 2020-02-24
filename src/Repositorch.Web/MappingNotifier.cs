using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Repositorch.Web
{
	public interface IMappingNotifier
	{
		Task Notify(string project, string progress, string error, bool working);
		Task NotifyOnConnect(string connectionId);
	}

	public class MappingNotifier : IMappingNotifier
	{
		private readonly IHubContext<MappingHub, IMappingWatcher> mappingHub;
		private readonly ConcurrentDictionary<string, (string progress,string error,bool working)> notifications;

		public MappingNotifier(IHubContext<MappingHub, IMappingWatcher> mappingHub)
		{
			this.mappingHub = mappingHub;
			this.notifications = new ConcurrentDictionary<string, (string, string, bool)>();
		}

		public async Task Notify(string project, string progress, string error, bool working)
		{
			notifications.AddOrUpdate(
				project,
				(progress, error, working),
				(k,v) => (progress,error,working));
			await mappingHub.Clients.Group(project)
				.Progress(progress, error, working);
		}

		public async Task NotifyOnConnect(string connectionId)
		{
			foreach (var n in notifications)
			{
				await mappingHub.Clients.Group(n.Key)
					.Progress(n.Value.progress, n.Value.error, n.Value.working);
			}
		}
	}
}
