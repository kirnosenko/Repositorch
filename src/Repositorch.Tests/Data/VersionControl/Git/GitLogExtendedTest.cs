using System;
using System.Linq;
using Xunit;
using FluentAssertions;

namespace Repositorch.Data.VersionControl.Git
{
	public class GitLogExtendedTest
	{

private readonly string log_1 =
@"7672db20c2060f20b01788e4a4289ebc5f818605
Linus Torvalds
torvalds@g5.osdl.org
2005-07-08 17:07:37 -0700
[PATCH] Expose object ID computation functions.

36	0	Documentation/git-hash-object.txt
0	33	Documentation/git-write-blob.txt
2	2	Documentation/git.txt
2	2	Makefile
2	2	README
7	1	cache.h
1	1	git-cvsimport-script
45	0	hash-object.c
17	8	sha1_file.c
1	1	update-cache.c
0	25	write-blob.c
 create mode 100644 Documentation/git-hash-object.txt
 delete mode 100644 Documentation/git-write-blob.txt
 create mode 100644 hash-object.c
 delete mode 100644 write-blob.c";

private readonly string log_2 =
@"235ac465991c28bc85e72250f211cb27cd51abcc
mr.exodia
mr.exodia.tpodt@gmail.com
2013-11-15 20:33:59 +0100
forgot x64 bin

0	2	.gitignore
-	-	bin/x64/BeaEngine.dll
-	-	bin/x64/Scylla.dll
-	-	bin/x64/TitanEngine.dll
-	-	bin/x64/dbghelp.dll
-	-	bin/x64/sqlite.dll
-	-	bin/x64/test.dll
-	-	bin/x64/test.exe
 create mode 100644 bin/x64/BeaEngine.dll
 create mode 100644 bin/x64/Scylla.dll
 create mode 100644 bin/x64/TitanEngine.dll
 create mode 100644 bin/x64/dbghelp.dll
 create mode 100644 bin/x64/sqlite.dll
 create mode 100644 bin/x64/test.dll
 create mode 100644 bin/x64/test.exe";

private readonly string log_3 =
 @"99665af5c0be0fe4319b39183e84917993153576
Petr Baudis
xpasky@machine.sinus.cz
2005-05-15 02:05:03 +0200
[PATCH 2/3] Rename git-diff-tree-helper to git-diff-helper.

1	1	Documentation/diff-format.txt
0	0	Documentation/{git-diff-tree-helper.txt => git-diff-helper.txt}
1	1	Documentation/git.txt
2	2	Makefile
0	0	diff-tree-helper.c => diff-helper.c
1	1	diff.h
 rename Documentation/{git-diff-tree-helper.txt => git-diff-helper.txt} (100%)
 rename diff-tree-helper.c => diff-helper.c (100%)";

private readonly string log_4 =
@"c15726cf2c77201f5685dd52d5e17443ef53ee40
mr.exodia
mr.exodia.tpodt@gmail.com
2013-11-20 17:55:59 +0100
BRIDGE: added BridgeSetting* exports

2	2	help/DeleteBPX_bpc_bc.htm
23	0	help/DeleteHardwareBreakpoint_bphc_bphwc.htm
39	0	help/DeleteMemoryBPX_membpc_bpmc.htm
2	2	help/DisableBPX_bpd_bd.htm
4	2	help/Special_Thanks.htm
7	4	help/{cls.htm => StartScylla_scylla_imprec.htm}
17	4	help/{cls.htm => StopDebug_stop_dbgstop.htm}
4	4	help/cls.htm
3	3	help/disasm_dis_d.htm
23	0	help/pause.htm
3	3	help/rtr.htm
253	143	help/x64_dbg.wcp
0	23	help/x_exit.htm
58	0	x64_dbg_bridge/bridgemain.cpp
10	2	x64_dbg_bridge/bridgemain.h
2	2	x64_dbg_dbg/_exports.cpp
61	25	x64_dbg_dbg/addrinfo.cpp
4	3	x64_dbg_dbg/addrinfo.h
7	5	x64_dbg_dbg/breakpoint.cpp
95	13	x64_dbg_dbg/debugger.cpp
1	0	x64_dbg_dbg/debugger.h
2	2	x64_dbg_dbg/x64_dbg.cpp
1	1	x64_dbg_gui/Project/Src/BasicView/Disassembly.cpp
2	2	x64_dbg_gui/Project/Src/BasicView/MemoryMapView.cpp
1	1	x64_dbg_gui/Project/Src/Disassembler/BeaHighlight.cpp
15	0	x64_dbg_gui/Project/Src/Gui/MainWindow.cpp
1	0	x64_dbg_gui/Project/Src/Gui/MainWindow.h
 create mode 100644 help/DeleteHardwareBreakpoint_bphc_bphwc.htm
 create mode 100644 help/DeleteMemoryBPX_membpc_bpmc.htm
 copy help/{cls.htm => StartScylla_scylla_imprec.htm} (53%)
 copy help/{cls.htm => StopDebug_stop_dbgstop.htm} (53%)
 create mode 100644 help/pause.htm
 delete mode 100644 help/x_exit.htm";

private readonly string log_5 =
@"f87d2079b7775dff93c56022b1a175088c378b2a
Mr. eXoDia
mr.exodia.tpodt@gmail.com
2014-05-15 02:06:42 +0200
GUI: some failures

0	0	x64_dbg_gui/Project/Src/{ => Gui}/TabBar.cpp
0	0	x64_dbg_gui/Project/Src/{ => Gui}/TabBar.h
0	0	x64_dbg_gui/Project/Src/{ => Gui}/TabWidget.cpp
0	0	x64_dbg_gui/Project/Src/{ => Gui}/TabWidget.h
 rename x64_dbg_gui/Project/Src/{ => Gui}/TabBar.cpp (100%)
 rename x64_dbg_gui/Project/Src/{ => Gui}/TabBar.h (100%)
 rename x64_dbg_gui/Project/Src/{ => Gui}/TabWidget.cpp (100%)
 rename x64_dbg_gui/Project/Src/{ => Gui}/TabWidget.h (100%)";

private readonly string log_6 =
@"6b32ee2381deb414378a76cae213a4fe633f8fcc
Junio C Hamano
junkio@cox.net
2006-04-10 19:44:35 -0700
Merge branch 'jc/withraw' into next

16	9	Documentation/git-diff-tree.txt
8	3	Makefile
11	0	combine-diff.c
10	7	diff-stages.c
49	35	diff.c
1	1	diff.h
0	70	diffcore-pathspec.c
 delete mode 100644 diffcore-pathspec.c

6b32ee2381deb414378a76cae213a4fe633f8fcc
Junio C Hamano
junkio@cox.net
2006-04-10 19:44:35 -0700
Merge branch 'jc/withraw' into next

2	2	Makefile
24	202	diff-tree.c
31	1	git.c
175	0	log-tree.c
23	0	log-tree.h
 create mode 100644 log-tree.c
 create mode 100644 log-tree.h";

