using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositorch.Data.Entities.Persistent.Mapping
{
	public class ModificationMapping : IEntityTypeConfiguration<Modification>
	{
		public void Configure(EntityTypeBuilder<Modification> builder)
		{
			builder.ToTable("Modifications");

			builder.HasKey(m => m.Id);

			builder.Property(m => m.Action)
				.IsRequired();
			builder.HasIndex(m => m.Action);

			builder.HasOne(m => m.Commit)
				.WithMany((string)null)
				.HasForeignKey(m => m.CommitNumber)
				.IsRequired();
			
			builder.HasOne(m => m.File)
				.WithMany((string)null)
				.HasForeignKey(m => m.FileId)
				.IsRequired();

			builder.HasOne(m => m.SourceCommit)
				.WithMany((string)null)
				.HasForeignKey(m => m.SourceCommitNumber)
				.IsRequired(false);

			builder.HasOne(m => m.SourceFile)
				.WithMany((string)null)
				.HasForeignKey(m => m.SourceFileId)
				.IsRequired(false);
		}
	}
}
