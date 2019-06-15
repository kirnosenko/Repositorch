using System;
using System.Linq;
using Xunit;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.DSL.Selection
{
	public class EntitySelectionExpressionTest : BaseRepositoryTest
	{
		[Fact]
		public void Should_combine_reselection_with_other_filters()
		{
			mappingDSL
				.AddCommit("1")
			.Submit()
				.AddCommit("2")
			.Submit()
				.AddCommit("3")
			.Submit()
				.AddCommit("4")
			.Submit()
				.AddCommit("5")
			.Submit();

			Assert.Equal(new string[] { "2", "4" }, selectionDSL
				.Commits()
				.BeforeNumber(5)
				.Reselect(commits =>
					from c in commits 
					where c.Revision != "3"
					select c
				)
				.AfterNumber(1)
				.Select(c => c.Revision));
		}
		[Fact]
		public void Should_keep_selection_for_fixed_expression()
		{
			mappingDSL
				.AddCommit("1")
					.AddFile("file1").Modified()
						.Code(100)
			.Submit()
				.AddCommit("2")
					.File("file1").Modified()
						.Code(-5)
						.Code(20)
			.Submit();

			var commit2code = selectionDSL
				.Commits().RevisionIs("2")
				.Modifications().InCommits()
				.CodeBlocks().InModifications()
				.Fixed();

			Assert.Equal(2, commit2code.Count());
			Assert.Equal(1, commit2code.Added().Count());
			Assert.Equal(1, commit2code.Deleted().Count());
			Assert.Equal(1, commit2code.Reselect(e => e.Added()).Count());
			Assert.Equal(2, commit2code.Added().Again().Count());
			Assert.Equal(2, commit2code.Count());
		}
		[Fact]
		public void Can_restore_selection_from_previous_expression_of_the_same_type()
		{
			mappingDSL
				.AddCommit("1")
			.Submit()
				.AddCommit("2")
			.Submit()
				.AddCommit("3")
			.Submit()
				.AddCommit("4")
			.Submit();

			Assert.Equal(4, selectionDSL
				.Commits().AfterNumber(2)
				.Modifications().InCommits()
				.Commits().Count());
			
			Assert.Equal(2, selectionDSL
				.Commits().AfterNumber(2)
				.Modifications().InCommits()
				.Commits().Again().Count());
		}
		[Fact]
		public void Can_do_something_with_dsl_subproduct()
		{
			mappingDSL
				.AddCommit("1")
			.Submit()
				.AddCommit("2")
			.Submit()
				.AddCommit("3")
			.Submit()
				.AddCommit("4")
			.Submit();

			selectionDSL.Commits()
				.AfterNumber(1)
				.Do(x => Assert.Equal(3, x.Count()))
				.BeforeNumber(3)
				.Do(x => Assert.Equal(1, x.Count()));
		}
		[Fact]
		public void Can_reselect_using_func()
		{
			mappingDSL
				.AddCommit("1")
			.Submit()
				.AddCommit("2")
			.Submit()
				.AddCommit("3")
			.Submit()
				.AddCommit("4")
			.Submit();

			Func<CommitSelectionExpression, CommitSelectionExpression> selector =
				e => e.AfterNumber(1).BeforeNumber(4);
			
			Assert.Equal(2, selectionDSL.Commits()
				.Reselect(selector).Count());
		}
		[Fact]
		public void Can_continue_expression_after_null_reselector()
		{
			mappingDSL
				.AddCommit("1")
			.Submit()
				.AddCommit("2")
			.Submit()
				.AddCommit("3")
			.Submit()
				.AddCommit("4")
			.Submit();

			Assert.Equal(2, selectionDSL
				.Commits()
				.Reselect((Func<CommitSelectionExpression, CommitSelectionExpression>)null)
				.BeforeNumber(3)
				.Count());
		}
	}
}
