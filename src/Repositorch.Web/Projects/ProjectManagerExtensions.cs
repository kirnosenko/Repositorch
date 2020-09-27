using System;
using System.Linq;
using Repositorch.Data.Entities;

namespace Repositorch.Web.Projects
{
	public static class ProjectManagerExtensions
	{
		public static ProjectData ExportProject(
			this IProjectManager manager,
			ProjectSettings settings)
		{
			var store = manager.GetProjectDataStore(settings);
			using (var s = store.OpenSession())
			{
				return new ProjectData()
				{
					Settings = settings,
					Commits = s.GetReadOnly<Commit>().OrderBy(x => x.Id).ToArray(),
					Tags = s.GetReadOnly<Tag>().OrderBy(x => x.Id).ToArray(),
					Authors = s.GetReadOnly<Author>().OrderBy(x => x.Id).ToArray(),
					Branches = s.GetReadOnly<Branch>().OrderBy(x => x.Id).ToArray(),
					Fixes = s.GetReadOnly<BugFix>().OrderBy(x => x.Id).ToArray(),
					Files = s.GetReadOnly<CodeFile>().OrderBy(x => x.Id).ToArray(),
					Modifications = s.GetReadOnly<Modification>().OrderBy(x => x.Id).ToArray(),
					Blocks = s.GetReadOnly<CodeBlock>().OrderBy(x => x.Id).ToArray(),
				};
			}
		}

		public static void ImportProject(
			this IProjectManager manager,
			ProjectData data)
		{
			var store = manager.GetProjectDataStore(data.Settings);
			using (var s = store.OpenSession())
			{
				foreach (var tag in data.Tags)
				{
					tag.Commit = data.Commits.Single(x => x.Id == tag.CommitId);
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
				foreach (var fix in data.Fixes)
				{
					fix.Commit = data.Commits.Single(x => x.Id == fix.CommitId);
				}
				foreach (var modification in data.Modifications)
				{
					modification.Commit = data.Commits
						.Single(x => x.Id == modification.CommitId);
					modification.File = data.Files
						.Single(x => x.Id == modification.FileId);
					if (modification.SourceCommitId.GetValueOrDefault() != 0)
					{
						modification.SourceCommit = data.Commits
							.Single(x => x.Id == modification.SourceCommitId);
					}
					if (modification.SourceFileId.GetValueOrDefault() != 0)
					{
						modification.SourceFile = data.Files
							.Single(x => x.Id == modification.SourceFileId);
					}
				}
				foreach (var block in data.Blocks)
				{
					block.Modification = data.Modifications
						.Single(x => x.Id == block.ModificationId);
					if (block.AddedInitiallyInCommitId.GetValueOrDefault() != 0)
					{
						block.AddedInitiallyInCommit = data.Commits
							.Single(x => x.Id == block.AddedInitiallyInCommitId);
					}
					if (block.TargetCodeBlockId.GetValueOrDefault() != 0)
					{
						block.TargetCodeBlock = data.Blocks
							.Single(x => x.Id == block.TargetCodeBlockId);
					}
				}

				foreach (var commit in data.Commits)
				{
					commit.Id = 0;
					commit.AuthorId = 0;
					commit.BranchId = 0;
				}
				foreach (var tag in data.Tags)
				{
					tag.Id = 0;
					tag.CommitId = 0;
				}
				foreach (var author in data.Authors)
				{
					author.Id = 0;
				}
				foreach (var branch in data.Branches)
				{
					branch.Id = 0;
				}
				foreach (var fix in data.Fixes)
				{
					fix.Id = 0;
					fix.CommitId = 0;
				}
				foreach (var file in data.Files)
				{
					file.Id = 0;
				}
				foreach (var modification in data.Modifications)
				{
					modification.Id = 0;
					modification.CommitId = 0;
					modification.FileId = 0;
					modification.SourceCommitId = null;
					modification.SourceFileId = null;
				}
				foreach (var block in data.Blocks)
				{
					block.Id = 0;
					block.ModificationId = 0;
					block.AddedInitiallyInCommitId = null;
					block.TargetCodeBlockId = null;
				}

				s.AddRange(data.Commits);
				s.AddRange(data.Tags);
				s.AddRange(data.Authors);
				s.AddRange(data.Branches);
				s.AddRange(data.Fixes);
				s.AddRange(data.Files);
				s.AddRange(data.Modifications);
				s.AddRange(data.Blocks);

				s.SubmitChanges();
			}
		}
	}
}
