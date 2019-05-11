using System;
using System.Linq;
using Xunit;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.DSL.Selection
{
	public class AuthorSelectionExpressionTest : BaseRepositoryTest
	{
		public AuthorSelectionExpressionTest()
		{
			mappingDSL
				.AddCommit("1").AuthorIs("alan")
			.Submit()
				.AddCommit("2").AuthorIs("bob").HasEmail("bob@bob.com")
			.Submit()
				.AddCommit("3").AuthorIs("alan")
			.Submit()
				.AddCommit("4").AuthorIs("alan")
			.Submit();
		}
		[Fact]
		public void Should_select_authors_of_commits()
		{
			Assert.Equal(
				new string[] { "alan" },
				selectionDSL
					.Commits().RevisionIs("1")
					.Authors().OfCommits().Select(a => a.Name));
			Assert.Equal(
				new string[] { "bob", "alan" },
				selectionDSL
					.Commits().AfterRevision("1")
					.Authors().OfCommits().Select(a => a.Name));
		}
		[Fact]
		public void Should_select_authors_by_name()
		{
			Assert.Equal(
				new string[] { "alan" },
				selectionDSL
					.Authors().WithName("alan").Select(a => a.Name));
		}
		[Fact]
		public void Should_select_authors_by_email()
		{
			Assert.Equal(
				new string[] { "bob" },
				selectionDSL
					.Authors().WithEmail("bob@bob.com").Select(a => a.Name));
		}
		[Fact]
		public void Should_select_commits_by_author()
		{
			Assert.Equal(
				new string[] { "2" },
				selectionDSL
					.Commits().AuthorIs("bob").Select(c => c.Revision));
			Assert.Equal(
				new string[] { "1", "3", "4" },
				selectionDSL
					.Commits().AuthorIs("alan").Select(c => c.Revision));
			Assert.Equal(
				new string[] { "1", "3", "4" },
				selectionDSL
					.Commits().AuthorIsNot("bob").Select(c => c.Revision));
			Assert.Equal(
				4,
				selectionDSL
					.Commits().AuthorsAre("alan", "bob", "ivan").Count());
		}
	}
}
