using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repositorch.Data.Entities.Persistent.Mapping
{
	public class CodeFileMapping : IEntityTypeConfiguration<CodeFile>
	{
		public void Configure(EntityTypeBuilder<CodeFile> builder)
		{
			builder.ToTable("CodeFiles");

			builder.HasKey(cf => cf.Id);

			builder.Property(cf => cf.Path)
				.IsRequired();

			builder.HasIndex(cf => cf.Path)
				.IsUnique();
		}
	}
}
