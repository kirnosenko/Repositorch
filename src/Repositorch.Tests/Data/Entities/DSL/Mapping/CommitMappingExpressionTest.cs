using System;
using System.Linq;
using Xunit;

namespace Repositorch.Data.Entities.DSL.Mapping
{
	public class CommitMappingExpressionTest : BaseRepositoryTest
	{
		[Fact]
		public void Should_add_commit()
		{
			mappingDSL
				.AddCommit("1")
					.By("alan")
					.At(DateTime.Today)
					.WithMessage("log")
			.Submit();

			Assert.Equal(1, Get<Commit>().Count());
			var c = Get<Commit>().Single();
			Assert.Equal("1", c.Revision);
			Assert.Equal("alan", c.Author);
			Assert.Equal(DateTime.Today, c.Date);
			Assert.Equal("log", c.Message);
		}
		[Fact]
		public void Can_use_existent_commit()
		{
			mappingDSL
				.AddCommit("1")
			.Submit()
				.Commit("1").By("alan")
			.Submit();

			Assert.Equal(1, Get<Commit>().Count());
			var c = Get<Commit>().Single();
			Assert.Equal("1", c.Revision);
			Assert.Equal("alan", c.Author);
		}
		[Fact]
		public void Should_add_commit_with_incremental_order_number()
		{
			mappingDSL
				.AddCommit("1")
			.Submit()
				.AddCommit("2")
			.Submit()
				.AddCommit("3")
			.Submit();

			Assert.Equal(
				new int[] { 1, 2, 3 },
				Get<Commit>().Select(c => c.OrderedNumber));
		}
	}
}
