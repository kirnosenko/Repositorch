using System;
using System.Linq;
using Xunit;
using FluentAssertions;

namespace Repositorch.Data.VersionControl.Git
{
	public class GitLogTest
	{

private readonly string log_1 =
@"4bb04f2190d526f8917663f0be62d8026e1ed100
Linus Torvalds
torvalds@ppc970.osdl.org
Rename '.dircache' directory to '.git'

2005-04-11 15:47:57 -0700

M	README
M	cache.h
M	init-db.c
M	read-cache.c
M	read-tree.c
M	update-cache.c";

private readonly string log_2 =
@"a3df180138b85a603656582bde6df757095618cf
Linus Torvalds
torvalds@ppc970.osdl.org
Rename git core commands to be 'git-xxxx' to avoid name clashes.

2005-04-29 14:09:11 -0700

M	Makefile
R100	show-diff.c	diff-files.c
R100	git-export.c	export.c
R100	git-mktag.c	mktag.c";

private readonly string log_3 =
@"35587ec664abcf6ed79d9d743ab58d0600cf0bc1
adrian
???
Created django.contrib and moved comments into it

2005-07-14 18:20:03 +0000

C100	django/views/comments/__init__.py	django/contrib/__init__.py
C100	django/views/comments/__init__.py	django/contrib/comments/__init__.py";

private readonly string log_4 =
@"766c647d9b9cf3e84353536ebb928153c96fdece
jezdez
???
Fixed the staticfiles management commands collectstatic and findstatic to not raise encoding related exceptions when handlings filenames with non-ASCII characters.

2011-02-14 23:45:32 +0000

M	django/contrib/staticfiles/management/commands/collectstatic.py
M	django/contrib/staticfiles/management/commands/findstatic.py
A	'tests/regressiontests/staticfiles_tests/apps/test/static/test/spec\314\247ial.txt'
M	tests/regressiontests/staticfiles_tests/tests.py".Replace("'", "\"");

private readonly string log_5 =
@"b51ad4314078298194d23d46e2b4473ffd32a88a
Linus Torvalds
torvalds@ppc970.osdl.org
Merge the new object model thing from Daniel Barkalow

2005-04-18 12:12:00 -0700

M	Makefile
A	blob.c
A	blob.h
A	commit.c
A	commit.h
M	fsck-cache.c
M	merge-base.c
A	object.c
A	object.h
M	rev-tree.c
A	tree.c
A	tree.h

b51ad4314078298194d23d46e2b4473ffd32a88a
Linus Torvalds
torvalds@ppc970.osdl.org
Merge the new object model thing from Daniel Barkalow

2005-04-18 12:12:00 -0700

M	Makefile
M	README
M	checkout-cache.c
M	commit-tree.c
M	fsck-cache.c
M	merge-base.c
A	merge-cache.c
M	read-cache.c
M	revision.h
M	show-diff.c
M	update-cache.c";

private readonly string log_6 =
@"10
alan
alan@mail
message

2000-01-01 00:00:00 -0700

M	file1
A	file2
M	file3

10
alan
alan@mail
message

2000-01-01 00:00:00 -0700

M	file1
M	file2
D	file3";

private readonly string log_7 =
@"10
alan
alan@mail
message

2000-01-01 00:00:00 -0700
HEAD -> master, tag: qwe, tag: ab,c, tag: 0.99
A	file1";

private readonly string log_8 =
@"824100eea88c38c5a8c7f84d17f832bf6611e26d
Duncan Ogilvie
mr.exodia.tpodt@gmail.com
DBG: implement memcpy command

2019-11-13 01:13:06 +0100
HEAD -> development, origin/development, origin/HEAD
M	src/dbg/commands/cmd-memory-operations.cpp
M	src/dbg/commands/cmd-memory-operations.h
M	src/dbg/commands/cmd-undocumented.cpp
M	src/dbg/x64dbg.cpp";

private readonly string log_9 =
@"e90a4c0ed17b66c302f48ec0a234cac6f27e5eec
Linus Torvalds
torvalds@ppc970.osdl.org
Add 'dotest' and 'applypatch' scripts to actually make things useful.

2005-04-18 16:11:32 -0700";

private readonly string log_10 =
@"d4935bfe2689e9542d33ceb41fb171380158692e
bob
bob@mail
ccc

2019-12-07 20:25:37 +0400
HEAD -> master
R100	aaa bbb.txt	aaa bbb ccc.txt";

private readonly string log_11 =
@"1daf97f3b862ded3d9d6c55ad1be42def826f463
Pavel Krymets
pavel@krymets.com
Move 
efs to shared runtime

2016-05-02 09:36:02 -0700

M	src/installer/DependencyContext.cs
M	src/installer/DependencyContextLoader.cs
A	src/installer/DependencyContextPaths.cs
M	src/installer/Resolution/AppBaseCompilationAssemblyResolver.cs
A	src/installer/TargetInfo.cs";

