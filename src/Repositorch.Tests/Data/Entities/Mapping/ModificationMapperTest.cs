using System;
using System.Linq;
using Xunit;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.Mapping
{
	public class ModificationMapperTest : BaseMapperTest
	{
		private ModificationMapper mapper;

		public ModificationMapperTest()
		{
			mapper = new ModificationMapper(vcsData);
		}
		[Fact]
		public void Should_map_modifacation_for_modified_file()
		{
			mappingDSL
				.AddCommit("9")
					.AddFile("file1").Modified()
					.AddFile("file2").Modified()
			.Submit();
			
			CommitMappingExpression commitExp = mappingDSL.AddCommit("10");
			mapper.Map(
				commitExp.File("file2")
			);
			mapper.Map(
				commitExp.AddFile("file3")
			);
			SubmitChanges();

			Assert.Equal(new string[] { "file2", "file3" }, Get<Modification>()
				.Where(m => m.Commit.Revision == "10")
				.Select(m => m.File.Path));
		}
	}
}
