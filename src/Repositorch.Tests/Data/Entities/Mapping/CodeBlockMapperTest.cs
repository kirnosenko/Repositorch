using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using NSubstitute;
using Repositorch.Data.VersionControl;
using Repositorch.Data.Entities.DSL.Selection;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.Mapping
{
	public class CodeBlockMapperTest : BaseMapperTest
	{
		private CodeBlockMapper mapper;
		
		public CodeBlockMapperTest()
		{
			mapper = new CodeBlockMapper(vcsData);
		}
		[Fact]
		public void Should_map_added_lines()
		{
			var blame = new TestBlame()
				.AddLinesFromRevision("abc", 3);
			
			vcsData.Blame(Arg.Is<string>("abc"), Arg.Is<string>("file1"))
				.Returns(blame);

			mapper.Map(
				mappingDSL.AddCommit("abc")
					.File("file1").Added().Modified()
			);
			SubmitChanges();

			Assert.Equal(1, Get<CodeBlock>().Count());
			var cb = Get<CodeBlock>().Single();
			Assert.Equal(3, cb.Size);
			Assert.Equal("abc", cb.AddedInitiallyInCommit.Revision);
		}
		[Fact]
		public void Should_not_take_blame_for_deleted_file()
		{
			mappingDSL
				.AddCommit("ab")
					.File("file1").Added()
						.Code(100)
				.Submit();
			mapper.Map(
				mappingDSL.AddCommit("abc")
					.File("file1").Removed()
			);
			SubmitChanges();

			Assert.Equal(new double[] { 100, -100 }, Get<CodeBlock>()
				.Select(cb => cb.Size));
			vcsData.DidNotReceive().Blame(Arg.Any<string>(), Arg.Any<string>());
		}
		[Fact]
		public void Should_remove_code_that_no_more_exists()
		{
			mappingDSL
				.AddCommit("a")
					.File("file1").Added()
						.Code(10)
			.Submit()
				.AddCommit("ab")
					.File("file1").Modified()
						.Code(20)
			.Submit();

			var blame = new TestBlame()
				.AddLinesFromRevision("abc", 10)
				.AddLinesFromRevision("ab", 15);
			vcsData.Blame("abc", "file1")
				.Returns(blame);
			
			mapper.Map(
				mappingDSL.AddCommit("abc")
					.File("file1").Modified()
			);
			SubmitChanges();

			var code = Get<CodeBlock>()
				.Where(cb => cb.Modification.Commit.Revision == "abc");

			Assert.Equal(new double[] { 10, -10, -5 }, code
				.Select(cb => cb.Size));
			Assert.Equal(new double[] { 10, 20 }, code
				.Where(cb => cb.Size < 0)
				.Select(cb => cb.TargetCodeBlock.Size));
		}
		[Fact]
		public void Should_map_all_code_as_is_for_copied_file()
		{
			mappingDSL
				.AddCommit("a").OnBranch(1)
					.File("file1").Added()
						.Code(10)
			.Submit()
				.AddCommit("ab").OnBranch(1)
					.File("file1").Modified()
						.Code(5)
			.Submit();

			var blame = new TestBlame()
				.AddLinesFromRevision("a", 10)
				.AddLinesFromRevision("ab", 5);
			vcsData.Blame("abc", "file2")
				.Returns(blame);

			mapper.Map(
				mappingDSL.AddCommit("abc").OnBranch(1)
					.File("file2").CopiedFrom("file1", "ab")
			);
			SubmitChanges();

			var code = Get<CodeBlock>()
				.Where(cb => cb.Modification.Commit.Revision == "abc");

			Assert.Equal(new double[] { 10, 5 }, code
				.Select(cb => cb.Size));
			Assert.Equal(new string[] { "a", "ab" }, code
				.Select(cb => cb.AddedInitiallyInCommit.Revision));
		}
		[Fact]
		public void Should_map_new_code_in_copied_file_as_new()
		{
			mappingDSL
				.AddCommit("a").OnBranch(1)
					.File("file1").Added()
						.Code(10)
			.Submit();

			var blame = new TestBlame()
				.AddLinesFromRevision("a", 10)
				.AddLinesFromRevision("abc", 5);
			vcsData.Blame("abc", "file2")
				.Returns(blame);
			
			mapper.Map(
				mappingDSL.AddCommit("abc").OnBranch(1)
					.File("file2").CopiedFrom("file1", "a")
			);
			SubmitChanges();

			Assert.Equal("abc", Get<CodeBlock>()
				.Single(cb => cb.Size == 5)
				.AddedInitiallyInCommit.Revision);
		}
	}
}
