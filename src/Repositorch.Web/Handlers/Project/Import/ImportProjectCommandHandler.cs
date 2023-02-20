using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Repositorch.Web.Projects;

namespace Repositorch.Web.Handlers.Project.Import
{
	public class ImportProjectCommandHandler : IRequestHandler<ImportProjectCommand>
	{
		private readonly IProjectManager projectManager;

		public ImportProjectCommandHandler(IProjectManager projectManager)
		{
			this.projectManager = projectManager;
		}

		public Task Handle(ImportProjectCommand request, CancellationToken cancellationToken)
		{
			var data = request.Data;
			LinkEntities(data);
			ClearOldIds(data);
			
			var store = projectManager.GetProjectDataStore(data.Settings);
			using (var s = store.OpenSession())
			{
				s.AddRange(data.Commits);
				s.AddRange(data.CommitAttributes);
				s.AddRange(data.Authors);
				s.AddRange(data.Branches);
				s.SubmitChanges();

				s.AddRange(data.Files);
				s.SubmitChanges();

				s.AddRange(data.Modifications);
				s.AddRange(data.Blocks);
				s.SubmitChanges();
			}

			return Task.CompletedTask;
		}

		private void LinkEntities(ProjectData data)
		{
			var commits = data.Commits.ToDictionary(x => x.Number, x => x);
			var files = data.Files.ToDictionary(x => x.Id, x => x);
			var modifications = data.Modifications.ToDictionary(x => x.Id, x => x);
			var blocks = data.Blocks.ToDictionary(x => x.Id, x => x);

			foreach (var attribute in data.CommitAttributes)
			{
				attribute.Commit = commits[attribute.CommitNumber];
			}
			foreach (var author in data.Authors)
			{
				author.Commits = data.Commits
					.Where(x => x.AuthorId == author.Id).ToList();
			}
			foreach (var branch in data.Branches)
			{
				branch.Commits = data.Commits
					.Where(x => x.BranchId == branch.Id).ToList();
			}
			foreach (var modification in data.Modifications)
			{
				modification.Commit = commits[modification.CommitNumber];
				modification.File = files[modification.FileId];
				if (modification.SourceCommitNumber.GetValueOrDefault() != 0)
				{
					modification.SourceCommit = commits[modification.SourceCommitNumber.Value];
				}
				if (modification.SourceFileId.GetValueOrDefault() != 0)
				{
					modification.SourceFile = files[modification.SourceFileId.Value];
				}
			}
			foreach (var block in data.Blocks)
			{
				block.Modification = modifications[block.ModificationId];
				if (block.AddedInitiallyInCommitNumber.GetValueOrDefault() != 0)
				{
					block.AddedInitiallyInCommit = commits[block.AddedInitiallyInCommitNumber.Value];
				}
				if (block.TargetCodeBlockId.GetValueOrDefault() != 0)
				{
					block.TargetCodeBlock = blocks[block.TargetCodeBlockId.Value];
				}
			}
		}

		private void ClearOldIds(ProjectData data)
		{
			foreach (var commit in data.Commits)
			{
				commit.AuthorId = 0;
				commit.BranchId = 0;
			}
			foreach (var attribute in data.CommitAttributes)
			{
				attribute.Id = 0;
				attribute.CommitNumber = 0;
			}
			foreach (var author in data.Authors)
			{
				author.Id = 0;
			}
			foreach (var branch in data.Branches)
			{
				branch.Id = 0;
			}
			foreach (var file in data.Files)
			{
				file.Id = 0;
			}
			foreach (var modification in data.Modifications)
			{
				modification.Id = 0;
				modification.CommitNumber = 0;
				modification.FileId = 0;
				modification.SourceCommitNumber = null;
				modification.SourceFileId = null;
			}
			foreach (var block in data.Blocks)
			{
				block.Id = 0;
				block.ModificationId = 0;
				block.AddedInitiallyInCommitNumber = null;
				block.TargetCodeBlockId = null;
			}
		}
	}
}
