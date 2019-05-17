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
		private class BlameStub : Dictionary<int, string>, IBlame
		{
		}
		private CodeBlockMapper mapper;
		
		public CodeBlockMapperTest()
		{
			mapper = new CodeBlockMapper(vcsData);
		}
		[Fact]
		public void Should_map_added_lines()
		{
			var blame = new BlameStub()
			{
				{ 1, "abc" },
				{ 2, "abc" },
				{ 3, "abc" },
			};
			
			vcsData.Blame(Arg.Is<string>("abc"), Arg.Is<string>("file1"))
				.Returns(blame);

			mapper.Map(
				mappingDSL.AddCommit("abc")
					.AddFile("file1").Modified()
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
					.AddFile("file1").Modified()
						.Code(100)
				.Submit();
			mapper.Map(
				mappingDSL.AddCommit("abc")
					.File("file1").Delete().Modified()
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
					.AddFile("file1").Modified()
						.Code(10)
			.Submit()
				.AddCommit("ab")
					.File("file1").Modified()
						.Code(20)
			.Submit();

			var blame = new BlameStub();
			for (int i = 1; i <= 10; i++)
			{
				blame[i] = "abc";
			}
			for (int i = 11; i <= 25; i++)
			{
				blame[i] = "ab";
			}
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
				.AddCommit("a")
					.AddFile("file1").Modified()
						.Code(10)
			.Submit()
				.AddCommit("ab")
					.File("file1").Modified()
						.Code(5)
			.Submit();

			var blame = new BlameStub();
			for (int i = 1; i <= 10; i++)
			{
				blame[i] = "a";
			}
			for (int i = 11; i <= 15; i++)
			{
				blame[i] = "ab";
			}
			vcsData.Blame("abc", "file2")
				.Returns(blame);

			mapper.Map(
				mappingDSL.AddCommit("abc")
					.AddFile("file2").CopiedFrom("file1", "ab").Modified()
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
				.AddCommit("a")
					.AddFile("file1").Modified()
						.Code(10)
			.Submit();

			var blame = new BlameStub();
			for (int i = 1; i <= 10; i++)
			{
				blame[i] = "a";
			}
			for (int i = 11; i <= 15; i++)
			{
				blame[i] = "abc";
			}
			vcsData.Blame("abc", "file2")
				.Returns(blame);
			
			mapper.Map(
				mappingDSL.AddCommit("abc")
					.AddFile("file2").CopiedFrom("file1", "a").Modified()
			);
			SubmitChanges();

			Assert.Equal("abc", Get<CodeBlock>()
				.Single(cb => cb.Size == 5)
				.AddedInitiallyInCommit.Revision);
		}
	}
}
