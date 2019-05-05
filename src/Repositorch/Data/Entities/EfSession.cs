using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Repositorch.Data.Entities
{
	public class EfSession : DbContext, ISession
	{
		public DbSet<Commit> Commits { get; set; }
		public DbSet<Branch> Branches { get; set; }

		private Action<DbContextOptionsBuilder> config;
		private Dictionary<Type, object> tables;

		public EfSession(Action<DbContextOptionsBuilder> config)
			: base()
		{
			this.config = config;
			this.tables = new Dictionary<Type, object>();
			ReadOnly = false;

			Database.EnsureCreated();
			tables.Add(typeof(Commit), Commits);
			tables.Add(typeof(Branch), Branches);
		}
		public bool ReadOnly
		{
			get; set;
		}

		IQueryable<T> ISession.Get<T>(bool readOnly)
		{
			var dbset = (DbSet<T>)tables[typeof(T)];
			if (ReadOnly || readOnly)
			{
				return dbset.AsNoTracking();
			}
			return dbset;
		}
		void ISession.Add<T>(T entity)
		{
			if (ReadOnly)
			{
				return;
			}
			var dbset = (DbSet<T>)tables[typeof(T)];
			dbset.Add(entity);
		}
		void ISession.AddRange<T>(IEnumerable<T> entities)
		{
			if (ReadOnly)
			{
				return;
			}
			var dbset = (DbSet<T>)tables[typeof(T)];
			dbset.AddRange(entities);
		}
		void ISession.Delete<T>(T entity)
		{
			if (ReadOnly)
			{
				return;
			}
			var dbset = (DbSet<T>)tables[typeof(T)];
			dbset.Remove(entity);
		}
		void ISession.SubmitChanges()
		{
			if (ReadOnly)
			{
				return;
			}
			SaveChanges();
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			config(optionsBuilder);
		}
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Branch>()
				.HasMany(b => b.Commits)
				.WithOne(c => c.Branch)
				.HasForeignKey(c => c.BranchId);
		}
	}
}
