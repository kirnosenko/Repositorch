using System;
using System.Linq;
using Xunit;

namespace Repositorch.Data.Entities.EF
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
				Commit c = new Commit();
				
				s.Add(c);
				s.SubmitChanges();

				Assert.NotEqual(0, c.Id);
			}
		}
		[Fact]
		public void Should_set_unique_id_for_each_entity()
		{
			using (var s = data.OpenSession())
			{
				Commit c1 = new Commit();
				Commit c2 = new Commit();
				Commit c3 = new Commit();

				s.Add(c1);
				s.Add(c2);
				s.Add(c3);
				s.SubmitChanges();

				Assert.Equal(3, s.Get<Commit>().Select(x => x.Id).Distinct().Count());
			}
		}
		[Fact]
		public void Should_set_foreign_id_during_submit()
		{
			using (var s = data.OpenSession())
			{
				Commit c = new Commit();
				BugFix bf = new BugFix();

				s.Add(c);
				s.Add(bf);
				bf.Commit = c;
				s.SubmitChanges();

				Assert.Equal(c.Id, bf.CommitId);
			}
		}
		[Fact]
		public void Should_update_foreign_id_during_submit()
		{
			using (var s = data.OpenSession())
			{
				Commit c1 = new Commit();
				Commit c2 = new Commit();
				BugFix bf = new BugFix();

				s.Add(c1);
				s.Add(bf);
				bf.Commit = c1;
				s.SubmitChanges();

				bf.Commit = c2;
				s.SubmitChanges();

				Assert.NotEqual(c1.Id, bf.CommitId);
				Assert.Equal(c2.Id, bf.CommitId);
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
				s.Delete(c);
			}
			using (var s = data.OpenSession())
			{
				Assert.Equal(1, s.Get<Commit>().Count());
			}
			using (var s = data.OpenSession())
			{
				s.Delete(c);
				s.SubmitChanges();
			}
			using (var s = data.OpenSession())
			{
				Assert.Equal(0, s.Get<Commit>().Count());
			}
		}
	}
}
