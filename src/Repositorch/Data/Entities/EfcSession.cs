using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Repositorch.Data.Entities
{
	public class EfcSession : DbContext, ISession
	{
		private Action<DbContextOptionsBuilder> config;
		private Dictionary<Type, object> tables;

		public EfcSession(Action<DbContextOptionsBuilder> config)
			: base()
		{
			this.config = config;
			this.tables = new Dictionary<Type, object>();

			tables.Add(typeof(Commit), Set<Commit>());
			tables.Add(typeof(Author), Set<Author>());
			tables.Add(typeof(BugFix), Set<BugFix>());
			tables.Add(typeof(Modification), Set<Modification>());
			tables.Add(typeof(CodeFile), Set<CodeFile>());
			Database.EnsureCreated();
		}

		IQueryable<T> IRepository.Get<T>()
		{
			return (DbSet<T>)tables[typeof(T)];
		}
		IQueryable<T> IRepository.GetReadOnly<T>()
		{
			return ((DbSet<T>)tables[typeof(T)]).AsNoTracking();
		}
		void IRepository.Add<T>(T entity)
		{
			((DbSet<T>)tables[typeof(T)]).Add(entity);
		}
		void IRepository.AddRange<T>(IEnumerable<T> entities)
		{
			((DbSet<T>)tables[typeof(T)]).AddRange(entities);
		}
		void IRepository.Delete<T>(T entity)
		{
			((DbSet<T>)tables[typeof(T)]).Remove(entity);
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
			modelBuilder.Entity<Author>()
				.HasMany(a => a.Commits)
				.WithOne((string)null)
				.HasForeignKey(c => c.AuthorId);

			/*modelBuilder.Entity<Branch>()
				.HasMany(b => b.Commits)
				.WithOne(c => c.Branch)
				.HasForeignKey(c => c.BranchId);
			*/
			modelBuilder.Entity<BugFix>()
				.HasOne(bf => bf.Commit)
				.WithOne((string)null)
				.HasForeignKey<BugFix>(bf => bf.CommitId);
			modelBuilder.Entity<CodeFile>()
				.HasOne(f => f.AddedInCommit)
				.WithMany((string)null)
				.HasForeignKey(f => f.AddedInCommitId);
			modelBuilder.Entity<CodeFile>()
				.HasOne(f => f.DeletedInCommit)
				.WithMany((string)null)
				.HasForeignKey(f => f.DeletedInCommitId);
			modelBuilder.Entity<Modification>()
				.HasOne(m => m.Commit)
				.WithMany((string)null)
				.HasForeignKey(m => m.CommitId);
			modelBuilder.Entity<Modification>()
				.HasOne(m => m.File)
				.WithMany((string)null)
				.HasForeignKey(m => m.FileId);
		}
	}
}
