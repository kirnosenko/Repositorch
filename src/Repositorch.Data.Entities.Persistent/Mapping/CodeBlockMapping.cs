using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositorch.Data.Entities.Persistent.Mapping
{
	public class CodeBlockMapping : IEntityTypeConfiguration<CodeBlock>
	{
		public void Configure(EntityTypeBuilder<CodeBlock> builder)
		{
			builder.ToTable("CodeBlocks");

			builder.HasKey(cb => cb.Id);

			builder.Property(cb => cb.Size)
				.IsRequired();
			builder.HasIndex(cb => cb.Size);

			builder.HasOne(cb => cb.Modification)
				.WithMany((string)null)
				.HasForeignKey(cb => cb.ModificationId)
				.IsRequired();

			builder.HasOne(cb => cb.AddedInitiallyInCommit)
				.WithMany((string)null)
				.HasForeignKey(cb => cb.AddedInitiallyInCommitNumber)
				.IsRequired(false);

			builder.HasOne(cb => cb.TargetCodeBlock)
				.WithMany((string)null)
				.HasForeignKey(cb => cb.TargetCodeBlockId)
				.IsRequired(false);
		}
	}
}
