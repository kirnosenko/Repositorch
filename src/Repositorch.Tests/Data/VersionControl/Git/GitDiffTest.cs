using System;
using Xunit;

namespace Repositorch.Data.VersionControl.Git
{
	public class GitDiffTest
	{

private string diff_1 =
@"59c1e249808c6ba38194733fa00efddb9e0eb488
diff --git a/cache.h b/cache.h
index 864f70bfe5..900824abfa 100644
--- a/cache.h
+++ b/cache.h
@@ -56,7 +56,7 @@ struct cache_entry {
 	unsigned int st_size;
 	unsigned char sha1[20];
 	unsigned short namelen;
-	unsigned char name[0];
+	char name[0];
 };
 
 const char *sha1_file_directory;
diff --git a/fsck-cache.c b/fsck-cache.c
index 0a97566e87..a01513ed4d 100644
--- a/fsck-cache.c
+++ b/fsck-cache.c
@@ -30,6 +30,7 @@ static int fsck_tree(unsigned char *sha1, void *data, unsigned long size)
 		size -= len + 20;
 		mark_needs_sha1(sha1, 'blob', file_sha1);
 	}
+	return 0;
 }

static int fsck_commit(unsigned char* sha1, void* data, unsigned long size)
@@ -49,6 +50,7 @@ static int fsck_commit(unsigned char* sha1, void* data, unsigned long size)
 		mark_needs_sha1(sha1, 'commit', parent_sha1);
data += 7 + 40 + 1; 	/* 'parent ' + <hex sha1> + '\n' */
 	}
+	return 0;
 }
 
 static int fsck_entry(unsigned char* sha1, char* tag, void* data, unsigned long size)
diff --git a/read-cache.c b/read-cache.c
index 2ede67dbe1..50d0be35e8 100644
--- a/read-cache.c
+++ b/read-cache.c
@@ -264,10 +264,9 @@ int read_cache(void)
 	size = 0; // avoid gcc warning
 	map = (void*)-1;
 	if (!fstat(fd, &st)) {
-		map = NULL;
 		size = st.st_size;
 		errno = EINVAL;
-		if (size > sizeof(struct cache_header))
+		if (size >= sizeof(struct cache_header))
 			map = mmap(NULL, size, PROT_READ, MAP_PRIVATE, fd, 0);
 	}
 	close(fd);
";

		private GitDiff diff;

		[Fact]
		public void Should_parse_file_names_touched_in_revision()
		{
			diff = new GitDiff(diff_1.ToStream());

			Assert.Equal(
				new string[]
				{
					"/cache.h",
					"/fsck-cache.c",
					"/read-cache.c"
				},
				diff.TouchedFiles);
		}
	}
}
