using System;
using System.Linq;
using Xunit;

namespace Repositorch.Data.Entities.Persistent
{
	public class InMemoryDataStoreTest
	{
		private IDataStore data;

		public InMemoryDataStoreTest()
		{
			data = new InMemoryDataStore(Guid.NewGuid().ToString());
		}
		[Fact]
		public void Can_submit_nothing()
		{
			using (var s = data.OpenSession())
			{
				s.SubmitChanges();
			}
		}
		[Fact]
		public void Should_set_id()
		{
			using (var s = data.OpenSession())
			{
				CodeFile file = new CodeFile();
				
				s.Add(file);
				s.SubmitChanges();

				Assert.NotEqual(0, file.Id);
			}
		}
		[Fact]
		public void Should_set_unique_id_for_each_entity()
		{
			using (var s = data.OpenSession())
			{
				CodeFile file1 = new CodeFile();
				CodeFile file2 = new CodeFile();
				CodeFile file3 = new CodeFile();

				s.Add(file1);
				s.Add(file2);
				s.Add(file3);
				s.SubmitChanges();

				Assert.Equal(3, s.Get<CodeFile>().Select(x => x.Id).Distinct().Count());
			}
		}
		[Fact]
		public void Should_set_foreign_id_during_submit()
		{
			using (var s = data.OpenSession())
			{
				Commit c = new Commit();
				CommitAttribute a = new CommitAttribute();

				s.Add(c);
				s.Add(a);
				a.Commit = c;
				s.SubmitChanges();

				Assert.Equal(c.Number, a.CommitNumber);
			}
		}
		[Fact]
		public void Should_update_foreign_id_during_submit()
		{
			using (var s = data.OpenSession())
			{
				Commit c1 = new Commit() { Number = 1 };
				Commit c2 = new Commit() { Number = 2 };
				CommitAttribute a = new CommitAttribute();

				s.Add(c1);
				s.Add(a);
				a.Commit = c1;
				s.SubmitChanges();

				a.Commit = c2;
				s.SubmitChanges();

				Assert.NotEqual(c1.Number, a.CommitNumber);
				Assert.Equal(c2.Number, a.CommitNumber);
			}
		}
		[Fact]
		public void Should_keep_entities_between_sessions()
		{
			Commit c = new Commit() { Message = "text" };
			
			using (var s = data.OpenSession())
			{
				s.Add(c);
				s.SubmitChanges();
			}

			using (var s = data.OpenSession())
			{
				Assert.Equal(1, s.Get<Commit>().Count());
				Assert.Equal(c.Message, s.Get<Commit>().Single().Message);
			}
		}
		[Fact]
		public void Should_save_changes_after_submit()
		{
			Commit c = new Commit() { Message = "text" };

			using (var s = data.OpenSession())
			{
				s.Add(c);
				s.SubmitChanges();
			}
			using (var s = data.OpenSession())
			{
				s.Get<Commit>().Single().Message = string.Empty;
				s.SubmitChanges();
			}
			using (var s = data.OpenSession())
			{
				Assert.Equal(string.Empty, s.Get<Commit>().Single().Message);
			}
		}
		[Fact]
		public void Should_not_save_changes_before_submit()
		{
			Commit c = new Commit() { Message = "text" };

			using (var s = data.OpenSession())
			{
				s.Add(c);
				s.SubmitChanges();
			}
			using (var s = data.OpenSession())
			{
				s.Get<Commit>().Single().Message = string.Empty;
				Assert.Equal(string.Empty, s.Get<Commit>().Single().Message);
			}
			using (var s = data.OpenSession())
			{
				Assert.Equal(c.Message, s.Get<Commit>().Single().Message);
			}
		}
		[Fact]
		public void Should_remove_entity_after_submit()
		{
			Commit c = new Commit();

			using (var s = data.OpenSession())
			{
				s.Add(c);
				s.SubmitChanges();
			}
			using (var s = data.OpenSession())
			{
				s.Remove(c);
			}
			using (var s = data.OpenSession())
			{
				Assert.Equal(1, s.Get<Commit>().Count());
			}
			using (var s = data.OpenSession())
			{
				s.Remove(c);
				s.SubmitChanges();
			}
			using (var s = data.OpenSession())
			{
				Assert.Equal(0, s.Get<Commit>().Count());
			}
		}
		[Fact]
		public void Should_allow_to_precess_entity_range()
		{
			var commits = new Commit[]
			{
				new Commit() { Number = 1 },
				new Commit() { Number = 2 },
			};
			using (var s = data.OpenSession())
			{
				s.AddRange(commits);
				s.SubmitChanges();
			}
			using (var s = data.OpenSession())
			{
				commits = s.Get<Commit>().ToArray();
				Assert.Equal(2, commits.Length);
				s.RemoveRange(commits);
				s.SubmitChanges();
			}
			using (var s = data.OpenSession())
			{
				Assert.Equal(0, s.Get<Commit>().Count());
				s.AddRange(Enumerable.Empty<Commit>());
				s.SubmitChanges();
			}
			using (var s = data.OpenSession())
			{
				Assert.Equal(0, s.Get<Commit>().Count());
				s.RemoveRange(Enumerable.Empty<Commit>());
				s.SubmitChanges();
			}
			using (var s = data.OpenSession())
			{
				Assert.Equal(0, s.Get<Commit>().Count());
			}
		}
	}
}
