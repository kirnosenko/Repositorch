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
	public class DataMapperTest
	{
		private IDataStore data;
		private IVcsData vcsData;
		private DataMapper mapper;
		private CommitMapper commitMapper;
		private BugFixMapper bugFixMapper;
		private CodeFileMapper fileMapper;
		
		public DataMapperTest()
		{
			data = new InMemoryDataStore(Guid.NewGuid().ToString());
			vcsData = Substitute.For<IVcsData>();
			mapper = new DataMapper(vcsData);
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
			mapper.Map(data, "1");

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
			mapper.Map(data, "10");

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
			mapper.Map(data, "1");

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

			mapper.Map(data, "1");

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
			mapper.Map(data, "1");
			mapper.Map(data, "1");

			commitMapper
				.Received(2)
				.Map(Arg.Any<RepositoryMappingExpression>());
		}
		[Fact]
		public void Should_map_until_last_revision()
		{
			List<string> revisions = new List<string>();

			vcsData
				.RevisionByNumber(Arg.Any<int>())
				.Returns(x => x[0].ToString());
				
			mapper.StopRevision = "5";
			mapper.OnRevisionMapping += (r, n) => revisions.Add(r);

			mapper.Map(data);

			Assert.Equal(new string[] { "1", "2", "3", "4", "5" }, revisions);
		}
		[Fact]
		public void Should_stop_if_no_more_revisions()
		{
			List<string> revisions = new List<string>();

			vcsData
				.RevisionByNumber(Arg.Any<int>())
				.Returns(x => (int)x[0] == 6 ? null : x[0].ToString());
			
			mapper.StopRevision = null;
			mapper.OnRevisionMapping += (r, n) => revisions.Add(r);

			mapper.Map(data);

			Assert.Equal(new string[] { "1", "2", "3", "4", "5" }, revisions);
		}
		[Fact]
		public void Should_execute_partial_mapping_for_all_mapped_revisions_from_specified()
		{
			List<string> revisions = new List<string>();

			data.UsingSession(s =>
				s.MappingDSL().AddCommit("1")
					.AddFile("file1").Modified()
				.Submit()
					.AddCommit("2")
						.AddFile("file2").Modified()
				.Submit()
					.AddCommit("3")
						.AddFile("file3").Modified()
				.Submit());
			vcsData
				.RevisionByNumber(Arg.Any<int>())
				.Returns(x => x[0].ToString());
				
			mapper.StartRevision = "2";
			mapper.OnRevisionMapping += (r, n) => revisions.Add(r);

			mapper.Map(data);

			Assert.Equal(new string[] { "2", "3" }, revisions);
		}
		[Fact]
		public void Can_replace_mappers()
		{
			CommitMapper commitMapper2 = Substitute.For<CommitMapper>((IVcsData)null);
			mapper.RegisterMapper(commitMapper2);
			mapper.RegisterMapper(commitMapper);

			mapper.Map(data, "1");

			commitMapper
				.Received(1)
				.Map(Arg.Any<RepositoryMappingExpression>());
			commitMapper2
				.DidNotReceive()
				.Map(Arg.Any<RepositoryMappingExpression>());
		}
	}
}
