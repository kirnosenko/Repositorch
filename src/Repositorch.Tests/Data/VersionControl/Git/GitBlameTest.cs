using System;
using System.Linq;
using Xunit;

namespace Repositorch.Data.VersionControl.Git
{
	public class GitBlameTest
	{
		private GitBlame blame;

private string blame0 =
@"54988bdad7dc3f09e40752221c144bf470d73aa7 5 5 1
author Jeff King
author-mail <peff@peff.net>
author-time 1219254933
author-tz -0400
committer Junio C Hamano
committer-mail <gitster@pobox.com>
committer-time 1219264249
committer-tz -0700
summary decorate: allow const objects to be decorated
previous e276c26b4b65711c27e3ef37e732d41eeae42094 decorate.h
filename decorate.h
54988bdad7dc3f09e40752221c144bf470d73aa7 15 15 2
filename decorate.h
a59b276e18f3d4a548caf549e05188cb1bd3a709 1 1 4
author Linus Torvalds
author-mail <torvalds@linux-foundation.org>
author-time 1176764595
author-tz -0700
committer Junio C Hamano
committer-mail <junkio@cox.net>
committer-time 1176767469
committer-tz -0700
summary Add a generic 'object decorator' interface, and make object refs use it
filename decorate.h
a59b276e18f3d4a548caf549e05188cb1bd3a709 6 6 9
filename decorate.h
a59b276e18f3d4a548caf549e05188cb1bd3a709 17 17 2
filename decorate.h
";

private string blame1 = @"";

		[Fact]
		public void Should_keep_revisions_for_each_line()
		{
			blame = GitBlame.Parse(blame0.ToStream());

			Assert.Equal(15,
				blame.Where(x => x.Value == "a59b276e18f3d4a548caf549e05188cb1bd3a709").Count());
			Assert.Equal(3,
				blame.Where(x => x.Value == "54988bdad7dc3f09e40752221c144bf470d73aa7").Count());
			Assert.Equal(new int[] { 5, 15, 16 },
				blame.Where(x => x.Value == "54988bdad7dc3f09e40752221c144bf470d73aa7")
				.Select(x => x.Key));
		}
		[Fact]
		public void Should_return_null_for_invalid_path()
		{
			blame = GitBlame.Parse(blame1.ToStream());

			Assert.Null(blame);
		}
	}
}
