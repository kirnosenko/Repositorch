using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositorch.Data.Entities.Persistent.Mapping
{
	public class BugFixMapping : IEntityTypeConfiguration<BugFix>
	{
		public void Configure(EntityTypeBuilder<BugFix> builder)
		{
			builder.ToTable("BugFixes");

			builder.HasKey(bf => bf.Id);

			builder.HasOne(bf => bf.Commit)
				.WithOne((string)null)
				.HasForeignKey<BugFix>(bf => bf.CommitId)
				.IsRequired();
		}
	}
}
