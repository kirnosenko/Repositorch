using System;
using System.Linq;
using Xunit;
using NSubstitute;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.Mapping
{
	public class CodeFileMapperTest : BaseMapperTest
	{
		private CodeFileMapper mapper;
		private TestLog log;

		public CodeFileMapperTest()
		{
			log = new TestLog("10", null, null, DateTime.Today, null);
			vcsData
				.Log(Arg.Is<string>("10"))
				.Returns(log);
			mapper = new CodeFileMapper(vcsData);
		}
		[Fact]
		public void Should_map_file()
		{
			log.FileAdded("file1");

			mapper.Map(
				mappingDSL.AddCommit("10")
			);
			SubmitChanges();

			Assert.Equal(1, Get<CodeFile>().Count());
			var f = Get<CodeFile>().Single();
			Assert.Equal("file1", f.Path);
		}
		[Fact]
		public void Should_use_path_selectors()
		{
			log.FileAdded("file1.123");
			log.FileAdded("file2.555");
			log.FileAdded("file3.123");
			log.FileAdded("file4.555");

			var selector = Substitute.For<IPathSelector>();
			selector
				.IsSelected(Arg.Any<string>())
				.Returns(x => (x[0] as string).EndsWith(".555"));
			mapper.PathSelectors = new IPathSelector[] { selector };

			mapper.Map(
				mappingDSL.AddCommit("10")
			);
			SubmitChanges();

			Assert.Equal(new string[] { "file2.555", "file4.555" }, Get<CodeFile>()
				.Select(x => x.Path));
		}
		[Fact]
		public void Should_use_all_path_selectors()
		{
			log.FileAdded("/dir1/file1.123");
			log.FileAdded("/dir1/file2.555");
			log.FileAdded("/dir2/file3.555");

			var selector1 = Substitute.For<IPathSelector>();
			selector1
				.IsSelected(Arg.Any<string>())
				.Returns(x => (x[0] as string).EndsWith(".555"));
			var selector2 = Substitute.For<IPathSelector>();
			selector2
				.IsSelected(Arg.Any<string>())
				.Returns(x => (x[0] as string).StartsWith("/dir1"));
			mapper.PathSelectors = new IPathSelector[] { selector1, selector2 };
			
			mapper.Map(
				mappingDSL.AddCommit("10")
			);
			SubmitChanges();

			Assert.Equal(1, Get<CodeFile>().Count());
			Assert.Equal("/dir1/file2.555", Get<CodeFile>()
				.Single().Path);
		}
		[Fact]
		public void Should_map_ranamed_file_during_partial_mapping()
		{
			mappingDSL
				.AddCommit("9")
					.File("file1.c").Added()
			.Submit()
				.AddCommit("10")
					.File("file1.c").Removed()
			.Submit()
				.AddCommit("11")
					.File("file3.c").Added()
			.Submit();
			
			log.FileRenamed("file1.cpp", "file1.c");

			var selector = Substitute.For<IPathSelector>();
			selector
				.IsSelected(Arg.Any<string>())
				.Returns(x => (x[0] as string).EndsWith(".cpp"));
			mapper.PathSelectors = new IPathSelector[] { selector };

			mapper.Map(
				mappingDSL.Commit("10")
			);
			SubmitChanges();

			Assert.Equal(1, Get<CodeFile>()
				.Where(x => x.Path == "file1.cpp").Count());
		}
	}
}
