﻿using System;
using System.Linq;
using Xunit;
using Repositorch.Data.Entities.DSL.Mapping;

namespace Repositorch.Data.Entities.DSL.Selection
{
	public class CommitSelectionExpressionTest : BaseRepositoryTest
	{
		[Fact]
		public void Should_select_commits_by_date()
		{
			mappingDSL
				.AddCommit("1").At(DateTime.Today.AddDays(-9))
			.Submit()
				.AddCommit("2").At(DateTime.Today.AddDays(-8))
			.Submit()
				.AddCommit("5").At(DateTime.Today.AddDays(-5))
			.Submit();

			Assert.Equal(
				1,
				selectionDSL
					.Commits().DateIsGreaterThan(DateTime.Today.AddDays(-8)).Count());
			Assert.Equal(
				2,
				selectionDSL
					.Commits().DateIsGreaterOrEquelThan(DateTime.Today.AddDays(-8)).Count());
			Assert.Equal(
				1,
				selectionDSL
					.Commits().DateIsLesserThan(DateTime.Today.AddDays(-8)).Count());
			Assert.Equal(
				2,
				selectionDSL
					.Commits().DateIsLesserOrEquelThan(DateTime.Today.AddDays(-8)).Count());
		}
		[Fact]
		public void Should_select_commits_relatively_specified()
		{
			mappingDSL
				.AddCommit("abc")
			.Submit()
				.AddCommit("abcd")
			.Submit()
				.AddCommit("abcde")
			.Submit();

			Assert.Equal(
				0,
				selectionDSL.Commits().BeforeRevision(1).Count());
			Assert.Equal(
				0,
				selectionDSL.Commits().BeforeRevision("abc").Count());
			Assert.Equal(
				1,
				selectionDSL.Commits().BeforeRevision(2).Count());
			Assert.Equal(
				1,
				selectionDSL.Commits().BeforeRevision("abcd").Count());
			Assert.Equal(
				1,
				selectionDSL.Commits().TillRevision(1).Count());
			Assert.Equal(
				1,
				selectionDSL.Commits().TillRevision("abc").Count());
			Assert.Equal(
				3,
				selectionDSL.Commits().TillRevision(3).Count());
			Assert.Equal(
				3,
				selectionDSL.Commits().TillRevision("abcde").Count());
			Assert.Equal(
				2,
				selectionDSL.Commits().FromRevision(2).Count());
			Assert.Equal(
				2,
				selectionDSL.Commits().FromRevision("abcd").Count());
			Assert.Equal(
				1,
				selectionDSL.Commits().FromRevision(3).Count());
			Assert.Equal(
				1,
				selectionDSL.Commits().FromRevision("abcde").Count());
			Assert.Equal(
				2,
				selectionDSL.Commits().AfterRevision(1).Count());
			Assert.Equal(
				2,
				selectionDSL.Commits().AfterRevision("abc").Count());
			Assert.Equal(
				0,
				selectionDSL.Commits().AfterRevision(3).Count());
			Assert.Equal(
				0,
				selectionDSL.Commits().AfterRevision("abcde").Count());
		}
		[Fact]
		public void Should_ignore_null_values_for_commit_relative_selection()
		{
			mappingDSL
				.AddCommit("abc")
			.Submit()
				.AddCommit("abcd")
			.Submit()
				.AddCommit("abcde")
			.Submit();

			Assert.Equal(
				3,
				selectionDSL.Commits()
					.AfterRevision(null)
					.FromRevision(null)
					.TillRevision(null)
					.BeforeRevision(null)
					.Count());
		}
	}
}
