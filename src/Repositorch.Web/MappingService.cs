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
        private readonly IHubContext<MappingHub, IMappingWatcher> mappingHub;
        private readonly LiteDatabase liteDb;

        private readonly ConcurrentDictionary<string, Task> mappings;
        private readonly ConcurrentQueue<string> projectsToStart;
        private readonly ConcurrentQueue<string> projectsToStop;

        public MappingService(
            IHubContext<MappingHub, IMappingWatcher> mappingHub,
            LiteDatabase liteDb)
        {
            this.mappingHub = mappingHub;
            this.liteDb = liteDb;
            this.mappings = new ConcurrentDictionary<string, Task>();
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
                    var projectTask = StartMappingTask(projectSettings, stopToken);
                    mappings.TryAdd(projectToStart, projectTask);
                }

                await Task.Delay(1000);
            }
        }

        private Task StartMappingTask(ProjectSettings settings, CancellationToken stopToken)
        {
            return Task.Run(async () => {
                try
                {
                    var mapper = CreateDataMapper(settings);
                    var mappingSettings = new VcsDataMapper.MappingSettings()
                    {
                        Check = VcsDataMapper.CheckMode.TOUCHED,
                    };
                    mapper.MapRevisions(mappingSettings);
                    await mappingHub.Clients.Group(settings.Name)
                        .Progress(null, null, false);
                }
                catch (Exception e)
                {
                    await mappingHub.Clients.Group(settings.Name)
                        .Progress(null, JsonConvert.SerializeObject(e, Formatting.Indented), false);
                }
            }, stopToken);
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
            dataMapper.RegisterMapper(
                new CommitMapper(vcsData));
            dataMapper.RegisterMapper(
                new TagMapper(vcsData));
            dataMapper.RegisterMapper(
                new AuthorMapper(vcsData));
            dataMapper.RegisterMapper(
                new BranchMapper(vcsData));
            dataMapper.RegisterMapper(
                new BugFixMapper(vcsData, new BugFixDetectorBasedOnLogMessage()));
            dataMapper.RegisterMapper(
                new CodeFileMapper(vcsData));
            dataMapper.RegisterMapper(
                new ModificationMapper(vcsData));
            dataMapper.RegisterMapper(
                new BlamePreLoader(vcsData), true);
            dataMapper.RegisterMapper(
                new CodeBlockMapper(vcsData));
            dataMapper.OnMapRevision += async revision =>
            {
                await mappingHub.Clients.Group(settings.Name)
                    .Progress($"Mapping: {revision}", null, true);
            };
            dataMapper.OnTruncateRevision += async revision =>
            {
                await mappingHub.Clients.Group(settings.Name)
                    .Progress($"Truncating: {revision}", null, true);
            };
            dataMapper.OnCheckRevision += async revision =>
            {
                await mappingHub.Clients.Group(settings.Name)
                    .Progress($"Checking: {revision}", null, true);
            };
            dataMapper.OnError += async message =>
            {
                await mappingHub.Clients.Group(settings.Name)
                    .Progress(null, $"Error: {message}", false);
            };
            return dataMapper;
        }
    }
}
