using System;
using System.Linq;
using Xunit;
using NSubstitute;
using Repositorch.Data.Entities.DSL.Mapping;
using Repositorch.Data.VersionControl;

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
		public void Should_map_all_files_touched_on_merged_branches()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
					.File("file2").Added()
					.File("file3").Added()
			.Submit()
				.AddCommit("2").OnBranch("1")
					.File("file1").Modified()
			.Submit()
				.AddCommit("3").OnBranch("11")
					.File("file2").Modified()
			.Submit()
				.AddCommit("4").OnBranch("101")
					.File("file3").Modified()
			.Submit();

			log.ParentRevisionsAre("3", "4");

			var expressions = mapper.Map(
				mappingDSL.AddCommit("10").OnBranch("111")
			);

			Assert.Equal(new string[] { "file2", "file3" },
				expressions.Select(x => x.CurrentEntity<CodeFile>().Path));
		}
		[Fact]
		public void Should_map_removed_on_branch_file_when_it_still_exists_on_another_branch()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
					.File("file2").Added()
			.Submit()
				.AddCommit("2").OnBranch("11")
					.File("file1").Modified()
			.Submit()
				.AddCommit("3").OnBranch("101")
					.File("file1").Removed()
					.File("file2").Removed()
			.Submit();

			log.ParentRevisionsAre("2", "3");
			log.FileAdded("file1");

			var expressions = mapper.Map(
				mappingDSL.AddCommit("10").OnBranch("111")
			);

			Assert.Equal(new string[] { "file1" },
				expressions.Select(x => x.CurrentEntity<CodeFile>().Path));
		}
		[Fact]
		public void Should_map_files_in_merge_that_were_not_touched_on_merged_branches()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
					.File("file2").Added()
					.File("file3").Added()
			.Submit()
				.AddCommit("2").OnBranch("11")
					.File("file1").Modified()
			.Submit()
				.AddCommit("3").OnBranch("101")
					.File("file1").Modified()
			.Submit();

			log.ParentRevisionsAre("2", "3");
			log.FileModified("file1");
			log.FileModified("file2");
			log.FileRemoved("file3");
			log.FileAdded("file4");

			var expressions = mapper.Map(
				mappingDSL.AddCommit("10").OnBranch("111")
			);

			Assert.Equal(new string[] { "file1", "file2", "file3", "file4" },
				expressions.Select(x => x.CurrentEntity<CodeFile>().Path));
		}
		[Fact]
		public void Should_map_modified_in_merge_file_only_if_it_was_touched_on_different_parent_branches()
		{
			mapper.FastMergeProcessing = true;

			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
					.File("file2").Added()
					.File("file3").Added()
					.File("file4").Added()
			.Submit()
				.AddCommit("2").OnBranch("11")
					.File("file1").Modified()
					.File("file2").Modified()   // ---------
			.Submit()                           //         |
				.AddCommit("3").OnBranch("101") //         |
					.File("file2").Modified()   // <-------- cherry pick changes, so no diff in log
					.File("file3").Modified()
					.File("file4").Modified()   // ---------
			.Submit()                           //         |
				.AddCommit("4").OnBranch("101") //         |
					.File("file4").Modified()   // <-------- reverse changes, so no diff in log
			.Submit();

			log.ParentRevisionsAre("2", "4");
			log.FileModified("file1");
			log.FileModified("file3");

			var expressions = mapper.Map(
				mappingDSL.AddCommit("10").OnBranch("111")
			);

			Assert.Equal(new string[] { "file2" }, expressions
				.Select(x => x.CurrentEntity<CodeFile>().Path));
		}
		[Fact]
		public void Should_indentify_files_untouched_on_different_parent_branches_in_case_of_many_parent_branches()
		{
			mapper.FastMergeProcessing = true;

			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
					.File("file2").Added()
					.File("file3").Added()
					.File("file4").Added()
					.File("file5").Added()
					.File("file6").Added()
			.Submit()
				.AddCommit("2").OnBranch("11")
					.File("file2").Modified()
					.File("file6").Modified()
			.Submit()
				.AddCommit("3").OnBranch("101")
					.File("file3").Modified()
					.File("file6").Modified()
			.Submit()
				.AddCommit("4").OnBranch("11")
					.File("file2").Modified()
					.File("file6").Modified()
			.Submit()
				.AddCommit("5").OnBranch("1011")
					.File("file4").Modified()
					.File("file6").Modified()
			.Submit()
				.AddCommit("6").OnBranch("10101")
					.File("file5").Modified()
					.File("file6").Modified()
			.Submit();

			log.ParentRevisionsAre("4", "5", "6");
			log.FileModified("file2");
			log.FileModified("file3");
			log.FileModified("file4");
			log.FileModified("file5");
			log.FileModified("file6");
			var expressions = mapper.Map(
				mappingDSL.AddCommit("10").OnBranch("11111")
			);

			Assert.Equal(new string[] { "file6" }, expressions
				.Select(x => x.CurrentEntity<CodeFile>().Path));
		}
		[Fact]
		public void Should_not_duplicate_the_same_file_touched_multiple_times()
		{
			mappingDSL
				.AddCommit("1").OnBranch("1")
					.File("file1").Added()
					.File("file2").Added()
					.File("file3").Added()
			.Submit()
				.AddCommit("2").OnBranch("11")
					.File("file2").Modified()
			.Submit()
				.AddCommit("3").OnBranch("101")
					.File("file3").Modified()
			.Submit()
				.AddCommit("4").OnBranch("11")
					.File("file2").Modified()
			.Submit()
				.AddCommit("5").OnBranch("101")
					.File("file3").Modified()
			.Submit();

			log.ParentRevisionsAre("4", "5");

			var expressions = mapper.Map(
				mappingDSL.AddCommit("10").OnBranch("111")
			);

			Assert.Equal(new string[] { "file2", "file3" },
				expressions.Select(x => x.CurrentEntity<CodeFile>().Path));
		}
		[Fact]
		public void Should_ignore_binary_files()
		{
			log.FileAdded("file1", TouchedFile.ContentType.TEXT);
			log.FileAdded("file2", TouchedFile.ContentType.BINARY);

			mapper.Map(
				mappingDSL.AddCommit("10")
			);
			SubmitChanges();

			Assert.Equal(1, Get<CodeFile>().Count());
			Assert.Equal("file1", Get<CodeFile>().Single().Path);
		}
		[Fact]
		public void Should_not_ignore_binary_files_if_they_were_added_as_non_binary()
		{
			mappingDSL
				.AddCommit("9")
					.File("file1").Added()
			.Submit();

			log.FileModified("file1", TouchedFile.ContentType.BINARY);
			
			var exp = mapper.Map(
				mappingDSL.AddCommit("10")
			);
			SubmitChanges();

			Assert.True(exp.Count() == 1);
		}
		[Fact]
		public void Should_ignore_non_binary_files_if_they_were_not_added_as_binary()
		{
			log.FileModified("file1", TouchedFile.ContentType.TEXT);

			var exp = mapper.Map(
				mappingDSL.AddCommit("10")
			);
			SubmitChanges();

			Assert.True(exp.Count() == 0);
		}
	}
}
