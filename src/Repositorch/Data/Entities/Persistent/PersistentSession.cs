using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Repositorch.Data.Entities.Persistent.Mapping;

namespace Repositorch.Data.Entities.Persistent
{
	public class PersistentSession : DbContext, ISession
	{
		private Action<DbContextOptionsBuilder> config;
		private Dictionary<Type, object> tables;

		public PersistentSession(Action<DbContextOptionsBuilder> config)
			: base()
		{
			this.config = config;
			this.tables = new Dictionary<Type, object>();

			tables.Add(typeof(Commit), Set<Commit>());
			tables.Add(typeof(Tag), Set<Tag>());
			tables.Add(typeof(Author), Set<Author>());
			tables.Add(typeof(Branch), Set<Branch>());
			tables.Add(typeof(BugFix), Set<BugFix>());
			tables.Add(typeof(CodeFile), Set<CodeFile>());
			tables.Add(typeof(Modification), Set<Modification>());
			tables.Add(typeof(CodeBlock), Set<CodeBlock>());

			Database.EnsureCreated();
		}

		IQueryable<T> IRepository.Get<T>()
		{
			return GetTable<T>();
		}
		IQueryable<T> IRepository.GetReadOnly<T>()
		{
			return GetTable<T>().AsNoTracking();
		}
		void IRepository.Add<T>(T entity)
		{
			GetTable<T>().Add(entity);
		}
		void IRepository.AddRange<T>(IEnumerable<T> entities)
		{
			if (!entities.Any())
			{
				return;
			}
			GetTable<T>().AddRange(entities);
		}
		void IRepository.Remove<T>(T entity)
		{
			GetTable<T>().Remove(entity);
		}
		void IRepository.RemoveRange<T>(IEnumerable<T> entities)
		{
			if (!entities.Any())
			{
				return;
			}
			GetTable<T>().RemoveRange(entities);
		}
		void ISession.SubmitChanges()
		{
			SaveChanges();
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			config(optionsBuilder);
		}
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfiguration(new CommitMapping());
			modelBuilder.ApplyConfiguration(new TagMapping());
			modelBuilder.ApplyConfiguration(new AuthorMapping());
			modelBuilder.ApplyConfiguration(new BranchMapping());
			modelBuilder.ApplyConfiguration(new BugFixMapping());
			modelBuilder.ApplyConfiguration(new CodeFileMapping());
			modelBuilder.ApplyConfiguration(new ModificationMapping());
			modelBuilder.ApplyConfiguration(new CodeBlockMapping());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private DbSet<T> GetTable<T>() where T : class
		{
			var key = typeof(T);
			return (DbSet<T>)tables[key];
		}
	}
}
