using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.SignalR;
using LiteDB;
using Repositorch.Data;
using Repositorch.Data.Entities;
using Repositorch.Data.Entities.Mapping;
using Repositorch.Data.VersionControl;
using Repositorch.Data.VersionControl.Git;
using Repositorch.Data.Entities.DSL.Selection;
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
            return Task.Run(() => {
                try
                {
                    var mapper = CreateDataMapper(settings);
                    var mappingSettings = new VcsDataMapper.MappingSettings()
                    {
                        Check = VcsDataMapper.CheckMode.TOUCHED,
                    };
                    mapper.MapRevisions(mappingSettings);
                }
                catch (Exception e)
                {
                    mappingHub.Clients.Group(settings.Name)
                        .Progress($"Error: {e.Message}", "", true);
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
            dataMapper.OnMapRevision += revision =>
            {
                mappingHub.Clients.Group(settings.Name)
                    .Progress($"Mapping: {revision}", "", true);
            };
            dataMapper.OnTruncateRevision += revision =>
            {
                mappingHub.Clients.Group(settings.Name)
                    .Progress($"Truncating: {revision}", "", true);
            };
            dataMapper.OnCheckRevision += revision =>
            {
                mappingHub.Clients.Group(settings.Name)
                    .Progress($"Checking: {revision}", "", true);
            };
            dataMapper.OnError += message =>
            {
                mappingHub.Clients.Group(settings.Name)
                    .Progress($"Error: {message}", "", false);
            };
            return dataMapper;
        }
    }
}
