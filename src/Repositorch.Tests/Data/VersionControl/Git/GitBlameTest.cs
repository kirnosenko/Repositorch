using System;
using System.Linq;
using Xunit;
using FluentAssertions;

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

		[Fact]
		public void Should_keep_revisions_for_each_line()
		{
			blame = GitBlame.Parse(blame0.ToStream());

			blame["a59b276e18f3d4a548caf549e05188cb1bd3a709"]
				.Should().Be(15);
			blame["54988bdad7dc3f09e40752221c144bf470d73aa7"]
				.Should().Be(3);
			blame["54988bdad7dc3f09e40752221c144bf470d73aa7"]
				.Should().Be(3);
		}
		[Fact]
		public void Should_return_null_for_invalid_path()
		{
			blame = GitBlame.Parse(null);

			blame.Should().BeNull();
		}
		[Fact]
		public void Should_return_empty_for_empty_file()
		{
			blame = GitBlame.Parse("".ToStream());

			blame.Should().NotBeNull();
			blame.Count.Should().Be(0);
		}
	}
}
