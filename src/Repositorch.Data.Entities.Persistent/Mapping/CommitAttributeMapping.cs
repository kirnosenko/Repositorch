using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositorch.Data.Entities.Persistent.Mapping
{
	public class CommitAttributeMapping : IEntityTypeConfiguration<CommitAttribute>
	{
		public void Configure(EntityTypeBuilder<CommitAttribute> builder)
		{
			builder.ToTable("CommitAttributes");

			builder.HasKey(ca => ca.Id);

			builder.Property(ca => ca.Type)
				.HasMaxLength(255)
				.IsRequired();
			builder.HasIndex(ca => ca.Type);

			builder.Property(ca => ca.Data)
				.IsRequired(false);

			builder.HasOne(ca => ca.Commit)
				.WithMany((string)null)
				.HasForeignKey(ca => ca.CommitNumber)
				.IsRequired();
		}
	}
}
