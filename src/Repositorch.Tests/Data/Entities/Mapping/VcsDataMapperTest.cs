using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;
using FluentAssertions;
using NSubstitute;
using Repositorch.Data.VersionControl;
using Repositorch.Data.Entities.DSL.Mapping;
using Repositorch.Data.Entities.Persistent;

namespace Repositorch.Data.Entities.Mapping
{
	public class VcsDataMapperTest : BaseMapperTest
	{
		private IDataStore data;
		private VcsDataMapper mapper;
		private VcsDataMapper.MappingSettings settings;
		private CommitMapper commitMapper;
		private BugFixMapper bugFixMapper;
		private CodeFileMapper fileMapper;
		
		public VcsDataMapperTest()
		{
			data = new InMemoryDataStore(Guid.NewGuid().ToString());
			settings = new VcsDataMapper.MappingSettings();
			mapper = new VcsDataMapper(data, vcsData, settings);
			commitMapper = Substitute.For<CommitMapper>((IVcsData)null);
			bugFixMapper = Substitute.For<BugFixMapper>(null, null);
			fileMapper = Substitute.For<CodeFileMapper>((IVcsData)null);
		}
		[Fact]
		public void Should_use_registered_mapper()
		{
			commitMapper
				.Map(Arg.Any<RepositoryMappingExpression>())
				.Returns(Enumerable.Empty<CommitMappingExpression>());
			
			mapper.RegisterMapper(commitMapper);
			mapper.MapRevision("1");

			commitMapper.Received(1)
				.Map(Arg.Is<RepositoryMappingExpression>(e => e != null));
		}
		[Fact]
		public void Should_set_revision_being_mapped()
		{
			commitMapper
				.Map(Arg.Any<RepositoryMappingExpression>())
				.Returns(Enumerable.Empty<CommitMappingExpression>());
				
			mapper.RegisterMapper(commitMapper);
			mapper.MapRevision("10");

			commitMapper.Received(1)
				.Map(Arg.Is<RepositoryMappingExpression>(e => e.Revision == "10"));
		}
		[Fact]
		public void Should_use_output_expression_for_registered_mapper()
		{
			CommitMappingExpression commitExp = data.UsingSession(
				s => s.MappingDSL().AddCommit("1"));
			commitMapper
				.Map(Arg.Any<RepositoryMappingExpression>())
				.Returns(new CommitMappingExpression[] { commitExp });
			bugFixMapper
				.Map(Arg.Any<CommitMappingExpression>())
				.Returns(Enumerable.Empty<BugFixMappingExpression>());
			
			mapper.RegisterMapper(commitMapper);
			mapper.RegisterMapper(bugFixMapper);
			mapper.MapRevision("1");

			bugFixMapper.Received(1)
				.Map(Arg.Is(commitExp));
		}
		[Fact]
		public void Should_organize_expressions_into_chain()
		{
			ICommitMappingExpression commitExp = null;
			IBugFixMappingExpression bugfixExp = null;
			ICodeFileMappingExpression fileExp = null;

			commitMapper
				.Map(Arg.Any<IRepositoryMappingExpression>())
				.Returns(x =>
				{
					commitExp = Substitute.For<ICommitMappingExpression>();
					return new ICommitMappingExpression[] { commitExp };
				});
			bugFixMapper
				.Map(Arg.Any<ICommitMappingExpression>())
				.Returns(x =>
				{
					bugfixExp = Substitute.For<IBugFixMappingExpression>();
					return new IBugFixMappingExpression[] { bugfixExp };
				});
			fileMapper
				.Map(Arg.Any<ICommitMappingExpression>())
				.Returns(x => 
				{
					fileExp = Substitute.For<ICodeFileMappingExpression>();
					return new ICodeFileMappingExpression[] { fileExp };
				});

			mapper.RegisterMapper(commitMapper);
			mapper.RegisterMapper(bugFixMapper);
			mapper.RegisterMapper(fileMapper);

			mapper.MapRevision("1");

			commitMapper.Received(1)
				.Map(Arg.Any<IRepositoryMappingExpression>());
			bugFixMapper.Received(1)
				.Map(commitExp);
			fileMapper.Received(1)
				.Map(bugfixExp);
			fileMapper.Received(1)
				.Map(Arg.Any<ICommitMappingExpression>());
		}

