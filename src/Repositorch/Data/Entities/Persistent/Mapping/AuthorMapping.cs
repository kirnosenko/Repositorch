using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositorch.Data.Entities.Persistent.Mapping
{
	public class AuthorMapping : IEntityTypeConfiguration<Author>
	{
		public void Configure(EntityTypeBuilder<Author> builder)
		{
			builder.ToTable("Authors");

			builder.HasKey(a => a.Id);

			builder.Property(a => a.Name)
				.HasMaxLength(255)
				.IsRequired();

			builder.Property(a => a.Email)
				.HasMaxLength(255)
				.IsRequired();

			builder.HasMany(a => a.Commits)
				.WithOne((string)null)
				.HasForeignKey(c => c.AuthorId);
		}
	}
}
