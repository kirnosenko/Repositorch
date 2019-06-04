using System;
using System.Linq;
using Xunit;

namespace Repositorch.Data.Entities.DSL.Mapping
{
	public class BranchMappingExpressionTest : BaseRepositoryTest
	{
		[Fact]
		public void Should_add_branch_or_use_the_existent()
		{
			mappingDSL
				.AddCommit("1").OnBranch(0b0001)
			.Submit()
				.AddCommit("2").OnBranch(0b0001)
			.Submit()
				.AddCommit("3").OnBranch(0b0011)
			.Submit()
				.AddCommit("4").OnBranch(0b0101)
			.Submit();

			Assert.Equal(3, Get<Branch>().Count());
			Assert.Equal(new uint[] { 0b0001, 0b0001, 0b0011, 0b0101 }, 
				from c in Get<Commit>()
				join b in Get<Branch>() on c.BranchId equals b.Id
				select b.Mask);
		}
	}
}