		[Fact]
		public void Should_not_break_expression_chain_with_missed_expression()
		{
			ICommitMappingExpression commitExp = null;
			ICodeFileMappingExpression fileExp = null;

			commitMapper
				.Map(Arg.Any<IRepositoryMappingExpression>())
				.Returns(x =>
				{
					commitExp = Substitute.For<ICommitMappingExpression>();
					return new ICommitMappingExpression[] { commitExp };
				});
			bugFixMapper
				.Map(Arg.Any<ICommitMappingExpression>())
				.Returns(x =>
				{
					return new IBugFixMappingExpression[] {};
				});
			fileMapper
				.Map(Arg.Any<ICommitMappingExpression>())
				.Returns(x =>
				{
					fileExp = Substitute.For<ICodeFileMappingExpression>();
					return new ICodeFileMappingExpression[] { fileExp };
				});

			mapper.RegisterMapper(commitMapper);
			mapper.RegisterMapper(bugFixMapper);
			mapper.RegisterMapper(fileMapper);

			mapper.MapRevision("1");

			fileMapper.Received(1)
				.Map(commitExp);
		}

		[Fact]
		public void Should_not_keep_expressions_between_sessions()
		{
			CommitMappingExpression commitExp = data.UsingSession(
				s => s.MappingDSL().AddCommit("1"));
			commitMapper
				.Map(Arg.Any<RepositoryMappingExpression>())
				.Returns(new CommitMappingExpression[] { commitExp });
			
			mapper.RegisterMapper(commitMapper);
			mapper.MapRevision("1");
			mapper.MapRevision("1");

			commitMapper.Received(2)
				.Map(Arg.Any<RepositoryMappingExpression>());
		}
		[Fact]
		public void Should_map_until_revision_limit()
		{
			List<string> revisions = new List<string>();

			vcsData
				.GetRevisionByNumber(Arg.Any<int>())
				.Returns(x => x[0].ToString());
				
			mapper.OnMapRevision += (r) => revisions.Add(r);
			settings.RevisionLimit = 5;
			var result = mapper.MapRevisions();

			result.Should().Be(VcsDataMapper.MappingResult.SUCCESS);
			Assert.Equal(new string[] { "1", "2", "3", "4", "5" }, revisions);
		}
		[Fact]
		public void Should_stop_if_no_more_revisions()
		{
			List<string> revisions = new List<string>();

			vcsData
				.GetRevisionByNumber(Arg.Any<int>())
				.Returns(x => (int)x[0] == 6 ? null : x[0].ToString());
			
			mapper.OnMapRevision += (r) => revisions.Add(r);
			var result = mapper.MapRevisions();

			result.Should().Be(VcsDataMapper.MappingResult.SUCCESS);
			Assert.Equal(new string[] { "1", "2", "3", "4", "5" }, revisions);
		}
		[Fact]
		public void Should_stop_if_all_revisions_already_mapped()
		{
			List<string> revisions = new List<string>();

			vcsData
				.GetRevisionByNumber(Arg.Any<int>())
				.Returns(x => null);

			mapper.OnMapRevision += (r) => revisions.Add(r);
			var result = mapper.MapRevisions();

			result.Should().Be(VcsDataMapper.MappingResult.SUCCESS);
			revisions.Count.Should().Be(0);
		}
		[Fact]
		public void Should_stop_for_canceled_token()
		{
			List<string> revisions = new List<string>();

			vcsData
				.GetRevisionByNumber(Arg.Any<int>())
				.Returns(x => (int)x[0] == 6 ? null : x[0].ToString());

			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();

				mapper.OnMapRevision += (r) => revisions.Add(r);
				var result = mapper.MapRevisions(cts.Token);

				result.Should().Be(VcsDataMapper.MappingResult.STOPPED);
				revisions.Count.Should().Be(0);
			}
		}
		[Fact]
		public void Should_truncate_last_mapped_commits()
		{
			data.UsingSession(s =>
				s.MappingDSL()
					.AddCommit("1")
				.Submit()
					.AddCommit("2")
				.Submit()
					.AddCommit("3")
				.Submit());
			
			mapper.Truncate(2);

			data.UsingSession(s =>
			{
				Assert.Equal(new string[] { "1", "2" },
					s.Get<Commit>().Select(x => x.Revision));
			});

			mapper.Truncate(1);

			data.UsingSession(s =>
			{
				Assert.Equal(new string[] { "1" },
					s.Get<Commit>().Select(x => x.Revision));
			});
		}
		[Fact]
		public void Should_truncate_linked_data_with_commits()
		{
			data.UsingSession(s =>
				s.MappingDSL()
					.AddCommit("1").OnBranch("1").AuthorIs("alan")
						.File("file1").Added().Code(100)
				.Submit()
					.AddCommit("2").OnBranch("11").AuthorIs("bob").IsBugFix()
						.File("file1").Modified().Code(-10)
						.File("file2").Added().Code(20)
				.Submit()
					.AddCommit("3").OnBranch("111").AuthorIs("ivan")
						.File("file2").Modified().Code(20)
				.Submit());
			
			mapper.Truncate(2);

			data.UsingSession(s =>
			{
				Assert.Equal(new string[] { "alan", "bob" },
					s.Get<Author>().Select(x => x.Name));
				Assert.Equal(2, s.Get<Branch>().Count());
				Assert.Equal(1, s.Get<CommitAttribute>().Count());
				Assert.Equal(new string[] { "file1", "file2" },
					s.Get<CodeFile>().Select(f => f.Path));
				Assert.Equal(3, s.Get<Modification>().Count());
				Assert.Equal(110, s.Get<CodeBlock>().Sum(x => x.Size));
			});

			mapper.Truncate(1);

			data.UsingSession(s =>
			{
				Assert.Equal(new string[] { "alan" },
					s.Get<Author>().Select(x => x.Name));
				Assert.Equal(1, s.Get<Branch>().Count());
				Assert.Equal(0, s.Get<CommitAttribute>().Count());
				Assert.Equal(new string[] { "file1" },
					s.Get<CodeFile>().Select(f => f.Path));
				Assert.Equal(1, s.Get<Modification>().Count());
				Assert.Equal(100, s.Get<CodeBlock>().Sum(x => x.Size));
			});
		}
		[Fact]
		public void Should_check_file_blame_vs_code_blocks()
		{
			data.UsingSession(s =>
				s.MappingDSL()
					.AddCommit("1").OnBranch("1").AuthorIs("alan")
						.File("file1").Added()
							.Code(100)
				.Submit());

			vcsData.Blame("1", "file1")
				.Returns(new TestBlame().AddLinesFromRevision("1", 100));

			Assert.True(mapper.CheckRevision("1", VcsDataMapper.CheckMode.ALL));

			data.UsingSession(s =>
				s.MappingDSL()
					.AddCommit("2").OnBranch("1").AuthorIs("alan")
						.File("file1").Modified()
							.Code(10)
							.Code(-10).ForCodeAddedInitiallyInRevision("1")
				.Submit());

			vcsData.Blame("2", "file1")
				.Returns(new TestBlame()
					.AddLinesFromRevision("1", 90)
					.AddLinesFromRevision("2", 10));

			Assert.True(mapper.CheckRevision("2", VcsDataMapper.CheckMode.ALL));

			vcsData.Blame("2", "file1")
				.Returns(new TestBlame()
					.AddLinesFromRevision("1", 89)
					.AddLinesFromRevision("2", 11));

			Assert.False(mapper.CheckRevision("2", VcsDataMapper.CheckMode.ALL));

			vcsData.Blame("2", "file1")
				.Returns(new TestBlame()
					.AddLinesFromRevision("1", 100));

			Assert.False(mapper.CheckRevision("2", VcsDataMapper.CheckMode.ALL));

			vcsData.Blame("2", "file1")
				.Returns(new TestBlame()
					.AddLinesFromRevision("0", 5)
					.AddLinesFromRevision("1", 90)
					.AddLinesFromRevision("2", 5));

			Assert.False(mapper.CheckRevision("2", VcsDataMapper.CheckMode.ALL));
		}
		[Fact]
		public void Should_check_code_blocks_for_removed_file()
		{
			data.UsingSession(s =>
				s.MappingDSL()
					.AddCommit("1").OnBranch("1").AuthorIs("alan")
						.File("file1").Added()
							.Code(100)
				.Submit()
					.AddCommit("2").OnBranch("1").AuthorIs("bob")
						.File("file1").Removed()
							.RemoveCode()
				.Submit());

			vcsData.Blame("2", "file1")
				.Returns((IBlame)null);

			Assert.True(mapper.CheckFile("2", "file1"));
		}
		[Fact]
		public void Should_truncate_unsuccessfully_mapped_revision()
		{
			commitMapper
				.Map(Arg.Any<RepositoryMappingExpression>())
				.Returns(x =>
				{
					data.UsingSession(s =>
						s.MappingDSL()
							.AddCommit("1").OnBranch("1").AuthorIs("alan")
								.File("file1").Added()
									.Code(100)
						.Submit());

					return Enumerable.Empty<CommitMappingExpression>();
				});
			mapper.RegisterMapper(commitMapper);

			vcsData.Blame("1", "file1")
				.Returns(new TestBlame().AddLinesFromRevision("1", 99));
			vcsData
				.GetRevisionByNumber(Arg.Any<int>())
				.Returns(x => x[0].ToString());

			settings.RevisionLimit = 1;
			settings.CheckMode = VcsDataMapper.CheckMode.ALL;
			var result = mapper.MapRevisions();

			result.Should().Be(VcsDataMapper.MappingResult.ERROR);
			data.UsingSession(s =>
			{
				Assert.Equal(0, s.Get<Commit>().Count());
			});
		}

