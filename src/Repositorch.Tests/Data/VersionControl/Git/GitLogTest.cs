using System;
using System.Linq;
using Xunit;

namespace Repositorch.Data.VersionControl.Git
{
	public class GitLogTest
	{

private string log_1 =
@"4bb04f2190d526f8917663f0be62d8026e1ed100
Linus Torvalds
torvalds@ppc970.osdl.org
2005-04-11 15:47:57 -0700
Rename '.dircache' directory to '.git'
M	README
M	cache.h
M	init-db.c
M	read-cache.c
M	read-tree.c
M	update-cache.c";

private string log_2 =
@"a3df180138b85a603656582bde6df757095618cf
Linus Torvalds
torvalds@ppc970.osdl.org
2005-04-29 14:09:11 -0700
Rename git core commands to be 'git-xxxx' to avoid name clashes.
M	Makefile
R100	show-diff.c	diff-files.c
R100	git-export.c	export.c
R100	git-mktag.c	mktag.c";

private string log_3 =
@"35587ec664abcf6ed79d9d743ab58d0600cf0bc1
adrian
???
2005-07-14 18:20:03 +0000
Created django.contrib and moved comments into it
C100	django/views/comments/__init__.py	django/contrib/__init__.py
C100	django/views/comments/__init__.py	django/contrib/comments/__init__.py";

private string log_4 =
@"766c647d9b9cf3e84353536ebb928153c96fdece
jezdez
???
2011-02-14 23:45:32 +0000
Fixed the staticfiles management commands collectstatic and findstatic to not raise encoding related exceptions when handlings filenames with non-ASCII characters.
M	django/contrib/staticfiles/management/commands/collectstatic.py
M	django/contrib/staticfiles/management/commands/findstatic.py
A	'tests/regressiontests/staticfiles_tests/apps/test/static/test/spec\314\247ial.txt'
M	tests/regressiontests/staticfiles_tests/tests.py".Replace("'", "\"");

private string log_5 =
@"b51ad4314078298194d23d46e2b4473ffd32a88a
Linus Torvalds
torvalds@ppc970.osdl.org
2005-04-18 12:12:00 -0700
Merge the new object model thing from Daniel Barkalow
MM	Makefile
MM	fsck-cache.c
MM	merge-base.c";

		private GitLog log;
		
		[Fact]
		public void Should_parse_general_data_about_revision()
		{
			log = new GitLog(log_1.ToStream());

			Assert.Equal("4bb04f2190d526f8917663f0be62d8026e1ed100", log.Revision);
			Assert.Equal("Linus Torvalds", log.AuthorName);
			Assert.Equal("torvalds@ppc970.osdl.org", log.AuthorEmail);
			Assert.Equal(new DateTime(2005, 4, 11, 22, 47, 57), log.Date);
			Assert.Equal("Rename '.dircache' directory to '.git'", log.Message);
		}
		[Fact]
		public void Should_keep_alphabetically_sorted_list_of_touched_paths()
		{
			log = new GitLog(log_1.ToStream());

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
		public void Should_interpret_renamed_file_as_deleted_and_added()
		{
			log = new GitLog(log_2.ToStream());

			Assert.Equal(
				new string[]
				{
					"/git-export.c",
					"/git-mktag.c",
					"/show-diff.c"
				},
				log.TouchedFiles
					.Where(x => x.Action == TouchedFile.TouchedFileAction.REMOVED)
					.Select(x => x.Path));

			Assert.Equal(
				new string[]
				{
					"/diff-files.c",
					"/export.c",
					"/mktag.c"
				},
				log.TouchedFiles
					.Where(x => x.Action == TouchedFile.TouchedFileAction.ADDED)
					.Select(x => x.Path));
		}
		[Fact]
		public void Should_keep_source_path_for_renamed_path()
		{
			log = new GitLog(log_2.ToStream());

			Assert.Equal(
				"/git-export.c",
				log.TouchedFiles
					.Single(x => x.Path == "/export.c")
					.SourcePath);
		}
		[Fact]
		public void Should_keep_information_about_copied_paths()
		{
			log = new GitLog(log_3.ToStream());

			Assert.Equal(
				2,
				log.TouchedFiles.Count());
			Assert.Equal(
				2,
				log.TouchedFiles
					.Where(x => x.Action == TouchedFile.TouchedFileAction.ADDED)
					.Count());
		}
		[Fact]
		public void Should_parse_filename_with_special_symbols()
		{
			log = new GitLog(log_4.ToStream());

			Assert.Single(
				log.TouchedFiles
					.Where(x => x.Path == "/tests/regressiontests/staticfiles_tests/apps/test/static/test/spec\\314\\247ial.txt"));
		}
		[Fact]
		public void Should_parse_filename_from_merge_log()
		{
			log = new GitLog(log_5.ToStream());

			Assert.Equal(3, log.TouchedFiles.Count());
		}
	}
}
