using System;
using Xunit;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.DSL.Selection.Metrics
{
	public class LocTest : BaseRepositoryTest
	{
		[Fact]
		public void Should_be_zero_for_empty_code()
		{
			Assert.Equal(0, selectionDSL
				.CodeBlocks().CalculateLOC());
		}
		[Fact]
		public void Loc_is_added_code_without_deleted_code()
		{
			mappingDSL
				.AddCommit("1")
					.File("file1").Added()
						.Code(100)
			.Submit()
				.AddCommit("2")
					.File("file1").Modified()
						.Code(-10).ForCodeAddedInitiallyInRevision("1")
						.Code(+20)
			.Submit();

			Assert.Equal(110, selectionDSL
				.CodeBlocks().CalculateLOC());
		}
	}
}