		[Fact]
		public void Should_not_return_lost_split_on_empty_db()
		{
			mapper.GetFirstLostSplit()
				.Should().BeNull();

			vcsData.GetSplitRevisionsTillRevision(Arg.Any<string>())
				.DidNotReceiveWithAnyArgs();
		}
		[Theory]
		[InlineData(new string[] { "1", "3" }, null)]
		[InlineData(new string[] { "1", "3", "4" }, "4")]
		[InlineData(new string[] { "1", "3", "4", "7" }, "4")]
		[InlineData(new string[] { "1", "3", "7" }, "7")]
		public void Should_return_first_lost_split(
			string[] repoSplitRevisions, string resultRevision)
		{
			vcsData.GetSplitRevisionsTillRevision(Arg.Any<string>())
				.Returns(repoSplitRevisions);
			data.UsingSession(s =>
				s.MappingDSL()
					.AddCommit("1").IsSplit()
				.Submit()
					.AddCommit("2")
				.Submit()
					.AddCommit("3").IsSplit()
				.Submit()
					.AddCommit("4").IsMerge()
				.Submit()
					.AddCommit("5")
				.Submit()
					.AddCommit("6")
				.Submit()
					.AddCommit("7").IsMerge()
				.Submit()
			);

			mapper.GetFirstLostSplit()?.Revision
				.Should().Be(resultRevision);
		}
	}
}
