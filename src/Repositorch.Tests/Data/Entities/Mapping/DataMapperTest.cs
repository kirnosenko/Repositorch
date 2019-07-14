using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using NSubstitute;
using Repositorch.Data.VersionControl;
using Repositorch.Data.Entities.EF;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.Mapping
{
	public class DataMapperTest : BaseMapperTest
	{
		private IDataStore data;
		private DataMapper mapper;
		private CommitMapper commitMapper;
		private BugFixMapper bugFixMapper;
		private CodeFileMapper fileMapper;
		
		public DataMapperTest()
		{
			data = new InMemoryDataStore(Guid.NewGuid().ToString());
			mapper = new DataMapper(data, vcsData);
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

			commitMapper
				.Received(1)
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

			commitMapper
				.Received(1)
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

			bugFixMapper
				.Received(1)
				.Map(Arg.Is(commitExp));
		}
		[Fact]
		public void Can_use_the_same_expression_for_several_mappers()
		{
			CommitMappingExpression commitExp = data.UsingSession(
				s => s.MappingDSL().AddCommit("1"));

			commitMapper
				.Map(Arg.Any<RepositoryMappingExpression>())
				.Returns(new CommitMappingExpression[] { commitExp });
			bugFixMapper
				.Map(Arg.Any<CommitMappingExpression>())
				.Returns(Enumerable.Empty<BugFixMappingExpression>());
			fileMapper
				.Map(Arg.Any<CommitMappingExpression>())
				.Returns(Enumerable.Empty<CodeFileMappingExpression>());

			mapper.RegisterMapper(commitMapper);
			mapper.RegisterMapper(bugFixMapper);
			mapper.RegisterMapper(fileMapper);

			mapper.MapRevision("1");

			fileMapper
				.Received(1)
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

			commitMapper
				.Received(2)
				.Map(Arg.Any<RepositoryMappingExpression>());
		}
		[Fact]
		public void Should_map_until_last_revision()
		{
			List<string> revisions = new List<string>();

			vcsData
				.GetRevisionByNumber(Arg.Any<int>())
				.Returns(x => x[0].ToString());
				
			mapper.OnMapRevision += (r) => revisions.Add(r);
			mapper.MapRevisions(stopRevision: "5");

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
			mapper.MapRevisions(stopRevision: null);

			Assert.Equal(new string[] { "1", "2", "3", "4", "5" }, revisions);
		}
		[Fact]
		public void Should_execute_partial_mapping_for_all_mapped_revisions_from_specified()
		{
			List<string> revisions = new List<string>();

			data.UsingSession(s =>
				s.MappingDSL()
					.AddCommit("1")
						.File("file1").Added()
				.Submit()
					.AddCommit("2")
						.File("file2").Added()
				.Submit()
					.AddCommit("3")
						.File("file3").Added()
				.Submit());
			vcsData
				.GetRevisionByNumber(Arg.Any<int>())
				.Returns(x => x[0].ToString());
				
			mapper.OnMapRevision += (r) => revisions.Add(r);
			mapper.MapRevisions(startRevision: "2");

			Assert.Equal(new string[] { "2", "3" }, revisions);
		}
		[Fact]
		public void Can_replace_mappers()
		{
			CommitMapper commitMapper2 = Substitute.For<CommitMapper>((IVcsData)null);
			mapper.RegisterMapper(commitMapper2);
			mapper.RegisterMapper(commitMapper);

			mapper.MapRevision("1");

			commitMapper
				.Received(1)
				.Map(Arg.Any<RepositoryMappingExpression>());
			commitMapper2
				.DidNotReceive()
				.Map(Arg.Any<RepositoryMappingExpression>());
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
					.AddCommit("1").OnBranch(0b001).AuthorIs("alan")
						.File("file1").Added().Code(100)
				.Submit()
					.AddCommit("2").OnBranch(0b011).AuthorIs("bob").IsBugFix()
						.File("file1").Modified().Code(-10)
						.File("file2").Added().Code(20)
				.Submit()
					.AddCommit("3").OnBranch(0b111).AuthorIs("ivan")
						.File("file2").Modified().Code(20)
				.Submit());
			
			mapper.Truncate(2);

			data.UsingSession(s =>
			{
				Assert.Equal(new string[] { "alan", "bob" },
					s.Get<Author>().Select(x => x.Name));
				Assert.Equal(2, s.Get<Branch>().Count());
				Assert.Equal(1, s.Get<BugFix>().Count());
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
				Assert.Equal(0, s.Get<BugFix>().Count());
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
					.AddCommit("1").OnBranch(0b001).AuthorIs("alan")
						.File("file1").Added()
							.Code(100)
				.Submit());

			vcsData.Blame("1", "file1")
				.Returns(new TestBlame().AddLinesFromRevision("1", 100));

			Assert.True(mapper.CheckRevision("1"));

			data.UsingSession(s =>
				s.MappingDSL()
					.AddCommit("2").OnBranch(0b001).AuthorIs("alan")
						.File("file1").Modified()
							.Code(10)
							.Code(-10).ForCodeAddedInitiallyInRevision("1")
				.Submit());

			vcsData.Blame("2", "file1")
				.Returns(new TestBlame()
					.AddLinesFromRevision("1", 90)
					.AddLinesFromRevision("2", 10));

			Assert.True(mapper.CheckRevision("2"));

			vcsData.Blame("2", "file1")
				.Returns(new TestBlame()
					.AddLinesFromRevision("1", 90)
					.AddLinesFromRevision("2", 11));

			Assert.False(mapper.CheckRevision("2"));
		}
		[Fact]
		public void Should_truncate_unsuccessfully_mapped_revision()
		{
			data.UsingSession(s =>
				s.MappingDSL()
					.AddCommit("1").OnBranch(0b001).AuthorIs("alan")
						.File("file1").Added()
							.Code(100)
				.Submit());

			vcsData.Blame("1", "file1")
				.Returns(new TestBlame().AddLinesFromRevision("1", 99));
			
			mapper.MapRevisions("1", "1", true);

			data.UsingSession(s =>
			{
				Assert.Equal(0, s.Get<Commit>().Count());
			});
		}
	}
}