		private GitLogExtended log;

		[Fact]
		public void Should_get_touched_files()
		{
			log = new GitLogExtended(log_1.ToStream(), null, null);

			log.TouchedFiles.Count()
				.Should().Be(11);
			log.TouchedFiles.Where(x => x.Action == TouchedFileAction.MODIFIED).Count()
				.Should().Be(7);
			log.TouchedFiles.Where(x => x.Action == TouchedFileAction.ADDED).Count()
				.Should().Be(2);
			log.TouchedFiles.Where(x => x.Action == TouchedFileAction.REMOVED).Count()
				.Should().Be(2);
		}
		[Fact]
		public void Should_identify_file_type()
		{
			log = new GitLogExtended(log_2.ToStream(), null, null);

			log.TouchedFiles.Count()
				.Should().Be(8);
			log.TouchedFiles.Where(x => x.Type == TouchedFile.ContentType.BINARY).Count()
				.Should().Be(7);
			var file = log.TouchedFiles.Single(x => x.Type == TouchedFile.ContentType.TEXT);
			Assert.Equal("/.gitignore", file.Path);
			Assert.Equal(TouchedFileAction.MODIFIED, file.Action);
			Assert.Null(file.SourcePath);
			Assert.Null(file.SourceRevision);
		}
		[Fact]
		public void Should_interpret_renamed_file_as_deleted_and_added()
		{
			log = new GitLogExtended(log_3.ToStream(), null, null);

			log.TouchedFiles.Count()
				.Should().Be(8);
			log.TouchedFiles
				.Where(x => x.Action == TouchedFileAction.MODIFIED)
				.Count()
					.Should().Be(4);
			log.TouchedFiles
				.Where(x => x.Action == TouchedFileAction.ADDED)
				.Select(x => x.Path)
					.Should().BeEquivalentTo(new string[]
					{
						"/Documentation/git-diff-helper.txt",
						"/diff-helper.c"
					});
			log.TouchedFiles
				.Where(x => x.Action == TouchedFileAction.REMOVED)
				.Select(x => x.Path)
					.Should().BeEquivalentTo(new string[]
					{
						"/Documentation/git-diff-tree-helper.txt",
						"/diff-tree-helper.c"
					});
		}
		[Fact]
		public void Should_keep_source_path_for_renamed_path()
		{
			log = new GitLogExtended(log_3.ToStream(), null, null);

			Assert.Equal(
				"/diff-tree-helper.c",
				log.TouchedFiles
					.Single(x => x.Path == "/diff-helper.c")
					.SourcePath);
			Assert.Equal(
				"/Documentation/git-diff-tree-helper.txt",
				log.TouchedFiles
					.Single(x => x.Path == "/Documentation/git-diff-helper.txt")
					.SourcePath);
		}
		[Fact]
		public void Should_interpret_copied_file_as_added_and_keep_source_path()
		{
			log = new GitLogExtended(log_4.ToStream(), null, null);

			Assert.Equal(27, log.TouchedFiles.Count());
			Assert.Equal(5, log.TouchedFiles
				.Where(x => x.Action == TouchedFileAction.ADDED)
				.Count());
			Assert.Equal(1, (int)log.TouchedFiles
				.Where(x => x.Action == TouchedFileAction.REMOVED)
				.Count());
			Assert.Equal("/help/cls.htm",
				log.TouchedFiles
					.Single(x => x.Path == "/help/StartScylla_scylla_imprec.htm")
					.SourcePath);
			Assert.Equal("/help/cls.htm",
				log.TouchedFiles
					.Single(x => x.Path == "/help/StopDebug_stop_dbgstop.htm")
					.SourcePath);
		}
		[Fact]
		public void Can_deal_with_folder_renaming()
		{
			log = new GitLogExtended(log_5.ToStream(), null, null);

			Assert.Equal(8, log.TouchedFiles.Count());
			Assert.Equal(4, log.TouchedFiles
				.Where(x => x.Action == TouchedFileAction.ADDED)
				.Count());
			Assert.Equal(4, log.TouchedFiles
				.Where(x => x.Action == TouchedFileAction.REMOVED)
				.Count());
			Assert.Equal("/x64_dbg_gui/Project/Src/TabBar.cpp",
				log.TouchedFiles
					.Single(x => x.Path == "/x64_dbg_gui/Project/Src/Gui/TabBar.cpp")
					.SourcePath);
		}
		[Fact]
		public void Should_parse_merge_log()
		{
			log = new GitLogExtended(log_6.ToStream(), null, null);

			log.TouchedFiles.Count()
				.Should().Be(11);
			log.TouchedFiles
				.Where(x => x.Action == TouchedFileAction.MODIFIED)
				.Count()
					.Should().Be(8);
			log.TouchedFiles
				.Where(x => x.Action == TouchedFileAction.ADDED)
				.Select(x => x.Path)
					.Should().BeEquivalentTo(new string[]
					{
						"/log-tree.c",
						"/log-tree.h",
					});
			log.TouchedFiles
				.Where(x => x.Action == TouchedFileAction.REMOVED)
				.Select(x => x.Path)
					.Should().BeEquivalentTo(new string[]
					{
						"/diffcore-pathspec.c",
					});
		}
	}
}