		private GitLog log;
		
		[Fact]
		public void Should_parse_general_data_about_revision()
		{
			log = new GitLog(log_1.ToStream(), null, null);

			Assert.Equal("4bb04f2190d526f8917663f0be62d8026e1ed100", log.Revision);
			Assert.Equal("Linus Torvalds", log.AuthorName);
			Assert.Equal("torvalds@ppc970.osdl.org", log.AuthorEmail);
			Assert.Equal(new DateTime(2005, 4, 11, 22, 47, 57), log.Date);
			Assert.Equal("Rename '.dircache' directory to '.git'", log.Message);
		}
		[Fact]
		public void Should_keep_information_about_parent_and_child_revisions()
		{
			var parents = new string[] { "2", "3" };
			var children = new string[] { "20", "30" };
			log = new GitLog(log_1.ToStream(), parents, children);

			Assert.Equal(parents, log.ParentRevisions);
			Assert.Equal(children, log.ChildRevisions);
		}
		[Fact]
		public void Should_keep_alphabetically_sorted_list_of_touched_paths()
		{
			log = new GitLog(log_1.ToStream(), null, null);

			Assert.Equal(
				new string[]
				{
					"/cache.h",
					"/init-db.c",
					"/read-cache.c",
					"/read-tree.c",
					"/README",
					"/update-cache.c"
				},
				log.TouchedFiles.Select(x => x.Path));
		}
		[Fact]
		public void Should_not_identify_file_type()
		{
			log = new GitLog(log_1.ToStream(), null, null);

			log.TouchedFiles.Where(x => x.Type != TouchedFile.ContentType.UNKNOWN)
				.Should().BeEmpty();
		}
		[Fact]
		public void Should_interpret_renamed_file_as_deleted_and_added()
		{
			log = new GitLog(log_2.ToStream(), null, null);

			Assert.Equal(
				new string[]
				{
					"/git-export.c",
					"/git-mktag.c",
					"/show-diff.c"
				},
				log.TouchedFiles
					.Where(x => x.Action == TouchedFileAction.REMOVED)
					.Select(x => x.Path));

			Assert.Equal(
				new string[]
				{
					"/diff-files.c",
					"/export.c",
					"/mktag.c"
				},
				log.TouchedFiles
					.Where(x => x.Action == TouchedFileAction.ADDED)
					.Select(x => x.Path));
		}
		[Fact]
		public void Should_keep_source_path_for_renamed_path()
		{
			log = new GitLog(log_2.ToStream(), null, null);

			Assert.Equal(
				"/git-export.c",
				log.TouchedFiles
					.Single(x => x.Path == "/export.c")
					.SourcePath);
		}
		[Fact]
		public void Should_be_ready_for_spaces_in_paths_for_renamed_file()
		{
			log = new GitLog(log_10.ToStream(), null, null);

			log.TouchedFiles.Select(x => x.Path)
				.Should().BeEquivalentTo(
					new string[] { "/aaa bbb.txt", "/aaa bbb ccc.txt" });
			log.TouchedFiles.Select(x => x.SourcePath)
				.Should().BeEquivalentTo(
					new string[] { null, "/aaa bbb.txt" });
		}
		[Fact]
		public void Should_interpret_copied_file_as_added_and_keep_source_path()
		{
			log = new GitLog(log_3.ToStream(), null, null);

			Assert.Equal(2, log.TouchedFiles.Count());
			Assert.Equal(2, log.TouchedFiles
				.Where(x => x.Action == TouchedFileAction.ADDED)
				.Count());
			Assert.Equal("/django/views/comments/__init__.py",
				log.TouchedFiles
					.Single(x => x.Path == "/django/contrib/__init__.py")
					.SourcePath);
			Assert.Equal("/django/views/comments/__init__.py",
				log.TouchedFiles
					.Single(x => x.Path == "/django/contrib/comments/__init__.py")
					.SourcePath);
		}
		[Fact]
		public void Should_parse_filename_with_special_symbols()
		{
			log = new GitLog(log_4.ToStream(), null, null);

			Assert.Single(
				log.TouchedFiles
					.Where(x => x.Path == "/tests/regressiontests/staticfiles_tests/apps/test/static/test/spec\\314\\247ial.txt"));
		}
		[Fact]
		public void Should_parse_merge_log()
		{
			log = new GitLog(log_5.ToStream(), null, null);

			Assert.Equal(20, log.TouchedFiles.Count());
			Assert.Equal(20, log.TouchedFiles.Select(x => x.Path).Distinct().Count());
			var makefile = log.TouchedFiles.Where(x => x.Path == "/Makefile").Single();
			Assert.Equal(TouchedFileAction.MODIFIED, makefile.Action);
			var blob = log.TouchedFiles.Where(x => x.Path == "/blob.c").Single();
			Assert.Equal(TouchedFileAction.ADDED, blob.Action);
			var revtree = log.TouchedFiles.Where(x => x.Path == "/rev-tree.c").Single();
			Assert.Equal(TouchedFileAction.MODIFIED, revtree.Action);
		}
		[Fact]
		public void Should_prefer_file_addition_or_removing_over_modification_in_merge_log()
		{
			log = new GitLog(log_6.ToStream(), null, null);

			Assert.Equal(3, log.TouchedFiles.Count());
			Assert.Equal(new string[] { "/file1", "/file2", "/file3" },
				log.TouchedFiles.Select(x => x.Path));
			Assert.Equal(new TouchedFileAction[]
				{
					TouchedFileAction.MODIFIED,
					TouchedFileAction.ADDED,
					TouchedFileAction.REMOVED
				},
				log.TouchedFiles.Select(x => x.Action));
		}
		[Fact]
		public void Should_get_tags_from_log()
		{
			log = new GitLog(log_7.ToStream(), null, null);

			Assert.Equal(new string[] { "qwe", "ab,c", "0.99" }, log.Tags);
		}
		[Fact]
		public void Should_get_no_tags_from_log_with_references_only()
		{
			log = new GitLog(log_8.ToStream(), null, null);

			Assert.NotNull(log.Tags);
			Assert.Empty(log.Tags);
		}
		[Fact]
		public void Should_parse_correctly_log_without_tags_and_empty_file_list()
		{
			log = new GitLog(log_9.ToStream(), null, null);

			Assert.Equal(new string[] { }, log.Tags);
			Assert.Equal(new TouchedFile[] { }, log.TouchedFiles);
		}

		[Fact]
		public void Should_parse_multiline_message()
		{
			log = new GitLog(log_11.ToStream(), null, null);

			Assert.Equal("Move " + Environment.NewLine + "efs to shared runtime", log.Message);
		}
	}
}
