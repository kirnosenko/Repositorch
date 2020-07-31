﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Repositorch.Web
{
	public interface IMappingWatcher
	{
		Task Progress(string project, string progress, IEnumerable<string> errors, bool working);
	}

	public class MappingHub : Hub<IMappingWatcher>
	{
		private readonly IMappingNotifier mappingNotifier;

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
