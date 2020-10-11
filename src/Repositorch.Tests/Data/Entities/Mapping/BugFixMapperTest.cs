using System;
using System.Linq;
using Xunit;
using FluentAssertions;
using NSubstitute;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.Mapping
{
	public class BugFixMapperTest : BaseMapperTest
	{
		private BugFixMapper mapper;
		private IBugFixDetector bugFixDetector;

		public BugFixMapperTest()
		{
			bugFixDetector = Substitute.For<IBugFixDetector>();
			mapper = new BugFixMapper(vcsData, bugFixDetector);
		}

		[Fact]
		public void Should_add_bugfix_for_fix_commit()
		{
			bugFixDetector.IsBugFix(Arg.Any<Commit>())
				.Returns(true);

			mapper.Map(
				mappingDSL.AddCommit("1")
			);
			SubmitChanges();

			Get<CommitAttribute>().Count()
				.Should().Be(1);
			Get<CommitAttribute>().Single().Commit.Revision
				.Should().Be("1");
		}
		[Fact]
		public void Should_not_add_bugfix_for_non_fix_commit()
		{
			bugFixDetector.IsBugFix(Arg.Any<Commit>())
				.Returns(false);

			mapper.Map(
				mappingDSL.AddCommit("1")
			);
			SubmitChanges();

			Get<CommitAttribute>().Count()
				.Should().Be(0);
		}
	}
}
