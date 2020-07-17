using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Repositorch.Data.Entities.Mapping;

namespace Repositorch.Web
{
	public class MappingService : BackgroundService
	{
		private readonly IProjectDataFactory projectFactory;
		private readonly IMappingNotifier mappingNotifier;
		
		private readonly ConcurrentDictionary<string, CancellationTokenSource> mappingTokens;
		private readonly ConcurrentQueue<string> projectsToStart;
		private readonly ConcurrentQueue<string> projectsToStop;

		public MappingService(
			IProjectDataFactory projectFactory,
			IMappingNotifier mappingNotifier)
		{
			this.projectFactory = projectFactory;
			this.mappingNotifier = mappingNotifier;
			this.mappingTokens = new ConcurrentDictionary<string, CancellationTokenSource>();
			this.projectsToStart = new ConcurrentQueue<string>();
			this.projectsToStop = new ConcurrentQueue<string>();
		}

		public void StartMapping(string projectName)
		{
			projectsToStart.Enqueue(projectName);
		}

		public void StopMapping(string projectName)
		{
			projectsToStop.Enqueue(projectName);
		}

		protected override async Task ExecuteAsync(CancellationToken stopToken)
		{
			while (!stopToken.IsCancellationRequested)
			{
				while (projectsToStart.TryDequeue(out var projectToStart))
				{
					var projectTask = StartMappingTask(projectToStart);
				}
				while (projectsToStop.TryDequeue(out var projectToStop))
				{
					if (mappingTokens.TryGetValue(projectToStop, out var mappingToken))
					{
						mappingToken.Cancel();	
					}
				}

				await Task.Delay(1000);
			}
		}

		private Task StartMappingTask(string projectName)
		{
			return Task.Run(async () => {
				using (var cts = new CancellationTokenSource())
				{
					mappingTokens.TryAdd(projectName, cts);
					try
					{
						var mapper = CreateDataMapper(projectName);
						var mappingSettings = new VcsDataMapper.MappingSettings()
						{
							Check = VcsDataMapper.CheckMode.TOUCHED,
						};
						if (mapper.MapRevisions(mappingSettings, cts.Token))
						{
							await mappingNotifier.Notify(
								projectName, null, null, false);
						}
					}
					catch (Exception e)
					{
						await mappingNotifier.Notify(
							projectName,
							null,
							JsonConvert.SerializeObject(e, Formatting.Indented),
							false);
					}
					finally
					{
						mappingTokens.TryRemove(projectName, out _);
					}
				}
			});
		}

		private VcsDataMapper CreateDataMapper(string projectName)
		{
			var data = projectFactory.GetProjectDataStore(projectName);
			var vcsData = projectFactory.GetProjectVcsData(projectName);

			VcsDataMapper dataMapper = new VcsDataMapper(data, vcsData);
			dataMapper.RegisterMapper(new CommitMapper(vcsData));
			dataMapper.RegisterMapper(new TagMapper(vcsData));
			dataMapper.RegisterMapper(new AuthorMapper(vcsData));
			dataMapper.RegisterMapper(new BranchMapper(vcsData));
			dataMapper.RegisterMapper(
				new BugFixMapper(vcsData, new BugFixDetectorBasedOnLogMessage()));
			dataMapper.RegisterMapper(new CodeFileMapper(vcsData));
			dataMapper.RegisterMapper(new ModificationMapper(vcsData));
			dataMapper.RegisterMapper(new BlamePreLoader(vcsData), true);
			dataMapper.RegisterMapper(new CodeBlockMapper(vcsData));
			dataMapper.OnMapRevision += async revision =>
			{
				await mappingNotifier.Notify(
					projectName, $"Mapping: {revision}", null, true);
			};
			dataMapper.OnTruncateRevision += async revision =>
			{
				await mappingNotifier.Notify(
					projectName, $"Truncating: {revision}", null, true);
			};
			dataMapper.OnCheckRevision += async revision =>
			{
				await mappingNotifier.Notify(
					projectName, $"Checking: {revision}", null, true);
			};
			dataMapper.OnError += async message =>
			{
				await mappingNotifier.Notify(
					projectName, null, $"Error: {message}", false);
			};

			return dataMapper;
		}
	}
}
