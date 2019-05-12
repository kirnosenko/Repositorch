using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Repositorch.Data.Entities
{
	public class EfcSession : DbContext, ISession
	{
		public DbSet<Author> Authors { get; set; }
		//public DbSet<Branch> Branches { get; set; }
		public DbSet<Commit> Commits { get; set; }
		public DbSet<BugFix> BugFixes { get; set; }

		private Action<DbContextOptionsBuilder> config;
		private Dictionary<Type, object> tables;

		public EfcSession(Action<DbContextOptionsBuilder> config)
			: base()
		{
			this.config = config;
			this.tables = new Dictionary<Type, object>();

			Database.EnsureCreated();
			tables.Add(typeof(Author), Authors);
			//tables.Add(typeof(Branch), Branches);
			tables.Add(typeof(Commit), Commits);
			tables.Add(typeof(BugFix), BugFixes);
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
			var dbset = (DbSet<T>)tables[typeof(T)];
			dbset.Add(entity);
		}
		void IRepository.AddRange<T>(IEnumerable<T> entities)
		{
			var dbset = (DbSet<T>)tables[typeof(T)];
			dbset.AddRange(entities);
		}
		void IRepository.Delete<T>(T entity)
		{
			var dbset = (DbSet<T>)tables[typeof(T)];
			dbset.Remove(entity);
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
				.WithOne(c => c.Author)
				.HasForeignKey(c => c.AuthorId);
			
			/*modelBuilder.Entity<Branch>()
				.HasMany(b => b.Commits)
				.WithOne(c => c.Branch)
				.HasForeignKey(c => c.BranchId);
			*/
			modelBuilder.Entity<BugFix>()
				.HasOne(bf => bf.Commit)
				.WithOne()
				.HasForeignKey<BugFix>(bf => bf.CommitID);
		}
	}
}
