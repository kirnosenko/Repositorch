using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.SignalR;
using LiteDB;
using Newtonsoft.Json;
using Repositorch.Data.Entities.Mapping;
using Repositorch.Data.VersionControl;
using Repositorch.Data.VersionControl.Git;
using Repositorch.Data.Entities.Persistent;

namespace Repositorch.Web
{
	public class MappingService : BackgroundService
	{
		private readonly IMappingNotifier mappingNotifier;
		private readonly LiteDatabase liteDb;

		private readonly ConcurrentDictionary<string, CancellationTokenSource> mappingTokens;
		private readonly ConcurrentQueue<string> projectsToStart;
		private readonly ConcurrentQueue<string> projectsToStop;

		public MappingService(
			IMappingNotifier mappingNotifier,
			LiteDatabase liteDb)
		{
			this.mappingNotifier = mappingNotifier;
			this.liteDb = liteDb;
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
					var projectSettings = liteDb.GetCollection<ProjectSettings>()
						.FindOne(x => x.Name == projectToStart);
					var projectTask = StartMappingTask(projectSettings);
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

		private Task StartMappingTask(ProjectSettings settings)
		{
			return Task.Run(async () => {
				using (var cts = new CancellationTokenSource())
				{
					mappingTokens.TryAdd(settings.Name, cts);
					try
					{
						var mapper = CreateDataMapper(settings);
						var mappingSettings = new VcsDataMapper.MappingSettings()
						{
							Check = VcsDataMapper.CheckMode.TOUCHED,
						};
						if (mapper.MapRevisions(mappingSettings, cts.Token))
						{
							await mappingNotifier.Notify(
								settings.Name, null, null, false);
						}
					}
					catch (Exception e)
					{
						await mappingNotifier.Notify(
							settings.Name,
							null,
							JsonConvert.SerializeObject(e, Formatting.Indented),
							false);
					}
					finally
					{
						mappingTokens.TryRemove(settings.Name, out _);
					}
				}
			});
		}

		private VcsDataMapper CreateDataMapper(ProjectSettings settings)
		{
			var data = new SqlServerDataStore(settings.Name);
			var gitClient = new CommandLineGitClient(settings.RepositoryPath)
			{
				Branch = settings.Branch,
				ExtendedLog = settings.UseExtendedLog,
			};
			var vcsData = new VcsDataCached(gitClient, 1000, 1000);

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
					settings.Name, $"Mapping: {revision}", null, true);
			};
			dataMapper.OnTruncateRevision += async revision =>
			{
				await mappingNotifier.Notify(
					settings.Name, $"Truncating: {revision}", null, true);
			};
			dataMapper.OnCheckRevision += async revision =>
			{
				await mappingNotifier.Notify(
					settings.Name, $"Checking: {revision}", null, true);
			};
			dataMapper.OnError += async message =>
			{
				await mappingNotifier.Notify(
					settings.Name, null, $"Error: {message}", false);
			};

			return dataMapper;
		}
	}
}
