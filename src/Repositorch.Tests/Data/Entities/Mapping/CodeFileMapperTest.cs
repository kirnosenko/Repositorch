using System;
using System.Linq;
using Xunit;
using NSubstitute;
using Repositorch.Data.Entities.DSL.Mapping;
using Repositorch.Data.Entities.DSL.Selection;

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
		[Fact]
		public void Should_map_added_or_removed_in_merge_file_only_if_it_has_different_history_state()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
					.File("file2").Added()
			.Submit()
				.AddCommit("2").OnBranch("11")
					.File("file1").Modified()
					.File("file2").Removed()
					.File("file3").Added()
			.Submit()
				.AddCommit("3").OnBranch("101")
					.File("file1").Removed()
					.File("file2").Modified()
					.File("file4").Added()
			.Submit();

			vcsData.GetRevisionParents("10")
				.Returns(new string[] { "2", "3" });
			log.FileAdded("file1");
			log.FileRemoved("file2");
			log.FileAdded("file3");
			log.FileAdded("file4");

			var expressions = mapper.Map(
				mappingDSL.AddCommit("10").OnBranch("111")
			);

			Assert.Equal(new string[] { "file1", "file2" }, expressions
				.Select(x => x.CurrentEntity<CodeFile>().Path));
		}
	}
}
