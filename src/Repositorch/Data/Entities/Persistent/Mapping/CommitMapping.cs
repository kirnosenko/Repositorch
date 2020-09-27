using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositorch.Data.Entities.Persistent.Mapping
{
	public class CommitMapping : IEntityTypeConfiguration<Commit>
	{
		public void Configure(EntityTypeBuilder<Commit> builder)
		{
			builder.ToTable("Commits");

			builder.HasKey(c => c.Number);

			builder.Property(c => c.Number)
				.IsRequired()
				.ValueGeneratedNever();

			builder.Property(c => c.Revision)
				.HasMaxLength(40)
				.IsUnicode(false)
				.IsRequired();

			builder.Property(c => c.Message)
				.IsRequired(false);

			builder.Property(c => c.Date)
				.IsRequired();

			builder.HasIndex(c => c.Revision)
				.IsUnique();
		}
	}
}
