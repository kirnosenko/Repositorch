using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using NSubstitute;
using Repositorch.Data.VersionControl;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.Mapping
{
	public class DataMapperTest : BaseRepositoryTest
	{
		private DataMapper mapper;
		private IVcsData vcsData;
		private CommitMapper commitMapper;
		private BugFixMapper bugFixMapper;
		
		public DataMapperTest()
		{
			vcsData = Substitute.For<IVcsData>();
			mapper = new DataMapper(vcsData);
			commitMapper = Substitute.For<CommitMapper>((IVcsData)null);
			bugFixMapper = Substitute.For<BugFixMapper>(null, null);
		}
		[Fact]
		public void Should_use_registered_mapper()
		{
			commitMapper
				.Map(Arg.Any<RepositoryMappingExpression>())
				.Returns(Enumerable.Empty<CommitMappingExpression>());
			
			mapper.RegisterMapper(commitMapper);
			mapper.Map(this, "1");

			commitMapper
				.Map(Arg.Is<RepositoryMappingExpression>(e => e != null))
				.Received(1);
		}
		[Fact]
		public void Should_set_revision_being_mapped()
		{
			commitMapper
				.Map(Arg.Any<RepositoryMappingExpression>())
				.Returns(Enumerable.Empty<CommitMappingExpression>());
				
			mapper.RegisterMapper(commitMapper);
			mapper.Map(this, "10");

			commitMapper
				.Map(Arg.Is<RepositoryMappingExpression>(e => e.Revision == "10"))
				.Received(1);
		}
		[Fact]
		public void Should_use_output_expression_for_registered_mapper()
		{
			CommitMappingExpression commitExp = mappingDSL.AddCommit("1");
			commitMapper
				.Map(Arg.Any<RepositoryMappingExpression>())
				.Returns(new CommitMappingExpression[] { commitExp });
			bugFixMapper
				.Map(Arg.Any<CommitMappingExpression>())
				.Returns(Enumerable.Empty<BugFixMappingExpression>());
			
			mapper.RegisterMapper(commitMapper);
			mapper.RegisterMapper(bugFixMapper);
			mapper.Map(this, "1");

			bugFixMapper
				.Map(Arg.Is(commitExp))
				.Received(1);
		}
		[Fact]
		public void Can_use_the_same_expression_for_several_mappers()
		{/*
			CommitMappingExpression commitExp = mappingDSL.AddCommit("1");

			commitMapperStub.Stub(x => x.Map(null))
				.IgnoreArguments()
				.Return(new CommitMappingExpression[] { commitExp });
			bugFixMapperStub.Stub(x => x.Map(null))
				.IgnoreArguments()
				.Return(Enumerable.Empty<BugFixMappingExpression>());
			fileMapperStub.Stub(x => x.Map(null))
				.IgnoreArguments()
				.Return(Enumerable.Empty<ProjectFileMappingExpression>());

			mapper.RegisterMapper(commitMapperStub);
			mapper.RegisterMapper(bugFixMapperStub);
			mapper.RegisterMapper(fileMapperStub);

			mapper.Map(data, "1");

			fileMapperStub.AssertWasCalled(x => x.Map(commitExp));*/
		}
		[Fact]
		public void Should_not_keep_expressions_between_sessions()
		{
			CommitMappingExpression commitExp = mappingDSL.AddCommit("1");
			commitMapper
				.Map(Arg.Any<RepositoryMappingExpression>())
				.Returns(new CommitMappingExpression[] { commitExp });
			
			mapper.RegisterMapper(commitMapper);
			mapper.Map(this, "1");
			mapper.Map(this, "1");

			commitMapper
				.Map(Arg.Any<RepositoryMappingExpression>())
				.Received(2);
		}
		[Fact]
		public void Should_map_until_last_revision()
		{/*
			List<string> revisions = new List<string>();

			vcsDataStub.Stub(x => x.RevisionByNumber(0))
				.IgnoreArguments()
				.Return("8");
			vcsDataStub.Stub(x => x.NextRevision("8"))
				.Return("9");
			vcsDataStub.Stub(x => x.NextRevision("9"))
				.Return("10");
			vcsDataStub.Stub(x => x.NextRevision("10"))
				.Return("11");

			mapper.StopRevision = "10";
			mapper.OnRevisionMapping += (r, n) => revisions.Add(r);

			mapper.Map(data);

			revisions.ToArray()
				.Should().Have.SameSequenceAs(new string[]
				{
					"8", "9", "10"
				});*/
		}
		[Fact]
		public void Should_stop_if_no_more_revisions()
		{/*
			List<string> revisions = new List<string>();

			vcsDataStub.Stub(x => x.RevisionByNumber(0))
				.IgnoreArguments()
				.Do((Func<int, string>)(n =>
				{
					return n.ToString();
				}));
			vcsDataStub.Stub(x => x.NextRevision(null))
				.IgnoreArguments()
				.Do((Func<string, string>)(r =>
				{
					if (r == "5")
					{
						return null;
					}
					return (Convert.ToInt32(r) + 1).ToString();
				}));

			mapper.StopRevision = null;
			mapper.OnRevisionMapping += (r, n) => revisions.Add(r);

			mapper.Map(data);

			revisions.ToArray()
				.Should().Have.SameSequenceAs(new string[]
				{
					"1", "2", "3", "4", "5"
				});*/
		}
		[Fact]
		public void Should_execute_partial_mapping_for_all_mapped_revisions_from_specified()
		{/*
			List<string> revisions = new List<string>();

			mappingDSL
				.AddCommit("1")
					.AddFile("file1").Modified()
			.Submit()
				.AddCommit("2")
					.AddFile("file2").Modified()
			.Submit()
				.AddCommit("3")
					.AddFile("file3").Modified()
			.Submit();
			vcsDataStub.Stub(x => x.NextRevision(null))
				.IgnoreArguments()
				.Do((Func<string, string>)(r =>
				{
					return (Convert.ToInt32(r) + 1).ToString();
				}));

			mapper.StartRevision = "2";
			mapper.OnRevisionMapping += (r, n) => revisions.Add(r);

			mapper.Map(data);

			revisions.ToArray()
				.Should().Have.SameSequenceAs(new string[]
				{
					"2", "3"
				});*/
		}
		[Fact]
		public void Should_create_schema_for_registered_expressions()
		{/*
			IDataStore dataStub = MockRepository.GenerateStub<IDataStore>();
			dataStub.Stub(x => x.CreateSchema(null))
				.IgnoreArguments()
				.Callback(new Func<Type[], bool>((t) =>
				{
					t.Should().Have.SameValuesAs(new Type[]
					{
						typeof(Commit), typeof(BugFix), typeof(ProjectFile)
					});
					return true;
				}));

			CommitMappingExpression commitExp = mappingDSL.AddCommit("1");
			commitMapperStub.Stub(x => x.Map(null))
				.IgnoreArguments()
				.Return(new CommitMappingExpression[] { commitExp });
			bugFixMapperStub.Stub(x => x.Map(null))
				.IgnoreArguments()
				.Return(Enumerable.Empty<BugFixMappingExpression>());
			fileMapperStub.Stub(x => x.Map(null))
				.IgnoreArguments()
				.Return(Enumerable.Empty<ProjectFileMappingExpression>());
			mapper.RegisterMapper(commitMapperStub);
			mapper.RegisterMapper(bugFixMapperStub);
			mapper.RegisterMapper(fileMapperStub);
			mapper.CreateDataBase = true;

			mapper.Map(data, "1");*/
		}
		[Fact]
		public void Can_replace_mappers()
		{
			commitMapper
				.Map(Arg.Is<RepositoryMappingExpression>(e => e != null))
				.Returns(Enumerable.Empty<CommitMappingExpression>());

			CommitMapper commitMapper2 = Substitute.For<CommitMapper>((IVcsData)null);
			mapper.RegisterMapper(commitMapper2);
			mapper.RegisterMapper(commitMapper);

			mapper.Map(this, "1");

			commitMapper
				.Map(Arg.Any<RepositoryMappingExpression>())
				.Received(1);
			commitMapper
				.Map(Arg.Any<RepositoryMappingExpression>())
				.DidNotReceive();
		}
	}
}
