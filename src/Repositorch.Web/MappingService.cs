﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Repositorch.Data.Entities.Mapping;

namespace Repositorch.Web
{
	public class MappingService : BackgroundService
	{
		class MappingInfo : IDisposable
		{
			public MappingInfo()
			{
				TokenSource = new CancellationTokenSource();
				Revision = string.Empty;
				Errors = new List<string>();
			}
			public void Dispose()
			{
				TokenSource.Dispose();
			}

			public CancellationTokenSource TokenSource { get; protected set; }
			public string Revision { get; set; }
			public List<string> Errors { get; protected set; }
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
							projectName, "Preparing for mapping...", null, true);
						var mapper = CreateDataMapper(projectName);
						var mappingSettings = new VcsDataMapper.MappingSettings()
						{
							Check = VcsDataMapper.CheckMode.TOUCHED,
						};
						if (mapper.MapRevisions(mappingSettings, mi.TokenSource.Token))
						{
							await mappingNotifier.Notify(
								projectName, null, null, false);
						}
						else
						{
							var errorsText = new StringBuilder();
							errorsText.AppendLine($"Error for revision {mi.Revision}");
							foreach (var err in mi.Errors)
							{
								errorsText.AppendLine(err);
							}
							await mappingNotifier.Notify(
								projectName, null, errorsText.ToString(), false);
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
				mappingInfo[projectName].Revision = revision;
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
			dataMapper.OnError += message =>
			{
				var projectErrors = mappingInfo[projectName].Errors;
				projectErrors.Add(message);
			};

			return dataMapper;
		}
	}
}
