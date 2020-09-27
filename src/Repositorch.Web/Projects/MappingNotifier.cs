using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Repositorch.Web.Projects
{
	public interface IMappingNotifier
	{
		Task Notify(
			string project,
			string progress,
			IEnumerable<string> errors,
			string time);
		Task NotifyOnConnect(string connectionId);
	}

	public class MappingNotifier : IMappingNotifier
	{
		private readonly IHubContext<MappingHub, IMappingWatcher> mappingHub;
		private readonly ConcurrentDictionary<string, (string progress,IEnumerable<string> errors,string time)> notifications;

		public MappingNotifier(IHubContext<MappingHub, IMappingWatcher> mappingHub)
		{
			this.mappingHub = mappingHub;
			this.notifications = new ConcurrentDictionary<string, (string, IEnumerable<string>, string)>();
		}

		public async Task Notify(string project, string progress, IEnumerable<string> errors, string time)
		{
			notifications.AddOrUpdate(
				project,
				(progress, errors, time),
				(k,v) => (progress, errors, time));
			await mappingHub.Clients.All
				.Progress(project, progress, errors, time);
		}

		public async Task NotifyOnConnect(string connectionId)
		{
			foreach (var n in notifications)
			{
				await mappingHub.Clients.Client(connectionId)
					.Progress(n.Key, n.Value.progress, n.Value.errors, n.Value.time);
			}
		}
	}
}
