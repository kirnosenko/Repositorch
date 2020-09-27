using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Repositorch.Data.Entities.Mapping;

namespace Repositorch.Web.Projects
{
	public class MappingService : BackgroundService
	{
		class MappingInfo : IDisposable
		{
			private CancellationTokenSource tokenSource;
			private string revision;
			private List<string> errors;
			private Stopwatch time;

			public MappingInfo()
			{
				tokenSource = new CancellationTokenSource();
				revision = string.Empty;
				errors = new List<string>();
				time = Stopwatch.StartNew();
			}
			public void Dispose()
			{
				tokenSource.Dispose();
				time.Stop();
			}

			public CancellationTokenSource TokenSource => tokenSource;
			public string Revision { get => revision; set => revision = value; }
			public List<string> Errors => errors;
			public string Time => time.Elapsed.ToFormatedTimeString();
		}

		private readonly IProjectManager projectFactory;
		private readonly IMappingNotifier mappingNotifier;
		
		private readonly ConcurrentDictionary<string, MappingInfo> mappingInfo;
		private readonly ConcurrentQueue<string> projectsToStart;
		private readonly ConcurrentQueue<string> projectsToStop;

		public MappingService(
			IProjectManager projectFactory,
			IMappingNotifier mappingNotifier)
		{
			this.projectFactory = projectFactory;
			this.mappingNotifier = mappingNotifier;
			this.mappingInfo = new ConcurrentDictionary<string, MappingInfo>();
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
					if (mappingInfo.TryGetValue(projectToStop, out var info))
					{
						info.TokenSource.Cancel();	
					}
				}

				await Task.Delay(100);
			}
		}

		private Task StartMappingTask(string projectName)
		{
			return Task.Run(async () => {
				using (var mi = new MappingInfo())
				{
					mappingInfo.TryAdd(projectName, mi);
					try
					{
						await mappingNotifier.Notify(
							projectName, "Preparing for mapping...", null, null);
						var mapper = CreateDataMapper(projectName);
						var mappingSettings = new VcsDataMapper.MappingSettings()
						{
							Check = VcsDataMapper.CheckMode.TOUCHED,
						};
						var result = mapper.MapRevisions(mappingSettings, mi.TokenSource.Token);
						switch (result)
						{
							case VcsDataMapper.MappingResult.SUCCESS:
								await mappingNotifier.Notify(
									projectName,
									null,
									Enumerable.Empty<string>(),
									mi.Time);
								break;
							case VcsDataMapper.MappingResult.ERROR:
								await mappingNotifier.Notify(
									projectName,
									$"Error for revision {mi.Revision}",
									mi.Errors,
									mi.Time);
								break;
							default:
								await mappingNotifier.Notify(
									projectName,
									null,
									null,
									mi.Time);
								break;
						}
					}
					catch (Exception e)
					{
						await mappingNotifier.Notify(
							projectName,
							$"Error for revision {mi.Revision}",
							new List<string>() { JsonConvert.SerializeObject(e, Formatting.Indented) },
							mi.Time);
					}
					finally
					{
						mappingInfo.TryRemove(projectName, out _);
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
				if (mappingInfo.TryGetValue(projectName, out var mi))
				{
					mi.Revision = revision;
				}
				await mappingNotifier.Notify(
					projectName, $"Mapping: {revision}", null, null);
			};
			dataMapper.OnTruncateRevision += async revision =>
			{
				await mappingNotifier.Notify(
					projectName, $"Truncating: {revision}", null, null);
			};
			dataMapper.OnCheckRevision += async revision =>
			{
				await mappingNotifier.Notify(
					projectName, $"Checking: {revision}", null, null);
			};
			dataMapper.OnError += message =>
			{
				if (mappingInfo.TryGetValue(projectName, out var mi))
				{
					mi.Errors.Add(message);
				}
			};

			return dataMapper;
		}
	}
}
