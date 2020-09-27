using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositorch.Data.Entities.Persistent.Mapping
{
	public class TagMapping : IEntityTypeConfiguration<Tag>
	{
		public void Configure(EntityTypeBuilder<Tag> builder)
		{
			builder.ToTable("Tags");

			builder.HasKey(t => t.Id);

			builder.Property(t => t.Title)
				.HasMaxLength(255)
				.IsRequired();

			builder.HasOne(t => t.Commit)
				.WithMany((string)null)
				.HasForeignKey(t => t.CommitNumber)
				.IsRequired();
		}
	}
}
