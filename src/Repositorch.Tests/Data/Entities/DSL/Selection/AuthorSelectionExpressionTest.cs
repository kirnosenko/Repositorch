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
			.Submit()
				.AddCommit("5").AuthorIs("chris")
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
				new string[] { "bob", "alan", "chris" },
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
					.Authors().NameIs("alan").Select(a => a.Name));
		}
		[Fact]
		public void Should_select_authors_by_name_list()
		{
			Assert.Equal(
				new string[] { "alan", "bob" },
				selectionDSL
					.Authors().NameIsOneOf("alan", "bob", "ivan").Select(a => a.Name));
		}
		[Fact]
		public void Should_select_authors_by_email()
		{
			Assert.Equal(
				new string[] { "bob" },
				selectionDSL
					.Authors().EmailIs("bob@bob.com").Select(a => a.Name));
		}
		[Fact]
		public void Should_select_commits_by_author()
		{
			Assert.Equal(
				new string[] { "2" },
				selectionDSL
					.Authors().NameIs("bob")
					.Commits().ByAuthors().Select(c => c.Revision));
			Assert.Equal(
				new string[] { "1", "3", "4" },
				selectionDSL
					.Authors().NameIs("alan")
					.Commits().ByAuthors().Select(c => c.Revision));
			Assert.Equal(
				new string[] { "1", "3", "4", "5" },
				selectionDSL
					.Authors().NameIs("bob")
					.Commits().NotByAuthors().Select(c => c.Revision));
		}
	}
}
