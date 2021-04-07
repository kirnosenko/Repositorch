using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositorch.Data.Entities.Persistent.Mapping
{
	public class BranchMapping : IEntityTypeConfiguration<Branch>
	{
		public void Configure(EntityTypeBuilder<Branch> builder)
		{
			builder.ToTable("Branches");

			builder.HasKey(b => b.Id);

			builder.OwnsOne(b => b.Mask).Property(x => x.Data)
				.IsUnicode(false)
				.IsRequired();

			builder.OwnsOne(b => b.Mask).Property(x => x.Offset)
				.IsRequired();

			builder.HasMany(b => b.Commits)
				.WithOne((string)null)
				.HasForeignKey(c => c.BranchId);
		}
	}
}
